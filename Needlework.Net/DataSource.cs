using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;
using Needlework.Net.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net
{
    public class DataSource
    {
        private readonly ILogger<DataSource> _logger;
        private readonly HttpClient _httpClient;
        private Document? _lcuSchemaDocument;
        private Document? _lolClientDocument;
        private readonly TaskCompletionSource<bool> _taskCompletionSource = new();


        public DataSource(HttpClient httpClient, ILogger<DataSource> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Document> GetLcuSchemaDocumentAsync()
        {
            await _taskCompletionSource.Task;
            return _lcuSchemaDocument ?? throw new InvalidOperationException();
        }

        public async Task<Document> GetLolClientDocumentAsync()
        {
            await _taskCompletionSource.Task;
            return _lolClientDocument ?? throw new InvalidOperationException();
        }

        public async Task InitializeAsync()
        {
            try
            {
                var reader = new OpenApiStreamReader();
                var lcuSchemaStream = await _httpClient.GetStreamAsync("https://raw.githubusercontent.com/dysolix/hasagi-types/main/swagger.json");
                var lcuSchemaRaw = reader.Read(lcuSchemaStream, out var _);
                _lcuSchemaDocument = new Document(lcuSchemaRaw);

                var lolClientStream = await _httpClient.GetStreamAsync("https://raw.githubusercontent.com/BlossomiShymae/poroschema/refs/heads/main/schemas/lolclient.json");
                var lolClientRaw = reader.Read(lolClientStream, out var _);
                _lolClientDocument = new Document(lolClientRaw);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize DataSource");
            }
            finally
            {
                _taskCompletionSource.SetResult(true);
            }
        }
    }
}
