using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Needlework.Net.Messages
{
    public class EditorUpdateMessage(EditorUpdate editorUpdate) : ValueChangedMessage<EditorUpdate>(editorUpdate)
    {
    }

    public class EditorUpdate
    {
        public string Text { get; }
        public string Key { get; }

        public EditorUpdate(string text, string key)
        {
            Text = text;
            Key = key;
        }
    }
}
