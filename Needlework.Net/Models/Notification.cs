using FluentAvalonia.UI.Controls;
using System;

namespace Needlework.Net.Models
{
    public record Notification(string Title, string Message, InfoBarSeverity InfoBarSeverity, TimeSpan? Duration = null, string? Url = null)
    {
    }
}
