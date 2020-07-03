using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    internal static class Error
    {
        public static Exception ArgumentNull(string parameter)
        {
            return new ArgumentNullException(parameter);
        }

        public static Exception Argument(string message, string parameter)
        {
            return new ArgumentException(message, parameter);
        }

        public static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported(string message)
        {
            return new NotSupportedException(message);
        }
    }
}
