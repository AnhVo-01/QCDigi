using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ColRole
{
    None = 0,
    Label,      // CONTENTS / REQUIREMENT
    SubLabel,   // cột con của nhóm (WIDTH / GUSSET / LENGTH)
    SpecVi,     // SPECIFICATIONS (VIETNAMMESE)
    SpecEn,     // SPECIFICATIONS (ENGLISH)
    Spec,       // SPECICATIONS / Target - gộp Việt + Anh trong 1 ô
    Unit,       // UNITS
    TolText,    // TOLERANCE / admissible variation - dạng chữ "+-3%", "Min"
    TolMin,
    TolMax,
    MeasMin,
    MeasMax
}

public enum MapSource { None = 0, Alias, Fuzzy, Llm, Manual }

public enum Severity { Info = 0, Warning = 1, Error = 2 }

namespace TcclPdfTool.Model
{
    public sealed class ImportResult
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";

        /// <summary>DM_SanPham.Id - ghi vào TieuChiChatLuong.SanPhamId.</summary>
        public int? SanPhamId { get; set; }

        /// <summary>XXThucHien.Id - ghi vào TieuChiChatLuong.XXTHId.</summary>
        public int? XxthId { get; set; }

        public string? MaSPNB { get; set; }
        public string FormType { get; set; } = "";
        public string DetectedTemplate { get; set; } = "";
        public List<string> ItemCodes { get; } = new();

        public Dictionary<string, TieuChiField> Fields { get; } = new(StringComparer.Ordinal);
        public List<ImportWarning> Warnings { get; } = new();
        public List<TableSection> Sections { get; } = new();
        public List<RawRow> UnmappedRows { get; } = new();

        public int RowsRead { get; set; }
        public bool Failed { get; set; }
        public string? Error { get; set; }

        public int NeedsReviewCount
        {
            get { int n = 0; foreach (var f in Fields.Values) if (f.NeedsReview) n++; return n; }
        }

        public void Warn(Severity sev, string code, string message, int page = 0, int row = 0)
            => Warnings.Add(new ImportWarning { Severity = sev, Code = code, Message = message, PageNumber = page, RowIndex = row });
    }

    public sealed class TieuChiField
    {
        public string MaTieuChi { get; set; } = "";
        public int DanhMucId { get; set; }
        public string? Tolerance { get; set; }
        public string? SpecificationsVi { get; set; }
        public string? SpecificationsEn { get; set; }
        public decimal? MinTolerance { get; set; }
        public decimal? MaxTolerance { get; set; }
        public decimal? MinMeasuring { get; set; }
        public decimal? MaxMeasuring { get; set; }

        /// <summary>Tolerance = "string": trị số đo giữ nguyên văn như đọc được, không quy ra số.</summary>
        public string? MinMeasuringText { get; set; }
        public string? MaxMeasuringText { get; set; }

        // --- siêu dữ liệu phục vụ rà soát, KHÔNG ghi vào TieuChiData ---
        public double Confidence { get; set; }
        public MapSource MappedBy { get; set; }
        public string SourceLabel { get; set; } = "";
        public int PageNumber { get; set; }
        public int RowIndex { get; set; }
        public bool NeedsReview { get; set; }
        public string? MeasuringNote { get; set; }

        /// <summary>Mô tả / nhãn của dòng nguồn được kế thừa từ ô gộp - xem RawRow.SpecInherited.</summary>
        public bool SpecInherited { get; set; }
        public bool LabelInherited { get; set; }

        /// <summary>Min/Max dung sai đọc được không đồng nhất - kiểu Tolerance đang tạm theo danh mục, cần người soát chọn.</summary>
        public bool ToleranceNeedsChoice { get; set; }
    }

    public sealed class ImportWarning
    {
        public Severity Severity { get; set; } = Severity.Warning;
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
        public int PageNumber { get; set; }
        public int RowIndex { get; set; }

        public override string ToString() => $"[{Severity}] {Code}: {Message}";
    }

    public sealed class TableSection
    {
        public int PageNumber { get; set; }
        public ColRole[] Roles { get; set; } = Array.Empty<ColRole>();
        public List<RawRow> Rows { get; } = new();
        public string HeaderText { get; set; } = "";
    }

    public sealed class RawRow
    {
        public int PageNumber { get; set; }
        public int RowIndex { get; set; }
        public string? Label { get; set; }
        public string? SubLabel { get; set; }
        public string? SpecVi { get; set; }
        public string? SpecEn { get; set; }
        public string? Spec { get; set; }
        public string? Unit { get; set; }
        public string? TolText { get; set; }
        public string? TolMin { get; set; }
        public string? TolMax { get; set; }
        public string? MeasMin { get; set; }
        public string? MeasMax { get; set; }

        /// <summary>
        /// Cột mô tả của dòng này lấy từ ô gộp của dòng trên, không phải đọc được ở dòng này.
        /// Dùng để tô màu cảnh báo khi rà soát - giá trị đúng về mặt nội dung nhưng không in riêng trên giấy.
        /// </summary>
        public bool SpecInherited { get; set; }

        /// <summary>Nhãn của dòng này lấy từ ô nhãn gộp của dòng trên.</summary>
        public bool LabelInherited { get; set; }

        /// <summary>Nhãn dùng để tra danh mục: ưu tiên nhãn con.</summary>
        public string EffectiveLabel =>
            !string.IsNullOrWhiteSpace(SubLabel) ? SubLabel! : Label ?? "";

        /// <summary>
        /// Dòng này có mang dữ liệu nào không.
        ///
        /// Phải xét cả cột dung sai và cột đo, không chỉ cột mô tả: có biểu mẫu ghi
        /// trị số của tiêu chí thẳng vào hai cột MIN/MAX mà để trống cột mô tả.
        /// Ví dụ thật: hai dòng PANTONE COLOR chỉ điền mã màu ở cột đo (7741C / 2465C),
        /// cột SPECIFICATIONS để trống vì ô mô tả được gộp chung cho cả hai dòng.
        /// Nếu chỉ xét cột mô tả thì những dòng đó bị loại ngay từ đầu và biến mất
        /// khỏi kết quả mà không có một cảnh báo nào.
        /// </summary>
        public bool HasAnyValue =>
            !string.IsNullOrWhiteSpace(SpecVi) || !string.IsNullOrWhiteSpace(SpecEn)
            || !string.IsNullOrWhiteSpace(Spec) || !string.IsNullOrWhiteSpace(TolText)
            || !string.IsNullOrWhiteSpace(TolMin) || !string.IsNullOrWhiteSpace(TolMax)
            || !string.IsNullOrWhiteSpace(MeasMin) || !string.IsNullOrWhiteSpace(MeasMax);
    }
}
