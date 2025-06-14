using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointTabItemViewModel : ObservableObject
{
    [ObservableProperty] private string _header = string.Empty;
    public IconSource IconSource { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 20.0, Foreground = Avalonia.Media.Brushes.White };
    public bool Selected { get; set; } = false;
    public required EndpointTabItemContentViewModel Content { get; init; }
}