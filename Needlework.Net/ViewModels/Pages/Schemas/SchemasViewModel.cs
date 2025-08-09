using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DebounceThrottle;
using Needlework.Net.Extensions;
using Needlework.Net.Helpers;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Schemas
{
    public partial class SchemasViewModel : PageBase, IEnableLogger
    {
        private readonly DebounceDispatcher _debounceDispatcher = new(TimeSpan.FromMilliseconds(500));

        private readonly DocumentService _documentService;

        private readonly SchemaPaneService _schemaPaneService;

        private readonly NotificationService _notificationService;

        private List<SchemaSearchDetailsViewModel> _schemas = [];

        public SchemasViewModel(DocumentService documentService, SchemaPaneService schemaPaneService, NotificationService notificationService) : base("Schemas", "fa-solid fa-file-lines")
        {
            _documentService = documentService;
            _schemaPaneService = schemaPaneService;
            _notificationService = notificationService;
        }

        [ObservableProperty]
        private bool _isBusy = true;

        [ObservableProperty]
        private string? _search;

        [ObservableProperty]
        private List<SchemaSearchDetailsViewModel> _schemaItems = [];

        [ObservableProperty]
        private Vector _offset = new();

        partial void OnSearchChanged(string? value)
        {
            _debounceDispatcher.Debounce(() =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        SchemaItems = _schemas.ToList();
                    });
                    return;
                }
                var items = _schemas.Where(schema => schema.Id.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Dispatcher.UIThread.Invoke(() => { SchemaItems = items; });
            });
        }

        public override async Task InitializeAsync()
        {
            try
            {
                var lcuSchemaDocument = await _documentService.GetLcuSchemaDocumentAsync();
                var lolClientDocument = await _documentService.GetLolClientDocumentAsync();
                Dispatcher.UIThread.Invoke(() =>
                {
                    var schemas = Enumerable.Concat(
                        lcuSchemaDocument.OpenApiDocument.Components.Schemas.Values.Select(schema => new SchemaSearchDetailsViewModel(Tab.LCU, OpenApiHelpers.WalkSchema(schema, lcuSchemaDocument.OpenApiDocument), _schemaPaneService)),
                        lolClientDocument.OpenApiDocument.Components.Schemas.Values.Select(schema => new SchemaSearchDetailsViewModel(Tab.GameClient, OpenApiHelpers.WalkSchema(schema, lolClientDocument.OpenApiDocument), _schemaPaneService))
                        ).ToList();
                    _schemas = schemas;
                    SchemaItems = schemas.ToList();
                    IsBusy = false;
                });
            }
            catch (Exception ex)
            {
                this.Log()
                    .Error(ex, "Failed to load schemas.");
                _notificationService.Notify("Schemas", ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error);
            }
        }
    }
}
