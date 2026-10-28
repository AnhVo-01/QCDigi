namespace TcclPdfTool
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            headerPanel = new Panel();
            appIcon = new PictureBox();
            lblTitle = new Label();
            lblSubtitle = new Label();
            separator = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            appVersion = new Label();
            panel1 = new Panel();
            statusRow = new FlowLayoutPanel();
            checkLoad = new PictureBox();
            lblProgress = new Label();
            progressInstall = new ProgressBar();
            footerPanel = new Panel();
            btn_install = new Button();
            button1 = new Button();
            btnClose = new Button();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)appIcon).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            statusRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)checkLoad).BeginInit();
            footerPanel.SuspendLayout();
            SuspendLayout();
            //
            // headerPanel
            //
            headerPanel.Controls.Add(appIcon);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(440, 76);
            headerPanel.TabIndex = 0;
            //
            // appIcon
            //
            appIcon.Location = new Point(20, 14);
            appIcon.Name = "appIcon";
            appIcon.Size = new Size(48, 48);
            appIcon.SizeMode = PictureBoxSizeMode.Zoom;
            appIcon.TabIndex = 0;
            appIcon.TabStop = false;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(80, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(306, 21);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Công cụ số hoá Tiêu chuẩn chất lượng";
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.ForeColor = SystemColors.GrayText;
            lblSubtitle.Location = new Point(82, 40);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(68, 15);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "TcclPdfTool";
            //
            // separator
            //
            separator.BorderStyle = BorderStyle.Fixed3D;
            separator.Location = new Point(20, 76);
            separator.Name = "separator";
            separator.Size = new Size(400, 2);
            separator.TabIndex = 1;
            //
            // tableLayoutPanel1
            //
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(label4, 1, 1);
            tableLayoutPanel1.Controls.Add(label5, 0, 2);
            tableLayoutPanel1.Controls.Add(appVersion, 1, 2);
            tableLayoutPanel1.Location = new Point(20, 90);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.Size = new Size(400, 78);
            tableLayoutPanel1.TabIndex = 2;
            //
            // label1
            //
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.GrayText;
            label1.Location = new Point(3, 5);
            label1.Name = "label1";
            label1.Size = new Size(89, 15);
            label1.TabIndex = 0;
            label1.Text = "Nhà phát hành:";
            //
            // label2
            //
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(133, 5);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.TabIndex = 1;
            label2.Text = "APH - CNTT";
            //
            // label3
            //
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.ForeColor = SystemColors.GrayText;
            label3.Location = new Point(3, 31);
            label3.Name = "label3";
            label3.Size = new Size(83, 15);
            label3.TabIndex = 2;
            label3.Text = "Thực hiện bởi:";
            //
            // label4
            //
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(133, 31);
            label4.Name = "label4";
            label4.Size = new Size(127, 15);
            label4.TabIndex = 3;
            label4.Text = "Vo Ngo - CNTT.PTDA7";
            //
            // label5
            //
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.ForeColor = SystemColors.GrayText;
            label5.Location = new Point(3, 57);
            label5.Name = "label5";
            label5.Size = new Size(105, 15);
            label5.TabIndex = 4;
            label5.Text = "Phiên bản hiện tại:";
            //
            // appVersion
            //
            appVersion.Anchor = AnchorStyles.Left;
            appVersion.AutoSize = true;
            appVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            appVersion.Location = new Point(133, 57);
            appVersion.Name = "appVersion";
            appVersion.Size = new Size(37, 15);
            appVersion.TabIndex = 5;
            appVersion.Text = "1.0.0";
            //
            // panel1
            //
            panel1.Controls.Add(statusRow);
            panel1.Controls.Add(progressInstall);
            panel1.Location = new Point(20, 180);
            panel1.Name = "panel1";
            panel1.Size = new Size(400, 60);
            panel1.TabIndex = 3;
            //
            // statusRow
            //
            statusRow.Controls.Add(checkLoad);
            statusRow.Controls.Add(lblProgress);
            statusRow.Location = new Point(0, 0);
            statusRow.Name = "statusRow";
            statusRow.Size = new Size(400, 38);
            statusRow.TabIndex = 0;
            statusRow.WrapContents = false;
            //
            // checkLoad
            //
            checkLoad.Image = (Image)resources.GetObject("checkLoad.Image");
            checkLoad.Location = new Point(3, 0);
            checkLoad.Margin = new Padding(3, 0, 3, 0);
            checkLoad.Name = "checkLoad";
            checkLoad.Size = new Size(22, 22);
            checkLoad.SizeMode = PictureBoxSizeMode.Zoom;
            checkLoad.TabIndex = 0;
            checkLoad.TabStop = false;
            //
            // lblProgress
            //
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(31, 3);
            lblProgress.Margin = new Padding(3, 3, 0, 0);
            lblProgress.MaximumSize = new Size(366, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(0, 15);
            lblProgress.TabIndex = 1;
            //
            // progressInstall
            //
            progressInstall.Location = new Point(3, 40);
            progressInstall.Name = "progressInstall";
            progressInstall.Size = new Size(394, 16);
            progressInstall.TabIndex = 1;
            //
            // footerPanel
            //
            footerPanel.BackColor = SystemColors.Control;
            footerPanel.Controls.Add(btn_install);
            footerPanel.Controls.Add(button1);
            footerPanel.Controls.Add(btnClose);
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Location = new Point(0, 254);
            footerPanel.Name = "footerPanel";
            footerPanel.Size = new Size(440, 50);
            footerPanel.TabIndex = 4;
            //
            // btn_install
            //
            btn_install.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_install.Location = new Point(112, 11);
            btn_install.Name = "btn_install";
            btn_install.Size = new Size(90, 28);
            btn_install.TabIndex = 0;
            btn_install.Text = "Cài đặt";
            btn_install.UseVisualStyleBackColor = true;
            btn_install.Click += btn_install_Click;
            //
            // button1
            //
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(208, 11);
            button1.Name = "button1";
            button1.Size = new Size(130, 28);
            button1.TabIndex = 1;
            button1.Text = "Kiểm tra cập nhật";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(344, 11);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 28);
            btnClose.TabIndex = 2;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = true;
            //
            // AboutForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            CancelButton = btnClose;
            ClientSize = new Size(440, 304);
            Controls.Add(headerPanel);
            Controls.Add(separator);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(panel1);
            Controls.Add(footerPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thông tin";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)appIcon).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            statusRow.ResumeLayout(false);
            statusRow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)checkLoad).EndInit();
            footerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel headerPanel;
        private PictureBox appIcon;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label separator;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label appVersion;
        private Panel panel1;
        private FlowLayoutPanel statusRow;
        private PictureBox checkLoad;
        private Label lblProgress;
        private ProgressBar progressInstall;
        private Panel footerPanel;
        private Button btn_install;
        private Button button1;
        private Button btnClose;
    }
}
