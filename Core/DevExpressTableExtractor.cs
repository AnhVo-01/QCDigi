using System.Text;
using DevExpress.Pdf;
using TcclPdfTool.Model;

namespace TcclPdfTool.Core;

/// <summary>
/// Trích bảng bằng DevExpress PDF Document API (thay cho Aspose.PDF TableAbsorber).
///
/// DevExpress không có sẵn bộ nhận dạng bảng, nên lưới được dựng lại từ chính nội dung trang:
///   1) Đọc các lệnh vẽ của trang để lấy đường kẻ ngang/dọc (nét vẽ và hình chữ nhật mảnh).
///   2) Gom các đường cắt nhau thành từng bảng; toạ độ đường kẻ chia bảng thành lưới ô.
///   3) Thiếu đường kẻ giữa hai ô kề nhau => ô gộp. Chữ của ô gộp đặt ở ô trên-trái,
///      các ô còn lại để trống - đúng dạng mà RawTableParser mong đợi.
///   4) Từng từ (kèm toạ độ, lấy qua NextWord) được đổ vào ô chứa tâm của nó.
/// </summary>
public sealed class DevExpressTableExtractor : ITableExtractor
{
    /// <summary>Hai đường kẻ cách nhau dưới ngưỡng này (điểm) coi như một.</summary>
    private const double LineTolerance = 2.0;

    /// <summary>Hình chữ nhật có một chiều không quá ngưỡng này được coi là đường kẻ.</summary>
    private const double RuleThickness = 3.0;

    /// <summary>Hàng/cột hẹp hơn ngưỡng này là do đường kẻ đôi - gộp vào hàng/cột bên cạnh.</summary>
    private const double MinCellSize = 4.0;

    /// <summary>
    /// Hình tô đặc cao hơn ngưỡng này được coi là NỀN Ô, không phải đường kẻ.
    /// Một số biểu mẫu phân cột bằng khối nền chứ không bằng nét kẻ, nên hai mép
    /// trái-phải của khối nền cũng phải tính là ranh giới cột - thiếu bước này
    /// là mất cột ngoài cùng bên phải (cột Tolerance in measuring MAX).
    /// </summary>
    private const double MinFillHeightForColumnEdge = 5.0;

    public (List<ExtractedTable> tables, string firstPageText) Extract(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"Không tìm thấy file PDF: {pdfPath}");

        using var processor = new PdfDocumentProcessor();
        processor.LoadDocument(pdfPath, true);

        var pages = processor.Document.Pages;
        var firstPageText = pages.Count > 0 ? processor.GetPageText(1) ?? "" : "";

        // NextWord duyệt toàn bộ tài liệu theo thứ tự đọc - gom một lượt theo trang.
        var wordsByPage = new Dictionary<int, List<Word>>();
        PdfPageWord? w;
        while ((w = processor.NextWord()) is not null)
        {
            if (!wordsByPage.TryGetValue(w.PageNumber, out var list))
                wordsByPage[w.PageNumber] = list = new List<Word>();
            foreach (var seg in w.Segments)
            {
                var text = seg.Text?.Trim();
                if (string.IsNullOrEmpty(text)) continue;
                var r = seg.Rectangle.BoundingRectangle;
                list.Add(new Word(text, r.Left, r.Bottom, r.Right, r.Top));
            }
        }

        var tables = new List<ExtractedTable>();
        for (int p = 0; p < pages.Count; p++)
        {
            var rules = RuleCollector.Collect(pages[p]);
            var words = wordsByPage.TryGetValue(p + 1, out var wl) ? wl : new List<Word>();
            foreach (var group in GroupIntoTables(rules))
            {
                var table = BuildTable(group, words, p + 1);
                if (table is not null && table.Rows.Count > 0) tables.Add(table);
            }
        }

