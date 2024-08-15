using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using Needlework.Net.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Websocket.Client;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class WebsocketViewModel : PageBase
    {
        [NotifyPropertyChangedFor(nameof(FilteredEventLog))]
        [ObservableProperty] private ObservableCollection<string> _eventLog = [];
        [NotifyPropertyChangedFor(nameof(FilteredEventLog))]
        [ObservableProperty] private string _search = string.Empty;
        [ObservableProperty] private bool _isAttach = true;
        [ObservableProperty] private bool _isTail = false;
        [ObservableProperty] private string? _selectedEventLog = null;

        private Dictionary<string, EventMessage> _events = [];

        public WebsocketClient? Client { get; set; }

        public WindowService WindowService { get; }

        public List<string> FilteredEventLog => string.IsNullOrWhiteSpace(Search) ? [.. EventLog] : [.. EventLog.Where(x => x.ToLower().Contains(Search.ToLower()))];

        public WebsocketViewModel(WindowService windowService) : base("Event Viewer", "plug", -100)
        {
            WindowService = windowService;

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

        [RelayCommand]
        private void Clear()
        {
            EventLog = [];
        }

        partial void OnSelectedEventLogChanged(string? value)
        {
            if (value == null) return;
            if (_events.TryGetValue(value, out var message))
            {
                var text = JsonSerializer.Serialize(message, App.JsonSerializerOptions);
                if (text.Length >= App.MaxCharacters) WindowService.ShowOopsiesWindow(text);
                else WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(text), nameof(WebsocketViewModel));
            }
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
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
            {
                if (!IsAttach) return;

                var line = $"{DateTime.Now:HH:mm:ss.fff} {message.Data?.EventType.ToUpper()} {message.Data?.Uri}";
                var log = EventLog.ToList();
                Trace.WriteLine($"Message: {line}");
                if (log.Count < 1000)
                {
                    log.Add(line);
                    _events[line] = message;
                }
                else
                {
                    var key = $"{log[0]}";
                    log.RemoveAt(0);
                    _events.Remove(key);

                    log.Add(line);
                    _events[line] = message;
                }

                EventLog = []; // This is a hack needed to update for ListBox
                EventLog = new ObservableCollection<string>(log);
            });
        }
    }
}
