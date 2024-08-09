using Microsoft.OpenApi.Models;

namespace Needlework.Net.Core;

public class LcuSchemaHandler
{
    internal OpenApiDocument OpenApiDocument { get; }

    public SortedDictionary<string, List<PathOperation>> Plugins { get; }

    public OpenApiInfo Info => OpenApiDocument.Info;

    public List<string> Paths => [.. OpenApiDocument.Paths.Keys];

    public LcuSchemaHandler(OpenApiDocument openApiDocument)
    {
        OpenApiDocument = openApiDocument;
        var plugins = new SortedDictionary<string, List<PathOperation>>();

        foreach ((var path, var pathItem) in openApiDocument.Paths)
        {
            foreach ((var method, var operation) in pathItem.Operations)
            {
                var operations = new List<PathOperation>();
                var pluginsKey = "_unknown";

                // Process and group endpoints into the following formats:
                // "_unknown" - group that should not be possible
                // "default" - no tags
                // "builtin" - 'builtin' not associated with an endpoint
                // "lol-summoner" etc. - 'plugin' associated with an endpoint
                // "performance", "tracing", etc.
                if (operation.Tags.Count == 0)
                {
                    pluginsKey = "default";
                    if (plugins.TryGetValue(pluginsKey, out var p))
                        p.Add(new(method.ToString(), path, operation));
                    else
                    {
                        operations.Add(new(method.ToString(), path, operation));
                        plugins[pluginsKey] = operations;
                    }
                }
                else
                {
                    foreach (var tag in operation.Tags)
                    {
                        var lowercaseTag = tag.Name.ToLower();
                        if (lowercaseTag == "plugins")
                            continue;
                        else if (lowercaseTag.Contains("plugin "))
                            pluginsKey = lowercaseTag.Replace("plugin ", "");
                        else
                            pluginsKey = lowercaseTag;

                        if (plugins.TryGetValue(pluginsKey, out var p))
                            p.Add(new(method.ToString(), path, operation));
                        else
                        {
                            operations.Add(new(method.ToString(), path, operation));
                            plugins[pluginsKey] = operations;
                        }
                    }
                }
            }
        }

        Plugins = plugins;
    }
}

public record PathOperation(string Method, string Path, OpenApiOperation Operation);