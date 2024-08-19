using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Needlework.Net.Models;

public static class Resources
{
    /// <summary>
    /// Get the OpenApi document of the LCU schema. Provided by dysolix. 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    public static async Task<OpenApiDocument> GetOpenApiDocumentAsync(HttpClient httpClient)
    {
        var stream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/dysolix/hasagi-types/main/swagger.json");
        
        var document = new OpenApiStreamReader().Read(stream, out var _);

        return document;
    }
}
