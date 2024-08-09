using CommunityToolkit.Mvvm.Messaging.Messages;
using Needlework.Net.Core;

namespace Needlework.Net.Desktop.Messages
{
    public class DataReadyMessage(LcuSchemaHandler handler) : ValueChangedMessage<LcuSchemaHandler>(handler)
    {
    }
}
