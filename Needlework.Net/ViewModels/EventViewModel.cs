using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Needlework.Net.ViewModels
{
    public class EventViewModel : ObservableObject
    {
        public string Time { get; }
        public string Type { get; }
        public string Uri { get; }

        public string Key => $"{Time} {Type} {Uri}";

        public EventViewModel(EventData eventData)
        {
            Time = $"{DateTime.Now:HH:mm:ss.fff}";
            Type = eventData?.EventType.ToUpper() ?? string.Empty;
            Uri = eventData?.Uri ?? string.Empty;
        }
    }
}
