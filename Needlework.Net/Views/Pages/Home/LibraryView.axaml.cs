using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Home;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.Home;

public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.Library.Language, v => v.LanguageRun.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Library.Repo, v => v.RepoRun.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Library.Description, v => v.DescriptionTextBlock.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Library.Description, v => v.DescriptionTextBlock.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Library.Link, v => v.OpenUrlButton.Content)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}