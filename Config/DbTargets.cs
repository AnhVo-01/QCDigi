using System.Text.RegularExpressions;

namespace TcclPdfTool.Config;

public sealed class DbTargets
{
    /// <summary>Kết nối chính - CSDL Sales, nơi chứa mọi bảng công cụ đọc và ghi.</summary>
    public string SalesConnectionString { get; set; } = "";

    /// <summary>
    /// Tên các CSDL nằm CÙNG instance với Sales, dùng cho join tên 3 phần.
    /// Ví dụ: { "KhachHang": "HoSoKhachHang_Prd" }. Bỏ trống thì phần join đó được bỏ qua.
    /// </summary>
    public Dictionary<string, string> SameServerDbNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Connection string của các CSDL nằm trên server KHÁC.
    /// Ví dụ: { "VatTu": "Server=192.168.9.26,55001;Initial Catalog=B8R3_NhuaAnPhat_TTMS;..." }.
    /// </summary>
    public Dictionary<string, string> OtherServerConnections { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Tên CSDL bị nối thẳng vào câu SQL nên phải kiểm tra. CRM đang nội suy thẳng
    // AppSettings vào chuỗi truy vấn - ở đây làm chặt hơn một bậc.
    private static readonly Regex SafeIdentifier = new(@"^[A-Za-z_][A-Za-z0-9_$#@]{0,127}$", RegexOptions.Compiled);

    /// <summary>
    /// Trả về tiền tố "[TenDb].dbo." để ghép vào câu SQL, hoặc null nếu chưa cấu hình.
    /// Ném lỗi nếu tên CSDL không hợp lệ - thà dừng còn hơn ghép một chuỗi lạ vào SQL.
    /// </summary>
    public string? Prefix(string key)
    {
        if (!SameServerDbNames.TryGetValue(key, out var name)) return null;
        name = (name ?? "").Trim();
        if (name.Length == 0) return null;
        if (!SafeIdentifier.IsMatch(name))
            throw new InvalidOperationException(
                $"Tên CSDL '{name}' (khoá '{key}') không hợp lệ - chỉ chấp nhận chữ, số và _ $ # @.");
        return $"[{name}].dbo.";
    }

    public bool Has(string key) => Prefix(key) is not null;

    /// <summary>Connection string của một CSDL trên server khác.</summary>
    public string? Connection(string key)
        => OtherServerConnections.TryGetValue(key, out var cs) && !string.IsNullOrWhiteSpace(cs) ? cs : null;
}
