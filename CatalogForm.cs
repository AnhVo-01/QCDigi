using System.ComponentModel;
using TcclPdfTool.Model;
using TcclPdfTool.Ultils;

namespace TcclPdfTool
{
    /// <summary>
    /// Danh mục DM_TieuChi để tra cứu khi rà soát: mã, tên, nhóm, FormType, dung sai.
    /// Chỉ để xem: mở song song với DetailForm (không chặn), gõ để lọc.
    /// </summary>
    public sealed class CatalogForm : Form
    {
        private readonly List<CatalogItem> _all;
        private readonly BindingList<CatalogItem> _view = new();

        private readonly TextBox _search = new() { Dock = DockStyle.Fill, PlaceholderText = "Gõ mã hoặc tên tiêu chí để lọc..." };
        private readonly DataGridView _grid = new();
        private readonly Label _count = new() { Dock = DockStyle.Right, AutoSize = true, Padding = new Padding(8, 6, 0, 0) };

        public CatalogForm(IReadOnlyList<DmTieuChi> catalog)
        {
            var byId = catalog.ToDictionary(c => c.Id);
            _all = catalog
                //.Where(c => !c.IsGroup)
                .Select(c => new CatalogItem
                {
                    MaTieuChi = c.MaTieuChi,
                    TenTieuChi = c.TieuChi,
                    Nhom = c.GroupId is int g && byId.TryGetValue(g, out var p) ? p.TieuChi : "",
                    FormType = c.FormType ?? "",
                    DungSai = c.Tolerance ?? ""
                })
                .OrderBy(x => x.MaTieuChi, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Text = "Danh mục tiêu chí (DM_TieuChi)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(820, 560);
            MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;

            var top = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(8, 6, 8, 4) };
            top.Controls.Add(_search);
            top.Controls.Add(_count);

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft
            };
            var btnClose = new Button { Text = "Đóng", Width = 90, FlatStyle = FlatStyle.System };
            btnClose.Click += (_, __) => Close();
            bottom.Controls.Add(btnClose);

            SetupGrid();

            Controls.Add(_grid);
            Controls.Add(top);
            Controls.Add(bottom);

            _search.TextChanged += (_, __) => ApplyFilter();
            _search.KeyDown += (_, e) =>
            {
                // Mũi tên xuống từ ô tìm kiếm để chuyển sang lưới mà không cần chuột.
                if (e.KeyCode == Keys.Down && _grid.Rows.Count > 0) { _grid.Focus(); e.Handled = true; }
            };
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            ApplyFilter();
        }

        private void SetupGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.None;

            _grid.Columns.Add(Col(nameof(CatalogItem.MaTieuChi), "Mã tiêu chí", 140));
            _grid.Columns.Add(Col(nameof(CatalogItem.TenTieuChi), "Tên tiêu chí", 0));
            _grid.Columns.Add(Col(nameof(CatalogItem.Nhom), "Nhóm", 150));
            _grid.Columns.Add(Col(nameof(CatalogItem.FormType), "FormType", 90));
            _grid.Columns.Add(Col(nameof(CatalogItem.DungSai), "Dung sai", 90));

            _grid.DataSource = _view;
        }

        private static DataGridViewTextBoxColumn Col(string prop, string header, int width)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, Name = prop };
            if (width > 0) c.Width = width;
            else c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            return c;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // CatalogForm
            // 
            ClientSize = new Size(284, 261);
            Name = "CatalogForm";
            ShowIcon = false;
            ResumeLayout(false);

        }

        /// <summary>Lọc theo mã hoặc tên, không phân biệt hoa thường và dấu tiếng Việt.</summary>
        private void ApplyFilter()
        {
            var q = TextUtil.Normalize(_search.Text);
            var hits = q.Length == 0
                ? _all
                : _all.Where(x => TextUtil.Normalize(x.MaTieuChi).Contains(q, StringComparison.Ordinal)
                               || TextUtil.Normalize(x.TenTieuChi).Contains(q, StringComparison.Ordinal)
                               || TextUtil.Normalize(x.Nhom).Contains(q, StringComparison.Ordinal))
                      .ToList();

            _view.RaiseListChangedEvents = false;
            _view.Clear();
            foreach (var h in hits) _view.Add(h);
            _view.RaiseListChangedEvents = true;
            _view.ResetBindings();

            _count.Text = $"{hits.Count}/{_all.Count} tiêu chí";
        }

        private sealed class CatalogItem
        {
            public string MaTieuChi { get; init; } = "";
            public string TenTieuChi { get; init; } = "";
            public string Nhom { get; init; } = "";
            public string FormType { get; init; } = "";
            public string DungSai { get; init; } = "";
        }
    }
}
