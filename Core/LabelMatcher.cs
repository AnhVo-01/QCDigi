using TcclPdfTool.Config;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

public sealed class MatchResult
{
    public DmTieuChi? Item { get; init; }
    public double Score { get; init; }
    public MapSource Source { get; init; }
    public string CandidateText { get; init; } = "";
    public bool Accepted { get; init; }
    public bool NeedsReview { get; init; }
}

/// <summary>
/// Map nhãn trên biểu mẫu về maTieuChi theo 2 tầng cục bộ (alias rồi fuzzy).
/// Tầng LLM nằm ngoài, chỉ được gọi cho những nhãn tầng này không chắc chắn -
/// nhờ vậy chi phí và độ trễ chỉ rơi vào phần thực sự khó.
/// </summary>
public sealed class LabelMatcher
{
    private readonly MappingConfig _cfg;
    private readonly List<DmTieuChi> _catalog;
    private readonly Dictionary<string, DmTieuChi> _byCode;
    private readonly List<(DmTieuChi item, HashSet<string> tokens)> _index;

    public LabelMatcher(IEnumerable<DmTieuChi> catalog, MappingConfig cfg)
    {
        _cfg = cfg;
        _catalog = catalog.ToList();
        _byCode = new Dictionary<string, DmTieuChi>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in _catalog) _byCode[d.MaTieuChi] = d;

        _index = new List<(DmTieuChi, HashSet<string>)>(_catalog.Count);
        foreach (var d in _catalog)
            _index.Add((d, new HashSet<string>(TextUtil.Tokenize(d.TieuChi), StringComparer.Ordinal)));
    }

    public IReadOnlyList<DmTieuChi> Catalog => _catalog;

    public DmTieuChi? ByCode(string? code)
        => code is not null && _byCode.TryGetValue(code, out var d) ? d : null;

    public bool ShouldIgnore(string label)
        => _cfg.NormalizedIgnore.Contains(TextUtil.Normalize(label));

    /// <summary>
    /// Thử map một dòng. Các ứng viên được thử theo thứ tự: nhãn con, nhãn cha,
    /// rồi ghép "cha + con" (giúp phân biệt WIDTH của nhóm DIMENTION với WIDTH của nhóm dao cắt).
    /// </summary>
    public MatchResult Match(RawRow row)
    {
        var candidates = BuildCandidates(row);

        // Tầng 1: alias tĩnh - tuyệt đối tin tưởng.
        foreach (var text in candidates)
        {
            if (_cfg.NormalizedAliases.TryGetValue(TextUtil.Normalize(text), out var code))
            {
                var item = ByCode(code);
                if (item is not null)
                    return new MatchResult
                    {
                        Item = item, Score = 1.0, Source = MapSource.Alias,
                        CandidateText = text, Accepted = true, NeedsReview = false
                    };
            }
        }

        // Tầng 2: so khớp mờ theo token.
        DmTieuChi? best = null;
        double bestScore = 0;
        string bestText = "";
        foreach (var text in candidates)
        {
            var tokens = TextUtil.Tokenize(text);
            if (tokens.Count == 0) continue;
            foreach (var (item, target) in _index)
            {
                var s = TextUtil.TokenScore(tokens, target);
                if (s > bestScore) { bestScore = s; best = item; bestText = text; }
            }
        }

        bool accepted = best is not null && bestScore >= _cfg.FuzzyAcceptThreshold;
        return new MatchResult
        {
            Item = accepted ? best : null,
            Score = bestScore,
            Source = accepted ? MapSource.Fuzzy : MapSource.None,
            CandidateText = bestText,
            Accepted = accepted,
            NeedsReview = accepted && bestScore < _cfg.FuzzyConfidentThreshold
        };
    }

    /// <summary>Danh sách nhãn ứng viên của một dòng, dùng chung cho alias, fuzzy và LLM.</summary>
    public static List<string> BuildCandidates(RawRow row)
    {
        var list = new List<string>();
        var label = TextUtil.Clean(row.Label);
        var sub = TextUtil.Clean(row.SubLabel);

        if (sub is not null) list.Add(sub);
        if (sub is null && label is not null) list.Add(label);
        if (sub is not null && label is not null) list.Add(label + " " + sub);
        return list;
    }

    /// <summary>Nhãn đầy đủ để hiển thị cho người duyệt và để gửi sang LLM.</summary>
    public static string DisplayLabel(RawRow row)
    {
        var label = TextUtil.Clean(row.Label);
        var sub = TextUtil.Clean(row.SubLabel);
        if (label is not null && sub is not null) return $"{label} > {sub}";
        return sub ?? label ?? "";
    }
}
