using System.Text.Json.Serialization;

namespace Needlework.Net.Models
{
    public class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        public bool IsLatest(int version) => int.Parse(TagName.Replace(".", "")) > version;
    }
}
