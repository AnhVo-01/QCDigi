using System.Text.Json.Serialization;

namespace TcclPdfTool.Github
{
    public class GithubAsset
    {
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}
