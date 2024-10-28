using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Needlework.Net
{
    public class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<object, Control> _controlCache = [];

        public Control Build(object? data)
        {
            var name = data?.GetType().Name;
            if (name is null)
            {
                return new TextBlock { Text = "Data is null or has no name." };
            }

            name = name.Replace("ViewModel", "View");
            var type = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name == name)
                .FirstOrDefault();

            if (type is null)
            {
                return new TextBlock { Text = $"No view for {name}." };
            }

            if (!_controlCache.TryGetValue(data!, out var res))
            {
                res ??= (Control)Activator.CreateInstance(type)!;
                _controlCache[data!] = res;
            }

            res.DataContext = data;
            return res;
        }

        public bool Match(object? data) => data is INotifyPropertyChanged;
    }
}
