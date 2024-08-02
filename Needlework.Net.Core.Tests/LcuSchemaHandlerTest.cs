using Xunit.Abstractions;

namespace Needlework.Net.Core.Tests;

public class LcuSchemaHandlerTest
{
    private readonly ITestOutputHelper _output;

    internal HttpClient HttpClient { get; } = new();

    public LcuSchemaHandlerTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task PluginsTestAsync()
    {
        var reader = new LcuSchemaHandler(await Resources.GetOpenApiDocumentAsync(HttpClient));

        var plugins = reader.Plugins.Keys.ToList();
        foreach (var plugin in plugins)
            _output.WriteLine($"Plugin: {plugin}");

        Assert.True(plugins.Count > 0);
    }
}