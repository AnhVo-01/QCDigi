using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.Text;
using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

/// <summary>
/// Trích bảng bằng Aspose.PDF TableAbsorber.
///
/// Cách dựng lưới: gom mọi mép trái của ô trong một bảng thành danh sách ranh giới cột,
/// rồi gán mỗi ô về đúng cột theo toạ độ. Ô gộp chiếm cột đầu, các cột còn lại để trống -
/// đúng dạng mà RawTableParser mong đợi.
/// </summary>
public sealed class AsposeTableExtractor : ITableExtractor
{
    /// <summary>Hai mép cách nhau dưới ngưỡng này (điểm) coi như cùng một cột.</summary>
    private const double ColumnTolerance = 4.0;

    private static bool _licenseApplied;

    /// <summary>
    /// Nạp license Aspose từ file .lic của công ty.
    ///
    /// Bỏ trống đường dẫn thì Aspose chạy ở chế độ dùng thử (chèn watermark và giới hạn
    /// số trang xử lý) - chấp nhận được khi chỉ dò thử, nhưng KHÔNG dùng cho đợt chuyển
    /// dữ liệu thật. Anphat đã có license Aspose.Total hợp lệ, dùng sẵn trong CRM
    /// (xem Aph.Application/packages.config và Utility.Utils.AddLicenseAsposeCells).
    /// </summary>
    public static void ApplyLicense(string? licensePath)
    {
        if (_licenseApplied) return;

        if (string.IsNullOrWhiteSpace(licensePath))
            return; // chạy thử - để người gọi tự quyết có cảnh báo hay không

        if (!File.Exists(licensePath))
            throw new FileNotFoundException($"Không tìm thấy file license Aspose: {licensePath}");

        new Aspose.Pdf.License().SetLicense(licensePath);
        _licenseApplied = true;
    }

    /// <summary>License đã nạp chưa - dùng để cảnh báo khi đang chạy ở chế độ dùng thử.</summary>
    public static bool LicenseApplied => _licenseApplied;

    /// <summary>Kiểm tra 5 byte đầu là "%PDF-" - chặn sớm trường hợp tải nhầm trang lỗi/HTML từ CDN
    /// (CDN có thể trả 200 OK kèm nội dung không phải PDF khi link hỏng/hết quyền).</summary>
    private static void EnsurePdfSignature(string pdfPath)
    {
        Span<byte> header = stackalloc byte[5];
        using (var fs = File.OpenRead(pdfPath))
        {
            int read = fs.Read(header);
            if (read < 5 || header[0] != (byte)'%' || header[1] != (byte)'P' ||
                header[2] != (byte)'D' || header[3] != (byte)'F' || header[4] != (byte)'-')
            {
                throw new InvalidDataException(
                    $"File không phải PDF hợp lệ (có thể CDN trả về trang lỗi thay vì file thật): {pdfPath}");
            }
        }
    }

    public (List<ExtractedTable> tables, string firstPageText) Extract(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"Không tìm thấy file PDF: {pdfPath}");

        EnsurePdfSignature(pdfPath);

        var tables = new List<ExtractedTable>();
        var firstPageText = "";

        using var doc = new Document(pdfPath);

        for (int p = 1; p <= doc.Pages.Count; p++)
        {
            var page = doc.Pages[p];

            try
            {
                if (p == 1)
                {
                    var ta = new TextAbsorber();
                    page.Accept(ta);
                    firstPageText = ta.Text ?? "";
                }

                var absorber = new TableAbsorber();
                absorber.Visit(page);

                foreach (AbsorbedTable table in absorber.TableList)
                {
                    var grid = BuildGrid(table, p);
                    if (grid is not null && grid.Rows.Count > 0) tables.Add(grid);
                }
            }
            catch (Exception ex)
            {
                // Một trang lỗi cấu trúc (font/resource hỏng...) không nên làm hỏng cả file -
                // bỏ qua trang này, các trang còn lại vẫn đọc bình thường.
                System.Diagnostics.Debug.WriteLine($"[AsposeTableExtractor] Bỏ qua trang {p} do lỗi: {ex}");
            }
        }

        return (tables, firstPageText);
    }

    private static ExtractedTable? BuildGrid(AbsorbedTable table, int pageNumber)
    {
        // 1) Thu thập ranh giới cột từ mép trái của tất cả các ô.
        var edges = new List<double>();
        foreach (AbsorbedRow row in table.RowList)
        {
            foreach (AbsorbedCell cell in row.CellList)
            {
                var x = cell.Rectangle.LLX;
                if (!edges.Any(e => Math.Abs(e - x) <= ColumnTolerance)) edges.Add(x);
            }
        }
        if (edges.Count == 0) return null;
        edges.Sort();

        int ColumnOf(double x)
        {
            int best = 0;
            double bestDiff = double.MaxValue;
            for (int i = 0; i < edges.Count; i++)
            {
                var d = Math.Abs(edges[i] - x);
                if (d < bestDiff) { bestDiff = d; best = i; }
            }
            return best;
        }

        // 2) Đổ chữ của từng ô vào đúng cột.
        var result = new ExtractedTable { PageNumber = pageNumber };
        foreach (AbsorbedRow row in table.RowList)
        {
            var cells = new string?[edges.Count];
            foreach (AbsorbedCell cell in row.CellList)
            {
                var text = CellText(cell);
                if (text.Length == 0) continue;
                var c = ColumnOf(cell.Rectangle.LLX);
                // Hai ô cùng rơi vào một cột thì nối lại, tránh mất chữ.
                cells[c] = string.IsNullOrEmpty(cells[c]) ? text : cells[c] + " " + text;
            }
            result.Rows.Add(cells);
        }

        return result;
    }

    private static string CellText(AbsorbedCell cell)
    {
        var sb = new StringBuilder();
        foreach (TextFragment fragment in cell.TextFragments)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(fragment.Text);
        }
        return sb.ToString().Trim();
    }
}
