using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Messages
{
    public class OopsiesDialogRequestedMessage(string text) : ValueChangedMessage<string>(text)
    {
    }
}
