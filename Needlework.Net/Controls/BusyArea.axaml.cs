using Avalonia;
using Avalonia.Controls;

namespace Needlework.Net.Controls;

public partial class BusyArea : UserControl
{
    public BusyArea()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsBusyProperty =
       AvaloniaProperty.Register<BusyArea, bool>(nameof(IsBusy), defaultValue: false);

    public bool IsBusy
    {
        get { return GetValue(IsBusyProperty); }
        set { SetValue(IsBusyProperty, value); }
    }

    public static readonly StyledProperty<string?> BusyTextProperty =
        AvaloniaProperty.Register<BusyArea, string?>(nameof(BusyText), defaultValue: null);

    public string? BusyText
    {
        get => GetValue(BusyTextProperty);
        set => SetValue(BusyTextProperty, value);
    }
}