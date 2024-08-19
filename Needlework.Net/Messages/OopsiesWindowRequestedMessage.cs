using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Messages
{
    public class OopsiesWindowRequestedMessage(string text) : ValueChangedMessage<string>(text)
    {
    }
}
