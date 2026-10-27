using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using TcclPdfTool.Config;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

/// <summary>
/// Ghép mọi thứ lại: bảng thô -> map nhãn -> dung sai -> trị số đo -> Func -> TieuChiData.
/// </summary>
public sealed class TieuChiDataBuilder
{
    private readonly LabelMatcher _matcher;
    private readonly MappingConfig _cfg;
    private readonly ILabelLlmMapper _llm;

    public TieuChiDataBuilder(LabelMatcher matcher, MappingConfig cfg, ILabelLlmMapper? llm = null)
    {
        _matcher = matcher;
        _cfg = cfg;
        _llm = llm ?? new NullLabelLlmMapper();
    }

    public async Task<ImportResult> BuildAsync(
        string filePath, IReadOnlyList<TableSection> sections, string? firstPageText,
        CancellationToken ct = default)
    {
        var result = new ImportResult
        {
            FilePath = filePath,
            FileName = System.IO.Path.GetFileName(filePath),
            DetectedTemplate = DetectTemplate(firstPageText)
        };
        result.Sections.AddRange(sections);

        // Bố cục cột nhận được là đầu mối đầu tiên khi một biểu mẫu ra kết quả lạ:
        // thiếu SpecVi/SpecEn nghĩa là header bị cắt sai, không phải lỗi ở phần map nhãn.
        foreach (var sec in sections)
        {
            var cols = string.Join(" | ", sec.Roles.Select(x => x == ColRole.None ? "." : x.ToString()));
            result.Warn(Severity.Info, "BO_CUC_COT",
                $"Trang {sec.PageNumber}, {sec.Rows.Count} dòng - vai trò cột: {cols}", sec.PageNumber);
        }

        // ---- vòng 1: alias + fuzzy, gom các dòng chưa quyết được ----
        var pending = new List<(RawRow row, int key)>();
        var rowByKey = new Dictionary<int, RawRow>();
        int keySeq = 0;

        foreach (var sec in sections)
        {
            foreach (var row in sec.Rows)
            {
                ct.ThrowIfCancellationRequested();
                result.RowsRead++;

                if (!row.HasAnyValue) continue;
                if (_matcher.ShouldIgnore(row.EffectiveLabel)) continue;

                var m = _matcher.Match(row);
                if (m.Accepted && m.Item is not null)
                {
                    Apply(result, row, m.Item, m.Score, m.Source, m.CandidateText, m.NeedsReview);
                }
                else
                {
                    var key = keySeq++;
                    rowByKey[key] = row;
                    pending.Add((row, key));
                }
            }
        }

        // ---- vòng 2: hỏi LLM cho phần còn lại, một lượt cho cả file ----
        if (pending.Count > 0)
        {
            var items = pending.Select(p => new LlmMapItem
            {
                Key = p.key,
                Label = LabelMatcher.DisplayLabel(p.row),
                SampleValue = p.row.SpecVi ?? p.row.Spec ?? p.row.SpecEn,
                Unit = p.row.Unit
            }).ToList();

            IReadOnlyList<LlmMapAnswer> answers;
            try
            {
                answers = await _llm.MapAsync(items, _matcher.Catalog, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                answers = Array.Empty<LlmMapAnswer>();
                result.Warn(Severity.Warning, "LLM_LOI",
                    $"Không gọi được dịch vụ map nhãn: {ex.Message}. Các nhãn chưa khớp sẽ để trống.");
            }

            var answered = new HashSet<int>();
            foreach (var a in answers)
            {
                if (a.MaTieuChi is null) continue;
                if (!rowByKey.TryGetValue(a.Key, out var row)) continue;
                var item = _matcher.ByCode(a.MaTieuChi);
                if (item is null)
                {
                    result.Warn(Severity.Warning, "LLM_MA_LA",
                        $"Mô hình trả về mã '{a.MaTieuChi}' không có trong danh mục - đã bỏ qua.",
                        row.PageNumber, row.RowIndex);
                    continue;
                }
                answered.Add(a.Key);
                // Kết quả từ LLM luôn phải qua mắt người.
                Apply(result, row, item, a.Confidence, MapSource.Llm, LabelMatcher.DisplayLabel(row), needsReview: true);
            }

            foreach (var (row, key) in pending)
                if (!answered.Contains(key)) result.UnmappedRows.Add(row);
        }

        if (result.UnmappedRows.Count > 0)
        {
            result.Warn(Severity.Warning, "CHUA_MAP",
                $"{result.UnmappedRows.Count} dòng trên biểu mẫu chưa khớp được tiêu chí nào trong danh mục. " +
                "Nếu là nhóm tiêu chí cơ lý (tensile, elongation, dart drop...) thì cần bổ sung DM_TieuChi trước.");
        }

        ApplyComputedFields(result);
        result.FormType = DetectFormType(result, firstPageText);
        return result;
    }

    // ------------------------------------------------------------------ nội bộ

    private void Apply(ImportResult result, RawRow row, DmTieuChi item,
                       double score, MapSource source, string candidate, bool needsReview)
    {
        string? vi = row.SpecVi, en = row.SpecEn;
        if (vi is null && en is null && row.Spec is not null)
            (vi, en) = TextUtil.SplitViEn(row.Spec);

        // Kiểu dung sai: ưu tiên suy từ giá trị đọc được, không đồng nhất thì tạm lấy theo DM_TieuChi
        // và để người soát chọn lại.
        var detectedType = ToleranceCalculator.DetectToleranceType(row.TolMin, row.TolMax, row.TolText);
        var tolNeedsChoice = detectedType is null;
        var tolType = detectedType ?? item.Tolerance;
        if (tolNeedsChoice)
        {
            needsReview = true;
            result.Warn(Severity.Warning, "DUNG_SAI_KHONG_DONG_NHAT",
                $"'{LabelMatcher.DisplayLabel(row)}': dung sai Min \"{row.TolMin}\" / Max \"{row.TolMax}\" không đồng nhất - " +
                $"tạm lấy kiểu theo danh mục ({item.Tolerance ?? "trống"}), cần chọn lại.",
                row.PageNumber, row.RowIndex);
        }

        var tolMin = TextUtil.ParseNumber(row.TolMin);
        var tolMax = TextUtil.ParseNumber(row.TolMax);

        // Cột dung sai dạng chữ ("+-3%", "Min") của mẫu QUALITY SPEC / INSPECTION REPORT.
        if (tolMin is null && tolMax is null && row.TolText is not null)
        {
            var (tmin, tmax, _) = ToleranceCalculator.ParseToleranceText(row.TolText);
            if (tmin is not null) { tolMin = tmin; tolMax = tmax; }
            else
            {
                result.Warn(Severity.Info, "DUNG_SAI_CHU",
                    $"'{LabelMatcher.DisplayLabel(row)}': dung sai ghi bằng chữ \"{row.TolText}\" - chưa quy ra số được.",
                    row.PageNumber, row.RowIndex);
            }
        }

        var printedMin = TextUtil.ParseNumber(row.MeasMin);
        var printedMax = TextUtil.ParseNumber(row.MeasMax);
        var (calcMin, calcMax) = ToleranceCalculator.Compute(vi ?? row.Spec, tolMin, tolMax, tolType);

        decimal? measMin = printedMin ?? calcMin;
        decimal? measMax = printedMax ?? calcMax;
        string? note = null;

        // Biểu mẫu in sẵn trị số đo -> dùng làm phép kiểm tra chéo cho phần đọc số.
        if (printedMin is not null && calcMin is not null && ToleranceCalculator.Disagrees(printedMin, calcMin))
        {
            note = $"Trị số đo in trên biểu mẫu ({printedMin}) lệch so với tự tính ({calcMin}).";
            needsReview = true;
            result.Warn(Severity.Warning, "LECH_TRI_SO_DO",
                $"'{LabelMatcher.DisplayLabel(row)}': {note}", row.PageNumber, row.RowIndex);
        }

        var field = new TieuChiField
        {
            MaTieuChi = item.MaTieuChi,
            DanhMucId = item.Id,
            Tolerance = tolType,
            ToleranceNeedsChoice = tolNeedsChoice,
            SpecificationsVi = vi,
            SpecificationsEn = en,
            MinTolerance = tolMin,
            MaxTolerance = tolMax,
            MinMeasuring = ToleranceCalculator.Round(measMin),
            MaxMeasuring = ToleranceCalculator.Round(measMax),
            MinMeasuringText = ToleranceCalculator.IsStringType(tolType) ? TextUtil.Clean(row.MeasMin) : null,
            MaxMeasuringText = ToleranceCalculator.IsStringType(tolType) ? TextUtil.Clean(row.MeasMax) : null,
            Confidence = score,
            MappedBy = source,
            SourceLabel = LabelMatcher.DisplayLabel(row),
            PageNumber = row.PageNumber,
            RowIndex = row.RowIndex,
            NeedsReview = needsReview,
            MeasuringNote = note,
            SpecInherited = row.SpecInherited,
            LabelInherited = row.LabelInherited
        };

        // Giá trị mượn từ ô gộp đúng về nội dung nhưng không in riêng ở dòng này - phải qua mắt người.
        if (row.SpecInherited || row.LabelInherited) field.NeedsReview = true;

        if (result.Fields.TryGetValue(item.MaTieuChi, out var existing))
        {
            // Cùng một mã bị gán cho hai dòng khác nhau - gần như chắc chắn có một dòng sai.
            result.Warn(Severity.Warning, "TRUNG_TIEU_CHI",
                $"Tiêu chí '{item.MaTieuChi}' xuất hiện hai lần: \"{existing.SourceLabel}\" " +
                $"và \"{field.SourceLabel}\" (trang {row.PageNumber}). Đang giữ dòng có độ tin cậy cao hơn.",
                row.PageNumber, row.RowIndex);

            existing.NeedsReview = true;

            // Dòng thua không được biến mất: đẩy sang danh sách chưa map để nó vẫn hiện trên
            // lưới rà soát, người soát tự quyết giữ dòng nào.
            if (field.Confidence <= existing.Confidence) { result.UnmappedRows.Add(row); return; }

            field.NeedsReview = true;
        }

        result.Fields[item.MaTieuChi] = field;
    }

    /// <summary>Chạy các tiêu chí có công thức (Func) sau khi đã có đủ tiêu chí nguồn.</summary>
    private void ApplyComputedFields(ImportResult result)
    {
        foreach (var dm in _matcher.Catalog)
        {
            if (string.IsNullOrWhiteSpace(dm.Func)) continue;

            // Đã đọc được trực tiếp từ biểu mẫu thì ưu tiên số trên giấy.
            if (result.Fields.TryGetValue(dm.MaTieuChi, out var have)
                && !string.IsNullOrWhiteSpace(have.SpecificationsVi)) continue;

            decimal? value;
            try
            {
                value = FuncEvaluator.Evaluate(dm.Func, result.Fields);
            }
            catch (FuncEvaluationException ex)
            {
                result.Warn(Severity.Warning, "FUNC_LOI",
                    $"Không chạy được công thức của '{dm.MaTieuChi}': {ex.Message}");
                continue;
            }

            if (value is null) continue;

            var rounded = Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
            var (mn, mx) = ToleranceCalculator.Compute(
                rounded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dm.MinTolerance, dm.MaxTolerance, dm.Tolerance);

            result.Fields[dm.MaTieuChi] = new TieuChiField
            {
                MaTieuChi = dm.MaTieuChi,
                DanhMucId = dm.Id,
                Tolerance = dm.Tolerance,
                SpecificationsVi = rounded.ToString(System.Globalization.CultureInfo.InvariantCulture),
                SpecificationsEn = null,
                MinTolerance = dm.MinTolerance,
                MaxTolerance = dm.MaxTolerance,
                MinMeasuring = mn,
                MaxMeasuring = mx,
                Confidence = 1.0,
                MappedBy = MapSource.Alias,
                SourceLabel = "(tính theo công thức)",
                NeedsReview = false
            };
        }
    }

    private string DetectTemplate(string? firstPageText)
    {
        if (string.IsNullOrWhiteSpace(firstPageText)) return "";
        var n = TextUtil.Normalize(firstPageText);
        if (n.Contains("order specifications", StringComparison.Ordinal)) return "ORDER SPECIFICATIONS";
        if (n.Contains("quality specifications", StringComparison.Ordinal)) return "QUALITY SPECIFICATIONS";
        if (n.Contains("inspection report", StringComparison.Ordinal)) return "INSPECTION REPORT";
        return "";
    }

    // ------------------------------------------------------------ FormType

    public const string FormTypePE = "PE";
    public const string FormTypeTH = "TH";
    public const string OuterBagLabelCode = "outerBagLabel";

    /// <summary>Nhãn "OUTER BAG LABEL" / "Tem dán bao bì" đã chuẩn hoá (bỏ dấu, chữ thường).</summary>
    private static readonly string[] OuterBagLabelKeys = { "outer bag label", "tem dan bao bi" };

    /// <summary>
    /// Biểu mẫu có OUTER BAG LABEL (Tem dán bao bì) - dù đọc thấy chữ hay đã gán vào tiêu chí
    /// outerBagLabel - thì là "TH", còn lại là "PE".
    /// Gọi lại sau khi sửa tay để FormType bám theo tiêu chí đã map.
    /// </summary>
    public static string DetectFormType(ImportResult result, string? firstPageText = null)
    {
        if (result.Fields.Keys.Any(k => string.Equals(k, OuterBagLabelCode, StringComparison.OrdinalIgnoreCase)))
            return FormTypeTH;

        if (HasOuterBagLabelText(firstPageText)) return FormTypeTH;

        foreach (var sec in result.Sections)
            foreach (var row in sec.Rows)
                if (HasOuterBagLabelText(row.Label) || HasOuterBagLabelText(row.SubLabel))
                    return FormTypeTH;

        return FormTypePE;
    }

    /// <summary>File mẫu Excel tương ứng FormType - ghi vào cột FilePath khi xuất.</summary>
    public static string TemplatePathFor(string formType)
        => formType == FormTypeTH ? "/Template/TCCL/FormTH.xlsx" : "/Template/TCCL/FormPE.xlsx";

    private static bool HasOuterBagLabelText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var n = TextUtil.Normalize(text);
        return OuterBagLabelKeys.Any(k => n.Contains(k, StringComparison.Ordinal));
    }

