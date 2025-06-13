using Needlework.Net.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;

namespace Needlework.Net.ViewModels.Pages.Endpoints
{
    public partial class EndpointSearchDetailsViewModel : ReactiveObject
    {
        private readonly Document _document;

        private readonly Tab _tab;

        private readonly Action<ReactiveObject> _onClicked;


        public EndpointSearchDetailsViewModel(Document document, Tab tab, Action<ReactiveObject> onClicked, string? plugin)
        {
            _document = document;
            _tab = tab;
            _onClicked = onClicked;
            _plugin = plugin;
        }

        [Reactive]
        private string? _plugin;

        [ReactiveCommand]
        private void OpenEndpoint()
        {
            if (string.IsNullOrEmpty(_plugin)) return;
            _onClicked.Invoke(new PluginViewModel(_plugin, _document, _tab));
        }
    }
}
