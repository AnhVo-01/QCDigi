using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TcclPdfTool.Ultils;

/// <summary>
/// Chuẩn hoá chuỗi, tách Việt/Anh, đọc số. Cố ý không phụ thuộc thư viện ngoài
/// để phần logic này compile và unit-test được ở mọi môi trường.
/// </summary>
public static class TextUtil
{
    private static readonly Regex NonAlnum = new(@"[^a-zA-Z0-9%/]+", RegexOptions.Compiled);
    private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex NumRx = new(@"[-+]?\d+(?:[.,]\d+)?", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    { "the", "of", "in", "per", "and", "or", "a", "an", "for", "with", "to" };

    /// <summary>Bỏ dấu tiếng Việt, hạ chữ thường, gom khoảng trắng. Dùng cho mọi phép so khớp.</summary>
    public static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var t = s.Replace('\n', ' ').Replace('\r', ' ');
        t = t.Replace('Đ', 'D').Replace('đ', 'd');
        t = t.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(t.Length);
        foreach (var c in t)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        t = NonAlnum.Replace(sb.ToString(), " ");
        t = MultiSpace.Replace(t, " ").Trim();
        return t.ToLowerInvariant();
    }

    /// <summary>Tách thành token đã bỏ từ dừng, phục vụ so khớp mờ.</summary>
    public static List<string> Tokenize(string? s)
    {
        var result = new List<string>();
        foreach (var w in Normalize(s).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            if (!StopWords.Contains(w)) result.Add(w);
        return result;
    }

    /// <summary>Chuỗi có chứa dấu tiếng Việt hay không.</summary>
    public static bool HasVietnameseDiacritics(string? s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.IndexOf('đ') >= 0 || s.IndexOf('Đ') >= 0) return true;
        var d = s.Normalize(NormalizationForm.FormD);
        foreach (var c in d)
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) return true;
        return false;
    }

    /// <summary>
    /// Tách ô gộp kiểu "Transparent/ Màu Trắng trong" thành (tiếng Việt, tiếng Anh).
    /// Quyết định vế nào là tiếng Việt dựa trên dấu. Không chắc thì trả cả cụm vào vế Việt.
    /// </summary>
    public static (string? vi, string? en) SplitViEn(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return (null, null);
        var t = s.Replace('\n', ' ').Trim();
        var idx = t.IndexOf('/');
        if (idx <= 0 || idx >= t.Length - 1) return (t, null);

        var a = t.Substring(0, idx).Trim();
        var b = t.Substring(idx + 1).Trim();
        if (a.Length == 0 || b.Length == 0) return (t, null);

        bool va = HasVietnameseDiacritics(a), vb = HasVietnameseDiacritics(b);
        if (vb && !va) return (b, a);
        if (va && !vb) return (a, b);
        return (t, null); // không phân biệt được - để nguyên cho người duyệt
    }

    /// <summary>Đọc số đầu tiên trong chuỗi. Chấp nhận cả dấu phẩy thập phân.</summary>
    public static decimal? ParseNumber(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var m = NumRx.Match(s.Replace(" ", ""));
        if (!m.Success) return null;
        var raw = m.Value.Replace(",", ".");
        return decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    /// <summary>Chuỗi chỉ gồm một con số (không lẫn chữ)? Dùng để biết ô là trị số hay mô tả.</summary>
    public static bool IsPureNumber(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var t = s.Trim().Replace(",", ".");
        return decimal.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    /// <summary>Gom khoảng trắng, xoá xuống dòng - dùng khi hiển thị và khi lưu.</summary>
    public static string? Clean(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = MultiSpace.Replace(s.Replace('\r', ' ').Replace('\n', ' '), " ").Trim();
        return t.Length == 0 ? null : t;
    }

    /// <summary>Điểm tương đồng 0..1 giữa hai tập token (kết hợp Jaccard và độ bao phủ hai chiều).</summary>
    public static double TokenScore(IReadOnlyCollection<string> candidate, HashSet<string> target)
    {
        if (candidate.Count == 0 || target.Count == 0) return 0;
        var cand = new HashSet<string>(candidate, StringComparer.Ordinal);
        int inter = 0;
        foreach (var t in cand) if (target.Contains(t)) inter++;
        if (inter == 0) return 0;
        int union = cand.Count + target.Count - inter;
        return (double)inter / union * 0.6
             + (double)inter / target.Count * 0.2
             + (double)inter / cand.Count * 0.2;
    }
}
