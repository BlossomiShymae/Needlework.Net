using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Desktop.Messages
{
    public class OopsiesWindowCanceledMessage(object? data) : ValueChangedMessage<object?>(data)
    {
    }
}
