using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

/// <summary>
/// Điều phối toàn bộ một file: đọc bảng -> gán vai trò cột -> map tiêu chí -> gắn khoá (XXTH, sản phẩm).
/// Không chạm tới CSDL và không chạm tới giao diện, nên kiểm thử được độc lập.
/// </summary>
public sealed class ImportEngine
{
    private readonly ITableExtractor _extractor;
    private readonly TieuChiDataBuilder _builder;

    public ImportEngine(ITableExtractor extractor, TieuChiDataBuilder builder)
    {
        _extractor = extractor;
        _builder = builder;
    }

    public async Task<ImportResult> ProcessAsync(FileJob job, string localPath, CancellationToken ct = default)
    {
        ImportResult result;
        try
        {
            var (tables, firstPageText) = _extractor.Extract(localPath);
            if (tables.Count == 0)
            {
                result = new ImportResult { FilePath = localPath, FileName = job.FileName, Failed = true };
                result.Error = "Mọi bộ đọc đều không dựng được bảng từ file này. " +
                               "Nhiều khả năng là bản scan (cần OCR) hoặc không phải biểu mẫu " +
                               "tiêu chuẩn chất lượng. Xem nhật ký để biết từng bộ đọc báo gì.";
                ApplyKeys(result, job);
                return result;
            }

            var sections = RawTableParser.Parse(tables);
            result = await _builder.BuildAsync(localPath, sections, firstPageText, ct).ConfigureAwait(false);
            result.FileName = job.FileName;

            ResolveKeys(result, job);
            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result = new ImportResult
            {
                FilePath = localPath,
                FileName = job.FileName,
                Failed = true,
                Error = ex.Message
            };
            ApplyKeys(result, job);
            return result;
        }
    }

    /// <summary>Khoá của bản ghi tiêu chí lấy thẳng từ MucDinhKem - không đoán từ nội dung PDF.</summary>
    private static void ApplyKeys(ImportResult result, FileJob job)
    {
        result.SanPhamId = job.SanPhamId;
        result.MaSPNB = job.MaSPNB;
        result.XxthId = job.XxthId;
    }

    /// <summary>
    /// Xác định khoá (XXTHId, SanPhamId) cho bản ghi tiêu chí.
    ///
    /// MucDinhKem đã gắn sẵn sản phẩm (SanPhamId -> DM_SanPham, kèm MaSPNB) và XXTH
    /// (qua Market_TBSX), nên chỉ cần lấy lại và cảnh báo khi thiếu.
    /// </summary>
    private static void ResolveKeys(ImportResult result, FileJob job)
    {
        ApplyKeys(result, job);
        if (job.MaSPNB is not null) result.ItemCodes.Add(job.MaSPNB);

        if (job.SanPhamId is null)
            result.Warn(Severity.Warning, "THIEU_SAN_PHAM",
                "File này không có SanPhamId trong MucDinhKem - không ghi được tiêu chí.");

        if (job.XxthId is null)
            result.Warn(Severity.Warning, "THIEU_XXTH",
                "File này chưa gắn với XXTH nào (Market_TBSX) - không ghi được tiêu chí.");
        else if (job.MatchCount > 1)
            result.Warn(Severity.Warning, "NHIEU_XXTH",
                $"File đang gắn với {job.MatchCount} XXTH - đã lấy XXTH '{job.MaXXTH}' (mới nhất). " +
                "Cần kiểm tra lại đúng XXTH.");
    }
}
