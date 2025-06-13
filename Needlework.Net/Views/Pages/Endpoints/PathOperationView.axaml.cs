using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class PathOperationView : ReactiveUserControl<PathOperationViewModel>
{
    public PathOperationView()
    {
        this.WhenActivated(disposables =>
        {
            // Add any activation logic here if needed
        });

        InitializeComponent();
    }
}