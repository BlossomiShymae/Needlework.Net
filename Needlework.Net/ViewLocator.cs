using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.ComponentModel;

namespace Needlework.Net
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null) return new TextBlock { Text = "data was null" };

            var name = param.GetType().FullName!
                .Replace("ViewModels", "Views")
                .Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null) return (Control)Activator.CreateInstance(type)!;
            else return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is INotifyPropertyChanged;
        }
    }
}
