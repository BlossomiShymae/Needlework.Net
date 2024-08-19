using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Messages
{
    public class ResponseUpdatedMessage(string data) : ValueChangedMessage<string>(data)
    {
    }
}
