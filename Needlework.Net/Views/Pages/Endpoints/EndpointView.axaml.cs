using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.Pages.Endpoints;
using Needlework.Net.ViewModels.Shared;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class EndpointView : UserControl
{
    private TextEditor? _requestEditor;
    private TextEditor? _responseEditor;
    private RequestViewModel? _lcuRequestVm;

    public EndpointView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _requestEditor = this.FindControl<TextEditor>("EndpointRequestEditor");
        _responseEditor = this.FindControl<TextEditor>("EndpointResponseEditor");
        _requestEditor?.ApplyJsonEditorSettings();
        _responseEditor?.ApplyJsonEditorSettings();

        var vm = (EndpointViewModel)DataContext!;
        vm.PathOperationSelected += Vm_PathOperationSelected;

        if (vm.SelectedPathOperation != null)
        {
            _lcuRequestVm = vm.SelectedPathOperation.Request.Value;
            vm.SelectedPathOperation.Request.Value.RequestText += LcuRequest_RequestText;
            vm.SelectedPathOperation.Request.Value.UpdateText += LcuRequest_UpdateText;
        }

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    private void Vm_PathOperationSelected(object? sender, string e)
    {
        var vm = (EndpointViewModel)DataContext!;
        if (vm.SelectedPathOperation != null)
        {
            _requestEditor!.Text = e;
            if (_lcuRequestVm != null)
            {
                _lcuRequestVm.RequestText -= LcuRequest_RequestText;
                _lcuRequestVm.UpdateText -= LcuRequest_UpdateText;
            }
            vm.SelectedPathOperation.Request.Value.RequestText += LcuRequest_RequestText;
            vm.SelectedPathOperation.Request.Value.UpdateText += LcuRequest_UpdateText;
            _lcuRequestVm = vm.SelectedPathOperation.Request.Value;
            _responseEditor!.Text = vm.SelectedPathOperation.Request.Value.ResponseBody ?? string.Empty;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        var vm = (EndpointViewModel)DataContext!;
        vm.PathOperationSelected -= Vm_PathOperationSelected;

        if (_lcuRequestVm != null)
        {
            _lcuRequestVm.RequestText -= LcuRequest_RequestText;
            _lcuRequestVm.UpdateText -= LcuRequest_UpdateText;
            _lcuRequestVm = null;
        }
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
          currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }

    private void LcuRequest_RequestText(object? sender, RequestViewModel e)
    {
        e.RequestBody = _requestEditor!.Text;
    }

    private void LcuRequest_UpdateText(object? sender, string e)
    {
        _responseEditor!.Text = e;
    }

}