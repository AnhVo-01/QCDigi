namespace TcclPdfTool.Model
{
    public sealed class FieldRow
    {
        public int Trang { get; set; }
        public int RowIndex { get; set; }
        public string? NhanGoc { get; set; }
        public string? MaTieuChi { get; set; }
        public string? TenTieuChi { get; set; }
        public string? SpecVi { get; set; }
        public string? SpecEn { get; set; }
        /// <summary>Kiểu dung sai: string / percent / number (hoặc giá trị riêng của DM_TieuChi).</summary>
        public string? Tolerance { get; set; }

        public string? TolMin { get; set; }
        public string? TolMax { get; set; }
        public string? MeasMin { get; set; }
        public string? MeasMax { get; set; }
        public string? Nguon { get; set; }
        public string? DoTinCay { get; set; }
        public bool CanSoat { get; set; }

        /// <summary>Mô tả lấy từ ô gộp của dòng trên - lưới tô màu để người soát nhận ra ngay.</summary>
        [System.ComponentModel.Browsable(false)]
        public bool SpecKeThua { get; set; }

        /// <summary>Nhãn lấy từ ô nhãn gộp của dòng trên.</summary>
        [System.ComponentModel.Browsable(false)]
        public bool NhanKeThua { get; set; }

        /// <summary>Dòng thô gốc khi dòng này chưa map được - để cập nhật lại UnmappedRows lúc áp dụng.</summary>
        [System.ComponentModel.Browsable(false)]
        public RawRow? Source { get; set; }
    }
}
