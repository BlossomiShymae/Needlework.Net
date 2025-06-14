using Avalonia.Collections;
using AvaloniaEdit.Document;
using BlossomiShymae.Briar;
using BlossomiShymae.Briar.WebSocket.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Flurl.Http;
using Flurl.Http.Configuration;
using Needlework.Net.Extensions;
using Needlework.Net.Messages;
using Needlework.Net.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Needlework.Net.ViewModels.Pages.WebSocket;

public partial class WebSocketViewModel : PageBase, IEnableLogger
{
    private Dictionary<string, EventMessage> _events = [];

    private readonly IFlurlClient _githubUserContentClient;

    private readonly NotificationService _notificationService;

    private readonly object _tokenLock = new();

    public WebSocketViewModel(IFlurlClientCache clients, NotificationService notificationService) : base("Event Viewer", "fa-solid fa-plug", -100)
    {
        _githubUserContentClient = clients.Get("GithubUserContentClient");
        _notificationService = notificationService;

        EventLog.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredEventLog));
    }

    public ObservableCollection<EventViewModel> EventLog { get; } = [];

    public SemaphoreSlim EventLogLock { get; } = new(1, 1);

    public WebsocketClient? Client { get; set; }

    public List<IDisposable> ClientDisposables = [];

    public CancellationTokenSource TokenSource { get; set; } = new();

    public IReadOnlyList<EventViewModel> FilteredEventLog => string.IsNullOrWhiteSpace(Search) ? EventLog : [.. EventLog.Where(x => x.Key.Contains(Search, StringComparison.InvariantCultureIgnoreCase))];

    [NotifyPropertyChangedFor(nameof(FilteredEventLog))]
    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private bool _isAttach = true;

    [ObservableProperty]
    private bool _isTail = false;

    [ObservableProperty]
    private EventViewModel? _selectedEventLog = null;

    [ObservableProperty]
    private IAvaloniaList<string> _eventTypes = new AvaloniaList<string>();

    [ObservableProperty]
    private string _eventType = "OnJsonApiEvent";

    [ObservableProperty]
    private TextDocument _document = new();

    public override async Task InitializeAsync()
    {
        await InitializeEventTypes();
        InitializeWebsocket();
    }

    private async Task InitializeEventTypes()
    {
        try
        {
            var file = await _githubUserContentClient.Request("/dysolix/hasagi-types/refs/heads/main/dist/lcu-events.d.ts")
                .GetStringAsync();
            var matches = EventTypesRegex().Matches(file);
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => EventTypes.AddRange(matches.Select(m => m.Groups[1].Value)));
        }
        catch (HttpRequestException ex)
        {
            var message = "Failed to get event types from GitHub. Please check your internet connection or try again later.";
            this.Log()
                .Error(ex, message);
            _notificationService.Notify("Needlework.Net", message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error);
        }
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

    partial void OnSelectedEventLogChanged(EventViewModel? value)
    {
        if (value == null) return;
        if (_events.TryGetValue(value.Key, out var message))
        {
            var text = JsonSerializer.Serialize(message, App.JsonSerializerOptions);
            if (text.Length >= App.MaxCharacters) WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(text));
            else Document = new(text);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        _events.Clear();
        EventLog.Clear();
        Document = new();
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

    partial void OnEventTypeChanged(string value)
    {
        InitializeWebsocket();
    }

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
