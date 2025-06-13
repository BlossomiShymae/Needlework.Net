using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System;
using System.Collections;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointsPage : ReactiveUserControl<EndpointsViewModel>
{
    public EndpointsPage()
    {
        this.WhenAnyValue(x => x.ViewModel!.GetEndpointCommand)
            .SelectMany(x => x.Execute())
            .Subscribe();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.IsBusy, v => v.BusyArea.IsBusy)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }

    private void TabView_TabCloseRequested(FluentAvalonia.UI.Controls.TabView sender, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Tab.Content is EndpointTabItemViewModel item && sender.TabItems is IList tabItems)
        {
            if (tabItems.Count > 1)
            {
                tabItems.Remove(item);
            }
        }
    }
}