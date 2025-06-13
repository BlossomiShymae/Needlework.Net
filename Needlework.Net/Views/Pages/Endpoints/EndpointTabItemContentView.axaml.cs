using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointTabItemContentView : ReactiveUserControl<EndpointTabItemContentViewModel>
{
    public EndpointTabItemContentView()
    {
        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Title, v => v.TitleTextBlock.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.ActiveViewModel, v => v.ViewModelViewHost.ViewModel)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.GoBackCommand, v => v.GoBackButton)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}