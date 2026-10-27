using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using TcclPdfTool.Core;

namespace TcclPdfTool.Config;

public sealed class AppConfig
{
    /// <summary>
    /// Kết nối chính tới SQL Server chứa CSDL Sales (DM_SanPham, XXThucHien, DM_TieuChi, TieuChiChatLuong).
    /// Không chứa mật khẩu - mật khẩu nằm trong file mã hoá của <see cref="CredentialStore"/>.
    /// </summary>
    [JsonPropertyName("database")]
    public DbConnectionInfo Database { get; set; } = new();

    /// <summary>
    /// Chuỗi kết nối kiểu cũ (có thể chứa mật khẩu dạng chữ). Chỉ đọc để chuyển sang <see cref="Database"/>
    /// một lần, sau đó bị xoá khỏi appsettings.json.
    /// </summary>
    [JsonPropertyName("connectionString")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegacyConnectionString { get; set; }

    /// <summary>
    /// Tên các CSDL nằm CÙNG SQL Server với Sales. Chỉ cần TÊN, không cần chuỗi kết nối -
    /// join chéo bằng tên 3 phần trên chính kết nối trên, đúng cách CRM đang làm.
    /// Ví dụ: { "KhachHang": "HoSoKhachHang_Prd" }
    /// </summary>
    [JsonPropertyName("sameServerDbNames")]
    public Dictionary<string, string> SameServerDbNames { get; set; } =
        new(StringComparer.OrdinalIgnoreCase) { ["ThongBaoSX"] = "", ["KhachHang"] = "" };

    /// <summary>
    /// Chuỗi kết nối của các CSDL nằm trên SERVER KHÁC (ACPThongBaoSX, DBVatTu...).
    /// Không join chéo được với Sales - phải truy vấn riêng rồi ghép trong C#.
    /// </summary>
    [JsonPropertyName("otherServerConnections")]
    public Dictionary<string, string> OtherServerConnections { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Thư mục gốc của web CRM - nơi chứa /FileUpload/... Có thể là đường dẫn UNC.</summary>
    [JsonPropertyName("fileRoot")]
    public string FileRoot { get; set; } = @"\\crm-server\wwwroot";

    [JsonPropertyName("asposeLicensePath")]
    public string AsposeLicensePath { get; set; } = "";

    [JsonPropertyName("congTyId")]
    public int CongTyId { get; set; } = 1;

    /// <summary>MarketType cần lọc trong MucDinhKem.</summary>
    [JsonPropertyName("marketType")]
    public string MarketType { get; set; } = "TCCL";

    [JsonPropertyName("fileNameFilter")]
    public string FileNameFilter { get; set; } = "";

    /// <summary>Số file tối đa nạp từ MucDinhKem mỗi lần. 0 = không giới hạn.</summary>
    [JsonPropertyName("maxRows")]
    public int MaxRows { get; set; } = 500;

    [JsonPropertyName("onlyNotImported")]
    public bool OnlyNotImported { get; set; } = true;

    //[JsonPropertyName("writeMode")]
    //public WriteMode WriteMode { get; set; } = WriteMode.Staging;

    [JsonPropertyName("outputFolder")]
    public string OutputFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TcclImport");

    [JsonPropertyName("llm")]
    public LlmOptions Llm { get; set; } = new();

    [JsonIgnore]
    public string MappingPath => Path.Combine(AppContext.BaseDirectory, "config", "mapping.json");

    // ------------------------------------------------------------------ nạp/lưu

    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, "config", "appsettings.json");

    public static AppConfig Load()
    {
        AppConfig cfg;
        try
        {
            cfg = File.Exists(DefaultPath)
                ? JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(DefaultPath), Opts) ?? new AppConfig()
                : new AppConfig();
        }
        catch { return new AppConfig(); /* file hỏng thì dùng mặc định, không chặn người dùng mở app */ }

        try { cfg.MigrateLegacyConnectionString(); }
        catch { /* chuyển không được thì giữ nguyên file cũ, người dùng thiết lập lại trong Cấu hình */ }
        return cfg;
    }

