using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System.Reactive.Disposables;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class PluginView : ReactiveUserControl<PluginViewModel>
{
    public PluginView()
    {
        this.WhenActivated(disposables =>
        {
            EndpointRequestEditor?.ApplyJsonEditorSettings();
            EndpointResponseEditor?.ApplyJsonEditorSettings();
            OnBaseThemeChanged(Application.Current!.ActualThemeVariant);

            this.OneWayBind(ViewModel, vm => vm.Search, v => v.SearchTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.FilteredPathOperations, v => v.PathOperationListBox.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedPathOperation, v => v.PathOperationListBox.SelectedItem)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.Method, v => v.RequestMethodTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.ResponsePath, v => v.RequestResponsePathTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation, v => v.ParamsStackPanel.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.PathParameters, v => v.PathParametersCard.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.PathParameters, v => v.PathParametersDataGrid.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.QueryParameters, v => v.QueryParametersCard.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.QueryParameters, v => v.QueryParametersDataGrid.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.ResponseUsername, v => v.UsernameTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.ResponsePassword, v => v.PasswordTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.ResponseAuthorization, v => v.AuthorizationTextBox.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.RequestBodyType, v => v.RequestBodyTypeCard.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.RequestBodyType, v => v.RequestBodyTypeRun.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.RequestClasses, v => v.RequestClassesBorder.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.RequestClasses, v => v.RequestClassItemsControl.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.ReturnType, v => v.ReturnTypeRun.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.ResponseClasses, v => v.ResponseClassesBorder.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Operation.ResponseClasses, v => v.ResponseClassItemsControl.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.Request.Value.ResponseStatus, v => v.ResponseStatusButton.Content)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SelectedPathOperation.IsBusy, v => v.SelectedPathOperationBusyArea.IsBusy)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.SelectedPathOperation.SendRequestCommand, v => v.SendRequestButton)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
          currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}