using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Needlework.Net.Models;

public class OpenApiDocumentWrapper
{
    internal OpenApiDocument OpenApiDocument { get; }

    public SortedDictionary<string, List<PathOperation>> Plugins { get; }

    public OpenApiInfo Info => OpenApiDocument.Info;

    public List<string> Paths => [.. OpenApiDocument.Paths.Keys];

    public OpenApiDocumentWrapper(OpenApiDocument openApiDocument)
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
                        p.Add(new(method.ToString(), path, pluginsKey, operation));
                    else
                    {
                        operations.Add(new(method.ToString(), path, pluginsKey, operation));
                        plugins[pluginsKey] = operations;
                    }
                }
                else
                {
                    foreach (var tag in operation.Tags)
                    {
                        if (tag.Name == "plugins")
                            continue;
                        else
                            pluginsKey = tag.Name;

                        if (plugins.TryGetValue(pluginsKey, out var p))
                            p.Add(new(method.ToString(), path, tag.Name, operation));
                        else
                        {
                            operations.Add(new(method.ToString(), path, tag.Name, operation));
                            plugins[pluginsKey] = operations;
                        }
                    }
                }
            }
        }

        plugins = new(plugins.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(x => x.Path).ToList()));

        Plugins = plugins;
    }
}