    /// <summary>
    /// appsettings.json kiểu cũ có "connectionString" kèm mật khẩu dạng chữ: tách ra thành
    /// <see cref="Database"/>, chuyển mật khẩu vào file mã hoá rồi ghi lại file không còn mật khẩu.
    /// </summary>
    private void MigrateLegacyConnectionString()
    {
        if (string.IsNullOrWhiteSpace(LegacyConnectionString)) { LegacyConnectionString = null; return; }

        if (string.IsNullOrWhiteSpace(Database.Server))
        {
            var b = new SqlConnectionStringBuilder(LegacyConnectionString);
            Database = DbConnectionInfo.FromBuilder(b);
            if (!b.IntegratedSecurity && !string.IsNullOrEmpty(b.Password))
            {
                CredentialStore.SetPassword(b.Password, remember: true);
                Database.RememberPassword = true;
            }
        }

        LegacyConnectionString = null;
        Save();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(DefaultPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(DefaultPath, JsonSerializer.Serialize(this, Opts));
    }

    /// <summary>Gom cấu hình CSDL thành đối tượng cho tầng truy cập dữ liệu.</summary>
    public DbTargets ToDbTargets() => new()
    {
        SalesConnectionString = Database.Build(CredentialStore.GetPassword()),
        SameServerDbNames = new Dictionary<string, string>(SameServerDbNames, StringComparer.OrdinalIgnoreCase),
        OtherServerConnections = new Dictionary<string, string>(OtherServerConnections, StringComparer.OrdinalIgnoreCase)
    };

    /// <summary>true nếu FileRoot là CDN (http/https) thay vì đường dẫn đĩa/UNC.</summary>
    [JsonIgnore]
    public bool IsRemoteFileRoot => FileRoot.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                  || FileRoot.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Đổi đường dẫn tương đối trong TepDinhKem thành đường dẫn thật trên đĩa,
    /// hoặc URL đầy đủ nếu FileRoot trỏ tới CDN (http/https).
    /// </summary>
    public string ResolveLocalPath(string relativePath)
    {
        if (IsRemoteFileRoot)
        {
            var relUrl = (relativePath ?? "").Replace('\\', '/').TrimStart('/');
            return FileRoot.TrimEnd('/') + "/" + relUrl;
        }

        var rel = (relativePath ?? "").Replace('/', Path.DirectorySeparatorChar)
                                      .TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(FileRoot, rel);
    }
}

/// <summary>Thông tin kết nối SQL Server, tương đương hộp thoại "Connect to Server" của SSMS.</summary>
public sealed class DbConnectionInfo
{
    [JsonPropertyName("server")]
    public string Server { get; set; } = "";

    /// <summary>true = Windows Authentication, false = SQL Server Authentication.</summary>
    [JsonPropertyName("windowsAuth")]
    public bool WindowsAuth { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = "";

    /// <summary>CSDL chính (Sales).</summary>
    [JsonPropertyName("databaseName")]
    public string DatabaseName { get; set; } = "APH_Sales";

    /// <summary>Optional / Mandatory / Strict - như mục Encryption của SSMS.</summary>
    [JsonPropertyName("encrypt")]
    public string Encrypt { get; set; } = "Mandatory";

    [JsonPropertyName("trustServerCertificate")]
    public bool TrustServerCertificate { get; set; } = true;

    [JsonPropertyName("rememberPassword")]
    public bool RememberPassword { get; set; } = true;

    public static readonly string[] EncryptOptions = { "Optional", "Mandatory", "Strict" };

    /// <summary>SQL Server Authentication mà chưa có mật khẩu (không lưu và chưa nhập trong phiên này).</summary>
    [JsonIgnore]
    public bool NeedsPassword => !WindowsAuth && CredentialStore.GetPassword() is null;

    /// <summary>Dựng chuỗi kết nối; <paramref name="database"/> null thì dùng <see cref="DatabaseName"/>.</summary>
    public string Build(string? password, string? database = null)
    {
        var b = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = database ?? DatabaseName,
            IntegratedSecurity = WindowsAuth,
            TrustServerCertificate = TrustServerCertificate,
            Encrypt = Encrypt switch
            {
                "Optional" => SqlConnectionEncryptOption.Optional,
                "Strict" => SqlConnectionEncryptOption.Strict,
                _ => SqlConnectionEncryptOption.Mandatory
            },
            ApplicationName = "TcclPdfTool"
        };
        if (!WindowsAuth)
        {
            b.UserID = Login;
            b.Password = password ?? "";
        }
        return b.ConnectionString;
    }

    public static DbConnectionInfo FromBuilder(SqlConnectionStringBuilder b) => new()
    {
        Server = b.DataSource,
        WindowsAuth = b.IntegratedSecurity,
        Login = b.UserID,
        DatabaseName = string.IsNullOrWhiteSpace(b.InitialCatalog) ? "APH_Sales" : b.InitialCatalog,
        Encrypt = b.Encrypt == SqlConnectionEncryptOption.Optional ? "Optional"
                : b.Encrypt == SqlConnectionEncryptOption.Strict ? "Strict" : "Mandatory",
        TrustServerCertificate = b.TrustServerCertificate
    };

    public DbConnectionInfo Clone() => (DbConnectionInfo)MemberwiseClone();

    public override string ToString()
        => $"{Server} - {(WindowsAuth ? "Windows Authentication" : Login)} - CSDL {DatabaseName}";
}
