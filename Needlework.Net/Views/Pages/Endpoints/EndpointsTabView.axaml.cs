using Avalonia.Controls;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System.Collections;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointsTabView : UserControl
{
    public EndpointsTabView()
    {
        InitializeComponent();
    }

    private void TabView_TabCloseRequested(FluentAvalonia.UI.Controls.TabView sender, FluentAvalonia.UI.Controls.TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Tab.Content is EndpointItem item)
            ((IList)sender.TabItems).Remove(item);
    }
}