using Microsoft.OpenApi.Models;

namespace Needlework.Net.Models;

public record PathOperation(string Method, string Path, string Tag, OpenApiOperation Operation);