using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Needlework.Net.ViewModels.Pages.Endpoints
{
    public partial class EndpointTabItemViewModel : ReactiveObject
    {
        public EndpointTabItemViewModel(EndpointTabItemContentViewModel content, string? header = null, IconSource? iconSource = null, bool? selected = null)
        {
            _content = content;
            _header = header ?? string.Empty;
            _iconSource = iconSource ?? new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 20.0, Foreground = Avalonia.Media.Brushes.White };
            _selected = selected ?? false;
        }

        [Reactive]
        private string _header;

        [Reactive]
        private IconSource _iconSource;

        [Reactive]
        private bool _selected;

        [Reactive]
        private EndpointTabItemContentViewModel _content;
    }
}
