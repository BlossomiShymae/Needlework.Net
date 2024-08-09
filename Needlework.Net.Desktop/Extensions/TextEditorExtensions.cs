using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Indentation.CSharp;

namespace Needlework.Net.Desktop.Extensions
{
    public static class TextEditorExtensions
    {
        public static void ApplyJsonEditorSettings(this TextEditor textEditor)
        {
            textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(textEditor.Options);
            textEditor.TextArea.RightClickMovesCaret = true;
            textEditor.TextArea.Options.EnableHyperlinks = false;
            textEditor.TextArea.Options.EnableEmailHyperlinks = false;
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");

            var purple = Color.FromRgb(189, 147, 249);
            var yellow = Color.FromRgb(241, 250, 140);
            var cyan = Color.FromRgb(139, 233, 253);
            textEditor.SyntaxHighlighting.GetNamedColor("Bool").Foreground = new SimpleHighlightingBrush(purple);
            textEditor.SyntaxHighlighting.GetNamedColor("Number").Foreground = new SimpleHighlightingBrush(purple);
            textEditor.SyntaxHighlighting.GetNamedColor("String").Foreground = new SimpleHighlightingBrush(yellow);
            textEditor.SyntaxHighlighting.GetNamedColor("Null").Foreground = new SimpleHighlightingBrush(purple);
            textEditor.SyntaxHighlighting.GetNamedColor("FieldName").Foreground = new SimpleHighlightingBrush(cyan);
            textEditor.SyntaxHighlighting.GetNamedColor("Punctuation").Foreground = new SimpleHighlightingBrush(yellow);
        }
    }
}
