using System.Globalization;
using System.Text;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool.Core;

/// <summary>Giá trị "undefined" của JavaScript - cần phân biệt với null vì Number(undefined) = NaN.</summary>
public sealed class JsUndefined
{
    public static readonly JsUndefined Instance = new();
    private JsUndefined() { }
    public override string ToString() => "undefined";
}

public sealed class FuncEvaluationException : Exception
{
    public FuncEvaluationException(string message) : base(message) { }
}

/// <summary>
/// Trình thông dịch tối giản cho cột DM_TieuChi.Func.
///
/// Hỗ trợ đúng tập cú pháp mà các biểu thức hiện có đang dùng:
///   Number(x), truy cập thành viên với ?. , toán tử || && ?: + - * / , so sánh, ngoặc, null.
/// Ví dụ thật trong danh mục:
///   Number(data.bagWeight?.specificationsVi) + Number(data.tapeWeight?.specificationsVi || 0)
///   data.cutWidth?.specificationsVi ? (Number(data.prodWidth?.specificationsVi)
///        - Number(data.cutWidth?.specificationsVi || 0)) / 2 : null
///
/// Gặp cú pháp lạ thì ném lỗi rõ ràng chứ không đoán bừa - tính sai một tiêu chí
/// chất lượng nguy hiểm hơn nhiều so với để trống cho người nhập.
/// </summary>
public static class FuncEvaluator
{
    public static decimal? Evaluate(string? expression, IReadOnlyDictionary<string, TieuChiField> data)
    {
        if (string.IsNullOrWhiteSpace(expression)) return null;

        var ctx = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var bag = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in data)
        {
            bag[kv.Key] = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["specificationsVi"] = kv.Value.SpecificationsVi,
                ["specificationsEn"] = kv.Value.SpecificationsEn,
                ["minTolerance"] = ToDouble(kv.Value.MinTolerance),
                ["maxTolerance"] = ToDouble(kv.Value.MaxTolerance),
                ["minMeasuring"] = ToDouble(kv.Value.MinMeasuring),
                ["maxMeasuring"] = ToDouble(kv.Value.MaxMeasuring),
                ["danhMucId"] = (double)kv.Value.DanhMucId
            };
        }
        ctx["data"] = bag;

        var parser = new Parser(expression!, ctx);
        var value = parser.ParseProgram();
        return ToDecimal(value);
    }

    private static object? ToDouble(decimal? d) => d is null ? null : (object)(double)d.Value;

    private static decimal? ToDecimal(object? v)
    {
        switch (v)
        {
            case null:
            case JsUndefined:
                return null;
            case double d:
                return double.IsNaN(d) || double.IsInfinity(d) ? null : (decimal)d;
            case bool b:
                return b ? 1m : 0m;
            case string s:
                return TextUtil.ParseNumber(s);
            default:
                return null;
        }
    }

    // ---------------------------------------------------------------- helpers

    internal static bool Truthy(object? v) => v switch
    {
        null => false,
        JsUndefined => false,
        bool b => b,
        double d => d != 0 && !double.IsNaN(d),
        string s => s.Length > 0,
        _ => true
    };

    internal static double ToNumber(object? v) => v switch
    {
        null => 0d,
        JsUndefined => double.NaN,
        bool b => b ? 1d : 0d,
        double d => d,
        string s => ParseJsNumber(s),
        _ => double.NaN
    };

    private static double ParseJsNumber(string s)
    {
        var t = s.Trim();
        if (t.Length == 0) return 0d;
        t = t.Replace(",", ".");
        return double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : double.NaN;
    }

    // ---------------------------------------------------------------- parser

    private sealed class Parser
    {
        private readonly string _s;
        private readonly Dictionary<string, object?> _ctx;
        private int _i;

        public Parser(string s, Dictionary<string, object?> ctx) { _s = s; _ctx = ctx; _i = 0; }

        public object? ParseProgram()
        {
            var v = ParseTernary();
            SkipWs();
            if (_i < _s.Length)
                throw new FuncEvaluationException($"Ký tự thừa tại vị trí {_i}: '{_s.Substring(_i)}'");
            return v;
        }

        private void SkipWs() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }

        private bool Eat(string tok)
        {
            SkipWs();
            if (_i + tok.Length <= _s.Length && string.CompareOrdinal(_s, _i, tok, 0, tok.Length) == 0)
            {
                // không để '=' nuốt mất '==' , '|' nuốt '||'
                _i += tok.Length;
                return true;
            }
            return false;
        }

        private bool Peek(string tok)
        {
            SkipWs();
            return _i + tok.Length <= _s.Length && string.CompareOrdinal(_s, _i, tok, 0, tok.Length) == 0;
        }

        private object? ParseTernary()
        {
            var cond = ParseOr();
            SkipWs();
            // '?' nhưng không phải '?.'
            if (_i < _s.Length && _s[_i] == '?' && !(_i + 1 < _s.Length && _s[_i + 1] == '.'))
            {
                _i++;
                var a = ParseTernary();
                SkipWs();
                if (_i >= _s.Length || _s[_i] != ':')
                    throw new FuncEvaluationException("Thiếu ':' trong biểu thức điều kiện");
                _i++;
                var b = ParseTernary();
                return Truthy(cond) ? a : b;
            }
            return cond;
        }

        private object? ParseOr()
        {
            var left = ParseAnd();
            while (Peek("||"))
            {
                _i += 2;
                var right = ParseAnd();
                left = Truthy(left) ? left : right;
            }
            return left;
        }

        private object? ParseAnd()
        {
            var left = ParseEquality();
            while (Peek("&&"))
            {
                _i += 2;
                var right = ParseEquality();
                left = Truthy(left) ? right : left;
            }
            return left;
        }

        private object? ParseEquality()
        {
            var left = ParseRelational();
            while (true)
            {
                SkipWs();
                if (Peek("===")) { _i += 3; left = JsEquals(left, ParseRelational()); }
                else if (Peek("!==")) { _i += 3; left = !JsEquals(left, ParseRelational()); }
                else if (Peek("==")) { _i += 2; left = JsEquals(left, ParseRelational()); }
                else if (Peek("!=")) { _i += 2; left = !JsEquals(left, ParseRelational()); }
                else break;
            }
            return left;
        }

        private static bool JsEquals(object? a, object? b)
        {
            bool an = a is null or JsUndefined, bn = b is null or JsUndefined;
            if (an || bn) return an && bn;
            if (a is string sa && b is string sb) return string.Equals(sa, sb, StringComparison.Ordinal);
            return ToNumber(a).Equals(ToNumber(b));
        }

        private object? ParseRelational()
        {
            var left = ParseAdditive();
            while (true)
            {
                SkipWs();
                if (Peek(">=")) { _i += 2; left = ToNumber(left) >= ToNumber(ParseAdditive()); }
                else if (Peek("<=")) { _i += 2; left = ToNumber(left) <= ToNumber(ParseAdditive()); }
                else if (Peek(">")) { _i += 1; left = ToNumber(left) > ToNumber(ParseAdditive()); }
                else if (Peek("<")) { _i += 1; left = ToNumber(left) < ToNumber(ParseAdditive()); }
                else break;
            }
            return left;
        }

        private object? ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (true)
            {
                SkipWs();
                if (_i < _s.Length && _s[_i] == '+')
                {
                    _i++;
                    var r = ParseMultiplicative();
                    // JS: chuỗi + gì cũng thành nối chuỗi. Ở đây chỉ gặp số nên cộng số.
                    if (left is string || r is string) left = JsConcatOrAdd(left, r);
                    else left = ToNumber(left) + ToNumber(r);
                }
                else if (_i < _s.Length && _s[_i] == '-')
                {
                    _i++;
                    left = ToNumber(left) - ToNumber(ParseMultiplicative());
                }
                else break;
            }
            return left;
        }

        private static object JsConcatOrAdd(object? a, object? b)
        {
            var na = ToNumber(a); var nb = ToNumber(b);
            if (!double.IsNaN(na) && !double.IsNaN(nb)) return na + nb;
            return (a?.ToString() ?? "") + (b?.ToString() ?? "");
        }

        private object? ParseMultiplicative()
        {
            var left = ParseUnary();
            while (true)
            {
                SkipWs();
                if (_i < _s.Length && _s[_i] == '*') { _i++; left = ToNumber(left) * ToNumber(ParseUnary()); }
                else if (_i < _s.Length && _s[_i] == '/') { _i++; left = ToNumber(left) / ToNumber(ParseUnary()); }
                else break;
            }
            return left;
        }

        private object? ParseUnary()
        {
            SkipWs();
            if (_i < _s.Length && _s[_i] == '-') { _i++; return -ToNumber(ParseUnary()); }
            if (_i < _s.Length && _s[_i] == '+') { _i++; return ToNumber(ParseUnary()); }
            if (_i < _s.Length && _s[_i] == '!') { _i++; return !Truthy(ParseUnary()); }
            return ParsePostfix();
        }

        private object? ParsePostfix()
        {
            var v = ParsePrimary();
            while (true)
            {
                SkipWs();
                if (Peek("?."))
                {
                    _i += 2;
                    var name = ReadIdent();
                    if (v is null or JsUndefined) { v = JsUndefined.Instance; continue; }
                    v = GetMember(v, name);
                }
                else if (_i < _s.Length && _s[_i] == '.')
                {
                    _i++;
                    var name = ReadIdent();
                    if (v is null or JsUndefined)
                        throw new FuncEvaluationException($"Truy cập '.{name}' trên giá trị rỗng");
                    v = GetMember(v, name);
                }
                else break;
            }
            return v;
        }

        private static object? GetMember(object? target, string name)
        {
            if (target is IDictionary<string, object?> dict)
                return dict.TryGetValue(name, out var v) ? v : JsUndefined.Instance;
            return JsUndefined.Instance;
        }

        private string ReadIdent()
        {
            SkipWs();
            int start = _i;
            while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_' || _s[_i] == '$')) _i++;
            if (start == _i) throw new FuncEvaluationException($"Thiếu tên thuộc tính tại vị trí {_i}");
            return _s.Substring(start, _i - start);
        }

        private object? ParsePrimary()
        {
            SkipWs();
            if (_i >= _s.Length) throw new FuncEvaluationException("Biểu thức kết thúc đột ngột");

            var c = _s[_i];

            if (c == '(')
            {
                _i++;
                var v = ParseTernary();
                SkipWs();
                if (_i >= _s.Length || _s[_i] != ')') throw new FuncEvaluationException("Thiếu ')'");
                _i++;
                return v;
            }

            if (c == '\'' || c == '"')
            {
                _i++;
                var sb = new StringBuilder();
                while (_i < _s.Length && _s[_i] != c)
                {
                    if (_s[_i] == '\\' && _i + 1 < _s.Length) _i++;
                    sb.Append(_s[_i++]);
                }
                if (_i >= _s.Length) throw new FuncEvaluationException("Chuỗi chưa đóng");
                _i++;
                return sb.ToString();
            }

            if (char.IsDigit(c) || (c == '.' && _i + 1 < _s.Length && char.IsDigit(_s[_i + 1])))
            {
                int start = _i;
                while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.')) _i++;
                var lit = _s.Substring(start, _i - start);
                return double.Parse(lit, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            if (char.IsLetter(c) || c == '_' || c == '$')
            {
                var name = ReadIdent();
                SkipWs();

                if (_i < _s.Length && _s[_i] == '(')   // lời gọi hàm
                {
                    _i++;
                    object? arg = null;
                    SkipWs();
                    if (_i < _s.Length && _s[_i] != ')') arg = ParseTernary();
                    SkipWs();
                    if (_i >= _s.Length || _s[_i] != ')') throw new FuncEvaluationException("Thiếu ')' khi gọi hàm");
                    _i++;
                    return name switch
                    {
                        "Number" or "parseFloat" or "parseInt" => ToNumber(arg),
                        "String" => arg?.ToString() ?? "",
                        "Boolean" => Truthy(arg),
                        _ => throw new FuncEvaluationException($"Hàm '{name}' chưa được hỗ trợ")
                    };
                }

                return name switch
                {
                    "null" or "undefined" => name == "null" ? null : JsUndefined.Instance,
                    "true" => true,
                    "false" => false,
                    "NaN" => double.NaN,
                    _ => _ctx.TryGetValue(name, out var v)
                            ? v
                            : throw new FuncEvaluationException($"Biến '{name}' không tồn tại")
                };
            }

            throw new FuncEvaluationException($"Ký tự không hợp lệ '{c}' tại vị trí {_i}");
        }
    }
}
