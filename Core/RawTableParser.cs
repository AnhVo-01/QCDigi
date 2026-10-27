using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

/// <summary>
/// Chuyển bảng thô (lưới chữ) thành các vùng có vai trò cột rõ ràng.
/// Thuật toán đã được kiểm chứng trên 7 biểu mẫu thật: nhận đúng bố cục cột
/// trang 1 của cả 3 loại mẫu (ORDER SPEC / QUALITY SPEC / INSPECTION REPORT).
/// </summary>
public static class RawTableParser
{
    // Từ khoá header khớp theo kiểu "chứa trong" - chỉ dùng cho từ đủ dài.
    private static readonly string[] HeaderSubstring =
    { "contents", "requirement", "specification", "specication", "tolerance", "admissible" };

    // Từ khoá ngắn phải khớp CHÍNH XÁC, nếu không "MINUTES" sẽ bị nhận nhầm là "MIN".
    private static readonly HashSet<string> HeaderExact = new(StringComparer.Ordinal)
    { "unit", "units", "target", "min", "max", "mean", "minimum", "maximum" };

    private static bool IsHeaderCell(string? cell)
    {
        var n = TextUtil.Normalize(cell);
        if (n.Length == 0) return false;
        if (HeaderExact.Contains(n)) return true;
        foreach (var k in HeaderSubstring) if (n.Contains(k, StringComparison.Ordinal)) return true;
        return n.StartsWith("target ", StringComparison.Ordinal);
    }

    private static int HeaderKeywordCount(string?[] row)
    {
        int n = 0;
        foreach (var c in row) if (IsHeaderCell(c)) n++;
        return n;
    }

    private static bool IsHeaderStart(string?[] row)
    {
        bool hasSpec = false;
        foreach (var c in row)
        {
            var n = TextUtil.Normalize(c);
            if (n.StartsWith("contents", StringComparison.Ordinal) ||
                n.StartsWith("requirement", StringComparison.Ordinal)) return true;
            if (n.Contains("specifi", StringComparison.Ordinal) || n.Contains("specica", StringComparison.Ordinal))
                hasSpec = true;
        }
        return hasSpec && HeaderKeywordCount(row) >= 3;
    }

    /// <summary>Lan tiêu đề nhóm sang phải để bù cho ô gộp (merged cell).</summary>
    private static string[] ForwardFill(string?[] row, int width)
    {
        var outp = new string[width];
        var last = "";
        for (int i = 0; i < width; i++)
        {
            var t = (i < row.Length ? row[i] : null)?.Trim() ?? "";
            if (t.Length > 0) last = t;
            outp[i] = last;
        }
        return outp;
    }

