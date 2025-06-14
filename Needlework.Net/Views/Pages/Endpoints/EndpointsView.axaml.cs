using Avalonia.Controls;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System.Collections;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointsView : UserControl
{
    public EndpointsView()
    {
        InitializeComponent();
    }

    private void TabView_TabCloseRequested(FluentAvalonia.UI.Controls.TabView sender, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Tab.DataContext is EndpointTabItemViewModel item && sender.TabItems is IList tabItems)
        {
            if (tabItems.Count > 1)
            {
                tabItems.Remove(item);
            }
        }
    }
}