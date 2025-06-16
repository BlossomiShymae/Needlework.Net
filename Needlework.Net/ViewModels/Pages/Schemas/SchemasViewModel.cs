using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DebounceThrottle;
using Needlework.Net.Helpers;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Schemas
{
    public partial class SchemasViewModel : PageBase
    {
        private readonly DebounceDispatcher _debounceDispatcher = new(TimeSpan.FromMilliseconds(500));

        private readonly DocumentService _documentService;

        public record SchemaTab(string Key, Tab Tab);

        private List<SchemaTab> _schemas = [];

        public SchemasViewModel(DocumentService documentService) : base("Schemas", "fa-solid fa-file-lines")
        {
            _documentService = documentService;
        }

        [ObservableProperty]
        private bool _isBusy = true;

        [ObservableProperty]
        private string? _search;

        [ObservableProperty]
        private List<SchemaItemViewModel> _schemaItems = [];

        partial void OnSearchChanged(string? value)
        {
            _debounceDispatcher.Debounce(() =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    SchemaItems = [];
                    return;
                }

                Task.Run(async () =>
                {
                    var lcuSchemaDocument = await _documentService.GetLcuSchemaDocumentAsync();
                    var lolClientDocument = await _documentService.GetLolClientDocumentAsync();
                    var items = _schemas.Where(schema => schema.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                        .Select((schema) => ToSchemaItemViewModel(schema, lcuSchemaDocument, lolClientDocument))
                        .ToList();
                    Dispatcher.UIThread.Invoke(() => { SchemaItems = items; });
                });
            });
        }

        private SchemaItemViewModel ToSchemaItemViewModel(SchemaTab schema, Document lcuSchemaDocument, Document lolClientDocument)
        {
            var document = schema.Tab switch
            {
                Tab.LCU => lcuSchemaDocument.OpenApiDocument,
                Tab.GameClient => lolClientDocument.OpenApiDocument,
                _ => throw new NotImplementedException()
            };
            var vm = OpenApiHelpers.WalkSchema(document.Components.Schemas[schema.Key], document);
            return new SchemaItemViewModel(vm);

        }

        public override async Task InitializeAsync()
        {
            var lcuSchemaDocument = await _documentService.GetLcuSchemaDocumentAsync();
            var lolClientDocument = await _documentService.GetLolClientDocumentAsync();
            Dispatcher.UIThread.Invoke(() =>
            {
                _schemas = Enumerable.Concat(
                    lcuSchemaDocument.OpenApiDocument.Components.Schemas.Keys.Select(key => new SchemaTab(key, Tab.LCU)),
                    lolClientDocument.OpenApiDocument.Components.Schemas.Keys.Select(key => new SchemaTab(key, Tab.GameClient))
                    ).ToList();
                IsBusy = false;
            });
        }
    }
}
