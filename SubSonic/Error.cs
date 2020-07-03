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

        public static Exception NotSupported(string message)
        {
            return new NotSupportedException(message);
        }

        public static Exception DivideByZero()
        {
            return new DivideByZeroException();
        }

        public static Exception InvalidOperation()
        {
            return new InvalidOperationException();
        }
    }
}
