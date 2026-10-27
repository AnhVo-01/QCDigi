namespace TcclPdfTool.Model
{
    /// <summary>
    /// Một bản ghi TieuChiChatLuong đúng như khi xuất CSV - dùng chung cho xuất file và ghi CSDL
    /// để hai đường ra luôn cùng dữ liệu.
    /// </summary>
    public sealed class TieuChiChatLuongRecord
    {
        public int SanPhamId { get; init; }
        public int XxthId { get; init; }
        public string FormType { get; init; } = "";
        public string FilePath { get; init; } = "";
        public decimal? MaxTolerance { get; init; }
        public decimal? MinTolerance { get; init; }
        public string TieuChiData { get; init; } = "";
        public DateTime? UpdateDate { get; init; }
    }

    /// <summary>Bản ghi TieuChiChatLuong đã có trong CSDL.</summary>
    public sealed class ExistingTieuChiChatLuong
    {
        public int Id { get; init; }
        public int SanPhamId { get; init; }
        public int XxthId { get; init; }
    }
}
