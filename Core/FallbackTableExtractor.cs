using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

/// <summary>
/// Thử lần lượt nhiều bộ đọc bảng, lấy kết quả của bộ ĐẦU TIÊN dựng được bảng.
///
/// Lý do cần cơ chế này: không bộ đọc nào đúng với mọi biểu mẫu.
/// Aspose.PDF 20.6 TableAbsorber bỏ sót bảng ở những file có nét kẻ mảnh (0.36pt),
/// trong khi bộ DevExpress tự dựng lưới từ nét kẻ lại đọc được. Ngược lại cũng có thể
/// xảy ra với biểu mẫu khác. Thử lần lượt rẻ hơn nhiều so với đoán trước.
///
/// Chỉ khi TẤT CẢ bộ đọc đều không ra bảng thì mới kết luận file có vấn đề -
/// nhờ vậy thông báo "có thể là bản scan" mới đáng tin.
/// </summary>
public sealed class FallbackTableExtractor : ITableExtractor
{
    private readonly List<Stage> _stages = new();

    /// <summary>Nơi ghi nhật ký. Gán từ MainForm để người dùng thấy bộ nào đã đọc được.</summary>
    public Action<string>? Log { get; set; }

    /// <summary>Tên bộ đọc đã dùng ở lần Extract gần nhất, null nếu không bộ nào thành công.</summary>
    public string? LastUsed { get; private set; }

    private sealed record Stage(string Name, Func<ITableExtractor> Create);

    public FallbackTableExtractor Add(string name, Func<ITableExtractor> create)
    {
        _stages.Add(new Stage(name, create));
        return this;
    }

    /// <summary>
    /// Thứ tự mặc định: DevExpress trước, Aspose sau.
    ///
    /// DevExpress đi trước vì nó tự dựng lưới từ nét kẻ nên kiểm soát được ngưỡng,
    /// còn TableAbsorber là hộp đen. License Aspose chỉ nạp khi thực sự phải dùng tới.
    /// </summary>
    public static FallbackTableExtractor Create(string? asposeLicensePath, Action<string>? log = null)
    {
        var chain = new FallbackTableExtractor { Log = log };
        chain.Add("DevExpress", () => new DevExpressTableExtractor());
        chain.Add("Aspose", () =>
        {
            AsposeTableExtractor.ApplyLicense(asposeLicensePath);
            return new AsposeTableExtractor();
        });
        return chain;
    }

    public (List<ExtractedTable> tables, string firstPageText) Extract(string pdfPath)
    {
        var notes = new List<string>();
        var fileName = Path.GetFileName(pdfPath);
        var firstText = "";

        foreach (var stage in _stages)
        {
            try
            {
                var (tables, text) = stage.Create().Extract(pdfPath);

                // Giữ lại chữ trang 1 của bộ đầu tiên đọc được: kể cả khi bộ đó không dựng
                // nổi bảng, chữ vẫn dùng để nhận dạng loại biểu mẫu.
                if (firstText.Length == 0 && !string.IsNullOrEmpty(text)) firstText = text;

                if (tables.Count > 0)
                {
                    LastUsed = stage.Name;
                    if (notes.Count > 0)
                        Log?.Invoke($"  [{fileName}] {string.Join("; ", notes)} " +
                                    $"-> '{stage.Name}' đọc được {tables.Count} bảng.");
                    return (tables, string.IsNullOrEmpty(text) ? firstText : text);
                }

                notes.Add($"'{stage.Name}' không thấy bảng");
            }
            catch (Exception ex)
            {
                // Một bộ hỏng (thiếu DLL, license, file lạ) không được chặn bộ còn lại.
                notes.Add($"'{stage.Name}' lỗi: {Short(ex.Message)}");
            }
        }

        LastUsed = null;
        Log?.Invoke($"  [{fileName}] không bộ đọc nào dựng được bảng ({string.Join("; ", notes)}).");
        return (new List<ExtractedTable>(), firstText);
    }

    private static string Short(string s)
    {
        s = s.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return s.Length <= 120 ? s : s.Substring(0, 119) + "…";
    }
}
