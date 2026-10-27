using TcclPdfTool.Config;
using TcclPdfTool.DBContext;

namespace TcclPdfTool
{
    public partial class SettingsForm : Form
    {
        private readonly AppConfig _cfg;

        // Thông tin kết nối đang sửa - chỉ ghi vào cấu hình/file mật khẩu khi bấm Lưu.
        private DbConnectionInfo _conn;
        private string? _password;
        private readonly Dictionary<string, string> _dbNames;

        private readonly List<KeyValuePair<int, string>> list =
        [
            new KeyValuePair<int, string>(1, "AAA"),
            new KeyValuePair<int, string>(2, "APH")
        ];

        public SettingsForm(AppConfig cfg)
        {
            _cfg = cfg;
            _conn = cfg.Database.Clone();
            _password = CredentialStore.GetPassword();
            _dbNames = new Dictionary<string, string>(cfg.SameServerDbNames, StringComparer.OrdinalIgnoreCase);
            InitializeComponent();
            _numCongTy.DataSource = list;
            _numCongTy.DisplayMember = "Value";
            _numCongTy.ValueMember = "Key";
            LoadValues();
        }

        private void LoadValues()
        {
            ShowConnectionSummary();
            _txtFileRoot.Text = _cfg.FileRoot;
            _txtLicense.Text = _cfg.AsposeLicensePath;
            _txtOutput.Text = _cfg.OutputFolder;
            _numMaxRows.Value = Math.Clamp(_cfg.MaxRows, (int)_numMaxRows.Minimum, (int)_numMaxRows.Maximum);
            //_numCongTy.Value = Math.Max(1, _cfg.CongTyId);
            //_cboWriteMode.SelectedIndex = _cfg.WriteMode == WriteMode.Staging ? 0 : 1;

            _chkLlm.Checked = _cfg.Llm.Enabled;
            _txtEndpoint.Text = _cfg.Llm.Endpoint;
            _txtApiKey.Text = _cfg.Llm.ApiKey;
            _txtModel.Text = _cfg.Llm.Model;
        }

        private async void TestConnectionAsync(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            try
            {
                var targets = new DbTargets
                {
                    SalesConnectionString = _conn.Build(_password),
                    SameServerDbNames = new Dictionary<string, string>(_dbNames, StringComparer.OrdinalIgnoreCase),
                    OtherServerConnections = new Dictionary<string, string>(
                    _cfg.OtherServerConnections, StringComparer.OrdinalIgnoreCase)
                };
                var repo = new Repository(targets);
                var info = await repo.TestConnectionAsync();
                MessageBox.Show(this, "Kết nối thành công: " + info, "Kiểm tra kết nối",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Không kết nối được",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { btnTest.Enabled = true; }
        }

        private void saveSetting_Click(object sender, EventArgs e)
        {
            _cfg.Database = _conn;
            foreach (var kv in _dbNames) _cfg.SameServerDbNames[kv.Key] = kv.Value;
            CredentialStore.SetPassword(_conn.WindowsAuth ? null : _password, _conn.RememberPassword && !_conn.WindowsAuth);
            _cfg.FileRoot = _txtFileRoot.Text.Trim();
            _cfg.AsposeLicensePath = _txtLicense.Text.Trim();
            _cfg.OutputFolder = _txtOutput.Text.Trim();
            _cfg.MaxRows = (int)_numMaxRows.Value;
            //_cfg.CongTyId = (int)_numCongTy.Value;
            //_cfg.WriteMode = _cboWriteMode.SelectedIndex == 0 ? WriteMode.Staging : WriteMode.Draft;

            //_cfg.Llm.Enabled = _chkLlm.Checked;
            //_cfg.Llm.Endpoint = _txtEndpoint.Text.Trim();
            //_cfg.Llm.ApiKey = _txtApiKey.Text.Trim();
            //_cfg.Llm.Model = _txtModel.Text.Trim();

            _cfg.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ConnSetup_Click(object sender, EventArgs e)
        {
            using var f = new ConnectionForm(_conn, _password, _dbNames);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            _conn = f.Info;
            _password = f.Password;
            foreach (var kv in f.DbNames) _dbNames[kv.Key] = kv.Value;
            ShowConnectionSummary();
        }

        private void ShowConnectionSummary()
        {
            _txtConnInfo.Text = string.IsNullOrWhiteSpace(_conn.Server)
                ? "(chưa thiết lập)"
                : $"{_conn.Server} - {(_conn.WindowsAuth ? "Windows Authentication" : _conn.Login)}" +
                  $" - Encryption {_conn.Encrypt}{(_conn.TrustServerCertificate ? ", trust cert" : "")}";
            string Db(string key) => _dbNames.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : "(trống)";
            _txtDbInfo.Text = $"Sales: {_conn.DatabaseName}   |   TBSX: {Db("ThongBaoSX")}   |   Khách hàng: {Db("KhachHang")}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using var d = new FolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(_txtFileRoot.Text)) d.SelectedPath = _txtFileRoot.Text;
            if (d.ShowDialog(this) == DialogResult.OK) _txtFileRoot.Text = d.SelectedPath;
        }

        private void BrowseFileButton_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog { Filter = "License|*.lic;*.xml" };
            if (d.ShowDialog(this) == DialogResult.OK) _txtLicense.Text = d.FileName;
        }

        private void OutputBtn_Click(object sender, EventArgs e)
        {
            using var d = new FolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(_txtOutput.Text)) d.SelectedPath = _txtOutput.Text;
            if (d.ShowDialog(this) == DialogResult.OK) _txtOutput.Text = d.SelectedPath;
        }
    }
}
