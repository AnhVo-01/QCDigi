using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

/// <summary>
/// Trừu tượng hoá việc đọc bảng khỏi PDF, để phần logic nghiệp vụ kiểm thử được
/// mà không cần file PDF thật hay license của thư viện nào.
/// </summary>
public interface ITableExtractor
{
    (List<ExtractedTable> tables, string firstPageText) Extract(string pdfPath);
}