    // ------------------------------------------------------------ xuất JSON

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Sinh chuỗi cho cột TieuChiChatLuong.TieuChiData.
    /// Cấu trúc bám đúng BuildTieuChiData() phía server: { maTieuChi: { DanhMucId, ... } }.
    /// Các trường phục vụ rà soát (Confidence, SourceLabel...) cố ý KHÔNG được ghi ra.
    /// </summary>
    public static string ToTieuChiDataJson(IReadOnlyDictionary<string, TieuChiField> fields)
    {
        var root = new JsonObject();
        foreach (var kv in fields.OrderBy(x => x.Value.DanhMucId))
        {
            var f = kv.Value;
            root[kv.Key] = new JsonObject
            {
                ["DanhMucId"] = f.DanhMucId,
                ["Tolerance"] = f.Tolerance,
                ["SpecificationsVi"] = f.SpecificationsVi,
                ["SpecificationsEn"] = f.SpecificationsEn,
                ["MinTolerance"] = f.MinTolerance,
                ["MaxTolerance"] = f.MaxTolerance,
                ["MinMeasuring"] = MeasuringNode(f.MinMeasuringText, f.MinMeasuring),
                ["MaxMeasuring"] = MeasuringNode(f.MaxMeasuringText, f.MaxMeasuring)
            };
        }
        return root.ToJsonString(JsonOpts);
    }

