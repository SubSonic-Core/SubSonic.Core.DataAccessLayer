using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static PropertyInfo GetPropertyInfo(this object source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName);
        }
    }
}
