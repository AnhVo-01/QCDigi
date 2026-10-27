namespace TcclPdfTool.Model
{
    /// <summary>
    /// Một file TCCL lấy từ MucDinhKem. Khoá nghiệp vụ của tiêu chí là (XXTHId, SanPhamId):
    /// không còn đi qua hợp đồng nữa.
    /// </summary>
    public sealed class FileJob
    {
        public int DinhKemId { get; set; }
        public string FileName { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string? LocalPath { get; set; }
        public string? MarketType { get; set; }

        /// <summary>DM_SanPham.Id (MucDinhKem.SanPhamId).</summary>
        public int? SanPhamId { get; set; }

        /// <summary>Lấy từ DM_SanPham theo SanPhamId.</summary>
        public string? MaSPNB { get; set; }

        /// <summary>XXThucHien.Id (qua Market_TBSX).</summary>
        public int? XxthId { get; set; }
        public string? MaXXTH { get; set; }

        public int CongTyId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool AlreadyImported { get; set; }

        /// <summary>Số XXTH mà file này đang được gắn (qua Market_TBSX). Lớn hơn 1 là cần kiểm tra.</summary>
        public int MatchCount { get; set; }

        /// <summary>Đủ khoá (XXTHId, SanPhamId) để ghi vào TieuChiChatLuong.</summary>
        public bool IsLinked => XxthId is not null && SanPhamId is not null;
    }
}
