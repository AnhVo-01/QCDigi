using System.ComponentModel;
using System.Diagnostics;
using TcclPdfTool.Config;
using TcclPdfTool.Core;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool
{
    public partial class DetailForm : Form
    {
        private readonly JobRow _row;
        private readonly List<DmTieuChi> _catalog;
        private readonly MappingConfig _mapping;
        private readonly AppConfig _cfg;
        private readonly BindingList<FieldRow> _fields = new();

        public DetailForm(JobRow row, List<DmTieuChi> catalog, MappingConfig mapping, AppConfig cfg)
        {
            _row = row;
            _catalog = catalog;
            _mapping = mapping;
            _cfg = cfg;
            Text = "Rà soát: " + row.Job.FileName;

            InitializeComponent();

            // Bắt buộc tắt trước khi gán DataSource: để mặc định (true) thì lưới tự sinh lại toàn bộ cột,
            // cột combo trong Designer bị thay bằng bản sao nên danh sách mã gán sau đó không hiện.
            _grid.AutoGenerateColumns = false;
            _grid.DataSource = _fields;
            _grid.DataError += (s, e) => e.ThrowException = false;

            // Ô mang giá trị mượn từ ô gộp được tô màu để người soát nhận ra ngay.
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.ShowCellToolTips = true;
            _grid.CellToolTipTextNeeded += Grid_CellToolTipTextNeeded;

            // Xoá dòng do form tự xử lý (có hỏi lại); không cho lưới tự thêm dòng trống.
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.KeyDown += Grid_KeyDown;

            // Chọn mã ở ô combo thì đánh dấu là sửa tay. Cột "Tên tiêu chí trong danh mục" cố ý
            // giữ nguyên tên lúc đọc file - người soát dựa vào đó để chọn mã cho đúng.
            _grid.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (_grid.IsCurrentCellDirty &&
                    (_grid.CurrentCell?.OwningColumn == MaTieuChi || _grid.CurrentCell?.OwningColumn == tolerance))
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _fields.Count) return;
                var row = e.RowIndex;
                var fr = _fields[row];

                if (e.ColumnIndex == MaTieuChi.Index)
                {
                    fr.Nguon = "Tay";
                    // Dòng chưa map có dung sai không đồng nhất: chọn mã xong thì gợi ý kiểu theo danh mục.
                    if (string.IsNullOrWhiteSpace(fr.Tolerance))
                    {
                        fr.Tolerance = FindCatalog(fr.MaTieuChi)?.Tolerance;
                        RecalcMeasuring(fr);
                    }
                }
                else if (e.ColumnIndex == TolMin.Index || e.ColumnIndex == TolMax.Index)
                {
                    var detected = ToleranceCalculator.DetectToleranceType(fr.TolMin, fr.TolMax);
                    if (detected is null)
                        fr.Tolerance ??= FindCatalog(fr.MaTieuChi)?.Tolerance;
                    else if (!(detected == "number" && fr.Tolerance == "percent"))
                        fr.Tolerance = detected;
                    RecalcMeasuring(fr);
                }
                else if (e.ColumnIndex == SpecVi.Index || e.ColumnIndex == tolerance.Index)
                {
                    RecalcMeasuring(fr);
                }
                else return;

                // Làm mới dòng sau khi lưới xử lý xong sự kiện, tránh đụng ô combo đang chỉnh.
                BeginInvoke(() => { if (row < _fields.Count) _fields.ResetItem(row); });
            };

            FormClosed += (_, __) => _catalogForm?.Close();
        }

        private CatalogForm? _catalogForm;

        /// <summary>
        /// Sửa trị số, dung sai hoặc kiểu dung sai thì tính lại trị số đo cho khớp và hiện ngay trên lưới.
        /// Không tính ra được (trị số dạng chữ, thiếu dung sai) thì giữ nguyên giá trị đang có.
        /// </summary>
        /// <summary>Nền xanh nhạt: giá trị không in riêng ở dòng này mà mượn từ ô gộp bên trên.</summary>
        private static readonly Color NenKeThua = Color.FromArgb(0xDE, 0xEB, 0xFF);

        private const string ChuThichKeThua =
            "Giá trị này không in riêng ở dòng này - lấy từ ô gộp của dòng bên trên. Hãy đối chiếu lại với bản PDF.";

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _fields.Count) return;
            var fr = _fields[e.RowIndex];

            if (fr.SpecKeThua && (e.ColumnIndex == SpecVi.Index || e.ColumnIndex == SpecEn.Index))
                e.CellStyle.BackColor = NenKeThua;
            else if (fr.NhanKeThua && e.ColumnIndex == NhanGoc.Index)
                e.CellStyle.BackColor = NenKeThua;
        }

        private void Grid_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _fields.Count) return;
            var fr = _fields[e.RowIndex];

            if ((fr.SpecKeThua && (e.ColumnIndex == SpecVi.Index || e.ColumnIndex == SpecEn.Index))
                || (fr.NhanKeThua && e.ColumnIndex == NhanGoc.Index))
                e.ToolTipText = ChuThichKeThua;
        }

        private static void RecalcMeasuring(FieldRow fr)
        {
            var (mn, mx) = ToleranceCalculator.Compute(fr.SpecVi,
                TextUtil.ParseNumber(fr.TolMin), TextUtil.ParseNumber(fr.TolMax), fr.Tolerance);
            if (mn is not null) fr.MeasMin = ToleranceCalculator.Round(mn)?.ToString();
            if (mx is not null) fr.MeasMax = ToleranceCalculator.Round(mx)?.ToString();
        }

        private DmTieuChi? FindCatalog(string? ma) =>
            string.IsNullOrWhiteSpace(ma) ? null
            : _catalog.FirstOrDefault(c => string.Equals(c.MaTieuChi, ma, StringComparison.OrdinalIgnoreCase));

        /// <summary>Mở danh mục DM_TieuChi để tra cứu (chỉ xem); mở lại khi đang mở thì chỉ đưa lên trước.</summary>
        private void btnCatalog_Click(object? sender, EventArgs e)
        {
            if (_catalog.Count == 0)
            {
                MessageBox.Show(this, "Không nạp được danh mục DM_TieuChi (xem nhật ký ở màn hình chính).",
                                "Danh mục tiêu chí", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_catalogForm is { IsDisposed: false })
            {
                _catalogForm.Activate();
                return;
            }

            _catalogForm = new CatalogForm(_catalog);
            _catalogForm.Show(this);
        }

        /// <summary>
        /// Phím Delete: bỏ các dòng đang chọn khỏi danh sách cần lưu.
        /// Chỉ bỏ trên màn hình - bấm "Áp dụng thay đổi" thì kết quả mới thực sự đổi.
        /// </summary>
        private void Grid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || _grid.IsCurrentCellInEditMode) return;

            // Chọn ô hay chọn cả dòng đều tính - lấy mọi dòng có ô đang được chọn.
            var indexes = _grid.SelectedCells.Cast<DataGridViewCell>()
                .Select(c => c.RowIndex)
                .Concat(_grid.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Index))
                .Where(i => i >= 0 && i < _fields.Count)
                .Distinct()
                .OrderByDescending(i => i)
                .ToList();
            if (indexes.Count == 0) return;

            e.Handled = true;
            var first = _fields[indexes[^1]];
            var msg = indexes.Count == 1
                ? $"Bỏ dòng \"{first.NhanGoc}\" khỏi danh sách cần lưu?"
                : $"Bỏ {indexes.Count} dòng đang chọn khỏi danh sách cần lưu?";
            if (MessageBox.Show(this, msg, "Bỏ dòng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            foreach (var i in indexes) _fields.RemoveAt(i);
        }

        private void DetailForm_Load(object sender, EventArgs e)
        {
            var r = _row.Result!;
            _info.Text =
            $"XXTH: {_row.Job.MaXXTH ?? "CHƯA GẮN"}    " +
            $"Sản phẩm: {(r.MaSPNB ?? (r.SanPhamId?.ToString() ?? "CHƯA XÁC ĐỊNH"))}    " +
            $"Mẫu: {(string.IsNullOrEmpty(r.DetectedTemplate) ? "không nhận diện được" : r.DetectedTemplate)}    " +
            $"FormType: {r.FormType}\r\n" +
            $"Đọc {r.RowsRead} dòng | map {r.Fields.Count} tiêu chí | {r.NeedsReviewCount} cần soát | {r.UnmappedRows.Count} chưa map";

            foreach (var w in r.Warnings) _warnings.Items.Add(w.ToString());

            LoadRows();

            int keThua = _fields.Count(f => f.SpecKeThua || f.NhanKeThua);
            if (keThua > 0)
                _info.Text += $"  |  {keThua} ô nền xanh = giá trị mượn từ ô gộp, cần đối chiếu PDF";

            // Danh sách combo = mã trong danh mục + mọi mã đang có trên lưới. Giá trị ô không nằm
            // trong danh sách thì DataGridView coi là lỗi và hiện ô trống - nên phải gộp đủ.
            var codes = _catalog.Select(c => c.MaTieuChi)
                .Concat(_fields.Select(f => f.MaTieuChi ?? ""))
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Prepend("")
                .ToList();
            MaTieuChi.DataSource = codes;
            MaTieuChi.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

            // Tương tự combo mã: gộp 3 kiểu chuẩn + mọi giá trị Tolerance trong danh mục + đang có trên lưới.
            tolerance.DataSource = new[] { "string", "percent", "number" }
                .Concat(_catalog.Select(c => c.Tolerance ?? ""))
                .Concat(_fields.Select(f => f.Tolerance ?? ""))
                .Where(t => t.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .Prepend("")
                .ToList();

            if (_catalog.Count == 0)
                _info.Text += "\r\nKHÔNG nạp được danh mục DM_TieuChi - combo tiêu chí chỉ có các mã đang dùng.";
        }

        private void LoadRows()
        {
            var r = _row.Result!;

            foreach (var f in r.Fields.Values.OrderBy(x => x.PageNumber).ThenBy(x => x.RowIndex))
            {
                _fields.Add(new FieldRow
                {
                    Trang = f.PageNumber,
                    RowIndex = f.RowIndex,
                    NhanGoc = f.SourceLabel,
                    // Lấy đúng cách viết hoa/thường trong danh mục để khớp mục của combo.
                    MaTieuChi = FindCatalog(f.MaTieuChi)?.MaTieuChi ?? f.MaTieuChi,
                    TenTieuChi = FindCatalog(f.MaTieuChi)?.TieuChi ?? "",
                    SpecVi = f.SpecificationsVi,
                    SpecEn = f.SpecificationsEn,
                    Tolerance = f.Tolerance,
                    TolMin = f.MinTolerance?.ToString(),
                    TolMax = f.MaxTolerance?.ToString(),
                    MeasMin = f.MinMeasuringText ?? f.MinMeasuring?.ToString(),
                    MeasMax = f.MaxMeasuringText ?? f.MaxMeasuring?.ToString(),
                    Nguon = f.MappedBy.ToString(),
                    DoTinCay = f.Confidence.ToString("F2"),
                    CanSoat = f.NeedsReview,
                    SpecKeThua = f.SpecInherited,
                    NhanKeThua = f.LabelInherited
                });
            }

            foreach (var u in r.UnmappedRows.OrderBy(x => x.PageNumber).ThenBy(x => x.RowIndex))
            {
                var (vi, en) = (u.SpecVi, u.SpecEn);
                if (vi is null && en is null && u.Spec is not null) (vi, en) = TextUtil.SplitViEn(u.Spec);

                // Chưa có mã nên chưa có Tolerance danh mục để dự phòng - không đồng nhất thì để trống chờ chọn.
                var tolType = ToleranceCalculator.DetectToleranceType(u.TolMin, u.TolMax, u.TolText);

                _fields.Add(new FieldRow
                {
                    Trang = u.PageNumber,
                    RowIndex = u.RowIndex,
                    NhanGoc = LabelMatcher.DisplayLabel(u),
                    MaTieuChi = "",
                    TenTieuChi = "",
                    SpecVi = vi,
                    SpecEn = en,
                    Tolerance = tolType,
                    TolMin = u.TolMin,
                    TolMax = u.TolMax,
                    MeasMin = u.MeasMin,
                    MeasMax = u.MeasMax,
                    Nguon = "-",
                    DoTinCay = "-",
                    CanSoat = true,
                    SpecKeThua = u.SpecInherited,
                    NhanKeThua = u.LabelInherited,
                    Source = u
                });
            }
        }

        private void btnPdf_Click(object sender, EventArgs e)
        {
            var path = _cfg.ResolveLocalPath(_row.Job.RelativePath);

            // File trên CDN là URL http/https - không thể dò bằng File.Exists, để trình duyệt/hệ điều hành xử lý.
            if (!_cfg.IsRemoteFileRoot && !File.Exists(path))
            {
                MessageBox.Show(this, "Không thấy file: " + path, "Mở PDF",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Không mở được PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ghi nhãn đang chọn vào mapping.json. Đây là cách hệ thống rẻ dần theo thời gian:
        /// nhãn đã sửa tay một lần thì file sau khớp ngay, không cần gọi mô hình nữa.
        /// </summary>
        private void btnAlias_Click(object sender, EventArgs e)
        {

            if (_grid.CurrentRow is null || _grid.CurrentRow.Index < 0 || _grid.CurrentRow.Index >= _fields.Count) return;
            var fr = _fields[_grid.CurrentRow.Index];

            if (string.IsNullOrWhiteSpace(fr.MaTieuChi) || string.IsNullOrWhiteSpace(fr.NhanGoc))
            {
                MessageBox.Show(this, "Dòng đang chọn chưa có mã tiêu chí để ghi nhớ.", "Ghi nhớ nhãn",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Nhãn hiển thị dạng "cha > con"; alias chỉ cần vế được dùng để tra.
            var label = fr.NhanGoc!;
            var gt = label.LastIndexOf('>');
            if (gt > 0) label = label.Substring(gt + 1).Trim();

            _mapping.AddAlias(label, fr.MaTieuChi!);
            _mapping.Save(_cfg.MappingPath);

            MessageBox.Show(this,
                $"Đã ghi nhớ:\r\n\"{label}\"  ->  {fr.MaTieuChi}\r\n\nCác file sau sẽ tự khớp nhãn này.",
                "Ghi nhớ nhãn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Ô đang gõ dở (chưa rời ô) thì đẩy giá trị xuống _fields trước khi đọc.
            _grid.EndEdit();
            BindingContext[_fields].EndCurrentEdit();

            var r = _row.Result!;
            r.Fields.Clear();

            foreach (var fr in _fields)
            {
                if (string.IsNullOrWhiteSpace(fr.MaTieuChi)) continue;
                var dm = _catalog.FirstOrDefault(c =>
                    string.Equals(c.MaTieuChi, fr.MaTieuChi, StringComparison.OrdinalIgnoreCase));
                if (dm is null) continue;

                var tolMin = TextUtil.ParseNumber(fr.TolMin);
                var tolMax = TextUtil.ParseNumber(fr.TolMax);
                var measMin = TextUtil.ParseNumber(fr.MeasMin);
                var measMax = TextUtil.ParseNumber(fr.MeasMax);

                // Ưu tiên kiểu đã suy ra/đã chọn trên lưới, trống mới lấy theo danh mục.
                var tolType = TextUtil.Clean(fr.Tolerance) ?? dm.Tolerance;
                var isString = ToleranceCalculator.IsStringType(tolType);

                // Người dùng sửa trị số hoặc dung sai mà chưa sửa trị số đo thì tính lại cho khớp.
                if (measMin is null || measMax is null)
                {
                    var (mn, mx) = ToleranceCalculator.Compute(fr.SpecVi, tolMin, tolMax, tolType);
                    measMin ??= mn;
                    measMax ??= mx;
                }

                r.Fields[dm.MaTieuChi] = new TieuChiField
                {
                    MaTieuChi = dm.MaTieuChi,
                    DanhMucId = dm.Id,
                    Tolerance = tolType,
                    SpecificationsVi = TextUtil.Clean(fr.SpecVi),
                    SpecificationsEn = TextUtil.Clean(fr.SpecEn),
                    MinTolerance = tolMin,
                    MaxTolerance = tolMax,
                    MinMeasuring = ToleranceCalculator.Round(measMin),
                    MaxMeasuring = ToleranceCalculator.Round(measMax),
                    MinMeasuringText = isString ? TextUtil.Clean(fr.MeasMin) : null,
                    MaxMeasuringText = isString ? TextUtil.Clean(fr.MeasMax) : null,
                    Confidence = fr.Nguon == "Tay" ? 1.0 : ParseConf(fr.DoTinCay),
                    MappedBy = fr.Nguon == "Tay" ? MapSource.Manual : MapSource.Fuzzy,
                    SourceLabel = fr.NhanGoc ?? "",
                    PageNumber = fr.Trang,
                    RowIndex = fr.RowIndex,
                    NeedsReview = false
                };
            }

            // Dòng chưa map còn lại trên lưới (chưa gán mã, chưa bị bỏ) mới tính là "chưa map".
            var remaining = _fields
                .Where(fr => fr.Source is not null && !r.Fields.Values.Any(f =>
                    f.PageNumber == fr.Trang && f.RowIndex == fr.RowIndex))
                .Select(fr => fr.Source!)
                .ToList();
            r.UnmappedRows.Clear();
            r.UnmappedRows.AddRange(remaining);

            _row.TrangThai = $"Đã soát tay ({r.Fields.Count} tiêu chí)";

            MessageBox.Show(this,
                $"Đã lưu thay đổi: {r.Fields.Count} tiêu chí, {r.UnmappedRows.Count} dòng chưa map.",
                "Áp dụng thay đổi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static double ParseConf(string? s)
        => double.TryParse(s, System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;
    }
}
