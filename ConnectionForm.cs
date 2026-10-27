using Microsoft.Data.SqlClient;
using TcclPdfTool.Config;

namespace TcclPdfTool
{
    /// <summary>
    /// Hộp thoại kết nối SQL Server kiểu "Connect to Server" của SSMS: nhập máy chủ + đăng nhập,
    /// bấm Kết nối để lấy danh sách CSDL rồi chọn các CSDL công cụ cần dùng.
    /// Form không tự lưu gì - người gọi đọc <see cref="Info"/>, <see cref="Password"/>, <see cref="DbNames"/>.
    /// </summary>
    public sealed class ConnectionForm : Form
    {
        private readonly TextBox _server = new() { Width = 300 };
        private readonly ComboBox _auth = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _login = new() { Width = 300 };
        private readonly TextBox _password = new() { Width = 300, UseSystemPasswordChar = true };
        private readonly CheckBox _remember = new() { Text = "Nhớ mật khẩu (mã hoá theo user Windows)", AutoSize = true };
        private readonly ComboBox _encrypt = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox _trustCert = new() { Text = "Trust server certificate", AutoSize = true };
        private readonly Button _btnConnect = new() { Text = "Kết nối", Width = 120, Height = 28 };
        private readonly Label _status = new() { AutoSize = true, ForeColor = SystemColors.GrayText };

        private readonly ComboBox _dbSales = DbCombo();
        private readonly ComboBox _dbTbsx = DbCombo();
        private readonly ComboBox _dbKhachHang = DbCombo();

        public DbConnectionInfo Info { get; private set; }
        public string Password { get; private set; } = "";
        public Dictionary<string, string> DbNames { get; } = new(StringComparer.OrdinalIgnoreCase);

        public ConnectionForm(DbConnectionInfo info, string? password, IReadOnlyDictionary<string, string> dbNames)
        {
            Info = info.Clone();
            BuildLayout();

            _auth.Items.AddRange(new object[] { "SQL Server Authentication", "Windows Authentication" });
            _encrypt.Items.AddRange(DbConnectionInfo.EncryptOptions);

            _server.Text = Info.Server;
            _auth.SelectedIndex = Info.WindowsAuth ? 1 : 0;
            _login.Text = Info.Login;
            _password.Text = password ?? "";
            _remember.Checked = Info.RememberPassword;
            _encrypt.SelectedItem = DbConnectionInfo.EncryptOptions.Contains(Info.Encrypt) ? Info.Encrypt : "Mandatory";
            _trustCert.Checked = Info.TrustServerCertificate;

            _dbSales.Text = Info.DatabaseName;
            _dbTbsx.Text = dbNames.TryGetValue("ThongBaoSX", out var tb) ? tb : "";
            _dbKhachHang.Text = dbNames.TryGetValue("KhachHang", out var kh) ? kh : "";

            _auth.SelectedIndexChanged += (_, _) => UpdateAuthFields();
            _btnConnect.Click += async (_, _) => await ConnectAsync();
            UpdateAuthFields();
        }

        private static ComboBox DbCombo() => new()
        {
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };

