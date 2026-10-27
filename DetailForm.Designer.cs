namespace TcclPdfTool
{
    partial class DetailForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailForm));
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnPdf = new Button();
            btnAlias = new Button();
            btnApply = new Button();
            btnCatalog = new Button();
            _info = new Label();
            splitContainer1 = new SplitContainer();
            _grid = new DataGridView();
            Trang = new DataGridViewTextBoxColumn();
            NhanGoc = new DataGridViewTextBoxColumn();
            MaTieuChi = new DataGridViewComboBoxColumn();
            TenTieuChi = new DataGridViewTextBoxColumn();
            SpecVi = new DataGridViewTextBoxColumn();
            SpecEn = new DataGridViewTextBoxColumn();
            tolerance = new DataGridViewComboBoxColumn();
            TolMin = new DataGridViewTextBoxColumn();
            TolMax = new DataGridViewTextBoxColumn();
            MeasMin = new DataGridViewTextBoxColumn();
            MeasMax = new DataGridViewTextBoxColumn();
            Nguon = new DataGridViewTextBoxColumn();
            DoTinCay = new DataGridViewTextBoxColumn();
            _warnings = new ListBox();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnPdf);
            flowLayoutPanel1.Controls.Add(btnAlias);
            flowLayoutPanel1.Controls.Add(btnApply);
            flowLayoutPanel1.Controls.Add(btnCatalog);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new Point(0, 515);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(8);
            flowLayoutPanel1.Size = new Size(1051, 46);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // btnPdf
            // 
            btnPdf.Anchor = AnchorStyles.Left;
            btnPdf.FlatStyle = FlatStyle.System;
            btnPdf.Location = new Point(11, 11);
            btnPdf.Name = "btnPdf";
            btnPdf.Size = new Size(120, 26);
            btnPdf.TabIndex = 0;
            btnPdf.Text = "Mở PDF gốc";
            btnPdf.UseVisualStyleBackColor = true;
            btnPdf.Click += btnPdf_Click;
            // 
            // btnAlias
            // 
            btnAlias.FlatStyle = FlatStyle.System;
            btnAlias.Location = new Point(137, 11);
            btnAlias.Name = "btnAlias";
            btnAlias.Size = new Size(180, 26);
            btnAlias.TabIndex = 1;
            btnAlias.Text = "Ghi nhớ nhãn đang chọn";
            btnAlias.UseVisualStyleBackColor = true;
            btnAlias.Click += btnAlias_Click;
            // 
            // btnApply
            // 
            btnApply.FlatStyle = FlatStyle.System;
            btnApply.Location = new Point(323, 11);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(150, 26);
            btnApply.TabIndex = 2;
            btnApply.Text = "Áp dụng thay đổi";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnCatalog
            // 
            btnCatalog.FlatStyle = FlatStyle.System;
            btnCatalog.Location = new Point(479, 11);
            btnCatalog.Name = "btnCatalog";
            btnCatalog.Size = new Size(160, 26);
            btnCatalog.TabIndex = 3;
            btnCatalog.Text = "Danh mục tiêu chí";
            btnCatalog.UseVisualStyleBackColor = true;
            btnCatalog.Click += btnCatalog_Click;
            // 
            // _info
            // 
            _info.Dock = DockStyle.Top;
            _info.Location = new Point(0, 0);
            _info.Name = "_info";
            _info.Padding = new Padding(8, 6, 8, 6);
            _info.Size = new Size(1051, 44);
            _info.TabIndex = 1;
            _info.Text = "label1";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 44);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(_grid);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(_warnings);
            splitContainer1.Size = new Size(1051, 471);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 2;
            // 
            // _grid
            // 
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grid.Columns.AddRange(new DataGridViewColumn[] { Trang, NhanGoc, MaTieuChi, TenTieuChi, SpecVi, SpecEn, tolerance, TolMin, TolMax, MeasMin, MeasMax, Nguon, DoTinCay });
            _grid.Dock = DockStyle.Fill;
            _grid.Location = new Point(0, 0);
            _grid.Name = "_grid";
            _grid.Size = new Size(1051, 400);
            _grid.TabIndex = 0;
            // 
            // Trang
            // 
            Trang.DataPropertyName = "Trang";
            Trang.HeaderText = "Tr";
            Trang.Name = "Trang";
            Trang.ReadOnly = true;
            Trang.Width = 34;
            // 
            // NhanGoc
            // 
            NhanGoc.DataPropertyName = "NhanGoc";
            NhanGoc.HeaderText = "Nhãn trên biểu mẫu";
            NhanGoc.Name = "NhanGoc";
            NhanGoc.Width = 250;
            // 
            // MaTieuChi
            // 
            MaTieuChi.DataPropertyName = "MaTieuChi";
            MaTieuChi.HeaderText = "Tiêu chí (sửa được)";
            MaTieuChi.Name = "MaTieuChi";
            MaTieuChi.Width = 190;
            // 
            // TenTieuChi
            // 
            TenTieuChi.DataPropertyName = "TenTieuChi";
            TenTieuChi.HeaderText = "Tên tiêu chí trong danh mục";
            TenTieuChi.Name = "TenTieuChi";
            TenTieuChi.ReadOnly = true;
            TenTieuChi.Resizable = DataGridViewTriState.True;
            TenTieuChi.SortMode = DataGridViewColumnSortMode.NotSortable;
            TenTieuChi.Width = 230;
            // 
            // SpecVi
            // 
            SpecVi.DataPropertyName = "SpecVi";
            SpecVi.HeaderText = "Tiếng Việt";
            SpecVi.Name = "SpecVi";
            SpecVi.Width = 150;
            // 
            // SpecEn
            // 
            SpecEn.DataPropertyName = "SpecEn";
            SpecEn.HeaderText = "Tiếng Anh";
            SpecEn.Name = "SpecEn";
            SpecEn.Width = 130;
            // 
            // tolerance
            // 
            tolerance.DataPropertyName = "Tolerance";
            tolerance.HeaderText = "Tolerance";
            tolerance.Name = "tolerance";
            tolerance.Resizable = DataGridViewTriState.True;
            tolerance.SortMode = DataGridViewColumnSortMode.Automatic;
            tolerance.Width = 80;
            // 
            // TolMin
            // 
            TolMin.DataPropertyName = "TolMin";
            TolMin.HeaderText = "Tol MIN";
            TolMin.Name = "TolMin";
            TolMin.Width = 62;
            // 
            // TolMax
            // 
            TolMax.DataPropertyName = "TolMax";
            TolMax.HeaderText = "Tol MAX";
            TolMax.Name = "TolMax";
            TolMax.Width = 62;
            // 
            // MeasMin
            // 
            MeasMin.DataPropertyName = "MeasMin";
            MeasMin.HeaderText = "Đo MIN";
            MeasMin.Name = "MeasMin";
            MeasMin.Width = 68;
            // 
            // MeasMax
            // 
            MeasMax.DataPropertyName = "MeasMax";
            MeasMax.HeaderText = "Đo MAX";
            MeasMax.Name = "MeasMax";
            MeasMax.Width = 68;
            // 
            // Nguon
            // 
            Nguon.DataPropertyName = "Nguon";
            Nguon.HeaderText = "Nguồn";
            Nguon.Name = "Nguon";
            Nguon.ReadOnly = true;
            Nguon.Width = 66;
            // 
            // DoTinCay
            // 
            DoTinCay.DataPropertyName = "DoTinCay";
            DoTinCay.HeaderText = "Tin cậy";
            DoTinCay.Name = "DoTinCay";
            DoTinCay.ReadOnly = true;
            DoTinCay.Width = 58;
            // 
            // _warnings
            // 
            _warnings.Dock = DockStyle.Fill;
            _warnings.FormattingEnabled = true;
            _warnings.ItemHeight = 15;
            _warnings.Location = new Point(0, 0);
            _warnings.Name = "_warnings";
            _warnings.Size = new Size(1051, 67);
            _warnings.TabIndex = 0;
            // 
            // DetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1051, 561);
            Controls.Add(splitContainer1);
            Controls.Add(_info);
            Controls.Add(flowLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "DetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "DetailForm";
            WindowState = FormWindowState.Maximized;
            Load += DetailForm_Load;
            flowLayoutPanel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnPdf;
        private Label _info;
        private Button btnAlias;
        private Button btnApply;
        private Button btnCatalog;
        private SplitContainer splitContainer1;
        private DataGridView _grid;
        private ListBox _warnings;
        private DataGridViewTextBoxColumn Trang;
        private DataGridViewTextBoxColumn NhanGoc;
        private DataGridViewComboBoxColumn MaTieuChi;
        private DataGridViewTextBoxColumn TenTieuChi;
        private DataGridViewTextBoxColumn SpecVi;
        private DataGridViewTextBoxColumn SpecEn;
        private DataGridViewComboBoxColumn tolerance;
        private DataGridViewTextBoxColumn TolMin;
        private DataGridViewTextBoxColumn TolMax;
        private DataGridViewTextBoxColumn MeasMin;
        private DataGridViewTextBoxColumn MeasMax;
        private DataGridViewTextBoxColumn Nguon;
        private DataGridViewTextBoxColumn DoTinCay;
    }
}