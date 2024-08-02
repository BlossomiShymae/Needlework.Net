using Needlework.Net.Desktop.ViewModels;
using Needlework.Net.Desktop.Views;
using SukiUI.Controls;
using System;
using System.Collections.Generic;

namespace Needlework.Net.Desktop.Services
{
    public class DialogService
    {
        public IServiceProvider ServiceProvider { get; }

        public Dictionary<string, SukiWindow> Dialogs { get; } = [];

        public DialogService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void ShowEndpoint(string endpoint)
        {
            if (!Dialogs.TryGetValue(endpoint, out var _))
            {
                var dialog = new EndpointView();
                dialog.DataContext = new EndpointViewModel(endpoint);
                dialog.Show();
                dialog.Closed += OnDialogClosed;
                Dialogs[endpoint] = dialog;
            }
        }

        private void OnDialogClosed(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            var dialog = (SukiWindow)sender;
            if (dialog.DataContext is EndpointViewModel vm)
            {
                Dialogs.Remove(vm.Endpoint);
                dialog.DataContext = null;
            }

            dialog.Closed -= OnDialogClosed;
        }
    }
}
