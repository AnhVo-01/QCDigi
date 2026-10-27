namespace TcclPdfTool
{
    partial class SettingsForm
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
            tableLayoutPanel1 = new TableLayoutPanel();
            _txtConnInfo = new TextBox();
            _btnConnSetup = new Button();
            _txtDbInfo = new TextBox();
            label1 = new Label();
            label5 = new Label();
            label4 = new Label();
            _txtOutput = new TextBox();
            label3 = new Label();
            _txtLicense = new TextBox();
            label2 = new Label();
            _txtFileRoot = new TextBox();
            label6 = new Label();
            FileRoot = new Button();
            BrowseFileButton = new Button();
            OutputBtn = new Button();
            _numCongTy = new ComboBox();
            label8 = new Label();
            _numMaxRows = new NumericUpDown();
            label7 = new Label();
            _chkLlm = new CheckBox();
            label9 = new Label();
            _txtEndpoint = new TextBox();
            label10 = new Label();
            _txtApiKey = new TextBox();
            label11 = new Label();
            _txtModel = new TextBox();
            btnTest = new Button();
            saveSetting = new Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numMaxRows).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel1.Controls.Add(_txtConnInfo, 1, 0);
            tableLayoutPanel1.Controls.Add(_btnConnSetup, 4, 0);
            tableLayoutPanel1.Controls.Add(_txtDbInfo, 1, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label5, 0, 6);
            tableLayoutPanel1.Controls.Add(label4, 0, 5);
            tableLayoutPanel1.Controls.Add(_txtOutput, 1, 5);
            tableLayoutPanel1.Controls.Add(label3, 0, 4);
            tableLayoutPanel1.Controls.Add(_txtLicense, 1, 4);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(_txtFileRoot, 1, 2);
            tableLayoutPanel1.Controls.Add(label6, 0, 1);
            tableLayoutPanel1.Controls.Add(FileRoot, 4, 2);
            tableLayoutPanel1.Controls.Add(BrowseFileButton, 4, 4);
            tableLayoutPanel1.Controls.Add(OutputBtn, 4, 5);
            tableLayoutPanel1.Controls.Add(_numCongTy, 1, 6);
            tableLayoutPanel1.Controls.Add(label8, 2, 6);
            tableLayoutPanel1.Controls.Add(_numMaxRows, 3, 6);
            tableLayoutPanel1.Controls.Add(label7, 0, 7);
            tableLayoutPanel1.Controls.Add(_chkLlm, 1, 8);
            tableLayoutPanel1.Controls.Add(label9, 0, 9);
            tableLayoutPanel1.Controls.Add(_txtEndpoint, 1, 9);
            tableLayoutPanel1.Controls.Add(label10, 0, 10);
            tableLayoutPanel1.Controls.Add(_txtApiKey, 1, 10);
            tableLayoutPanel1.Controls.Add(label11, 0, 11);
            tableLayoutPanel1.Controls.Add(_txtModel, 1, 11);
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 13;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(841, 346);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _txtConnInfo
            // 
            tableLayoutPanel1.SetColumnSpan(_txtConnInfo, 3);
            _txtConnInfo.Location = new Point(213, 3);
            _txtConnInfo.Name = "_txtConnInfo";
            _txtConnInfo.ReadOnly = true;
            _txtConnInfo.Size = new Size(519, 23);
            _txtConnInfo.TabIndex = 1;
            // 
            // _btnConnSetup
            // 
            _btnConnSetup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnConnSetup.BackColor = SystemColors.ControlLight;
            _btnConnSetup.FlatStyle = FlatStyle.System;
            _btnConnSetup.Location = new Point(756, 3);
            _btnConnSetup.Name = "_btnConnSetup";
            _btnConnSetup.Size = new Size(82, 23);
            _btnConnSetup.TabIndex = 2;
            _btnConnSetup.Text = "Thiết lập...";
            _btnConnSetup.UseVisualStyleBackColor = false;
            _btnConnSetup.Click += ConnSetup_Click;
            // 
            // _txtDbInfo
            // 
            tableLayoutPanel1.SetColumnSpan(_txtDbInfo, 4);
            _txtDbInfo.Location = new Point(213, 32);
            _txtDbInfo.Name = "_txtDbInfo";
            _txtDbInfo.ReadOnly = true;
            _txtDbInfo.Size = new Size(625, 23);
            _txtDbInfo.TabIndex = 14;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 0;
            label1.Text = "Kết nối CSDL";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.Location = new Point(3, 152);
            label5.Name = "label5";
            label5.Size = new Size(111, 15);
            label5.TabIndex = 12;
            label5.Text = "Công ty (CongTyId)";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(3, 123);
            label4.Name = "label4";
            label4.Size = new Size(122, 15);
            label4.TabIndex = 9;
            label4.Text = "Thư mục xuất kết quả";
            // 
            // _txtOutput
            // 
            tableLayoutPanel1.SetColumnSpan(_txtOutput, 3);
            _txtOutput.Location = new Point(213, 119);
            _txtOutput.Name = "_txtOutput";
            _txtOutput.Size = new Size(519, 23);
            _txtOutput.TabIndex = 10;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 94);
            label3.Name = "label3";
            label3.Size = new Size(131, 15);
            label3.TabIndex = 4;
            label3.Text = "File license Aspose (.lic)";
            // 
            // _txtLicense
            // 
            tableLayoutPanel1.SetColumnSpan(_txtLicense, 3);
            _txtLicense.Location = new Point(213, 90);
            _txtLicense.Name = "_txtLicense";
            _txtLicense.Size = new Size(519, 23);
            _txtLicense.TabIndex = 5;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 65);
            label2.Name = "label2";
            label2.Size = new Size(204, 15);
            label2.TabIndex = 2;
            label2.Text = "Thư mục gốc web (chứa /FileUpload)";
            // 
            // _txtFileRoot
            // 
            tableLayoutPanel1.SetColumnSpan(_txtFileRoot, 3);
            _txtFileRoot.Location = new Point(213, 61);
            _txtFileRoot.Name = "_txtFileRoot";
            _txtFileRoot.Size = new Size(519, 23);
            _txtFileRoot.TabIndex = 3;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Left;
            label6.AutoSize = true;
            label6.Location = new Point(3, 36);
            label6.Name = "label6";
            label6.Size = new Size(81, 15);
            label6.TabIndex = 13;
            label6.Text = "CSDL sử dụng";
            // 
            // FileRoot
            // 
            FileRoot.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FileRoot.BackColor = SystemColors.ControlLight;
            FileRoot.FlatStyle = FlatStyle.System;
            FileRoot.Location = new Point(756, 61);
            FileRoot.Name = "FileRoot";
            FileRoot.Size = new Size(82, 23);
            FileRoot.TabIndex = 6;
            FileRoot.Text = "Chọn...";
            FileRoot.UseVisualStyleBackColor = false;
            FileRoot.Click += button1_Click;
            // 
            // BrowseFileButton
            // 
            BrowseFileButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BrowseFileButton.BackColor = SystemColors.ControlLight;
            BrowseFileButton.FlatStyle = FlatStyle.System;
            BrowseFileButton.Location = new Point(756, 90);
            BrowseFileButton.Name = "BrowseFileButton";
            BrowseFileButton.Size = new Size(82, 23);
            BrowseFileButton.TabIndex = 7;
            BrowseFileButton.Text = "Chọn...";
            BrowseFileButton.UseVisualStyleBackColor = false;
            BrowseFileButton.Click += BrowseFileButton_Click;
            // 
            // OutputBtn
            // 
            OutputBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OutputBtn.BackColor = SystemColors.ControlLight;
            OutputBtn.FlatStyle = FlatStyle.System;
            OutputBtn.Location = new Point(756, 119);
            OutputBtn.Name = "OutputBtn";
            OutputBtn.Size = new Size(82, 23);
            OutputBtn.TabIndex = 8;
            OutputBtn.Text = "Chọn...";
            OutputBtn.UseVisualStyleBackColor = false;
            OutputBtn.Click += OutputBtn_Click;
            // 
            // _numCongTy
            // 
            _numCongTy.FormattingEnabled = true;
            _numCongTy.Items.AddRange(new object[] { "1:AAA", "2:APH" });
            _numCongTy.Location = new Point(213, 148);
            _numCongTy.Name = "_numCongTy";
            _numCongTy.Size = new Size(204, 23);
            _numCongTy.TabIndex = 17;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new Point(522, 152);
            label8.Name = "label8";
            label8.Size = new Size(105, 15);
            label8.TabIndex = 18;
            label8.Text = "Giới hạn dòng đọc";
            // 
            // _numMaxRows
            // 
            tableLayoutPanel1.SetColumnSpan(_numMaxRows, 2);
            _numMaxRows.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            _numMaxRows.Location = new Point(633, 148);
            _numMaxRows.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            _numMaxRows.Name = "_numMaxRows";
            _numMaxRows.Size = new Size(205, 23);
            _numMaxRows.TabIndex = 19;
            _numMaxRows.ThousandsSeparator = true;
            _numMaxRows.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Left;
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(3, 185);
            label7.Name = "label7";
            label7.Size = new Size(165, 15);
            label7.TabIndex = 20;
            label7.Text = "Dịch vụ map nhãn (tuỳ chọn)";
            // 
            // _chkLlm
            // 
            _chkLlm.Anchor = AnchorStyles.Left;
            _chkLlm.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(_chkLlm, 2);
            _chkLlm.Location = new Point(213, 214);
            _chkLlm.Name = "_chkLlm";
            _chkLlm.Size = new Size(408, 19);
            _chkLlm.TabIndex = 21;
            _chkLlm.Text = "Bật - chỉ gọi cho những nhãn mà alias và so khớp mờ không quyết được";
            _chkLlm.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Left;
            label9.AutoSize = true;
            label9.Location = new Point(3, 243);
            label9.Name = "label9";
            label9.Size = new Size(55, 15);
            label9.TabIndex = 22;
            label9.Text = "Endpoint";
            // 
            // _txtEndpoint
            // 
            tableLayoutPanel1.SetColumnSpan(_txtEndpoint, 3);
            _txtEndpoint.Location = new Point(213, 239);
            _txtEndpoint.Name = "_txtEndpoint";
            _txtEndpoint.Size = new Size(519, 23);
            _txtEndpoint.TabIndex = 23;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Left;
            label10.AutoSize = true;
            label10.Location = new Point(3, 272);
            label10.Name = "label10";
            label10.Size = new Size(46, 15);
            label10.TabIndex = 24;
            label10.Text = "API key";
            // 
            // _txtApiKey
            // 
            tableLayoutPanel1.SetColumnSpan(_txtApiKey, 3);
            _txtApiKey.Location = new Point(213, 268);
            _txtApiKey.Name = "_txtApiKey";
            _txtApiKey.Size = new Size(519, 23);
            _txtApiKey.TabIndex = 25;
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Left;
            label11.AutoSize = true;
            label11.Location = new Point(3, 301);
            label11.Name = "label11";
            label11.Size = new Size(41, 15);
            label11.TabIndex = 26;
            label11.Text = "Model";
            // 
            // _txtModel
            // 
            tableLayoutPanel1.SetColumnSpan(_txtModel, 3);
            _txtModel.Location = new Point(213, 297);
            _txtModel.Name = "_txtModel";
            _txtModel.Size = new Size(519, 23);
            _txtModel.TabIndex = 27;
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTest.BackColor = SystemColors.ActiveCaption;
            btnTest.FlatStyle = FlatStyle.System;
            btnTest.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnTest.Location = new Point(12, 364);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(150, 30);
            btnTest.TabIndex = 9;
            btnTest.Text = "Kiểm tra kết nối";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.Click += TestConnectionAsync;
            // 
            // saveSetting
            // 
            saveSetting.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveSetting.BackColor = SystemColors.ControlLight;
            saveSetting.FlatStyle = FlatStyle.System;
            saveSetting.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            saveSetting.Location = new Point(768, 364);
            saveSetting.Name = "saveSetting";
            saveSetting.Size = new Size(85, 30);
            saveSetting.TabIndex = 10;
            saveSetting.Text = "Lưu";
            saveSetting.UseVisualStyleBackColor = false;
            saveSetting.Click += saveSetting_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(865, 406);
            Controls.Add(saveSetting);
            Controls.Add(btnTest);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "SettingsForm";
            Text = "Cấu hình";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numMaxRows).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private TextBox _txtConnInfo;
        private Button _btnConnSetup;
        private TextBox _txtDbInfo;
        private Label label2;
        private TextBox _txtFileRoot;
        private Label label3;
        private TextBox _txtLicense;
        private Button FileRoot;
        private Button BrowseFileButton;
        private Button OutputBtn;
        private Label label4;
        private TextBox _txtOutput;
        private Button btnTest;
        private Button saveSetting;
        private Label label5;
        private Label label6;
        private ComboBox _numCongTy;
        private Label label8;
        private NumericUpDown _numMaxRows;
        private Label label7;
        private CheckBox _chkLlm;
        private Label label10;
        private Label label9;
        private TextBox _txtEndpoint;
        private TextBox _txtApiKey;
        private Label label11;
        private TextBox _txtModel;
    }
}