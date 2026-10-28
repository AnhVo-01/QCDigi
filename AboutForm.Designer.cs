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
            tableLayoutPanel1 = new TableLayoutPanel();
            appVersion = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            label2 = new Label();
            checkLoad = new PictureBox();
            panel1 = new Panel();
            progressInstall = new ProgressBar();
            lblProgress = new Label();
            button1 = new Button();
            btn_install = new Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)checkLoad).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(appVersion, 1, 2);
            tableLayoutPanel1.Controls.Add(label5, 0, 2);
            tableLayoutPanel1.Controls.Add(label4, 1, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 1, 0);
            tableLayoutPanel1.Controls.Add(checkLoad, 1, 4);
            tableLayoutPanel1.Controls.Add(panel1, 0, 5);
            tableLayoutPanel1.Controls.Add(button1, 2, 6);
            tableLayoutPanel1.Controls.Add(btn_install, 1, 6);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 72.89156F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 27.1084347F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(463, 356);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // appVersion
            // 
            appVersion.Anchor = AnchorStyles.Left;
            appVersion.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(appVersion, 2);
            appVersion.Location = new Point(118, 67);
            appVersion.Name = "appVersion";
            appVersion.Size = new Size(40, 15);
            appVersion.TabIndex = 5;
            appVersion.Text = "1.0.0.0";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.Location = new Point(3, 67);
            label5.Name = "label5";
            label5.Size = new Size(105, 15);
            label5.TabIndex = 4;
            label5.Text = "Phiên bản hiện tại:";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 2);
            label4.Location = new Point(118, 37);
            label4.Name = "label4";
            label4.Size = new Size(127, 15);
            label4.TabIndex = 3;
            label4.Text = "Vo Ngo - CNTT.PTDA7";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 37);
            label3.Name = "label3";
            label3.Size = new Size(83, 15);
            label3.TabIndex = 2;
            label3.Text = "Thực hiện bởi:";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(89, 15);
            label1.TabIndex = 0;
            label1.Text = "Nhà phát hành:";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label2, 2);
            label2.Location = new Point(118, 7);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.TabIndex = 1;
            label2.Text = "APH - CNTT";
            // 
            // checkLoad
            // 
            checkLoad.Dock = DockStyle.Fill;
            checkLoad.Image = (Image)resources.GetObject("checkLoad.Image");
            checkLoad.Location = new Point(118, 113);
            checkLoad.Name = "checkLoad";
            checkLoad.Size = new Size(225, 119);
            checkLoad.SizeMode = PictureBoxSizeMode.CenterImage;
            checkLoad.TabIndex = 6;
            checkLoad.TabStop = false;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top;
            tableLayoutPanel1.SetColumnSpan(panel1, 3);
            panel1.Controls.Add(progressInstall);
            panel1.Controls.Add(lblProgress);
            panel1.Location = new Point(3, 238);
            panel1.Name = "panel1";
            panel1.Size = new Size(457, 48);
            panel1.TabIndex = 9;
            // 
            // progressInstall
            // 
            progressInstall.Location = new Point(0, 22);
            progressInstall.Name = "progressInstall";
            progressInstall.Size = new Size(454, 20);
            progressInstall.TabIndex = 1;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(4, 4);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(84, 15);
            lblProgress.TabIndex = 0;
            lblProgress.Text = "100MB/100MB";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.BackColor = SystemColors.Control;
            button1.FlatStyle = FlatStyle.System;
            button1.Location = new Point(349, 304);
            button1.Name = "button1";
            button1.Size = new Size(111, 28);
            button1.TabIndex = 7;
            button1.Text = "Kiểm tra cập nhật";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // btn_install
            // 
            btn_install.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_install.BackColor = SystemColors.ActiveCaption;
            btn_install.FlatStyle = FlatStyle.System;
            btn_install.Location = new Point(268, 304);
            btn_install.Name = "btn_install";
            btn_install.Size = new Size(75, 28);
            btn_install.TabIndex = 8;
            btn_install.Text = "Cài đặt";
            btn_install.UseVisualStyleBackColor = false;
            btn_install.Click += btn_install_Click;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(463, 356);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AboutForm";
            Text = "Thông tin";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)checkLoad).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label appVersion;
        private Label label5;
        private Label label4;
        private PictureBox checkLoad;
        private Button button1;
        private Button btn_install;
        private Panel panel1;
        private ProgressBar progressInstall;
        private Label lblProgress;
    }
}