        // Bảng trên trang được sắp từ trên xuống, giống thứ tự TableAbsorber trả về.
        return (tables, firstPageText);
    }

    // ------------------------------------------------------------------ gom bảng

    /// <summary>Gom các đường kẻ cắt/chạm nhau thành từng nhóm - mỗi nhóm là một bảng.</summary>
    private static List<List<Rule>> GroupIntoTables(List<Rule> rules)
    {
        var parent = Enumerable.Range(0, rules.Count).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }

        for (int i = 0; i < rules.Count; i++)
            for (int j = i + 1; j < rules.Count; j++)
                if (Touches(rules[i], rules[j])) parent[Find(i)] = Find(j);

        return rules.Select((r, i) => (r, root: Find(i)))
                    .GroupBy(x => x.root, x => x.r)
                    .Select(g => g.ToList())
                    .Where(g => g.Count(r => r.Horizontal) >= 2 && g.Count(r => !r.Horizontal) >= 2)
                    .OrderByDescending(g => g.Max(r => r.Horizontal ? r.Pos : r.End))
                    .ToList();
    }

    private static bool Touches(Rule a, Rule b)
    {
        const double t = LineTolerance;
        if (a.Horizontal == b.Horizontal)
            return Math.Abs(a.Pos - b.Pos) <= t && a.Start <= b.End + t && b.Start <= a.End + t;
        var (h, v) = a.Horizontal ? (a, b) : (b, a);
        return v.Pos >= h.Start - t && v.Pos <= h.End + t && h.Pos >= v.Start - t && h.Pos <= v.End + t;
    }

    // ------------------------------------------------------------------ dựng lưới

    private static ExtractedTable? BuildTable(List<Rule> rules, List<Word> words, int pageNumber)
    {
        var hs = rules.Where(r => r.Horizontal).ToList();
        var vs = rules.Where(r => !r.Horizontal).ToList();

        // Toạ độ biên cột tăng dần (trái -> phải), biên hàng giảm dần (trên -> dưới).
        // Mép ngoài lấy cả theo đầu mút đường kẻ: có biểu mẫu thiếu hẳn viền phải/dưới.
        var xs = Cluster(vs.Select(r => r.Pos).Append(hs.Min(r => r.Start)).Append(hs.Max(r => r.End)));
        var ys = Cluster(hs.Select(r => r.Pos).Append(vs.Min(r => r.Start)).Append(vs.Max(r => r.End)));
        ys.Reverse();
        int nCols = xs.Count - 1, nRows = ys.Count - 1;
        if (nCols < 1 || nRows < 1) return null;

        bool HasVertical(int k, int row)   // biên dọc xs[k] có kẻ trong hàng row không
        {
            double top = ys[row], bottom = ys[row + 1], mid = (top + bottom) / 2;
            return vs.Any(v => Math.Abs(v.Pos - xs[k]) <= LineTolerance
                               && v.Start <= mid && v.End >= mid);
        }
        bool HasHorizontal(int k, int col) // biên ngang ys[k] có kẻ trong cột col không
        {
            double mid = (xs[col] + xs[col + 1]) / 2;
            return hs.Any(h => Math.Abs(h.Pos - ys[k]) <= LineTolerance
                               && h.Start <= mid && h.End >= mid);
        }

        // Ô gộp: hai ô kề nhau mà không có đường kẻ ngăn cách thì thuộc cùng một ô.
        var parent = Enumerable.Range(0, nRows * nCols).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }
        void Union(int a, int b)
        {
            int ra = Find(a), rb = Find(b);
            if (ra == rb) return;
            // Giữ gốc là ô có chỉ số nhỏ nhất = ô trên cùng bên trái của vùng gộp.
            if (ra < rb) parent[rb] = ra; else parent[ra] = rb;
        }
        for (int r = 0; r < nRows; r++)
            for (int c = 0; c < nCols; c++)
            {
                if (c + 1 < nCols && !HasVertical(c + 1, r)) Union(r * nCols + c, r * nCols + c + 1);
                if (r + 1 < nRows && !HasHorizontal(r + 1, c)) Union(r * nCols + c, (r + 1) * nCols + c);
            }

        // Đổ từng từ vào ô chứa tâm của nó, rồi dồn về ô gốc của vùng gộp.
        var cellWords = new Dictionary<int, List<Word>>();
        foreach (var word in words)
        {
            double cx = (word.Left + word.Right) / 2, cy = (word.Bottom + word.Top) / 2;
            if (cx < xs[0] || cx > xs[^1] || cy > ys[0] || cy < ys[^1]) continue;
            int c = IndexOf(xs, cx, ascending: true);
            int r = IndexOf(ys, cy, ascending: false);
            if (c < 0 || r < 0) continue;
            var root = Find(r * nCols + c);
            if (!cellWords.TryGetValue(root, out var list)) cellWords[root] = list = new List<Word>();
            list.Add(word);
        }

        var result = new ExtractedTable { PageNumber = pageNumber };
        var gridRows = new string?[nRows][];
        for (int r = 0; r < nRows; r++) gridRows[r] = new string?[nCols];
        foreach (var (root, list) in cellWords)
            gridRows[root / nCols][root % nCols] = CellText(list);

        // Hàng trống hoàn toàn (do gộp dọc) không mang thông tin - bỏ đi.
        foreach (var row in gridRows)
            if (row.Any(c => !string.IsNullOrEmpty(c))) result.Rows.Add(row);

        return result;
    }

    /// <summary>Gom các toạ độ gần nhau và bỏ khoảng quá hẹp (đường kẻ đôi).</summary>
    private static List<double> Cluster(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var outp = new List<double>();
        foreach (var v in sorted)
        {
            if (outp.Count > 0 && v - outp[^1] < MinCellSize) continue;
            outp.Add(v);
        }
        return outp;
    }

    /// <summary>Khoảng [bounds[i], bounds[i+1]] chứa v.</summary>
    private static int IndexOf(List<double> bounds, double v, bool ascending)
    {
        for (int i = 0; i < bounds.Count - 1; i++)
        {
            bool inside = ascending ? v >= bounds[i] && v <= bounds[i + 1]
                                    : v <= bounds[i] && v >= bounds[i + 1];
            if (inside) return i;
        }
        return -1;
    }

    /// <summary>
    /// Ghép chữ trong ô: từ trên xuống thành từng dòng (nối bằng xuống dòng),
    /// trong dòng từ trái sang phải (nối bằng dấu cách khi hai từ có khoảng hở).
    /// </summary>
    private static string CellText(List<Word> words)
    {
        var lines = new List<List<Word>>();
        foreach (var w in words.OrderByDescending(w => w.Top))
        {
            var line = lines.FirstOrDefault(l => SameLine(l[0], w));
            if (line is null) lines.Add(line = new List<Word>());
            line.Add(w);
        }

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (sb.Length > 0) sb.Append('\n');
            Word? prev = null;
            foreach (var w in line.OrderBy(w => w.Left))
            {
                // DevExpress tách "13/7/2019" thành "13/", "7/", "2019" sát nhau - không chèn cách.
                if (prev is not null && w.Left - prev.Right > 0.05 * w.Height) sb.Append(' ');
                sb.Append(w.Text);
                prev = w;
            }
        }
        return sb.ToString().Trim();
    }

    private static bool SameLine(Word a, Word b)
    {
        var overlap = Math.Min(a.Top, b.Top) - Math.Max(a.Bottom, b.Bottom);
        return overlap > 0.5 * Math.Min(a.Height, b.Height);
    }

    // ------------------------------------------------------------------ kiểu dữ liệu

    private sealed record Word(string Text, double Left, double Bottom, double Right, double Top)
    {
        public double Height => Top - Bottom;
    }

    /// <summary>Đường kẻ: ngang (Pos = y, Start..End theo x) hoặc dọc (Pos = x, Start..End theo y).</summary>
    private sealed record Rule(bool Horizontal, double Pos, double Start, double End);

    /// <summary>
    /// Đọc lệnh vẽ của trang, theo dõi ma trận biến đổi (CTM), để lấy ra các đường kẻ
    /// ngang/dọc ở toạ độ trang - cùng hệ toạ độ với vị trí từ mà NextWord trả về.
    /// </summary>
    private sealed class RuleCollector
    {
        private readonly List<Rule> _rules = new();
        private readonly Stack<Matrix> _stack = new();
        private Matrix _ctm = Matrix.Identity;

        // Đường dẫn đang dựng: các subpath (dãy điểm), đã quy đổi về toạ độ trang.
        // Viền ô có thể là lệnh "re" hoặc đa giác m-l-l-l-h - cả hai đều quy về subpath.
        private readonly List<List<Pt>> _subpaths = new();
        private List<Pt>? _open;

        public static List<Rule> Collect(PdfPage page)
        {
            var c = new RuleCollector();
            c.Walk(page.Commands);
            return c._rules;
        }

        private void Walk(IEnumerable<PdfCommand> commands)
        {
            foreach (var cmd in commands)
            {
                switch (cmd)
                {
                    case PdfSaveGraphicsStateCommand:
                        _stack.Push(_ctm); break;
                    case PdfRestoreGraphicsStateCommand:
                        if (_stack.Count > 0) _ctm = _stack.Pop(); break;
                    case PdfModifyTransformationMatrixCommand m:
                        _ctm = Matrix.From(m.Matrix).Then(_ctm); break;

                    case PdfBeginPathCommand b:
                        _open = new List<Pt> { _ctm.Apply(b.StartPoint.X, b.StartPoint.Y) };
                        _subpaths.Add(_open); break;
                    case PdfAppendLineSegmentCommand l:
                        _open?.Add(_ctm.Apply(l.EndPoint.X, l.EndPoint.Y)); break;
                    case PdfClosePathCommand:
                        if (_open is { Count: > 1 }) _open.Add(_open[0]);
                        break;
                    case PdfAppendRectangleCommand r:
                        _subpaths.Add(new List<Pt>
                        {
                            _ctm.Apply(r.X, r.Y), _ctm.Apply(r.X + r.Width, r.Y),
                            _ctm.Apply(r.X + r.Width, r.Y + r.Height), _ctm.Apply(r.X, r.Y + r.Height),
                            _ctm.Apply(r.X, r.Y)
                        });
                        _open = null; break;

                    case PdfEndPathWithoutFillingAndStrokingCommand:
                        ClearPath(); break; // đường dẫn chỉ dùng để cắt (clip) - không phải nét kẻ
                    case PdfStrokePathCommand:
                    case PdfCloseAndStrokePathCommand:
                    case PdfFillAndStrokePathUsingEvenOddRuleCommand:
                    case PdfFillAndStrokePathUsingNonzeroWindingNumberRuleCommand:
                    case PdfCloseFillAndStrokePathUsingEvenOddRuleCommand:
                    case PdfCloseFillAndStrokePathUsingNonzeroWindingNumberRuleCommand:
                        EmitStroke(); break;
                    case PdfFillPathUsingEvenOddRuleCommand:
                    case PdfFillPathUsingNonzeroWindingNumberRuleCommand:
                        EmitFill(); break;

                    case PdfPaintXObjectCommand x when x.XObject is PdfForm form:
                        _stack.Push(_ctm);
                        if (form.Matrix is not null) _ctm = Matrix.From(form.Matrix).Then(_ctm);
                        Walk(form.Commands);
                        _ctm = _stack.Pop();
                        break;
                    case PdfCommandGroup g:
                        Walk(g.Children); break;
                }
            }
        }

        private void EmitStroke()
        {
            foreach (var sp in _subpaths)
                for (int i = 1; i < sp.Count; i++) AddLine(sp[i - 1], sp[i]);
            ClearPath();
        }

        /// <summary>Tô đặc: chỉ hình mảnh mới là đường kẻ (Excel/Word xuất viền ô kiểu này).</summary>
        private void EmitFill()
        {
            foreach (var sp in _subpaths)
            {
                if (sp.Count < 3) continue;
                double minX = sp.Min(p => p.X), maxX = sp.Max(p => p.X);
                double minY = sp.Min(p => p.Y), maxY = sp.Max(p => p.Y);
                double w = maxX - minX, h = maxY - minY;
                if (h <= RuleThickness && w > h)
                    _rules.Add(new Rule(true, (minY + maxY) / 2, minX, maxX));
                else if (w <= RuleThickness && h > w)
                    _rules.Add(new Rule(false, (minX + maxX) / 2, minY, maxY));
                else if (h >= MinFillHeightForColumnEdge && w > RuleThickness)
                {
                    // Nền ô / ô gộp: lấy hai mép đứng làm ranh giới cột.
                    _rules.Add(new Rule(false, minX, minY, maxY));
                    _rules.Add(new Rule(false, maxX, minY, maxY));
                }
            }
            ClearPath();
        }

        private void AddLine(Pt a, Pt b)
        {
            if (Math.Abs(a.Y - b.Y) <= 0.5 && Math.Abs(a.X - b.X) > LineTolerance)
                _rules.Add(new Rule(true, (a.Y + b.Y) / 2, Math.Min(a.X, b.X), Math.Max(a.X, b.X)));
            else if (Math.Abs(a.X - b.X) <= 0.5 && Math.Abs(a.Y - b.Y) > LineTolerance)
                _rules.Add(new Rule(false, (a.X + b.X) / 2, Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y)));
        }

        private void ClearPath() { _subpaths.Clear(); _open = null; }
    }

    private readonly record struct Pt(double X, double Y);

    /// <summary>Ma trận affine theo quy ước PDF: [x y 1] x M.</summary>
    private readonly record struct Matrix(double A, double B, double C, double D, double E, double F)
    {
        public static readonly Matrix Identity = new(1, 0, 0, 1, 0, 0);

        public static Matrix From(PdfTransformationMatrix m) => new(m.A, m.B, m.C, m.D, m.E, m.F);

        /// <summary>Áp dụng this trước rồi tới <paramref name="o"/> (this x o).</summary>
        public Matrix Then(Matrix o) => new(
            A * o.A + B * o.C, A * o.B + B * o.D,
            C * o.A + D * o.C, C * o.B + D * o.D,
            E * o.A + F * o.C + o.E, E * o.B + F * o.D + o.F);

        public Pt Apply(double x, double y) => new(A * x + C * y + E, B * x + D * y + F);
    }
}
