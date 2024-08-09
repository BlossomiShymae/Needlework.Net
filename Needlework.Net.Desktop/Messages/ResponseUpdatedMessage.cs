using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Desktop.Messages
{
    public class ResponseUpdatedMessage(string data) : ValueChangedMessage<string>(data)
    {
    }
}
