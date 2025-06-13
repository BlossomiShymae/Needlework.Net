using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointTabListView : ReactiveUserControl<EndpointTabListViewModel>
{
    public EndpointTabListView()
    {
        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Search, v => v.SearchTextBox.Text)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}