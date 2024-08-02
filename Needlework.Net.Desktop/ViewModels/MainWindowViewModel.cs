using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public IAvaloniaReadOnlyList<PageBase> Pages { get; }

        public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";

        public MainWindowViewModel(IEnumerable<PageBase> pages)
        {
            Pages = new AvaloniaList<PageBase>(pages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        }

        [RelayCommand]
        private void OpenUrl(string url)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                }
            };
            process.Start();
        }

        [RelayCommand]
        private void OpenConsole()
        {

        }
    }
}
