using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Websocket.Client;

namespace Needlework.Net.ViewModels
{
    public partial class WebsocketViewModel : PageBase
    {
        public ObservableCollection<EventViewModel> EventLog { get; } = [];
        public SemaphoreSlim EventLogLock { get; } = new(1, 1);

        [NotifyPropertyChangedFor(nameof(FilteredEventLog))]
        [ObservableProperty] private string _search = string.Empty;
        [ObservableProperty] private bool _isAttach = true;
        [ObservableProperty] private bool _isTail = false;
        [ObservableProperty] private EventViewModel? _selectedEventLog = null;

        private Dictionary<string, EventMessage> _events = [];

        public WebsocketClient? Client { get; set; }

        public IReadOnlyList<EventViewModel> FilteredEventLog => string.IsNullOrWhiteSpace(Search) ? EventLog : [.. EventLog.Where(x => x.Key.Contains(Search, StringComparison.InvariantCultureIgnoreCase))];

        public WebsocketViewModel() : base("Event Viewer", "plug", -100)
        {
            EventLog.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredEventLog));
            var thread = new Thread(InitializeWebsocket) { IsBackground = true };
            thread.Start();
        }

        private void InitializeWebsocket()
        {
            while (true)
            {
                try
                {
                    var client = Connector.CreateLcuWebsocketClient();
                    client.EventReceived.Subscribe(OnMessage);
                    client.DisconnectionHappened.Subscribe(OnDisconnection);
                    client.ReconnectionHappened.Subscribe(OnReconnection);

                    client.Start();
                    client.Send(new EventMessage(RequestType.Subscribe, EventMessage.Kinds.OnJsonApiEvent));
                    Client = client;
                    return;
                }
                catch (Exception) { }
                Thread.Sleep(TimeSpan.FromSeconds(5));
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
            Trace.WriteLine($"-- Reconnection --\nType{info.Type}");
        }

        private void OnDisconnection(DisconnectionInfo info)
        {
            Trace.WriteLine($"-- Disconnection --\nType:{info.Type}\nSubProocol:{info.SubProtocol}\nCloseStatus:{info.CloseStatus}\nCloseStatusDescription:{info.CloseStatusDescription}\nExceptionMessage:{info?.Exception?.Message}\n:InnerException:{info?.Exception?.InnerException}");
            Client?.Dispose();
            var thread = new Thread(InitializeWebsocket) { IsBackground = true };
            thread.Start();
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
    }
}
