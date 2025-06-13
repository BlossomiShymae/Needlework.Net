using System.Text.Json.Serialization;

namespace Needlework.Net.Models
{
    public class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        public bool IsLatest(string assemblyVersion) => int.Parse(TagName.Replace(".", string.Empty)) > int.Parse(assemblyVersion.Replace(".", string.Empty));
    }
}
