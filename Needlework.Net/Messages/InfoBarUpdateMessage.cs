using CommunityToolkit.Mvvm.Messaging.Messages;
using Needlework.Net.ViewModels;

namespace Needlework.Net.Messages
{
    public class InfoBarUpdateMessage(InfoBarViewModel vm) : ValueChangedMessage<InfoBarViewModel>(vm)
    {
    }
}