    /// <summary>Kiểu "string" có trị số đo dạng chữ thì ghi nguyên văn, còn lại ghi số.</summary>
    private static JsonNode? MeasuringNode(string? text, decimal? value)
        => text is not null ? JsonValue.Create(text) : value is null ? null : JsonValue.Create(value.Value);

    private static readonly JsonSerializerOptions CompactJsonOpts = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// TieuChiData đúng dạng chuẩn của bảng TieuChiChatLuong (một dòng, không có trường Tolerance dạng chữ) -
    /// dùng cho file xuất ra để nạp thẳng vào CSDL.
    /// </summary>
    public static string ToStandardTieuChiDataJson(IReadOnlyDictionary<string, TieuChiField> fields)
    {
        var root = new JsonObject();
        foreach (var kv in fields.OrderBy(x => x.Value.DanhMucId))
        {
            var f = kv.Value;
            root[kv.Key] = new JsonObject
            {
                ["DanhMucId"] = f.DanhMucId,
                ["SpecificationsVi"] = f.SpecificationsVi,
                ["SpecificationsEn"] = f.SpecificationsEn,
                ["MinTolerance"] = f.MinTolerance,
                ["MaxTolerance"] = f.MaxTolerance,
                ["MinMeasuring"] = MeasuringNode(f.MinMeasuringText, f.MinMeasuring),
                ["MaxMeasuring"] = MeasuringNode(f.MaxMeasuringText, f.MaxMeasuring)
            };
        }
        return root.ToJsonString(CompactJsonOpts);
    }
}