    /// <summary>
    /// Dựng bản đồ vai trò cột từ khối dòng header bắt đầu tại <paramref name="start"/>.
    /// Trả về số dòng header đã tiêu thụ.
    /// </summary>
    private static (ColRole[] roles, int consumed, string headerText) BuildColumnMap(
        List<string?[]> rows, int start, int width)
    {
        var block = new List<string?[]> { rows[start] };
        int j = start + 1;
        while (j < rows.Count && j - start < 3)
        {
            var r = rows[j];
            bool labelEmpty = r.Length == 0 || string.IsNullOrWhiteSpace(r[0]);
            if (labelEmpty || HeaderKeywordCount(r) >= 2) { block.Add(r); j++; }
            else break;
        }

        var span = ForwardFill(block[0], width);
        var sub = new string[width];
        Array.Fill(sub, "");
        for (int b = 1; b < block.Count; b++)
        {
            for (int k = 0; k < width; k++)
            {
                var t = (k < block[b].Length ? block[b][k] : null)?.Trim() ?? "";
                if (t.Length > 0 && IsHeaderCell(t) && sub[k].Length == 0) sub[k] = t;
            }
        }

        var roles = new ColRole[width];
        string? labelSpan = null;
        bool hasTolText = false;
        bool subLabelTaken = false;

        // Không cho phép gán trùng vai trò: cột thứ hai mang cùng tiêu đề nhóm
        // thực chất là cột giá trị của nhóm đó.
        void Put(int k, ColRole role)
        {
            bool unique = role is ColRole.Spec or ColRole.SpecVi or ColRole.SpecEn or ColRole.Unit or ColRole.TolText;
            if (unique && Array.IndexOf(roles, role) >= 0)
            {
                if (Array.IndexOf(roles, ColRole.Spec) < 0) { roles[k] = ColRole.Spec; return; }
                if (Array.IndexOf(roles, ColRole.TolText) < 0) { roles[k] = ColRole.TolText; return; }
                roles[k] = ColRole.None; return;
            }
            roles[k] = role;
        }

        for (int k = 0; k < width; k++)
        {
            var s = TextUtil.Normalize(span[k]);
            var b = TextUtil.Normalize(sub[k]);
            var combo = (s + " " + b).Trim();

            if (labelSpan is null &&
                (s.Contains("contents", StringComparison.Ordinal) || s.Contains("requirement", StringComparison.Ordinal)))
            { roles[k] = ColRole.Label; labelSpan = span[k]; continue; }

            // Cột ngay sau nhãn, thừa hưởng cùng tiêu đề nhóm => cột nhãn con.
            if (labelSpan is not null && k > 0 && !subLabelTaken &&
                string.Equals(span[k], labelSpan, StringComparison.Ordinal) && roles[k] == ColRole.None)
            { roles[k] = ColRole.SubLabel; subLabelTaken = true; continue; }

            if (combo.Contains("vietnam", StringComparison.Ordinal)) { Put(k, ColRole.SpecVi); continue; }
            if (combo.Contains("english", StringComparison.Ordinal)) { Put(k, ColRole.SpecEn); continue; }
            if (combo.Contains("specifi", StringComparison.Ordinal) || combo.Contains("specica", StringComparison.Ordinal)
                || combo.StartsWith("target", StringComparison.Ordinal)) { Put(k, ColRole.Spec); continue; }
            if (combo.Contains("unit", StringComparison.Ordinal)) { Put(k, ColRole.Unit); continue; }

            if (combo.Contains("admissible", StringComparison.Ordinal)
                || (combo.Contains("tolerance", StringComparison.Ordinal) && b.Length == 0))
            { Put(k, ColRole.TolText); hasTolText = true; continue; }

            bool isMin = b == "min", isMax = b == "max";
            if (s.Contains("tolerance", StringComparison.Ordinal) && (isMin || isMax))
            {
                bool percent = s.Contains("percent", StringComparison.Ordinal);
                bool measuring = s.Contains("measur", StringComparison.Ordinal);
                bool asTol = percent || (!measuring && !hasTolText);
                roles[k] = asTol ? (isMin ? ColRole.TolMin : ColRole.TolMax)
                                 : (isMin ? ColRole.MeasMin : ColRole.MeasMax);
                continue;
            }
            if ((isMin || isMax) && labelSpan is not null)
            {
                // Cặp MIN/MAX thứ hai của vùng là trị số đo, không phải dung sai lần nữa.
                // Trước đây chỗ này chỉ nhìn hasTolText - tức chỉ nhận ra cột dung sai khi nó
                // ghi bằng CHỮ. Biểu mẫu ghi dung sai bằng một cặp MIN/MAX (TOLERANCE (%) MIN|MAX
                // rồi MEASURING MIN|MAX) thì cả hai cặp đều bị gán TolMin/TolMax, trị số đo
                // lặng lẽ nằm vào ô dung sai và trị số đo tự tính ra sai.
                bool measuring = s.Contains("measur", StringComparison.Ordinal);
                bool tolTaken = hasTolText
                                || Array.IndexOf(roles, ColRole.TolMin) >= 0
                                || Array.IndexOf(roles, ColRole.TolMax) >= 0;

                roles[k] = (measuring || tolTaken)
                    ? (isMin ? ColRole.MeasMin : ColRole.MeasMax)
                    : (isMin ? ColRole.TolMin : ColRole.TolMax);
                continue;
            }
        }

        var headerText = string.Join(" | ", block[0].Where(x => !string.IsNullOrWhiteSpace(x)));
        return (roles, block.Count, headerText);
    }

