using Microsoft.OpenApi.Models;

namespace Needlework.Net.Core;

public class LcuSchemaHandler
{
    internal OpenApiDocument OpenApiDocument { get; } 

    public SortedDictionary<string, OpenApiPathItem> Plugins { get; } = [];

    public OpenApiInfo Info => OpenApiDocument.Info;

    public LcuSchemaHandler(OpenApiDocument openApiDocument)
    {
        OpenApiDocument = openApiDocument;

        // Group paths by plugins
        foreach (var tag in OpenApiDocument.Tags)
        {
            foreach (var path in OpenApiDocument.Paths)
            {
                var containsTag = false;
                var sentinelTag = string.Empty;

                foreach (var operation in path.Value.Operations)
                {
                    foreach (var operationTag in operation.Value.Tags)
                    {
                        var lhs = tag.Name.Replace("Plugin ", string.Empty);
                        var rhs = operationTag.Name.Replace("Plugin ", string.Empty);
                    
                        if (lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase))
                        {
                            containsTag = true;
                            sentinelTag = lhs.ToLower();
                            break; // Break early since all operations in a path share the same tags
                        }
                    }

                    if (containsTag)
                        break; // Ditto
                }

                if (containsTag)
                    Plugins[sentinelTag] = path.Value;
            }
        }
    }
}