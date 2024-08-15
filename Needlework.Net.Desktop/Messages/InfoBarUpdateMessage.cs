using CommunityToolkit.Mvvm.Messaging.Messages;
using Needlework.Net.Desktop.ViewModels;

namespace Needlework.Net.Desktop.Messages
{
    public class InfoBarUpdateMessage(InfoBarViewModel vm) : ValueChangedMessage<InfoBarViewModel>(vm)
    {
    }
}