    /// <summary>
    /// Chỉnh lại vai trò cột cho nhóm bảng THỬ CƠ LÝ (RESISTANCE TESTS và tương đương),
    /// dựa trên dữ liệu thật của vùng chứ không chỉ dựa vào dòng tiêu đề.
    ///
    /// Hai biến thể gặp trong thực tế:
    ///   ORDER SPEC   : CONTENTS | UNITS        | giá trị | TOLERANCE
    ///   QUALITY SPEC : CONTENTS | SPECICATIONS | Unit    | giá trị | Tolerance
    ///
    /// Cả hai đều có cột giá trị mang tiêu đề là mã hàng hoặc để trống, nên vòng dò theo
    /// từ khoá hoặc bỏ sót cột (biến thể 1), hoặc đẩy trị số sang cột dung sai (biến thể 2).
    /// Trường hợp sau nguy hiểm hơn: trị số lặng lẽ nằm sai chỗ thay vì mất hẳn.
    ///
    /// Quy tắc: vùng CÓ cột đơn vị, mà cột giá trị suy ra từ tiêu đề lại RỖNG ở mọi dòng
    /// dữ liệu, thì cột có dữ liệu đầu tiên sau cột đơn vị mới là giá trị thật (SpecVi),
    /// cột có dữ liệu kế tiếp là dung sai dạng chữ.
    ///
    /// Gán SpecVi chứ không phải Spec: trị số đi thẳng vào SpecificationsVi, khỏi qua
    /// bước tách Việt/Anh vốn chỉ dành cho ô mô tả.
    ///
    /// Trang 1 không bao giờ có cột đơn vị nên không bị ảnh hưởng.
    /// </summary>
    private static void RefineMechanicalTestRoles(
        ColRole[] roles, List<string?[]> rows, int firstData, int width)
    {
        int unit = Array.IndexOf(roles, ColRole.Unit);
        if (unit < 0) return;

        // Cột nào thực sự mang dữ liệu trong vùng này?
        var hasData = new bool[width];
        int dataRows = 0;
        for (int r = firstData; r < rows.Count; r++)
        {
            if (IsHeaderStart(rows[r])) break;   // đã sang vùng khác
            dataRows++;
            for (int k = 0; k < width && k < rows[r].Length; k++)
                if (!string.IsNullOrWhiteSpace(rows[r][k])) hasData[k] = true;
        }
        if (dataRows == 0) return;

        // Cột giá trị theo tiêu đề có dữ liệu thật thì bố cục vốn đã đúng - không đụng vào.
        for (int k = 0; k < width; k++)
            if ((roles[k] is ColRole.Spec or ColRole.SpecVi or ColRole.SpecEn) && hasData[k]) return;

        // Hạ vai trò cột giá trị "ma": tiêu đề có nhưng không mang dữ liệu nào.
        for (int k = 0; k < width; k++)
            if (roles[k] is ColRole.Spec or ColRole.SpecVi or ColRole.SpecEn) roles[k] = ColRole.None;

        int value = -1, tol = -1;
        for (int k = unit + 1; k < width; k++)
        {
            if (!hasData[k]) continue;
            if (value < 0) { value = k; continue; }
            tol = k; break;
        }
        if (value < 0) return;

        roles[value] = ColRole.SpecVi;
        if (tol >= 0 && roles[tol] == ColRole.None) roles[tol] = ColRole.TolText;
    }

