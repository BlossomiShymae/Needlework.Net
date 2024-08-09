using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Desktop.Messages
{
    public class OopsiesWindowRequestedMessage(string text) : ValueChangedMessage<string>(text)
    {
    }
}
