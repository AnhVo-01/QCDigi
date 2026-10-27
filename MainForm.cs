using System.Globalization;
using System.Text;
using TcclPdfTool.Config;
using TcclPdfTool.Core;
using TcclPdfTool.DBContext;
using TcclPdfTool.Model;

namespace TcclPdfTool
{
    public partial class MainForm : Form
    {
        private AppConfig _cfg = AppConfig.Load();
        private MappingConfig _mapping = new();
        private CancellationTokenSource? _cts;


        public MainForm()
        {
            InitializeComponent();

            _mapping = MappingConfig.Load(_cfg.MappingPath);
            Log($"Sẵn sàng. Alias đang có: {_mapping.Aliases.Count}. Cấu hình: {AppConfig.DefaultPath}");

            jobRows.CellMouseClick += JobRows_CellMouseClick;
            jobRows.CellDoubleClick += JobRows_CellDoubleClick;
            Shown += (_, _) => EnsureDbPassword();
        }

        /// <summary>
        /// SQL Server Authentication mà không nhớ mật khẩu thì hỏi lại ngay khi mở app,
        /// thay vì để lần truy vấn đầu tiên báo lỗi đăng nhập.
        /// </summary>
        private void EnsureDbPassword()
        {
            if (string.IsNullOrWhiteSpace(_cfg.Database.Server))
            {
                Log("Chưa thiết lập kết nối CSDL - vào Cấu hình > Thiết lập... để nhập.");
                return;
            }
            if (!_cfg.Database.NeedsPassword) return;

            using var f = new ConnectionForm(_cfg.Database, null, _cfg.SameServerDbNames);
            if (f.ShowDialog(this) != DialogResult.OK)
            {
                Log("Chưa nhập mật khẩu CSDL - các thao tác với CSDL sẽ không kết nối được.");
                return;
            }

            _cfg.Database = f.Info;
            foreach (var kv in f.DbNames) _cfg.SameServerDbNames[kv.Key] = kv.Value;
            CredentialStore.SetPassword(f.Info.WindowsAuth ? null : f.Password, f.Info.RememberPassword && !f.Info.WindowsAuth);
            _cfg.Save();
        }

        private void JobRows_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (jobRows.Rows[e.RowIndex].Tag is JobRow row) OpenDetail(row);
        }

