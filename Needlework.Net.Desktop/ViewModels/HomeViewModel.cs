using Avalonia.Media;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class HomeViewModel : PageBase
    {
        [ObservableProperty] private string _statusText = string.Empty;
        [ObservableProperty] private IBrush? _statusForeground;
        [ObservableProperty] private string _statusAddress = string.Empty;

        public HomeViewModel() : base("Home", Material.Icons.MaterialIconKind.Home, int.MinValue)
        {
            StartProcessing();
        }

        private void StartProcessing()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    void Set(string text, Color color, string address)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                        {
                            StatusText = text;
                            StatusForeground = new SolidColorBrush(color.ToUInt32());
                            StatusAddress = address;
                        });
                    }

                    try
                    {
                        var processInfo = Connector.GetProcessInfo();
                        Set("Online", Colors.Green, $"https://127.0.0.1:{processInfo.AppPort}/");
                    }
                    catch (InvalidOperationException)
                    {
                        Set("Offline", Colors.Red, "N/A");
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            })
            { IsBackground = true };
            thread.Start();
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
    }
}
