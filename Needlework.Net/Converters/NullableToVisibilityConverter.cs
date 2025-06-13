using ReactiveUI;
using System;

namespace Needlework.Net.Converters
{
    public class NullableToVisibilityConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if (typeof(object).IsAssignableFrom(fromType) && toType == typeof(bool))
            {
                return 100;
            }
            return 0;
        }

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            result = from != null;
            return true;
        }
    }
}
