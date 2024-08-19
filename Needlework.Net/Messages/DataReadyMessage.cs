using CommunityToolkit.Mvvm.Messaging.Messages;
using Needlework.Net.Core;

namespace Needlework.Net.Messages
{
    public class DataReadyMessage(LcuSchemaHandler handler) : ValueChangedMessage<LcuSchemaHandler>(handler)
    {
    }
}
