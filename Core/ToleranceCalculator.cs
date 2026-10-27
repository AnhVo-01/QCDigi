using System.Text.RegularExpressions;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

/// <summary>
/// Tính MinMeasuring/MaxMeasuring từ trị số và dung sai.
///
/// Công thức đã đối chiếu khớp với dữ liệu thật của hệ thống:
///   610 x (1 - 3%)  = 591.7   (prodWidth trong TieuChiData mẫu)
///   965 x (1 + 3%)  = 993.95  -> 994.0
///   650 x (1 - 3%)  = 630.50  (ảnh màn hình TXKEVP0384300)
/// => làm tròn 1 chữ số thập phân, làm tròn nửa ra xa số 0.
/// </summary>
public static class ToleranceCalculator
{
    public const int Decimals = 1;

    public static decimal? Round(decimal? v)
        => v is null ? null : Math.Round(v.Value, Decimals, MidpointRounding.AwayFromZero);

    /// <summary>Kiểu "string": trị số đo là chữ, giữ nguyên văn - không quy ra số.</summary>
    public static bool IsStringType(string? toleranceType)
        => string.Equals(toleranceType?.Trim(), "string", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// percent: giá trị x (1 + dungSai/100). number/mm: giá trị + dungSai.
    /// string hoặc thiếu dữ liệu: trả null (không suy đoán).
    /// </summary>
    public static (decimal? min, decimal? max) Compute(
        string? specValue, decimal? tolMin, decimal? tolMax, string? toleranceType)
    {
        var v = TextUtil.ParseNumber(specValue);
        if (v is null || tolMin is null || tolMax is null) return (null, null);

        var t = (toleranceType ?? "").Trim().ToLowerInvariant();
        switch (t)
        {
            case "percent":
            case "%":
                return (Round(v.Value * (1m + tolMin.Value / 100m)),
                        Round(v.Value * (1m + tolMax.Value / 100m)));
            case "number":
            case "mm":
                return (Round(v.Value + tolMin.Value), Round(v.Value + tolMax.Value));
            default:
                return (null, null);
        }
    }

    /// <summary>
    /// Kiểu dung sai suy ra từ chính giá trị đọc được trên biểu mẫu:
    /// không có Min/Max -> "string"; có dấu "%" -> "percent"; còn lại -> "number".
    /// Trả null khi Min/Max không đồng nhất (chỉ có một vế, hoặc một vế có "%" một vế không) -
    /// để người soát tự chọn.
    /// </summary>
    /// <param name="tolText">Cột dung sai dạng chữ ("+-3%") - chỉ dùng khi không có cột Min/Max riêng.</param>
    public static string? DetectToleranceType(string? tolMin, string? tolMax, string? tolText = null)
    {
        var min = TextUtil.ParseNumber(tolMin);
        var max = TextUtil.ParseNumber(tolMax);

        if (min is null && max is null)
        {
            var (tmin, _, ttype) = ParseToleranceText(tolText);
            return tmin is not null ? ttype : "string";
        }
        if (min is null || max is null) return null;

        // Số 0 có hay không có "%" cũng như nhau - không tính là lệch kiểu ("0" và "+3%" vẫn là percent).
        bool? pMin = min.Value == 0 ? null : tolMin!.Contains('%');
        bool? pMax = max.Value == 0 ? null : tolMax!.Contains('%');
        if (pMin is not null && pMax is not null && pMin != pMax) return null;

        return (pMin ?? pMax ?? (tolMin!.Contains('%') || tolMax!.Contains('%'))) ? "percent" : "number";
    }

    // "+-3%", "± 2.5%", "+-5mm", "0% + 3%", "Min", "Mean"
    private static readonly Regex SymmetricRx =
        new(@"^\s*(?:\+\s*[-−]|[-−]\s*\+|±)\s*(\d+(?:[.,]\d+)?)\s*(%?)", RegexOptions.Compiled);
    private static readonly Regex PairRx =
        new(@"([-+]?\d+(?:[.,]\d+)?)\s*(%?)\s*(?:,|;|\s)\s*\+?\s*([-+]?\d+(?:[.,]\d+)?)\s*(%?)", RegexOptions.Compiled);

    /// <summary>
    /// Đọc cột dung sai dạng chữ của mẫu QUALITY SPEC / INSPECTION REPORT.
    /// Trả về (min, max, kiểu). Không đọc được thì trả null hết - để người duyệt điền.
    /// </summary>
    public static (decimal? min, decimal? max, string? type) ParseToleranceText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null, null);
        var t = text.Replace(" ", "");

        var m = SymmetricRx.Match(t);
        if (m.Success)
        {
            var v = TextUtil.ParseNumber(m.Groups[1].Value);
            if (v is not null)
            {
                var type = m.Groups[2].Value == "%" ? "percent" : "number";
                return (-v.Value, v.Value, type);
            }
        }

        var p = PairRx.Match(t);
        if (p.Success)
        {
            var a = TextUtil.ParseNumber(p.Groups[1].Value);
            var b = TextUtil.ParseNumber(p.Groups[3].Value);
            if (a is not null && b is not null)
            {
                var type = (p.Groups[2].Value == "%" || p.Groups[4].Value == "%") ? "percent" : "number";
                return (a, b, type);
            }
        }

        return (null, null, null);
    }

    /// <summary>
    /// Đối chiếu trị số đo in trên biểu mẫu với trị số tự tính.
    /// Lệch quá ngưỡng nghĩa là đọc sai cột hoặc dung sai sai - phải báo cho người duyệt.
    /// </summary>
    /// <param name="relativeTolerance">Sai khác tương đối cho phép, mặc định 1%.</param>
    /// <param name="absoluteTolerance">Sai khác tuyệt đối bỏ qua, để không báo động vì làm tròn.</param>
    public static bool Disagrees(decimal? printed, decimal? computed,
                                 decimal relativeTolerance = 0.01m, decimal absoluteTolerance = 0.2m)
    {
        if (printed is null || computed is null) return false;
        var diff = Math.Abs(printed.Value - computed.Value);
        if (diff <= absoluteTolerance) return false;
        var scale = Math.Max(Math.Abs(printed.Value), 1m);
        return diff / scale > relativeTolerance;
    }
}
