using Xunit.Abstractions;

namespace Needlework.Net.Core.Tests;

public class ResourcesTest
{
    private readonly ITestOutputHelper _output;

    internal HttpClient HttpClient { get; } = new();

    public ResourcesTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DocumentTestAsync()
    {
        var document = await Resources.GetOpenApiDocumentAsync(HttpClient);

        Assert.True(document.Info.Title == "LCU SCHEMA");
    }
}