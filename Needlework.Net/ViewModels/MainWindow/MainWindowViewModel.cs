using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Constants;
using Needlework.Net.Extensions;
using Needlework.Net.Helpers;
using Needlework.Net.Messages;
using Needlework.Net.Services;
using Needlework.Net.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.MainWindow;

public partial class MainWindowViewModel
    : ObservableObject, IRecipient<OopsiesDialogRequestedMessage>, IEnableLogger
{
    private readonly DocumentService _documentService;

    private readonly NotificationService _notificationService;

    private readonly DialogService _dialogService;

    private readonly SchemaPaneService _schemaPaneService;

    public MainWindowViewModel(DialogService dialogService, DocumentService documentService, NotificationService notificationService, SchemaPaneService schemaPaneService)
    {
        _dialogService = dialogService;
        _documentService = documentService;
        _notificationService = notificationService;
        _schemaPaneService = schemaPaneService;

        _notificationService.Notifications.Subscribe(async notification =>
        {
            var vm = new NotificationViewModel(notification);
            Notifications.Add(vm);
            await Task.Delay(notification.Duration ?? TimeSpan.FromSeconds(10));
            Notifications.Remove(vm);
        });

        _schemaPaneService.SchemaPaneItems.Subscribe(async item =>
        {
            var document = item.Tab switch
            {
                Pages.Endpoints.Tab.LCU => await documentService.GetLcuSchemaDocumentAsync(),
                Pages.Endpoints.Tab.GameClient => await documentService.GetLolClientDocumentAsync(),
                _ => throw new NotImplementedException()
            };
            var propertyClassViewModel = OpenApiHelpers.WalkSchema(document.OpenApiDocument.Components.Schemas[item.Key], document.OpenApiDocument);
            var schemaViewModel = new SchemaViewModel(propertyClassViewModel);
            if (Schemas.ToList().Find(schema => schema.Id == schemaViewModel.Id) == null)
            {
                Schemas.Add(schemaViewModel);
                IsPaneOpen = true;

                OpenSchemaPaneCommand.NotifyCanExecuteChanged();
                CloseSchemaAllCommand.NotifyCanExecuteChanged();
            }
        });

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ObservableCollection<SchemaViewModel> _schemas = [];

    [ObservableProperty]
    private SchemaViewModel? _selectedSchema;

    [ObservableProperty]
    private ObservableCollection<NotificationViewModel> _notifications = [];

    [ObservableProperty]
    private SchemaSearchDetailsViewModel? _selectedSchemaSearchDetails;

    public string AppName => AppInfo.Name;

    public string Title => $"{AppInfo.Name} {AppInfo.Version}";

    partial void OnSelectedSchemaSearchDetailsChanged(SchemaSearchDetailsViewModel? value)
    {
        if (value == null) return;
        Task.Run(async () =>
        {
            var document = value.Tab switch
            {
                Pages.Endpoints.Tab.LCU => await _documentService.GetLcuSchemaDocumentAsync(),
                Pages.Endpoints.Tab.GameClient => await _documentService.GetLolClientDocumentAsync(),
                _ => throw new NotImplementedException()
            };
            var propertyClassViewModel = OpenApiHelpers.WalkSchema(document.OpenApiDocument.Components.Schemas[value.Key], document.OpenApiDocument);
            var schemaViewModel = new SchemaViewModel(propertyClassViewModel);
            Dispatcher.UIThread.Post(() =>
            {
                if (Schemas.ToList().Find(schema => schema.Id == schemaViewModel.Id) == null)
                {
                    Schemas.Add(schemaViewModel);
                    IsPaneOpen = true;

                    OpenSchemaPaneCommand.NotifyCanExecuteChanged();
                    CloseSchemaAllCommand.NotifyCanExecuteChanged();
                }
            });
        });
    }

    partial void OnSelectedSchemaChanged(SchemaViewModel? value)
    {
        CloseSchemaCommand.NotifyCanExecuteChanged();
    }

    partial void OnSchemasChanged(ObservableCollection<SchemaViewModel> value)
    {
        if (!value.Any())
        {
            IsPaneOpen = false;
        }
    }

    public async Task<IEnumerable<object>> PopulateAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (searchText == null) return [];

        var lcuSchemaDocument = await _documentService.GetLcuSchemaDocumentAsync(cancellationToken);
        var gameClientDocument = await _documentService.GetLolClientDocumentAsync(cancellationToken);
        var lcuResults = lcuSchemaDocument.OpenApiDocument.Components.Schemas.Keys.Where(key => key.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Select(key => new SchemaSearchDetailsViewModel(key, Pages.Endpoints.Tab.LCU));
        var gameClientResults = gameClientDocument.OpenApiDocument.Components.Schemas.Keys.Where(key => key.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Select(key => new SchemaSearchDetailsViewModel(key, Pages.Endpoints.Tab.GameClient));

        return Enumerable.Concat(lcuResults, gameClientResults);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSchemaPane))]
    private void OpenSchemaPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    private bool CanOpenSchemaPane()
    {
        return Schemas.Any();
    }

    [RelayCommand(CanExecute = nameof(CanCloseSchema))]
    private void CloseSchema()
    {
        if (SelectedSchema is SchemaViewModel selection)
        {
            SelectedSchema = null;
            Schemas = new ObservableCollection<SchemaViewModel>(Schemas.Where(schema => schema != selection));

            OpenSchemaPaneCommand.NotifyCanExecuteChanged();
            CloseSchemaCommand.NotifyCanExecuteChanged();
            CloseSchemaAllCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanCloseSchema()
    {
        return SelectedSchema != null;
    }

    [RelayCommand(CanExecute = nameof(CanCloseSchemaAll))]
    private void CloseSchemaAll()
    {
        SelectedSchema = null;
        Schemas = [];

        OpenSchemaPaneCommand.NotifyCanExecuteChanged();
        CloseSchemaCommand.NotifyCanExecuteChanged();
        CloseSchemaAllCommand.NotifyCanExecuteChanged();
    }

    private bool CanCloseSchemaAll()
    {
        return Schemas.Any();
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        var process = new Process() { StartInfo = new ProcessStartInfo(url) { UseShellExecute = true } };
        process.Start();
    }

    public void Receive(OopsiesDialogRequestedMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(async () => await _dialogService.ShowAsync<OopsiesDialog>(message.Value));
    }
}
