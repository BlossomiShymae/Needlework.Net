using Microsoft.OpenApi.Any;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.Desktop.ViewModels
{
    public class PropertyEnumViewModel
    {
        public string Type { get; } = "Enum";
        public string Values { get; }

        public PropertyEnumViewModel(IList<IOpenApiAny> enumValue)
        {
            Values = $"[{string.Join(", ", enumValue.Select(x => ((OpenApiString)x).Value).ToList())}]";
        }
    }
}