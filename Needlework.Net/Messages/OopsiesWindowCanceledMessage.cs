using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Messages
{
    public class OopsiesWindowCanceledMessage(object? data) : ValueChangedMessage<object?>(data)
    {
    }
}
