using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.Converters
{
    public class EnumerableToVisibilityConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if (typeof(IEnumerable<object>).IsAssignableFrom(fromType) && toType == typeof(bool))
            {
                return 100;
            }
            return 0;
        }

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            try
            {
                result = from is IEnumerable<object> values && values.Any();
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}