        private void BuildLayout()
        {
            Text = "Kết nối CSDL";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);

            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Location = new Point(12, 12)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            void Header(string text)
            {
                var l = new Label
                {
                    Text = text, AutoSize = true, Font = new Font(Font, FontStyle.Bold),
                    Margin = new Padding(0, grid.RowCount == 0 ? 0 : 12, 0, 4)
                };
                grid.Controls.Add(l, 0, grid.RowCount);
                grid.SetColumnSpan(l, 2);
                grid.RowCount++;
            }
            void Row(string label, Control c)
            {
                var l = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 6, 3, 3) };
                grid.Controls.Add(l, 0, grid.RowCount);
                grid.Controls.Add(c, 1, grid.RowCount);
                grid.RowCount++;
            }
            void Full(Control c)
            {
                grid.Controls.Add(c, 1, grid.RowCount);
                grid.RowCount++;
            }

            Header("Máy chủ");
            Row("Server name:", _server);
            Row("Authentication:", _auth);
            Row("Login:", _login);
            Row("Password:", _password);
            Full(_remember);

            Header("Bảo mật kết nối");
            Row("Encryption:", _encrypt);
            Full(_trustCert);

            var connectRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 8, 0, 0) };
            _status.Margin = new Padding(8, 7, 0, 0);
            connectRow.Controls.Add(_btnConnect);
            connectRow.Controls.Add(_status);
            Full(connectRow);

            Header("CSDL sử dụng");
            Row("CSDL chính (Sales):", _dbSales);
            Row("CSDL TBSX (MucDinhKem):", _dbTbsx);
            Row("CSDL Khách hàng:", _dbKhachHang);

            var btnOk = new Button { Text = "Lưu", Width = 90, Height = 28 };
            var btnCancel = new Button { Text = "Huỷ", Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
            btnOk.Click += (_, _) => Accept();
            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft, AutoSize = true,
                Anchor = AnchorStyles.Right, Margin = new Padding(0, 16, 0, 0)
            };
            buttons.Controls.Add(btnCancel);
            buttons.Controls.Add(btnOk);
            grid.Controls.Add(buttons, 0, grid.RowCount);
            grid.SetColumnSpan(buttons, 2);
            grid.RowCount++;

            Controls.Add(grid);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void UpdateAuthFields()
        {
            var sql = _auth.SelectedIndex == 0;
            _login.Enabled = _password.Enabled = _remember.Enabled = sql;
        }

        /// <summary>Gom giá trị trên form thành DbConnectionInfo (chưa có CSDL chính nếu chưa chọn).</summary>
        private DbConnectionInfo ReadInfo() => new()
        {
            Server = _server.Text.Trim(),
            WindowsAuth = _auth.SelectedIndex == 1,
            Login = _login.Text.Trim(),
            DatabaseName = _dbSales.Text.Trim(),
            Encrypt = _encrypt.SelectedItem as string ?? "Mandatory",
            TrustServerCertificate = _trustCert.Checked,
            RememberPassword = _remember.Checked
        };

        private async Task ConnectAsync()
        {
            var info = ReadInfo();
            if (info.Server.Length == 0) { _status.Text = "Chưa nhập Server name."; return; }

            _btnConnect.Enabled = false;
            _status.ForeColor = SystemColors.GrayText;
            _status.Text = "Đang kết nối...";
            Cursor = Cursors.WaitCursor;
            try
            {
                // Kết nối vào master để liệt kê CSDL - chưa chắc đã chọn CSDL chính.
                var cs = new SqlConnectionStringBuilder(info.Build(_password.Text, "master")) { ConnectTimeout = 10 };
                using var cn = new SqlConnection(cs.ConnectionString);
                await cn.OpenAsync();

                using var cmd = new SqlCommand(
                    "SELECT name FROM sys.databases WHERE database_id > 4 AND HAS_DBACCESS(name) = 1 ORDER BY name", cn);
                var names = new List<string>();
                using (var rd = await cmd.ExecuteReaderAsync())
                    while (await rd.ReadAsync()) names.Add(rd.GetString(0));

                foreach (var combo in new[] { _dbSales, _dbTbsx, _dbKhachHang })
                {
                    var current = combo.Text;
                    combo.Items.Clear();
                    combo.Items.AddRange(names.ToArray<object>());
                    combo.Text = current;
                }

                _status.ForeColor = Color.ForestGreen;
                _status.Text = $"Đã kết nối ({cn.ServerVersion}) - {names.Count} CSDL truy cập được.";
            }
            catch (Exception ex)
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "Không kết nối được.";
                MessageBox.Show(this, ex.Message, "Không kết nối được", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                _btnConnect.Enabled = true;
            }
        }

        private void Accept()
        {
            var info = ReadInfo();
            if (info.Server.Length == 0 || info.DatabaseName.Length == 0)
            {
                MessageBox.Show(this, "Cần nhập Server name và chọn CSDL chính (Sales).", "Thiếu thông tin",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!info.WindowsAuth && info.Login.Length == 0)
            {
                MessageBox.Show(this, "SQL Server Authentication cần Login.", "Thiếu thông tin",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Info = info;
            Password = info.WindowsAuth ? "" : _password.Text;
            DbNames["ThongBaoSX"] = _dbTbsx.Text.Trim();
            DbNames["KhachHang"] = _dbKhachHang.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
