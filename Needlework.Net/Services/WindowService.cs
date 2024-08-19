using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels;
using Needlework.Net.Views;
using System;

namespace Needlework.Net.Services
{
    public class WindowService : IRecipient<OopsiesWindowCanceledMessage>
    {
        public IServiceProvider ServiceProvider { get; }

        public OopsiesWindow? OopsiesWindow { get; set; }

        public WindowService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            WeakReferenceMessenger.Default.Register<OopsiesWindowCanceledMessage>(this);
        }

        public void ShowOopsiesWindow(string text)
        {
            if (OopsiesWindow != null) OopsiesWindow!.Close();

            var window = new OopsiesWindow();
            window.DataContext = new OopsiesWindowViewModel(text);
            window.Show(App.MainWindow!);
            window.Closed += OnOopsiesWindowClosed;
            OopsiesWindow = window;
        }

        public void OnOopsiesWindowClosed(object? sender, EventArgs e)
        {
            if (sender == null) return;

            var window = (OopsiesWindow)sender;
            window.DataContext = null;
            window.Closed -= OnOopsiesWindowClosed;
            OopsiesWindow = null;
        }

        public void Receive(OopsiesWindowCanceledMessage message)
        {
            if (OopsiesWindow is OopsiesWindow window) window.Close();
        }
    }
}
