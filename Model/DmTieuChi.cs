using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcclPdfTool.Model
{
    public sealed class DmTieuChi
    {
        public int Id { get; set; }
        public string MaTieuChi { get; set; } = "";
        public string TieuChi { get; set; } = "";
        public string? FormType { get; set; }
        public string? Tolerance { get; set; }
        public int CongTyId { get; set; }
        public bool IsGroup { get; set; }
        public int? GroupId { get; set; }
        public string? Func { get; set; }
        public decimal? MinTolerance { get; set; }
        public decimal? MaxTolerance { get; set; }
    }
}