    /// <summary>
    /// Lan giá trị của Ô MÔ TẢ ĐƯỢC GỘP xuống các dòng con cùng nhóm.
    ///
    /// Biểu mẫu hay gộp một ô mô tả cho nhiều dòng tiêu chí liền nhau, ví dụ:
    ///     PANTONE COLOR | 1 (Mã màu 1) |            |                       | 7741C | 2465C
    ///     PANTONE COLOR | 2 (Mã màu 2) | Như market | As confirmed artworks | 7741C | 2465C
    /// Khi tách lưới, chữ của ô gộp chỉ thuộc về MỘT dòng, dòng còn lại trống -
    /// trong khi trên giấy nó áp cho cả hai.
    ///
    /// Ba điều kiện để coi là ô gộp, cố ý chặt để không lan nhầm:
    ///   1. Các dòng liền nhau cùng một nhãn nhóm (nhãn trống = nằm dưới ô nhãn gộp).
    ///   2. Mỗi dòng trong nhóm phải có nhãn con riêng - tức là tiêu chí độc lập,
    ///      không phải dòng chữ bị xuống hàng.
    ///   3. ĐÚNG MỘT dòng giữ giá trị. Từ hai dòng trở lên nghĩa là các ô riêng biệt,
    ///      không được đụng vào.
    ///
    /// Chỉ lan ba cột mô tả. Dung sai và trị số đo là của từng dòng, không bao giờ lan.
    /// </summary>
    private static void SpreadMergedSpecCells(TableSection section)
    {
        var rows = section.Rows;
        int i = 0;
        while (i < rows.Count)
        {
            var groupLabel = TextUtil.Clean(rows[i].Label);
            int j = i + 1;
            while (j < rows.Count)
            {
                var lab = TextUtil.Clean(rows[j].Label);
                if (lab is null || string.Equals(lab, groupLabel, StringComparison.Ordinal)) j++;
                else break;
            }

            TrySpreadGroup(rows, i, j);
            i = Math.Max(j, i + 1);
        }
    }

    /// <summary>Nhóm dài hơn ngưỡng này gần như chắc chắn là gom nhầm - không lan.</summary>
    private const int MaxMergedGroupRows = 4;

    /// <summary>
    /// Dòng có "danh tính riêng" hay không: có nhãn con, hoặc tự mang ít nhất một trị số
    /// của riêng nó (đơn vị, dung sai, trị số đo).
    ///
    /// Đây là điều kiện để phân biệt một dòng thật nằm trong ô gộp với một dòng kẻ thừa
    /// của bảng. Dòng kẻ thừa không có gì cả: nếu lan giá trị vào thì nó "sống lại" và
    /// sinh ra một tiêu chí trùng lặp vốn không hề có trên giấy.
    ///
    /// Trước đây chỗ này bắt buộc phải có SubLabel. Điều kiện đó quá chặt: những vùng bảng
    /// không hề có cột nhãn con (ví dụ hai dòng PANTONE COLOR chỉ khác nhau ở cột đo)
    /// không bao giờ lọt qua, nên giá trị trong ô gộp không bao giờ được lan - đúng hiện
    /// tượng "thông tin vẫn giữ nguyên ở một dòng".
    /// </summary>
    private static bool HasOwnIdentity(RawRow r) =>
        !string.IsNullOrWhiteSpace(r.SubLabel)
        || !string.IsNullOrWhiteSpace(r.Unit)
        || !string.IsNullOrWhiteSpace(r.TolText)
        || !string.IsNullOrWhiteSpace(r.TolMin) || !string.IsNullOrWhiteSpace(r.TolMax)
        || !string.IsNullOrWhiteSpace(r.MeasMin) || !string.IsNullOrWhiteSpace(r.MeasMax);

    private static void TrySpreadGroup(List<RawRow> rows, int from, int to)
    {
        int count = to - from;
        if (count < 2 || count > MaxMergedGroupRows) return;

        // Nhóm phải xuất phát từ một nhãn thật. Nhiều dòng cùng để trống nhãn thì không
        // phải ô gộp, chỉ là mấy dòng rỗng nằm cạnh nhau.
        if (string.IsNullOrWhiteSpace(rows[from].Label)) return;

        for (int k = from; k < to; k++)
            if (!HasOwnIdentity(rows[k])) return;

        SpreadColumn(rows, from, to, r => r.SpecVi, (r, v) => r.SpecVi = v);
        SpreadColumn(rows, from, to, r => r.SpecEn, (r, v) => r.SpecEn = v);
        SpreadColumn(rows, from, to, r => r.Spec, (r, v) => r.Spec = v);
    }

