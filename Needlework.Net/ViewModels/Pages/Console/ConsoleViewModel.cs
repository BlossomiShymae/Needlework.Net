using DynamicData;
using Needlework.Net.ViewModels.Shared;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Console;

public partial class ConsoleViewModel : PageBase
{
    private readonly DataSource _dataSource;

    public ConsoleViewModel(IScreen? screen = null, DataSource? dataSource = null) : base("Console", "terminal", -200)
    {
        _dataSource = dataSource ?? Locator.Current.GetService<DataSource>()!;
        _request = new(Endpoints.Tab.LCU);

        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

        GetRequestPathsCommand.Subscribe(paths =>
        {
            RequestPaths.Clear();
            RequestPaths.AddRange(paths);
            IsBusy = false;
        });
        GetRequestPathsCommand.ThrownExceptions.Subscribe(ex =>
        {
            this.Log()
                .Error(ex, "Failed to load request paths from LCU Schema document.");
            IsBusy = false;
        });
    }

    public ObservableCollection<string> RequestMethods { get; } = ["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"];

    public override string? UrlPathSegment => "console";

    public override ReactiveUI.IScreen HostScreen { get; }

    [Reactive]
    private ObservableCollection<string> _requestPaths = [];

    [Reactive]
    private bool _isBusy = true;

    [Reactive]
    private RequestViewModel _request;


    [ReactiveCommand]
    public async Task<List<string>> GetRequestPathsAsync()
    {
        var document = await _dataSource.GetLcuSchemaDocumentAsync();
        return document.Paths;
    }

    [ReactiveCommand]
    private async Task SendRequestAsync()
    {
        await Request.ExecuteAsync();
    }
}
