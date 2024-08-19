using CommunityToolkit.Mvvm.Messaging.Messages;
using Needlework.Net.Models;

namespace Needlework.Net.Messages
{
    public class DataReadyMessage(OpenApiDocumentWrapper wrapper) : ValueChangedMessage<OpenApiDocumentWrapper>(wrapper)
    {
    }
}