    private static void SpreadColumn(List<RawRow> rows, int from, int to,
                                     Func<RawRow, string?> get, Action<RawRow, string?> set)
    {
        string? value = null;
        int holders = 0;
        for (int k = from; k < to; k++)
        {
            var v = get(rows[k]);
            if (string.IsNullOrWhiteSpace(v)) continue;
            holders++;
            value = v;
        }
        if (holders != 1) return;

        for (int k = from; k < to; k++)
            if (string.IsNullOrWhiteSpace(get(rows[k])))
            {
                set(rows[k], value);
                rows[k].SpecInherited = true;
            }
    }

    /// <summary>Phân tích toàn bộ bảng của một file thành các vùng dữ liệu.</summary>
    public static List<TableSection> Parse(IEnumerable<ExtractedTable> tables)
    {
        var sections = new List<TableSection>();

        foreach (var table in tables)
        {
            int width = table.ColCount;
            if (width == 0) continue;

            var rows = table.Rows;
            TableSection? current = null;
            string? lastLabel = null;
            int i = 0;

            while (i < rows.Count)
            {
                var row = rows[i];
                if (IsHeaderStart(row))
                {
                    var (roles, consumed, headerText) = BuildColumnMap(rows, i, width);
                    RefineMechanicalTestRoles(roles, rows, i + consumed, width);
                    current = new TableSection
                    {
                        PageNumber = table.PageNumber,
                        Roles = roles,
                        HeaderText = headerText
                    };
                    sections.Add(current);
                    lastLabel = null;
                    i += consumed;
                    continue;
                }

                if (current is null) { i++; continue; }

                var rec = new RawRow { PageNumber = table.PageNumber, RowIndex = i };
                for (int k = 0; k < current.Roles.Length && k < row.Length; k++)
                {
                    var val = TextUtil.Clean(row[k]);
                    if (val is null) continue;
                    switch (current.Roles[k])
                    {
                        case ColRole.Label: rec.Label = val; break;
                        case ColRole.SubLabel: rec.SubLabel = val; break;
                        case ColRole.SpecVi: rec.SpecVi = val; break;
                        case ColRole.SpecEn: rec.SpecEn = val; break;
                        case ColRole.Spec: rec.Spec = val; break;
                        case ColRole.Unit: rec.Unit = val; break;
                        case ColRole.TolText: rec.TolText = val; break;
                        case ColRole.TolMin: rec.TolMin = val; break;
                        case ColRole.TolMax: rec.TolMax = val; break;
                        case ColRole.MeasMin: rec.MeasMin = val; break;
                        case ColRole.MeasMax: rec.MeasMax = val; break;
                    }
                }

                // Ô nhãn gộp theo chiều dọc: dòng dưới không in lại nhãn nhưng vẫn mang trị số
                // của riêng nó. Không mượn lại nhãn thì dòng đó bị loại ngay tại đây và biến mất
                // khỏi kết quả mà không có cảnh báo nào.
                if (!string.IsNullOrWhiteSpace(rec.Label))
                {
                    lastLabel = rec.Label;
                }
                else if (string.IsNullOrWhiteSpace(rec.SubLabel) && rec.HasAnyValue && lastLabel is not null)
                {
                    rec.Label = lastLabel;
                    rec.LabelInherited = true;
                }

                if (!string.IsNullOrWhiteSpace(rec.Label) || !string.IsNullOrWhiteSpace(rec.SubLabel))
                    current.Rows.Add(rec);

                i++;
            }
        }

        foreach (var section in sections) SpreadMergedSpecCells(section);

        return sections;
    }
}
