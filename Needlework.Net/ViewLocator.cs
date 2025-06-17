using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Needlework.Net
{
    public class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Func<Control>> _viewRegister = [];

        public void Register<T>(Func<Control> viewActivator)
            where T : INotifyPropertyChanged
        {
            _viewRegister[typeof(T)] = viewActivator;
        }

        public Control Build(object? data)
        {
            if (!_viewRegister.TryGetValue(data!.GetType(), out var activator))
            {
                throw new Exception("Data type has no registered view activator.");
            }

            var res = activator();
            res!.DataContext = data;
            return res;
        }

        public bool Match(object? data) => data is INotifyPropertyChanged;
    }
}
