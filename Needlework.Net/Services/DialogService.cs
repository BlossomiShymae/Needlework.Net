using FluentAvalonia.UI.Controls;
using System;
using System.Threading.Tasks;

namespace Needlework.Net.Services
{
    public class DialogService
    {
        public async Task<ContentDialogResult> ShowAsync<T>(object data)
            where T : IDialog, IDisposable
        {
            T dialog = Activator.CreateInstance<T>();

            var result = await dialog.ShowAsync(data);
            dialog.Dispose();

            return result;
        }
    }
}
