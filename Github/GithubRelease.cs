using System.Text.Json.Serialization;

namespace TcclPdfTool.Github
{
    public class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        public bool Prerelease { get; set; }

        public string Name { get; set; }

        public List<GithubAsset> Assets { get; set; }
    }
}
