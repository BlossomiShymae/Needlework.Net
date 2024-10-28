using FluentAvalonia.UI.Controls;
using Needlework.Net.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Needlework.Net.Views.MainWindow;

public class OopsiesDialog : IDialog, IDisposable
{
    private bool _isDisposing;
    private string? _text;
    private ContentDialog _dialog;

    public OopsiesDialog()
    {
        _dialog = new ContentDialog
        {
            PrimaryButtonText = "Open",
            CloseButtonText = "Cancel",
            Title = "Oopsies",
            Content = "This response is too large to handle for performance reasons.\nIt can be viewed in an external editor or viewer.",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
            DefaultButton = ContentDialogButton.Primary
        };
        _dialog.PrimaryButtonClick += OnPrimaryButtonClick;
    }

    public async Task<ContentDialogResult> ShowAsync(object data)
    {
        _text = (string)data;
        var result = await _dialog.ShowAsync();
        return result;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var temp = Path.GetTempFileName().Replace(".tmp", ".json");
        File.WriteAllText(temp, _text);
        Process.Start("explorer", "\"" + temp + "\"");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposing)
        {
            if (disposing)
            {
                _text = null;
                _dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            }

            _isDisposing = true;
        }
    }
}