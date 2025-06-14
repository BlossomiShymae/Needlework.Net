using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Needlework.Net.Models;

public class Library
{
    [JsonPropertyName("repo")]
    public required string Repo { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("owner")]
    public required string Owner { get; init; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; init; } = [];

    public string Link
    {
        get
        {
            if (Owner.Equals("jellies")) return $"https://github.com/elliejs/{Repo}";
            return $"https://github.com/{Owner}/{Repo}";
        }
    }
}
