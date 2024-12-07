namespace Needlework.Net.Models;

public class Library
{
    public required string Repo { get; init; }
    public string? Description { get; init; }
    public required string Language { get; init; }
    public required string Link { get; init; }
}
