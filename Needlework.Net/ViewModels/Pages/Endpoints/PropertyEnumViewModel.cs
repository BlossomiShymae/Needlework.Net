using Microsoft.OpenApi.Any;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public class PropertyEnumViewModel
{
    public PropertyEnumViewModel(IList<IOpenApiAny> enumValue)
    {
        Values = $"[{string.Join(", ", enumValue.Select(x => $"\"{((OpenApiString)x).Value}\"").ToList())}]";
    }

    public string Type { get; } = "Enum";

    public string Values { get; }

}