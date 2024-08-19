using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace Needlework.Net.Services
{
    public interface IDialog
    {
        public Task<ContentDialogResult> ShowAsync(object data);
    }
}
