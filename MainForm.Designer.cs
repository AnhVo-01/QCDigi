namespace TcclPdfTool
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            btnOpen = new ToolStripButton();
            btnLoad = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            lblDonHang = new ToolStripLabel();
            txtDonHang = new ToolStripTextBox();
            btnLoadByOrder = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnSettings = new ToolStripButton();
            toolStrip1 = new ToolStrip();
            _btnRun = new ToolStripButton();
            _btnStop = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            _btnSave = new ToolStripButton();
            _btnExport = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            jobRows = new DataGridView();
            Chon = new DataGridViewCheckBoxColumn();
            TenFile = new DataGridViewTextBoxColumn();
            MaXXTH = new DataGridViewTextBoxColumn();
            MaSPNB = new DataGridViewTextBoxColumn();
            Mau = new DataGridViewTextBoxColumn();
            DocDuoc = new DataGridViewTextBoxColumn();
            MapDuoc = new DataGridViewTextBoxColumn();
            CanSoat = new DataGridViewTextBoxColumn();
            ChuaMap = new DataGridViewTextBoxColumn();
            TrangThai = new DataGridViewTextBoxColumn();
            _log = new ListBox();
            statusStrip1 = new StatusStrip();
            txtOpen = new ToolStripStatusLabel();
            _progress = new ToolStripProgressBar();
            _status = new ToolStripStatusLabel();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)jobRows).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnOpen
            // 
            btnOpen.Image = (Image)resources.GetObject("btnOpen.Image");
            btnOpen.ImageTransparentColor = Color.Magenta;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(83, 22);
            btnOpen.Text = "Mở tài liệu";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnLoad
            // 
            btnLoad.Image = (Image)resources.GetObject("btnLoad.Image");
            btnLoad.ImageTransparentColor = Color.Magenta;
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(106, 22);
            btnLoad.Text = "Nạp danh sách";
            btnLoad.Click += btnLoad_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // lblDonHang
            // 
            lblDonHang.Name = "lblDonHang";
            lblDonHang.Size = new Size(62, 22);
            lblDonHang.Text = "Đơn hàng:";
            // 
            // txtDonHang
            // 
            txtDonHang.BackColor = SystemColors.Window;
            txtDonHang.Name = "txtDonHang";
            txtDonHang.Size = new Size(200, 25);
            txtDonHang.ToolTipText = "Mã XXTH. Nhiều mã thì cách nhau bởi dấu phẩy, dán được cả cột từ Excel.";
            txtDonHang.KeyDown += txtDonHang_KeyDown;
            // 
            // btnLoadByOrder
            // 
            btnLoadByOrder.Image = Properties.Resources.icons8_search_48;
            btnLoadByOrder.Name = "btnLoadByOrder";
            btnLoadByOrder.Size = new Size(130, 22);
            btnLoadByOrder.Text = "Nạp theo đơn hàng";
            btnLoadByOrder.ToolTipText = "Chỉ nạp file TCCL của những mã XXTH đã nhập (hiển thị cả file đã import).";
            btnLoadByOrder.Click += btnLoadByOrder_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnSettings
            // 
            btnSettings.Image = (Image)resources.GetObject("btnSettings.Image");
            btnSettings.ImageTransparentColor = Color.Magenta;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 22);
            btnSettings.Text = "Cấu hình";
            btnSettings.Click += btnSettings_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnOpen, btnLoad, toolStripSeparator4, lblDonHang, txtDonHang, btnLoadByOrder, toolStripSeparator1, _btnRun, _btnStop, toolStripSeparator2, _btnSave, _btnExport, toolStripSeparator3, btnSettings });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1051, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // _btnRun
            // 
            _btnRun.Image = (Image)resources.GetObject("_btnRun.Image");
            _btnRun.ImageTransparentColor = Color.Magenta;
            _btnRun.Name = "_btnRun";
            _btnRun.Size = new Size(81, 22);
            _btnRun.Text = "Chạy xử lý";
            _btnRun.Click += _btnRun_Click;
            // 
            // _btnStop
            // 
            _btnStop.Image = (Image)resources.GetObject("_btnStop.Image");
            _btnStop.ImageTransparentColor = Color.Magenta;
            _btnStop.Name = "_btnStop";
            _btnStop.Size = new Size(56, 22);
            _btnStop.Text = "Dừng";
            _btnStop.Click += _btnStop_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // _btnSave
            // 
            _btnSave.Image = (Image)resources.GetObject("_btnSave.Image");
            _btnSave.ImageTransparentColor = Color.Magenta;
            _btnSave.Name = "_btnSave";
            _btnSave.Size = new Size(84, 22);
            _btnSave.Text = "Ghi dữ liệu";
            _btnSave.Click += _btnSave_Click;
            // 
            // _btnExport
            // 
            _btnExport.Image = (Image)resources.GetObject("_btnExport.Image");
            _btnExport.ImageTransparentColor = Color.Magenta;
            _btnExport.Name = "_btnExport";
            _btnExport.Size = new Size(75, 22);
            _btnExport.Text = "Xuất CSV";
            _btnExport.Click += _btnExport_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // jobRows
            // 
            jobRows.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            jobRows.BorderStyle = BorderStyle.Fixed3D;
            jobRows.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            jobRows.Columns.AddRange(new DataGridViewColumn[] { Chon, TenFile, MaXXTH, MaSPNB, Mau, DocDuoc, MapDuoc, CanSoat, ChuaMap, TrangThai });
            jobRows.Location = new Point(4, 28);
            jobRows.Name = "jobRows";
            jobRows.Size = new Size(1043, 409);
            jobRows.TabIndex = 2;
            // 
            // Chon
            // 
            Chon.HeaderText = "";
            Chon.MinimumWidth = 30;
            Chon.Name = "Chon";
            Chon.Width = 30;
            // 
            // TenFile
            // 
            TenFile.HeaderText = "Tên File";
            TenFile.Name = "TenFile";
            TenFile.ReadOnly = true;
            TenFile.Width = 300;
            // 
            // MaXXTH
            // 
            MaXXTH.HeaderText = "Đơn hàng";
            MaXXTH.Name = "MaXXTH";
            MaXXTH.ReadOnly = true;
            MaXXTH.Width = 130;
            // 
            // MaSPNB
            // 
            MaSPNB.HeaderText = "Sản phẩm";
            MaSPNB.Name = "MaSPNB";
            MaSPNB.ReadOnly = true;
            MaSPNB.Width = 130;
            // 
            // Mau
            // 
            Mau.HeaderText = "Loại mẫu";
            Mau.Name = "Mau";
            Mau.ReadOnly = true;
            Mau.Width = 170;
            // 
            // DocDuoc
            // 
            DocDuoc.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            DocDuoc.HeaderText = "Dòng đọc";
            DocDuoc.Name = "DocDuoc";
            DocDuoc.ReadOnly = true;
            DocDuoc.Width = 84;
            // 
            // MapDuoc
            // 
            MapDuoc.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            MapDuoc.HeaderText = "Map được";
            MapDuoc.Name = "MapDuoc";
            MapDuoc.ReadOnly = true;
            MapDuoc.Width = 86;
            // 
            // CanSoat
            // 
            CanSoat.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            CanSoat.HeaderText = "Cần soát";
            CanSoat.Name = "CanSoat";
            CanSoat.ReadOnly = true;
            CanSoat.Width = 78;
            // 
            // ChuaMap
            // 
            ChuaMap.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            ChuaMap.HeaderText = "Chưa map";
            ChuaMap.Name = "ChuaMap";
            ChuaMap.ReadOnly = true;
            ChuaMap.Width = 87;
            // 
            // TrangThai
            // 
            TrangThai.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TrangThai.HeaderText = "Trạng thái";
            TrangThai.Name = "TrangThai";
            TrangThai.ReadOnly = true;
            // 
            // _log
            // 
            _log.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _log.FormattingEnabled = true;
            _log.ItemHeight = 15;
            _log.Location = new Point(4, 441);
            _log.MaximumSize = new Size(0, 150);
            _log.Name = "_log";
            _log.Size = new Size(1043, 94);
            _log.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { txtOpen, _progress, _status });
            statusStrip1.Location = new Point(0, 539);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1051, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // txtOpen
            // 
            txtOpen.Name = "txtOpen";
            txtOpen.Size = new Size(161, 17);
            txtOpen.Text = "Không có tệp nào được chọn";
            // 
            // _progress
            // 
            _progress.Name = "_progress";
            _progress.Size = new Size(200, 16);
            // 
            // _status
            // 
            _status.Name = "_status";
            _status.Size = new Size(0, 17);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1051, 561);
            Controls.Add(statusStrip1);
            Controls.Add(_log);
            Controls.Add(jobRows);
            Controls.Add(toolStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Công cụ số hoá Tiêu chuẩn chất lượng (TCCL)";
            Load += Form1_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)jobRows).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolStripButton btnOpen;
        private ToolStripButton btnLoad;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnSettings;
        private ToolStrip toolStrip1;
        private DataGridView jobRows;
        private ListBox _log;
        private ToolStripButton _btnRun;
        private ToolStripButton _btnStop;
        private ToolStripSeparator toolStripSeparator2;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel txtOpen;
        private ToolStripProgressBar _progress;
        private ToolStripStatusLabel _status;
        private ToolStripButton _btnSave;
        private ToolStripButton _btnExport;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripLabel lblDonHang;
        private ToolStripTextBox txtDonHang;
        private ToolStripButton btnLoadByOrder;
        private DataGridViewCheckBoxColumn Chon;
        private DataGridViewTextBoxColumn TenFile;
        private DataGridViewTextBoxColumn MaXXTH;
        private DataGridViewTextBoxColumn MaSPNB;
        private DataGridViewTextBoxColumn Mau;
        private DataGridViewTextBoxColumn DocDuoc;
        private DataGridViewTextBoxColumn MapDuoc;
        private DataGridViewTextBoxColumn CanSoat;
        private DataGridViewTextBoxColumn ChuaMap;
        private DataGridViewTextBoxColumn TrangThai;
    }
}
