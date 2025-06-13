using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointSearchDetailsView : ReactiveUserControl<EndpointSearchDetailsViewModel>
{
    public EndpointSearchDetailsView()
    {
        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Plugin, v => v.DetailsButton.Content)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.OpenEndpointCommand, v => v.DetailsButton)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}