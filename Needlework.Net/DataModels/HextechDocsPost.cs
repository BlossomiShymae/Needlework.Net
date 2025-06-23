namespace Needlework.Net.DataModels
{
    public class HextechDocsPost
    {
        public required string Path { get; init; }

        public required string Title { get; init; }

        public required string Excerpt { get; init; }

        public string Url => $"https://hextechdocs.dev{Path}";
    }
}
