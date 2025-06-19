using FastCache;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.OpenApi.Readers;
using Needlework.Net.Constants;
using Needlework.Net.Extensions;
using Needlework.Net.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Needlework.Net
{
    public class DocumentService : IEnableLogger
    {
        private readonly OpenApiStreamReader _reader = new();

        private readonly IFlurlClient _githubUserContentClient;

        public DocumentService(IFlurlClientCache clients)
        {
            _githubUserContentClient = clients.Get(FlurlClientKeys.GithubUserContentClient);
        }

        public async Task<Document> GetLcuSchemaDocumentAsync(CancellationToken cancellationToken = default)
        {
            if (Cached<Document>.TryGet(nameof(GetLcuSchemaDocumentAsync), out var cached))
            {
                return cached;
            }

            var lcuSchemaStream = await _githubUserContentClient.Request("/dysolix/hasagi-types/main/swagger.json")
                .GetStreamAsync(cancellationToken: cancellationToken);
            var lcuSchemaRaw = _reader.Read(lcuSchemaStream, out var diagnostic);
            foreach (var error in diagnostic.Errors)
            {
                this.Log()
                    .Warning("Diagnostic error: {Message}", error);
            }
            var document = new Document(lcuSchemaRaw);

            return cached.Save(document, TimeSpan.FromMinutes(60));
        }

        public async Task<Document> GetLolClientDocumentAsync(CancellationToken cancellationToken = default)
        {
            if (Cached<Document>.TryGet(nameof(GetLolClientDocumentAsync), out var cached))
            {
                return cached;
            }

            var lolClientStream = await _githubUserContentClient.Request("/AlsoSylv/Irelia/refs/heads/master/schemas/game_schema.json")
                .GetStreamAsync(cancellationToken: cancellationToken);
            var lolClientRaw = _reader.Read(lolClientStream, out var diagnostic);
            foreach (var error in diagnostic.Errors)
            {
                this.Log()
                    .Warning("Diagnostic error: {Message}", error);
            }
            var document = new Document(lolClientRaw);

            return cached.Save(document, TimeSpan.FromMinutes(60));
        }
    }
}
