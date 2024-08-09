using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using System.Diagnostics;
using System.IO;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class OopsiesWindowViewModel(string text) : ObservableObject
    {
        public string Text { get; } = text;

        [RelayCommand]
        private void OpenDefaultEditor()
        {
            var temp = Path.GetTempFileName().Replace(".tmp", ".json");
            File.WriteAllText(temp, Text);
            Process.Start("explorer", "\"" + temp + "\"");
            CloseDialog();
        }

        [RelayCommand]
        private void CloseDialog()
        {
            WeakReferenceMessenger.Default.Send(new OopsiesWindowCanceledMessage(null));
        }
    }
}
