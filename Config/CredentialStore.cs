using System.Security.Cryptography;
using System.Text;

namespace TcclPdfTool.Config;

/// <summary>
/// Giữ mật khẩu SQL ngoài appsettings.json. Mật khẩu được mã hoá bằng Windows DPAPI theo user hiện tại
/// (DataProtectionScope.CurrentUser): chỉ đúng user Windows đó trên đúng máy đó giải mã được,
/// chép file sang máy/user khác là vô dụng.
/// </summary>
public static class CredentialStore
{
    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, "config", "credential.dat");

    // Entropy riêng của ứng dụng - ứng dụng khác chạy cùng user không giải mã nhầm được bằng DPAPI trơn.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("TcclPdfTool.SqlCredential.v1");

    /// <summary>Mật khẩu chỉ dùng trong phiên này (không tick "Nhớ mật khẩu"). Không bao giờ ghi ra đĩa.</summary>
    private static string? _sessionPassword;

    /// <summary>Mật khẩu hiện dùng: ưu tiên mật khẩu phiên, sau đó tới file đã mã hoá.</summary>
    public static string? GetPassword() => _sessionPassword ?? LoadFromFile();

    /// <summary>
    /// Đặt mật khẩu. <paramref name="remember"/> = true thì ghi file mã hoá;
    /// false thì chỉ giữ trong bộ nhớ và xoá file cũ (nếu có).
    /// </summary>
    public static void SetPassword(string? password, bool remember)
    {
        if (remember)
        {
            _sessionPassword = null;
            SaveToFile(password ?? "");
        }
        else
        {
            _sessionPassword = password;
            Delete();
        }
    }

    public static bool HasSavedPassword => File.Exists(DefaultPath);

    public static void Delete()
    {
        try { if (File.Exists(DefaultPath)) File.Delete(DefaultPath); }
        catch { /* không xoá được thì lần sau ghi đè */ }
    }

    private static void SaveToFile(string password)
    {
        var dir = Path.GetDirectoryName(DefaultPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(DefaultPath, cipher);
    }

    private static string? LoadFromFile()
    {
        if (!File.Exists(DefaultPath)) return null;
        try
        {
            var plain = ProtectedData.Unprotect(File.ReadAllBytes(DefaultPath), Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch (CryptographicException)
        {
            // File do user/máy khác tạo hoặc đã hỏng - coi như chưa lưu mật khẩu.
            return null;
        }
    }
}
