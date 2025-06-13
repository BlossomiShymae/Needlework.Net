using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.Pages.Console;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.Console;

public partial class ConsolePage : ReactiveUserControl<ConsoleViewModel>
{
    public ConsolePage()
    {
        this.WhenAnyValue(x => x.ViewModel!.GetRequestPathsCommand)
            .SelectMany(x => x.Execute())
            .Subscribe();

        this.WhenActivated(disposables =>
        {
            ResponseEditor.ApplyJsonEditorSettings();
            RequestEditor.ApplyJsonEditorSettings();
            OnBaseThemeChanged(Application.Current!.ActualThemeVariant);

            this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.BusyArea.IsBusy)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.RequestMethods, v => v.RequestMethodsComboBox.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.Method, v => v.RequestMethodsComboBox.SelectedItem)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.RequestPaths, v => v.RequestPathsAutoCompleteBox.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.RequestPath, v => v.RequestPathsAutoCompleteBox.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.ResponsePath, v => v.ResponsePathTextBox.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.ResponseStatus, v => v.ResponseStatusButton.Content)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.RequestDocument, v => v.RequestEditor.Document)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Request.ResponseDocument, v => v.ResponseEditor.Document)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.SendRequestCommand, v => v.SendRequestButton)
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