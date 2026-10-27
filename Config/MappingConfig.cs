using System.Text.Json;
using System.Text.Json.Serialization;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Config;

/// <summary>
/// Bảng alias tĩnh: nhãn đã gặp trên biểu mẫu -> maTieuChi.
/// Mỗi lần người duyệt sửa một nhãn lạ, thêm vào đây là lần sau khỏi gọi LLM.
/// </summary>
public sealed class MappingConfig
{
    [JsonPropertyName("aliases")]
    public Dictionary<string, string> Aliases { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Nhãn cần bỏ qua hẳn (tiêu đề phụ, dòng trang trí...).</summary>
    [JsonPropertyName("ignore")]
    public List<string> Ignore { get; set; } = new();

    /// <summary>Quy tắc đoán FormType: chuỗi xuất hiện trên trang 1 -> FormType.</summary>
    [JsonPropertyName("formTypeRules")]
    public List<FormTypeRule> FormTypeRules { get; set; } = new();

    [JsonPropertyName("defaultFormType")]
    public string DefaultFormType { get; set; } = "PE";

    /// <summary>Ngưỡng chấp nhận kết quả so khớp mờ. Dưới ngưỡng này sẽ hỏi LLM.</summary>
    [JsonPropertyName("fuzzyAcceptThreshold")]
    public double FuzzyAcceptThreshold { get; set; } = 0.72;

    /// <summary>Trên ngưỡng chấp nhận nhưng dưới ngưỡng này thì vẫn bật cờ cần soát.</summary>
    [JsonPropertyName("fuzzyConfidentThreshold")]
    public double FuzzyConfidentThreshold { get; set; } = 0.85;

    private Dictionary<string, string>? _normalizedAliases;
    private HashSet<string>? _normalizedIgnore;

    public Dictionary<string, string> NormalizedAliases
    {
        get
        {
            if (_normalizedAliases is null)
            {
                _normalizedAliases = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var kv in Aliases)
                {
                    var key = TextUtil.Normalize(kv.Key);
                    if (key.Length > 0) _normalizedAliases[key] = kv.Value;
                }
            }
            return _normalizedAliases;
        }
    }

    public HashSet<string> NormalizedIgnore
    {
        get
        {
            if (_normalizedIgnore is null)
            {
                _normalizedIgnore = new HashSet<string>(StringComparer.Ordinal);
                foreach (var s in Ignore)
                {
                    var n = TextUtil.Normalize(s);
                    if (n.Length > 0) _normalizedIgnore.Add(n);
                }
            }
            return _normalizedIgnore;
        }
    }

    public void AddAlias(string label, string maTieuChi)
    {
        Aliases[label] = maTieuChi;
        _normalizedAliases = null;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static MappingConfig Load(string path)
    {
        if (!File.Exists(path)) return CreateDefault();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MappingConfig>(json, JsonOpts) ?? CreateDefault();
    }

    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOpts));
    }

    public static MappingConfig CreateDefault() => new()
    {
        FormTypeRules = new List<FormTypeRule>
        {
            new() { Contains = "ORDER SPECIFICATIONS",   FormType = "PE" },
            new() { Contains = "QUALITY SPECIFICATIONS", FormType = "PE" },
            new() { Contains = "INSPECTION REPORT",      FormType = "TH" }
        }
    };
}

public sealed class FormTypeRule
{
    [JsonPropertyName("contains")]
    public string Contains { get; set; } = "";

    [JsonPropertyName("formType")]
    public string FormType { get; set; } = "";
}
