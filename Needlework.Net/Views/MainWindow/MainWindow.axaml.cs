using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;
using Needlework.Net.ViewModels.MainWindow;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.MainWindow;

public partial class MainWindow : AppWindow, IViewFor<MainWindowViewModel>
{
    public MainWindow()
    {
        TitleBar.ExtendsContentIntoTitleBar = true;
        TransparencyLevelHint = [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None];
        Background = IsWindows11 ? null : Background;

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.PageItems, v => v.NavigationView.MenuItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedPageItem, v => v.NavigationView.SelectedItem)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Notifications, v => v.NotificationItemsControl.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionButton.Content)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }

    public static readonly StyledProperty<MainWindowViewModel?> ViewModelProperty = AvaloniaProperty
           .Register<MainWindow, MainWindowViewModel?>(nameof(ViewModel));

    public MainWindowViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel?)value;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataContextProperty)
        {
            if (ReferenceEquals(change.OldValue, ViewModel)
                && change.NewValue is null or MainWindowViewModel)
            {
                SetCurrentValue(ViewModelProperty, change.NewValue);
            }
        }
        else if (change.Property == ViewModelProperty)
        {
            if (ReferenceEquals(change.OldValue, DataContext))
            {
                SetCurrentValue(DataContextProperty, change.NewValue);
            }
        }
    }
}