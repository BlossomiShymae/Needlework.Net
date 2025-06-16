using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using BitFaster.Caching;
using BitFaster.Caching.Lru;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Needlework.Net
{
    public class ViewLocator : IDataTemplate
    {
        private class ObjectComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                    return false;

                return x.Equals(y);
            }

            public int GetHashCode([DisallowNull] object obj)
            {
                return obj.GetHashCode();
            }
        }

        private readonly ICache<object, Control> _controlCache = new ConcurrentLruBuilder<object, Control>()
            .WithExpireAfterAccess(TimeSpan.FromMinutes(5))
            .WithKeyComparer(new ObjectComparer())
            .WithCapacity(1024)
            .WithMetrics()
            .Build();

        private readonly Dictionary<Type, Func<Control>> _viewRegister = [];

        public ViewLocator()
        {
            _controlCache.Events.Value!.ItemRemoved += (source, args) =>
            {
                var descendants = args.Value!.GetVisualDescendants();
                foreach (var descendant in descendants)
                {
                    if (descendant.DataContext is INotifyPropertyChanged key)
                    {
                        _controlCache.TryRemove(key, out _);
                    }
                }
            };
        }

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

            bool isCold = !_controlCache.TryGet(data!, out var res);
            if (isCold)
            {
                res ??= activator();
                _controlCache.AddOrUpdate(data!, res);
            }

            res!.DataContext = data;
            return res;
        }

        public bool Match(object? data) => data is INotifyPropertyChanged;
    }
}
