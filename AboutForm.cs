using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using TcclPdfTool.Github;

namespace TcclPdfTool
{
    public partial class AboutForm : Form
    {
        // Repo chứa các bản phát hành (GitHub Releases) của ứng dụng
        private const string GithubOwner = "AnhVo-01";
        private const string GithubRepo = "QCDigi";
        private static readonly string LatestReleaseUrl = $"https://api.github.com/repos/{GithubOwner}/{GithubRepo}/releases/latest";

        private static readonly HttpClient http = CreateHttpClient();

        private readonly Version currentVersion;
        private GithubAsset? latestInstaller;
        private CancellationTokenSource? downloadCts;

        public AboutForm()
        {
            InitializeComponent();

            currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
            appVersion.Text = currentVersion.ToString(3);

            btn_install.Visible = false;
            checkLoad.Visible = false;
            panel1.Visible = false;
            lblProgress.Visible = false;
            progressInstall.Visible = false;

            FormClosing += (_, _) => downloadCts?.Cancel();
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            // GitHub API bắt buộc có User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(GithubRepo, "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            btn_install.Visible = false;
            latestInstaller = null;
            checkLoad.Visible = true;
            ShowStatus("Đang kiểm tra cập nhật...");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var response = await http.GetAsync(LatestReleaseUrl, cts.Token);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ShowStatus("Chưa có bản phát hành nào.");
                    return;
                }
                if (!response.IsSuccessStatusCode)
                {
                    ShowStatus($"Không thể kiểm tra cập nhật (HTTP {(int)response.StatusCode}).");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync(cts.Token);
                var release = JsonSerializer.Deserialize<GithubRelease>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (release == null || !TryParseVersion(release.TagName, out var latest))
                {
                    ShowStatus("Không đọc được thông tin phiên bản trên GitHub.");
                    return;
                }

                if (latest <= Normalize(currentVersion))
                {
                    ShowStatus($"Bạn đang dùng phiên bản mới nhất ({currentVersion.ToString(3)}).");
                    return;
                }

                latestInstaller = release.Assets?.FirstOrDefault(a =>
                    a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

                if (latestInstaller == null)
                {
                    ShowStatus($"Đã có phiên bản mới {latest.ToString(3)} nhưng không tìm thấy file cài đặt.");
                    return;
                }

                ShowStatus($"Đã có phiên bản mới: {latest.ToString(3)}. Nhấn \"Cài đặt\" để cập nhật.");
                btn_install.Visible = true;
            }
            catch (OperationCanceledException)
            {
                ShowStatus("Hết thời gian chờ khi kiểm tra cập nhật.");
            }
            catch (Exception ex)
            {
                ShowStatus($"Không thể kiểm tra cập nhật: {ex.Message}");
            }
            finally
            {
                checkLoad.Visible = false;
                button1.Enabled = true;
            }
        }

        private void ShowStatus(string text)
        {
            panel1.Visible = true;
            lblProgress.Visible = true;
            lblProgress.Text = text;
        }

        // Tag dạng "v1.2.3", "1.2.3" hoặc "v1.2.3-beta"
        private static bool TryParseVersion(string? tag, out Version version)
        {
            version = new Version();
            if (string.IsNullOrWhiteSpace(tag)) return false;

            var clean = tag.Trim().TrimStart('v', 'V');
            var suffix = clean.IndexOfAny(new[] { '-', '+', ' ' });
            if (suffix >= 0) clean = clean[..suffix];

            if (!Version.TryParse(clean, out var parsed)) return false;
            version = Normalize(parsed);
            return true;
        }

        // Đưa về đủ 4 phần để "1.2.3" và "1.2.3.0" được coi là bằng nhau
        private static Version Normalize(Version v) =>
            new(v.Major, v.Minor, Math.Max(v.Build, 0), Math.Max(v.Revision, 0));

        private async void btn_install_Click(object sender, EventArgs e)
        {
            // Đang tải -> nút đóng vai trò "Huỷ"
            if (downloadCts != null)
            {
                downloadCts.Cancel();
                btn_install.Enabled = false;
                return;
            }

            if (latestInstaller == null)
            {
                MessageBox.Show(this, "Không tìm thấy file cài đặt.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tempPath = Path.Combine(Path.GetTempPath(), latestInstaller.Name);
            downloadCts = new CancellationTokenSource();
            var token = downloadCts.Token;
            bool completed = false;

            button1.Enabled = false;
            btn_install.Text = "Huỷ";
            progressInstall.Value = 0;
            progressInstall.Visible = true;
            ShowStatus("Đang tải...");

            try
            {
                using (var response = await http.GetAsync(latestInstaller.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes > 0;
                    progressInstall.Style = canReportProgress ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;

                    using var contentStream = await response.Content.ReadAsStreamAsync(token);
                    using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);

                    var buffer = new byte[81920];
                    long totalRead = 0;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read), token);
                        totalRead += read;

                        if (canReportProgress)
                        {
                            int percent = (int)Math.Min(totalRead * 100 / totalBytes, 100);
                            progressInstall.Value = percent;
                            lblProgress.Text = $"Đang tải... {FormatBytes(totalRead)} / {FormatBytes(totalBytes)} ({percent}%)";
                        }
                        else
                        {
                            lblProgress.Text = $"Đã tải: {FormatBytes(totalRead)}";
                        }
                    }
                }

                completed = true;

                // Tải xong -> chạy bộ cài rồi thoát ứng dụng để bộ cài ghi đè file
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });

                Application.Exit();
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed) ShowStatus("Đã huỷ tải bản cập nhật.");
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus($"Lỗi khi tải: {ex.Message}");
            }
            finally
            {
                downloadCts.Dispose();
                downloadCts = null;

                if (!completed) TryDelete(tempPath);

                if (!IsDisposed)
                {
                    btn_install.Text = "Cài đặt";
                    btn_install.Enabled = true;
                    button1.Enabled = true;
                    progressInstall.Visible = false;
                }
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* file tạm, bỏ qua */ }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F2} GB";
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} B";
        }
    }
}