        /// <summary>Bấm vào header cột "Chon" để tick/bỏ tick toàn bộ các dòng.</summary>
        private void JobRows_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1 || e.ColumnIndex != Chon.Index) return;

            jobRows.EndEdit();

            var dataRows = jobRows.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            bool newValue = dataRows.Any(r => r.Cells[Chon.Index].Value is not bool b || !b);

            foreach (var r in dataRows)
            {
                r.Cells[Chon.Index].Value = newValue;
                if (r.Tag is JobRow jr) jr.Chon = newValue;
            }

            jobRows.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Maximize the window automatically on load
            this.WindowState = FormWindowState.Maximized;

            _progress.Visible = false;
            _btnRun.Enabled = false; // Disable the Run button initially
            _btnStop.Enabled = false; // Disable the Stop button initially
        }

        // ------------------------------------------------------------------ hành động

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Select file PDF"
            };
            if (d.ShowDialog(this) == DialogResult.OK) txtOpen.Text = d.FileName;
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            await LoadListAsync(null);
        }

        private void txtDonHang_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;   // chặn tiếng "bíp" của WinForms
            btnLoadByOrder.PerformClick();
        }

        private async void btnLoadByOrder_Click(object sender, EventArgs e)
        {
            var codes = ParseOrderCodes(txtDonHang.Text);
            if (codes.Count == 0)
            {
                MessageBox.Show(this,
                    "Hãy nhập ít nhất một mã XXTH.\r\nNhiều mã thì cách nhau bởi dấu phẩy - dán được cả cột từ Excel.",
                    "Nạp theo đơn hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtDonHang.Focus();
                return;
            }
            await LoadListAsync(codes);
        }

        private static List<string> ParseOrderCodes(string? raw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in raw.Split([',', ';', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
            {
                var code = part.Trim();
                if (code.Length > 0 && seen.Add(code)) result.Add(code);
            }
            return result;
        }

        private async Task LoadListAsync(IReadOnlyList<string>? maXxth)
        {
            var theoDon = maXxth is { Count: > 0 };
            try
            {
                Busy(true, theoDon ? "Đang nạp theo đơn hàng..." : "Đang nạp danh sách file...");
                var repo = new Repository(_cfg.ToDbTargets());

                if (!await repo.HasXxthColumnAsync())
                {
                    Log("Bảng TieuChiChatLuong chưa có cột XXTHId - tiêu chí giờ gắn theo (XXTHId, SanPhamId), " +
                        "cần bổ sung cột này trước.");
                    return;
                }

                // Tra một đơn cụ thể thì hiện cả file đã import: giấu đi người dùng sẽ
                // tưởng đơn đó thiếu file, trong khi thực ra đã làm rồi.
                var chiFileChuaImport = !theoDon && _cfg.OnlyNotImported;

                var jobs = await repo.GetCandidateFilesAsync(
                    _cfg.CongTyId, _cfg.FileNameFilter, chiFileChuaImport,
                    _cfg.MaxRows, _cfg.MarketType, maXxth);

                jobRows.Rows.Clear();
                foreach (var j in jobs)
                {
                    var row = new JobRow(j);
                    var idx = jobRows.Rows.Add(row.ToCells());
                    jobRows.Rows[idx].Tag = row;   // giữ JobRow để lúc xử lý lấy lại FileJob
                }

                if (theoDon)
                {
                    Log($"Nạp theo đơn hàng ({maXxth!.Count} mã XXTH): {jobs.Count} file TCCL, kể cả file đã import.");

                    var khongThay = maXxth
                        .Where(m => !jobs.Any(j => string.Equals(j.MaXXTH, m, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                    if (khongThay.Count > 0)
                        Log($"  Không thấy file TCCL nào cho: {string.Join(", ", khongThay)}");

                    var daImport = jobs.Count(j => j.AlreadyImported);
                    if (daImport > 0)
                        Log($"  Trong đó {daImport} file đã có tiêu chí trong CSDL - chạy lại sẽ ghi đè bản nháp.");
                }
                else
                {
                    Log($"Nạp được {jobs.Count} file TCCL từ MucDinhKem (MarketType='{_cfg.MarketType}').");
                }
                txtOpen.Text = "Đã chọn " + jobs.Count + " tệp";

                var chuaLienKet = jobs.Count(j => !j.IsLinked);
                var thieuXxth = jobs.Count(j => j.XxthId is null);
                var thieuSp = jobs.Count(j => j.SanPhamId is null);
                var nhieuXxth = jobs.Count(j => j.MatchCount > 1);
                Log($"Nạp được {jobs.Count} file TCCL từ MucDinhKem (MarketType='TCCL').");

                if (chuaLienKet > 0)
                    Log($"  {chuaLienKet} file chưa gắn được XXTH (đứt chuỗi Market_TBSX -> XXThucHien).");
                if (thieuXxth > 0)
                    Log($"  {thieuXxth} file chưa gắn XXTH (không có Market_TBSX hợp lệ) - sẽ không ghi được.");
                if (thieuSp > 0)
                    Log($"  {thieuSp} file không có SanPhamId trong MucDinhKem - cần kiểm tra dữ liệu nguồn.");
                if (nhieuXxth > 0)
                    Log($"  {nhieuXxth} file gắn với nhiều XXTH - đang lấy XXTH mới nhất, cần kiểm tra lại.");
                if (_cfg.MaxRows > 0 && jobs.Count == _cfg.MaxRows)
                    Log($"(Đã chạm giới hạn {_cfg.MaxRows} dòng - tăng trong Cấu hình nếu cần.)");
            }
            catch (Exception ex)
            {
                Error(theoDon ? "Không nạp được theo đơn hàng" : "Không nạp được danh sách", ex);
            }
            finally
            {
                Busy(false, "");
                _btnRun.Enabled = jobRows.Rows.Count > 0; // Enable the Run button if there are jobs loaded
            }
        }


        private async void _btnRun_Click(object sender, EventArgs e)
        {
            _btnRun.Enabled = false; // Disable the Run button to prevent multiple clicks
            _btnStop.Enabled = true; // Enable the Stop button to allow cancellation
            jobRows.EndEdit(); // Ensure any edits in the DataGridView are committed

            foreach (DataGridViewRow gr in jobRows.Rows)
            {
                if (gr.IsNewRow || gr.Tag is not JobRow jr) continue;
                jr.Chon = gr.Cells["Chon"].Value is bool b && b;
            }

            var targets = jobRows.Rows.Cast<DataGridViewRow>()
                .Where(gr => !gr.IsNewRow && gr.Tag is JobRow)
                .Select(gr => (Grid: gr, Row: (JobRow)gr.Tag!))
                .Where(t => t.Row.Chon)
                .ToList();
            if (targets.Count == 0) { Log("Chưa chọn file nào."); return; }

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            try
            {
                Busy(true, "Đang xử lý...");
                _btnStop.Enabled = true;
                _progress.Visible = true;
                _progress.Maximum = targets.Count;
                _progress.Value = 0;

                var repo = new Repository(_cfg.ToDbTargets());

                // Thử DevExpress trước, không ra bảng mới rơi về Aspose.
                var extractor = FallbackTableExtractor.Create(_cfg.AsposeLicensePath, Log);
                if (string.IsNullOrWhiteSpace(_cfg.AsposeLicensePath))
                    Log("Chưa khai license Aspose - nếu phải rơi về Aspose thì nó chạy chế độ dùng thử.");

                ILabelLlmMapper llm = _cfg.Llm.Enabled
                    ? new OpenAiCompatibleMapper(_cfg.Llm)
                    : new NullLabelLlmMapper();

                // Nạp toàn bộ danh mục của công ty một lần (mọi FormType). Loại biểu mẫu chỉ
                // biết được sau khi đã mở file, nên lọc trước theo FormType sẽ thành bài toán con gà - quả trứng.
                var catalog = await repo.GetCatalogAsync(_cfg.CongTyId, ct);
                if (catalog.Count == 0)
                {
                    Log("CẢNH BÁO: danh mục DM_TieuChi của công ty này rỗng - sẽ không map được tiêu chí nào.");
                }
                var matcher = new LabelMatcher(catalog, _mapping);
                Log($"Nạp danh mục: {catalog.Count} tiêu chí (CongTyId={_cfg.CongTyId}).");

                int done = 0, ok = 0, failed = 0;
                foreach (var (gridRow, row) in targets)
                {
                    ct.ThrowIfCancellationRequested();

                    var source = _cfg.ResolveLocalPath(row.Job.RelativePath);
                    string? tempFile = null;

                    try
                    {
                        string local;
                        if (_cfg.IsRemoteFileRoot)
                        {
                            tempFile = await DownloadToTempFileAsync(source, ct);
                            if (tempFile is null)
                            {
                                row.SetResult(new ImportResult
                                {
                                    FileName = row.Job.FileName,
                                    SanPhamId = row.Job.SanPhamId,
                                    XxthId = row.Job.XxthId,
                                    MaSPNB = row.Job.MaSPNB,
                                    Failed = true,
                                    Error = "Không tải được file từ CDN: " + source
                                });
                                failed++; RefreshGridRow(gridRow, row); Step(ref done, targets.Count); continue;
                            }
                            local = tempFile;
                        }
                        else
                        {
                            if (!File.Exists(source))
                            {
                                row.SetResult(new ImportResult
                                {
                                    FileName = row.Job.FileName,
                                    SanPhamId = row.Job.SanPhamId,
                                    XxthId = row.Job.XxthId,
                                    MaSPNB = row.Job.MaSPNB,
                                    Failed = true,
                                    Error = "Không thấy file trên đĩa: " + source
                                });
                                failed++; RefreshGridRow(gridRow, row); Step(ref done, targets.Count); continue;
                            }
                            local = source;
                        }

                        var builder = new TieuChiDataBuilder(matcher, _mapping, llm);
                        var engine = new ImportEngine(extractor, builder);
                        var result = await engine.ProcessAsync(row.Job, local, ct);

                        row.SetResult(result);
                        if (result.Failed) failed++; else ok++;
                        RefreshGridRow(gridRow, row);

                        foreach (var w in result.Warnings.Where(w => w.Severity >= Severity.Warning))
                            Log($"  [{row.Job.FileName}] {w.Code}: {w.Message}");

                        Step(ref done, targets.Count);
                    }
                    finally
                    {
                        if (tempFile is not null)
                        {
                            try { File.Delete(tempFile); } catch { /* best-effort dọn file tạm */ }
                        }
                    }
                }

                (llm as IDisposable)?.Dispose();
                Log($"Xong: {ok} file đọc được, {failed} file lỗi.");
            }
            catch (OperationCanceledException) { Log("Đã dừng theo yêu cầu."); }
            catch (Exception ex) { Error("Lỗi khi xử lý", ex); }
            finally
            {
                _btnStop.Enabled = false;
                _progress.Visible = false;
                Busy(false, "");
                jobRows.Refresh();
            }
        }

        private void Step(ref int done, int total)
        {
            done++;
            _progress.Value = Math.Min(done, _progress.Maximum);
            _status.Text = $"Đã xử lý {done}/{total}";
            jobRows.Refresh();
            Application.DoEvents();
        }

        /// <summary>Đồng bộ nội dung + màu nền của một dòng lưới theo trạng thái JobRow hiện tại.</summary>
        private static void RefreshGridRow(DataGridViewRow gridRow, JobRow row)
        {
            var cells = row.ToCells();
            for (int i = 0; i < cells.Length && i < gridRow.Cells.Count; i++)
                gridRow.Cells[i].Value = cells[i];

            gridRow.DefaultCellStyle.BackColor = GetRowColor(row);
        }

        private static readonly HttpClient _httpClient = new();

        /// <summary>Tải file PDF từ CDN về một file tạm, trả về đường dẫn tạm hoặc null nếu tải thất bại.</summary>
        private static async Task<string?> DownloadToTempFileAsync(string url, CancellationToken ct)
        {
            try
            {
                using var resp = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!resp.IsSuccessStatusCode) return null;

                var ext = Path.GetExtension(new Uri(url).LocalPath);
                if (string.IsNullOrEmpty(ext)) ext = ".pdf";
                var tempFile = Path.Combine(Path.GetTempPath(), $"tccl_{Guid.NewGuid():N}{ext}");

                await using (var fs = File.Create(tempFile))
                    await resp.Content.CopyToAsync(fs, ct);

                return tempFile;
            }
            catch
            {
                return null;
            }
        }

        private static Color GetRowColor(JobRow row)
        {
            if (row.Result?.Failed == true) return ColorTranslator.FromHtml("#FFE2E2");
            if (row.DaLuu) return ColorTranslator.FromHtml("#DFF5E2");
            if (row.Result is not null && (row.Result.NeedsReviewCount > 0 || row.Result.UnmappedRows.Count > 0))
                return ColorTranslator.FromHtml("#FFF7D6");
            return Color.White;
        }
        private void _btnStop_Click(object sender, EventArgs e)
        {
            _btnStop.Enabled = false;
            _btnRun.Enabled = true;
            _cts?.Cancel();
        }

        private async void _btnSave_Click(object sender, EventArgs e)
        {
            jobRows.EndEdit();

            // Đồng bộ ô tick về JobRow rồi lấy các dòng đã đọc được dữ liệu.
            var targets = GridJobs()
                .Where(t => t.Row.Chon && t.Row.CanSave)
                .ToList();
            if (targets.Count == 0) { Log("Không có bản ghi nào đủ điều kiện ghi."); return; }

            try
            {
                Busy(true, "Đang kiểm tra...");
                var repo = new Repository(_cfg.ToDbTargets());

                if (!await repo.HasXxthColumnAsync())
                {
                    Log("Bảng TieuChiChatLuong chưa có cột XXTHId - không ghi được.");
                    return;
                }

                // Dựng bản ghi giống hệt khi xuất CSV; file thiếu khoá (XXTHId, SanPhamId) thì chắc chắn không ghi được.
                var blocked = new List<(DataGridViewRow Grid, JobRow Row, string Reason)>();
                var writable = new List<(DataGridViewRow Grid, JobRow Row, TieuChiChatLuongRecord Rec)>();
                foreach (var (grid, row) in targets)
                {
                    var rec = ToRecord(row);
                    if (rec is null) blocked.Add((grid, row, MissingKeyReason(row) ?? "thiếu khoá"));
                    else writable.Add((grid, row, rec));
                }

                if (writable.Count == 0)
                {
                    Busy(false, "");
                    foreach (var b in blocked) Log($"  [{b.Row.Job.FileName}] bỏ qua: {b.Reason}");
                    Log("Không có file nào đủ khoá để ghi vào TieuChiChatLuong.");
                    return;
                }

                // Tra trước các bản ghi đã có cùng (SanPhamId, XXTHId) để hỏi có ghi đè không.
                var existing = await repo.GetExistingAsync(
                    writable.Select(w => (w.Rec.SanPhamId, w.Rec.XxthId)).ToList());
                var existingKeys = existing.Select(x => (x.SanPhamId, x.XxthId)).ToHashSet();
                Busy(false, "");

                var dupRows = writable.Where(w => existingKeys.Contains((w.Rec.SanPhamId, w.Rec.XxthId))).ToList();
                int newCount = writable.Count - dupRows.Count;
                bool overwrite = false;

                if (dupRows.Count == 0)
                {
                    var msg = $"Thêm mới {writable.Count} bản ghi vào TieuChiChatLuong?";
                    if (blocked.Count > 0)
                        msg += $"\n\n{blocked.Count} file thiếu khoá nhận dạng (sản phẩm/XXTH) sẽ bị bỏ qua.";
                    if (MessageBox.Show(this, msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                else
                {
                    var names = dupRows.Take(10).Select(w => "  - " + w.Row.Job.FileName).ToList();
                    if (dupRows.Count > 10) names.Add($"  ... và {dupRows.Count - 10} file khác");

                    var msg = $"{dupRows.Count} bản ghi đã tồn tại trong TieuChiChatLuong (cùng SanPhamId và XXTHId)";
                    msg += ":\n" + string.Join("\n", names);
                    if (newCount > 0) msg += $"\n\n{newCount} bản ghi chưa có sẽ được thêm mới.";
                    if (blocked.Count > 0)
                        msg += $"\n{blocked.Count} file thiếu khoá nhận dạng (sản phẩm/XXTH) sẽ bị bỏ qua.";
                    msg += "\n\nGhi đè các bản ghi đã tồn tại?" +
                           "\n  Yes: ghi đè" +
                           "\n  No: giữ nguyên bản đã có, chỉ thêm mới" +
                           "\n  Cancel: huỷ, không ghi gì";

                    var answer = MessageBox.Show(this, msg, "Bản ghi đã tồn tại",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (answer == DialogResult.Cancel) return;
                    overwrite = answer == DialogResult.Yes;

                    if (!overwrite && newCount == 0)
                    {
                        Log("Mọi bản ghi đều đã tồn tại và không ghi đè - không có gì để ghi.");
                        return;
                    }
                }

                Busy(true, "Đang ghi...");
                Directory.CreateDirectory(_cfg.OutputFolder);
                int inserted = 0, updated = 0, kept = 0, failed = 0;

                foreach (var b in blocked)
                {
                    b.Row.TrangThai = "Không ghi: " + b.Reason;
                    RefreshGridRow(b.Grid, b.Row);
                    Log($"  [{b.Row.Job.FileName}] bỏ qua: {b.Reason}");
                }

                foreach (var (gridRow, row, rec) in writable)
                {
                    bool exists = existingKeys.Contains((rec.SanPhamId, rec.XxthId));

                    // Luôn lưu một bản JSON ra đĩa để đối chiếu, kể cả khi ghi CSDL thất bại.
                    var safe = Path.GetFileNameWithoutExtension(row.Job.FileName);
                    File.WriteAllText(Path.Combine(_cfg.OutputFolder, safe + ".json"), rec.TieuChiData, Encoding.UTF8);

                    try
                    {
                        var id = await repo.SaveAsync(rec, overwrite);
                        if (id == 0)
                        {
                            row.TrangThai = "Đã tồn tại - không ghi đè";
                            kept++;
                        }
                        else
                        {
                            row.DaLuu = true;
                            row.TrangThai = exists ? $"Đã ghi đè CSDL (Id={id})" : $"Đã thêm vào CSDL (Id={id})";
                            if (exists) updated++; else inserted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        row.TrangThai = "Lỗi ghi: " + ex.Message;
                        Log($"  [{row.Job.FileName}] không ghi được: {ex.Message}");
                    }

                    RefreshGridRow(gridRow, row);
                    Application.DoEvents();
                }

                Log($"TieuChiChatLuong: thêm mới {inserted}, ghi đè {updated}" +
                    (kept > 0 ? $", giữ nguyên {kept} bản đã có" : "") +
                    (failed + blocked.Count > 0 ? $", bỏ qua/lỗi {failed + blocked.Count}" : "") +
                    $". JSON lưu tại: {_cfg.OutputFolder}");
            }
            catch (Exception ex)
            {
                Error("Lỗi khi ghi", ex);
            }
            finally
            {
                Busy(false, "");
                jobRows.Refresh();
            }
        }

        /// <summary>Các dòng lưới đang gắn JobRow, kèm đồng bộ trạng thái ô tick về JobRow.</summary>
        private List<(DataGridViewRow Grid, JobRow Row)> GridJobs()
        {
            var list = new List<(DataGridViewRow, JobRow)>();
            foreach (DataGridViewRow gr in jobRows.Rows)
            {
                if (gr.IsNewRow || gr.Tag is not JobRow jr) continue;
                jr.Chon = gr.Cells[Chon.Index].Value is bool b && b;
                list.Add((gr, jr));
            }
            return list;
        }

        /// <summary>
        /// Lý do một file không ghi được vào TieuChiChatLuong (null = đủ khoá).
        /// Khoá nhận dạng bản ghi là (XXTHId, SanPhamId) - đúng điều kiện của Repository.SaveDraftAsync.
        /// </summary>
        private static string? MissingKeyReason(JobRow row)
        {
            var r = row.Result!;
            if ((r.SanPhamId ?? row.Job.SanPhamId) is null) return "thiếu SanPhamId";
            if ((r.XxthId ?? row.Job.XxthId) is null) return "chưa gắn XXTH";
            return null;
        }

        private void _btnExport_Click(object sender, EventArgs e)
        {
            if (jobRows.Rows.Count == 0)
            {
                Log("Chưa có dữ liệu để xuất."); return;
            }

            jobRows.EndEdit();

            // Chỉ xuất các dòng đã tick và đã đọc được tiêu chí - đúng điều kiện như khi ghi CSDL.
            var targets = GridJobs().Where(t => t.Row.Chon && t.Row.CanSave).ToList();
            if (targets.Count == 0) { Log("Không có bản ghi nào đủ điều kiện xuất."); return; }

            using var d = new SaveFileDialog
            {
                Filter = "CSV (UTF-8)|*.csv",
                FileName = $"TieuChiChatLuong_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (d.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                // Ghi CSV trực tiếp, không qua Aspose.Cells - bản miễn phí chèn watermark/sheet cảnh báo.
                var sb = new StringBuilder();
                sb.AppendLine(CsvLine("SanPhamId", "XXTHId", "FormType", "FilePath", "MaxTolerance", "MinTolerance", "TieuChiData", "UpdateDate"));

                int exported = 0, skipped = 0;
                foreach (var (_, r) in targets)
                {
                    var rec = ToRecord(r);
                    if (rec is null)
                    {
                        skipped++;
                        Log($"  [{r.Job.FileName}] bỏ qua khi xuất: {MissingKeyReason(r)}");
                        continue;
                    }

                    sb.AppendLine(CsvLine(
                        rec.SanPhamId.ToString(CultureInfo.InvariantCulture),
                        rec.XxthId.ToString(CultureInfo.InvariantCulture),
                        rec.FormType,
                        rec.FilePath,
                        rec.MaxTolerance?.ToString(CultureInfo.InvariantCulture) ?? "",
                        rec.MinTolerance?.ToString(CultureInfo.InvariantCulture) ?? "",
                        rec.TieuChiData,
                        rec.UpdateDate?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? ""));
                    exported++;
                }

                // UTF-8 có BOM để Excel mở đúng tiếng Việt.
                File.WriteAllText(d.FileName, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                Log($"Đã xuất {exported} bản ghi TieuChiChatLuong" +
                    (skipped > 0 ? $", bỏ qua {skipped} file thiếu khoá" : "") + ": " + d.FileName);
            }
            catch (Exception ex)
            {
                Error("Lỗi khi xuất file", ex);
            }
        }

        /// <summary>Một dòng CSV theo RFC 4180: ô có dấu phẩy, nháy kép hoặc xuống dòng thì bọc nháy kép.</summary>
        private static string CsvLine(params string[] values)
            => string.Join(",", values.Select(v =>
                v.IndexOfAny(CsvSpecialChars) >= 0 ? "\"" + v.Replace("\"", "\"\"") + "\"" : v));

        private static readonly char[] CsvSpecialChars = { ',', '"', '\r', '\n' };

        /// <summary>
        /// Dựng bản ghi TieuChiChatLuong từ kết quả đọc file - dùng chung cho xuất CSV và ghi CSDL.
        /// Trả về null khi thiếu khoá (SanPhamId, XXTHId).
        /// </summary>
        private static TieuChiChatLuongRecord? ToRecord(JobRow r)
        {
            var res = r.Result!;
            var sanPhamId = res.SanPhamId ?? r.Job.SanPhamId;
            var xxthId = res.XxthId ?? r.Job.XxthId;
            if (sanPhamId is null || xxthId is null) return null;

            // Tính lại theo tiêu chí hiện tại - người soát có thể đã map/bỏ outerBagLabel.
            res.FormType = TieuChiDataBuilder.DetectFormType(res);
            var (minTol, maxTol) = CommonTolerance(res.Fields.Values);

            return new TieuChiChatLuongRecord
            {
                SanPhamId = sanPhamId.Value,
                XxthId = xxthId.Value,
                FormType = res.FormType,
                FilePath = TieuChiDataBuilder.TemplatePathFor(res.FormType),
                MaxTolerance = maxTol,
                MinTolerance = minTol,
                TieuChiData = TieuChiDataBuilder.ToStandardTieuChiDataJson(res.Fields),
                UpdateDate = r.Job.CreatedDate
            };
        }

        /// <summary>
        /// (MinTolerance, MaxTolerance) chung của mọi tiêu chí: chỉ trả về khi tất cả tiêu chí có
        /// cùng một cặp giá trị, ngược lại trả (null, null) để bỏ trống cột.
        /// </summary>
        private static (decimal? Min, decimal? Max) CommonTolerance(IEnumerable<TieuChiField> fields)
        {
            var pairs = fields.Select(f => (f.MinTolerance, f.MaxTolerance)).Distinct().ToList();
            return pairs.Count == 1 ? pairs[0] : (null, null);
        }

        private static bool _asposeCellsLicenseApplied;

        private void OpenDetail(JobRow row)
        {
            if (row.Result is null) { Log("File này chưa được xử lý."); return; }

            var catalog = new List<DmTieuChi>();
            try
            {
                var repo = new Repository(_cfg.ToDbTargets());
                // Dùng cùng danh mục như lúc chạy (mọi FormType): file có thể đã map sang tiêu chí
                // của FormType khác, lọc hẹp ở đây thì combo mất mã và ô hiện trống.
                catalog = repo.GetCatalogAsync(_cfg.CongTyId).GetAwaiter().GetResult();
                if (catalog.Count == 0)
                    Log($"Danh mục DM_TieuChi của CongTyId={_cfg.CongTyId} rỗng - combo tiêu chí sẽ không có lựa chọn.");
            }
            catch (Exception ex)
            {
                Log("Không nạp được danh mục để sửa tay: " + ex.Message);
            }

            using var f = new DetailForm(row, catalog, _mapping, _cfg);
            f.ShowDialog(this);

            // Lưới không bind dữ liệu - phải tự đẩy kết quả đã soát (số tiêu chí, trạng thái) lên dòng tương ứng.
            var gridRow = jobRows.Rows.Cast<DataGridViewRow>().FirstOrDefault(gr => ReferenceEquals(gr.Tag, row));
            if (gridRow is not null) RefreshGridRow(gridRow, row);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using var f = new SettingsForm(_cfg);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            _cfg = AppConfig.Load();
            _mapping = MappingConfig.Load(_cfg.MappingPath);
            Log("Đã lưu cấu hình.");
        }

        // ------------------------------------------------------------------ tiện ích

        private void Busy(bool busy, string status)
        {
            //btnLoad.Enabled = _btnRun.Enabled = _btnSave.Enabled = _btnExport.Enabled = btnSettings.Enabled = !busy;
            _status.Text = status;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            Application.DoEvents();
        }

        private void Log(string message)
        {
            var index = _log.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            _log.TopIndex = Math.Max(0, index - _log.ClientSize.Height / Math.Max(1, _log.ItemHeight) + 1);
        }

        private void Error(string title, Exception ex)
        {
            Log($"{title}: {ex.Message}");
            MessageBox.Show(this, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
