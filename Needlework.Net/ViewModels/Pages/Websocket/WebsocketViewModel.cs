using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Needlework.Net.Messages;
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

namespace Needlework.Net.ViewModels.Pages.Websocket;

public partial class WebsocketViewModel : PageBase
{
    public ObservableCollection<EventViewModel> EventLog { get; } = [];
    public SemaphoreSlim EventLogLock { get; } = new(1, 1);

    [NotifyPropertyChangedFor(nameof(FilteredEventLog))]
    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private bool _isAttach = true;
    [ObservableProperty] private bool _isTail = false;
    [ObservableProperty] private EventViewModel? _selectedEventLog = null;

    [ObservableProperty] private IAvaloniaList<string> _eventTypes = new AvaloniaList<string>();
    [ObservableProperty] private string _eventType = "OnJsonApiEvent";

    private Dictionary<string, EventMessage> _events = [];

    public WebsocketClient? Client { get; set; }

    public List<IDisposable> ClientDisposables = [];

    private readonly object _tokenLock = new();
    public CancellationTokenSource TokenSource { get; set; } = new();

    public HttpClient HttpClient { get; }

    public IReadOnlyList<EventViewModel> FilteredEventLog => string.IsNullOrWhiteSpace(Search) ? EventLog : [.. EventLog.Where(x => x.Key.Contains(Search, StringComparison.InvariantCultureIgnoreCase))];

    private readonly ILogger<WebsocketViewModel> _logger;

    public WebsocketViewModel(HttpClient httpClient, ILogger<WebsocketViewModel> logger) : base("Event Viewer", "plug", -100)
    {
        _logger = logger;
        HttpClient = httpClient;
        EventLog.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredEventLog));
        Task.Run(async () =>
        {
            await InitializeEventTypes();
            InitializeWebsocket();
        });
    }

    public override Task InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }

    private async Task InitializeEventTypes()
    {
        try
        {
            var file = await HttpClient.GetStringAsync("https://raw.githubusercontent.com/dysolix/hasagi-types/refs/heads/main/dist/lcu-events.d.ts");
            var matches = EventTypesRegex().Matches(file);
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => EventTypes.AddRange(matches.Select(m => m.Groups[1].Value)));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get event types");
            WeakReferenceMessenger.Default.Send(new InfoBarUpdateMessage(new("Failed to get event types", true, ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error, TimeSpan.FromSeconds(10))));
        }
    }

    private void InitializeWebsocket()
    {
        lock (_tokenLock)
        {
            if (Client != null)
            {
                _logger.LogDebug("Disposing old connection");
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
            _logger.LogDebug("Initialized new connection: {EventType}", EventType);
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
            else WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(text), nameof(WebsocketViewModel));
        }
    }

    [RelayCommand]
    private void Clear()
    {
        _events.Clear();
        EventLog.Clear();
    }

    private void OnReconnection(ReconnectionInfo info)
    {
        _logger.LogTrace("Reconnected: {Type}", info.Type);
    }

    private void OnDisconnection(DisconnectionInfo info)
    {
        _logger.LogTrace("Disconnected: {Type}", info.Type);
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
