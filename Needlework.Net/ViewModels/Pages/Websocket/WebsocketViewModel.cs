using BlossomiShymae.Briar;
using BlossomiShymae.Briar.WebSocket.Events;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Needlework.Net.ViewModels.Pages.WebSocket;

public partial class WebSocketViewModel : PageBase, IEnableLogger
{
    private Dictionary<string, EventMessage> _events = [];

    private readonly object _tokenLock = new();

    private readonly IFlurlClient _githubUserContentClient;

    // public IReadOnlyList<EventViewModel> FilteredEventLog => string.IsNullOrWhiteSpace(Search) ? EventLog : [.. EventLog.Where(x => x.Key.Contains(Search, StringComparison.InvariantCultureIgnoreCase))];


    public WebSocketViewModel(IScreen? screen = null, IFlurlClientCache? clients = null) : base("Event Viewer", "plug", -100)
    {
        _githubUserContentClient = clients?.Get("GithubUserContentClient") ?? Locator.Current.GetService<IFlurlClientCache>()?.Get("GithubUserContentClient")!;

        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

        //EventLog.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredEventLog));
        //Task.Run(async () =>
        //{
        //    await InitializeEventTypes();
        //    InitializeWebsocket();
        //});
    }

    public override string? UrlPathSegment => "websocket";

    public override ReactiveUI.IScreen HostScreen { get; }


    public CancellationTokenSource TokenSource { get; set; } = new();

    public WebsocketClient? Client { get; set; }

    public List<IDisposable> ClientDisposables = [];

    public SemaphoreSlim EventLogLock { get; } = new(1, 1);

    [Reactive]
    public ObservableCollection<EventViewModel> EventLog { get; } = [];

    [Reactive]
    private string _search = string.Empty;

    [Reactive]
    private bool _isAttach = true;

    [Reactive]
    private bool _isTail;

    [Reactive]
    private EventViewModel? _selectedEventLog;

    [Reactive]
    private ObservableCollection<string> _eventTypes = [];

    [Reactive]
    private string _eventType = "OnJsonApiEvent";

    [ObservableAsProperty]
    private ObservableCollection<EventViewModel> _filteredEventLog = [];

    [ReactiveCommand]
    private async Task<List<string>> GetEventTypesAsync()
    {
        var file = await _githubUserContentClient.Request("/dysolix/hasagi-types/refs/heads/main/dist/lcu-events.d.ts")
            .GetStringAsync();
        var matches = EventTypesRegex().Matches(file);
        var eventTypes = matches.Select(m => m.Groups[1].Value)
            .ToList();
        return eventTypes;
    }

    private void InitializeWebsocket()
    {
        lock (_tokenLock)
        {
            if (Client != null)
            {
                this.Log()
                    .Debug("Disposing old connection");
                foreach (var disposable in ClientDisposables)
                    disposable.Dispose();
                ClientDisposables.Clear();
                Client.Dispose();
            }
            TokenSource.Cancel();
            var tokenSource = new CancellationTokenSource();
            var thread = new Thread(() =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var client = Connector.CreateLcuWebsocketClient();
                        ClientDisposables.Add(client.EventReceived.Subscribe(OnMessage));
                        ClientDisposables.Add(client.DisconnectionHappened.Subscribe(OnDisconnection));
                        ClientDisposables.Add(client.ReconnectionHappened.Subscribe(OnReconnection));

                        client.Start();
                        client.Send(new EventMessage(EventRequestType.Subscribe, new EventKind() { Prefix = EventType }));
                        Client = client;
                        return;
                    }
                    catch (Exception) { }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            })
            { IsBackground = true };
            thread.Start();
            this.Log()
                .Debug("Initialized new connection: {EventType}", EventType);
            TokenSource = tokenSource;
        }
    }

    //partial void OnSelectedEventLogChanged(EventViewModel? value)
    //{
    //    if (value == null) return;
    //    if (_events.TryGetValue(value.Key, out var message))
    //    {
    //        var text = JsonSerializer.Serialize(message, App.JsonSerializerOptions);
    //        if (text.Length >= App.MaxCharacters) WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(text));
    //        else WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(text), nameof(WebSocketViewModel));
    //    }
    //}

    [ReactiveCommand]
    private void Clear()
    {
        _events.Clear();
        EventLog.Clear();
    }

    private void OnReconnection(ReconnectionInfo info)
    {
        this.Log()
            .Debug("Reconnected: {Type}", info.Type);
    }

    private void OnDisconnection(DisconnectionInfo info)
    {
        this.Log()
            .Debug("Disconnected: {Type}", info.Type);
        InitializeWebsocket();
    }

    //partial void OnEventTypeChanged(string value)
    //{
    //    InitializeWebsocket();
    //}

    private void OnMessage(EventMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(async () =>
        {
            if (!IsAttach) return;

            var line = new EventViewModel(message.Data!);

            await EventLogLock.WaitAsync();
            try
            {
                if (EventLog.Count < 1000)
                {
                    EventLog.Add(line);
                    _events[line.Key] = message;
                }
                else
                {
                    var _event = EventLog[0];
                    EventLog.RemoveAt(0);
                    _events.Remove(_event.Key);

                    EventLog.Add(line);
                    _events[line.Key] = message;
                }
            }
            finally
            {
                EventLogLock.Release();
            }
        });
    }

    [GeneratedRegex("\"(.*?)\":")]
    public static partial Regex EventTypesRegex();
}
