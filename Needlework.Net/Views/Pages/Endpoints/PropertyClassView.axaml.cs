using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class PropertyClassView : ReactiveUserControl<PropertyClassViewModel>
{
    public PropertyClassView()
    {
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.Id, v => v.IdTextBlock.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.PropertyFields, v => v.PropertyFieldsCard.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.PropertyFields, v => v.PropertyFieldsDataGrid.ItemsSource)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.PropertyEnums, v => v.PropertyEnumsCard.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.PropertyEnums, v => v.PropertyEnumsDataGrid.ItemsSource)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}