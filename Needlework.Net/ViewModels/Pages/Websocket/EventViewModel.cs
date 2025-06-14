using BlossomiShymae.Briar.WebSocket.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Needlework.Net.ViewModels.Pages.WebSocket;

public class EventViewModel : ObservableObject
{
    public EventViewModel(EventData eventData)
    {
        Time = $"{DateTime.Now:HH:mm:ss.fff}";
        Type = eventData?.EventType?.ToUpper() ?? string.Empty;
        Uri = eventData?.Uri ?? string.Empty;
    }

    public string Time { get; }

    public string Type { get; }

    public string Uri { get; }

    public string Key => $"{Time} {Type} {Uri}";
}
