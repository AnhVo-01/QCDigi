using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcclPdfTool.Model
{
    public sealed class ExtractedTable
    {
        public int PageNumber { get; set; }
        public List<string?[]> Rows { get; } = new();
        public int ColCount
        {
            get
            {
                int n = 0;
                foreach (var r in Rows) if (r.Length > n) n = r.Length;
                return n;
            }
        }
    }
}
