using System.ComponentModel;

namespace TcclPdfTool.Model
{
    public sealed class JobRow : INotifyPropertyChanged
    {
        public FileJob Job { get; }
        public ImportResult? Result { get; private set; }

        public JobRow(FileJob job)
        {
            Job = job;
            if (job.AlreadyImported) _trangThai = "Đã import";
            else if (job.SanPhamId is null) _trangThai = "Thiếu sản phẩm";
            else if (job.XxthId is null) _trangThai = "Chưa gắn XXTH";
        }

        /// <summary>
        /// Giá trị các ô theo đúng thứ tự cột của lưới jobRows:
        /// Chon, TenFile, MaXXTH, MaSPNB, Mau, DocDuoc, MapDuoc, CanSoat, ChuaMap, TrangThai.
        /// </summary>
        public object[] ToCells() => new object[]
        {
            Chon, TenFile, XXTH, SanPham, Mau, DocDuoc, MapDuoc, CanSoat, ChuaMap, TrangThai
        };

        public bool Chon { get; set; } = true;
        public string TenFile => Job.FileName;
        public string XXTH => Job.MaXXTH ?? "";
        public string MaSPNB => Job.MaSPNB ?? "";
        public string SanPham => Result?.MaSPNB ?? Job.MaSPNB ?? "";
        public string Mau => Result?.DetectedTemplate ?? "";
        public string DocDuoc => Result is null ? "" : Result.RowsRead.ToString();
        public string MapDuoc => Result is null ? "" : Result.Fields.Count.ToString();
        public string CanSoat => Result is null ? "" : Result.NeedsReviewCount.ToString();
        public string ChuaMap => Result is null ? "" : Result.UnmappedRows.Count.ToString();

        private string _trangThai = "Chưa xử lý";
        public string TrangThai
        {
            get => _trangThai;
            set { _trangThai = value; OnChanged(nameof(TrangThai)); }
        }

        public bool DaLuu { get; set; }

        public void SetResult(ImportResult r)
        {
            Result = r;
            if (r.Failed) TrangThai = "Lỗi: " + Truncate(r.Error ?? "", 80);
            else if (r.SanPhamId is null) TrangThai = "Thiếu sản phẩm";
            else if (r.XxthId is null) TrangThai = "Chưa gắn XXTH";
            else if (r.NeedsReviewCount > 0 || r.UnmappedRows.Count > 0) TrangThai = "Đã đọc - cần soát";
            else TrangThai = "Đã đọc";
            OnChanged(null);
        }

        /// <summary>Đã đọc được tiêu chí để ghi xuống CSDL chưa.</summary>
        public bool CanSave => Result is { Failed: false } && Result.Fields.Count > 0;

        private static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "...";

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? ""));
    }
}
