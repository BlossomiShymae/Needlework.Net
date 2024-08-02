using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Needlework.Net.Desktop.ViewModels;
using System;

namespace Needlework.Net.Desktop
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return new TextBlock { Text = "data was null" };

            var name = param.GetType().FullName!.Replace("ViewModels", "Views")
                .Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object? data)
        {
            if (data is PageBase) return true;
            return false;
        }
    }
}
