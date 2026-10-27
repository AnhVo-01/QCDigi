using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

public sealed class LlmMapItem
{
    /// <summary>Khoá tham chiếu để ghép kết quả trả về đúng dòng.</summary>
    public int Key { get; set; }
    public string Label { get; set; } = "";
    public string? SampleValue { get; set; }
    public string? Unit { get; set; }
}

public sealed class LlmMapAnswer
{
    public int Key { get; set; }
    /// <summary>maTieuChi được chọn, hoặc null nếu LLM cho rằng không có tiêu chí phù hợp.</summary>
    public string? MaTieuChi { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Tầng map bằng mô hình ngôn ngữ. Chỉ được gọi cho những nhãn mà alias và
/// so khớp mờ không quyết được, nên số lượt gọi rất nhỏ so với số dòng.
/// </summary>
public interface ILabelLlmMapper
{
    Task<IReadOnlyList<LlmMapAnswer>> MapAsync(
        IReadOnlyList<LlmMapItem> items,
        IReadOnlyList<DmTieuChi> catalog,
        CancellationToken ct = default);
}

/// <summary>Dùng khi chưa cấu hình LLM: không map gì thêm, để người duyệt xử lý.</summary>
public sealed class NullLabelLlmMapper : ILabelLlmMapper
{
    public Task<IReadOnlyList<LlmMapAnswer>> MapAsync(
        IReadOnlyList<LlmMapItem> items, IReadOnlyList<DmTieuChi> catalog, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<LlmMapAnswer>>(new List<LlmMapAnswer>());
}
