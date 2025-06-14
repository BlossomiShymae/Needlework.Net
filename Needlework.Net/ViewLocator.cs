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
                throw new Exception("Data type name is null.");
            }
            if (!name.Contains("ViewModel"))
            {
                throw new Exception("Data type name must end with 'ViewModel'.");
            }

            name = name.Replace("ViewModel", "View");
            var type = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name == name)
                .FirstOrDefault();

            if (type is null)
            {
                throw new Exception("Data type has no view.");
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
