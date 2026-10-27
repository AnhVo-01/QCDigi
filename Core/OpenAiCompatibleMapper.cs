using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

public sealed class LlmOptions
{
    /// <summary>Điểm cuối kiểu OpenAI. Dùng được với OpenAI, Azure OpenAI, vLLM, Ollama...</summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gpt-4o-mini";
    public int TimeoutSeconds { get; set; } = 120;
    /// <summary>Số nhãn hỏi trong một lượt. Nhỏ quá thì tốn lượt, lớn quá thì mô hình dễ lẫn.</summary>
    public int BatchSize { get; set; } = 40;
    public bool Enabled { get; set; }
}

/// <summary>
/// Map nhãn bằng mô hình ngôn ngữ.
///
/// Nguyên tắc an toàn: mô hình CHỈ được chọn mã trong danh mục, không được sinh trị số.
/// Mọi con số vẫn do bộ đọc PDF lấy ra và do code tính. Nhờ vậy sai sót tệ nhất
/// của mô hình chỉ là gán nhầm nhãn - thứ mà người duyệt nhìn ra ngay.
/// </summary>
public sealed class OpenAiCompatibleMapper : ILabelLlmMapper, IDisposable
{
    private readonly LlmOptions _opt;
    private readonly HttpClient _http;

    public OpenAiCompatibleMapper(LlmOptions options, HttpClient? http = null)
    {
        _opt = options;
        _http = http ?? new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(Math.Max(10, options.TimeoutSeconds));
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
    }

    public async Task<IReadOnlyList<LlmMapAnswer>> MapAsync(
        IReadOnlyList<LlmMapItem> items, IReadOnlyList<DmTieuChi> catalog, CancellationToken ct = default)
    {
        var answers = new List<LlmMapAnswer>();
        if (!_opt.Enabled || items.Count == 0) return answers;

        for (int offset = 0; offset < items.Count; offset += _opt.BatchSize)
        {
            ct.ThrowIfCancellationRequested();
            var batch = items.Skip(offset).Take(_opt.BatchSize).ToList();
            answers.AddRange(await MapBatchAsync(batch, catalog, ct).ConfigureAwait(false));
        }
        return answers;
    }

    private async Task<List<LlmMapAnswer>> MapBatchAsync(
        List<LlmMapItem> batch, IReadOnlyList<DmTieuChi> catalog, CancellationToken ct)
    {
        var payload = new JsonObject
        {
            ["model"] = _opt.Model,
            ["temperature"] = 0,
            ["response_format"] = new JsonObject { ["type"] = "json_object" },
            ["messages"] = new JsonArray
            {
                new JsonObject { ["role"] = "system", ["content"] = SystemPrompt },
                new JsonObject { ["role"] = "user",   ["content"] = BuildUserPrompt(batch, catalog) }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, _opt.Endpoint)
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Dịch vụ map nhãn trả về {(int)res.StatusCode}: {Truncate(body, 400)}");

        var content = JsonNode.Parse(body)?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Dịch vụ map nhãn trả về nội dung rỗng.");

        return ParseAnswers(content!, catalog);
    }

    private const string SystemPrompt =
        "Bạn là trợ lý ghép nhãn cho hệ thống tiêu chuẩn chất lượng bao bì nhựa.\n" +
        "Người dùng đưa một danh mục tiêu chí (mã + tên) và một danh sách nhãn lấy từ biểu mẫu PDF.\n" +
        "Nhiệm vụ: với mỗi nhãn, chọn ĐÚNG MỘT mã trong danh mục, hoặc null nếu không có mã nào thực sự phù hợp.\n" +
        "Quy tắc bắt buộc:\n" +
        "- Chỉ được dùng mã có trong danh mục. Không được bịa mã mới.\n" +
        "- Không suy đoán: không chắc thì trả null. Trả null an toàn hơn nhiều so với gán nhầm.\n" +
        "- Phân biệt kỹ các cặp dễ lẫn: trọng lượng túi (bagWeight) với trọng lượng dây (tapeWeight) " +
        "và trọng lượng túi cả dây (bagTapeWeight); số cái mỗi cuộn (pcs) với số cuộn mỗi thùng (cartonRolls); " +
        "thông số túi chính với thông số túi bọc ngoài (nhóm outer*).\n" +
        "- confidence là số thực 0..1 thể hiện mức chắc chắn của chính bạn.\n" +
        "Chỉ trả JSON dạng: {\"results\":[{\"key\":0,\"maTieuChi\":\"prodWidth\",\"confidence\":0.95}]}";

    private static string BuildUserPrompt(List<LlmMapItem> batch, IReadOnlyList<DmTieuChi> catalog)
    {
        var sb = new StringBuilder();
        sb.AppendLine("DANH MỤC TIÊU CHÍ (mã | tên):");
        foreach (var d in catalog)
            sb.AppendLine($"{d.MaTieuChi} | {TextUtil.Clean(d.TieuChi)}");

        sb.AppendLine();
        sb.AppendLine("NHÃN CẦN GHÉP (key | nhãn | giá trị mẫu | đơn vị):");
        foreach (var it in batch)
            sb.AppendLine($"{it.Key} | {TextUtil.Clean(it.Label)} | {TextUtil.Clean(it.SampleValue) ?? "-"} | {TextUtil.Clean(it.Unit) ?? "-"}");

        return sb.ToString();
    }

    private static List<LlmMapAnswer> ParseAnswers(string content, IReadOnlyList<DmTieuChi> catalog)
    {
        var valid = new HashSet<string>(catalog.Select(c => c.MaTieuChi), StringComparer.OrdinalIgnoreCase);
        var result = new List<LlmMapAnswer>();

        var json = StripCodeFence(content);
        JsonNode? root;
        try { root = JsonNode.Parse(json); }
        catch (JsonException ex) { throw new InvalidOperationException($"Không đọc được JSON từ mô hình: {ex.Message}"); }

        var arr = root?["results"] as JsonArray ?? root as JsonArray;
        if (arr is null) return result;

        foreach (var node in arr)
        {
            if (node is null) continue;
            var key = node["key"]?.GetValue<int>();
            if (key is null) continue;

            string? code = null;
            var codeNode = node["maTieuChi"];
            if (codeNode is not null && codeNode.GetValueKind() == JsonValueKind.String)
                code = codeNode.GetValue<string>();

            if (!string.IsNullOrWhiteSpace(code) && !valid.Contains(code!)) code = null; // chặn mã bịa
            double conf = 0.5;
            try { conf = node["confidence"]?.GetValue<double>() ?? 0.5; } catch { /* để mặc định */ }

            result.Add(new LlmMapAnswer { Key = key.Value, MaTieuChi = code, Confidence = conf });
        }
        return result;
    }

    private static string StripCodeFence(string s)
    {
        var t = s.Trim();
        if (!t.StartsWith("```", StringComparison.Ordinal)) return t;
        var nl = t.IndexOf('\n');
        if (nl < 0) return t;
        t = t.Substring(nl + 1);
        var end = t.LastIndexOf("```", StringComparison.Ordinal);
        return (end >= 0 ? t.Substring(0, end) : t).Trim();
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "...";

    public void Dispose() => _http.Dispose();
}
