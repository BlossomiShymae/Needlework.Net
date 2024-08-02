using System;

namespace Needlework.Net.Desktop
{
    public class TextUpdatedEventArgs(string text) : EventArgs
    {
        public string Text { get; } = text;
    }
}
