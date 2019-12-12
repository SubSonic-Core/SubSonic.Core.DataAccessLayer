using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SubSonic
{
    internal static partial class InternalExtensions
    {
        public static string Format(this string instance, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, instance, args);
        }

        public static bool IsNullOrEmpty(this string instance)
        {
            return string.IsNullOrEmpty(instance.IsNotNull(str => str.Trim()));
        }

        public static bool IsNotNullOrEmpty(this string instance)
        {
            return IsNullOrEmpty(instance);
        }
    }
}
