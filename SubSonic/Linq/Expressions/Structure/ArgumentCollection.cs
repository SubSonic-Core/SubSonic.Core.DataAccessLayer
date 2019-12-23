using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Expressions.Structure
{
    public class ArgumentCollection<TArgument>
        : IDisposable
    {
        private static Stack<ArgumentCollection<TArgument>> __instance;

        private readonly Queue<TArgument> arguments;
        private readonly string method;

        protected ArgumentCollection()
        {
            if (__instance.IsNull())
            {
                __instance = new Stack<ArgumentCollection<TArgument>>();
            }

            __instance.Push(this);
        }

        public ArgumentCollection(string method)
            : this()
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("", nameof(method));
            }

            arguments = new Queue<TArgument>();

            this.method = method;
        }

        public string Method 
        { 
            get
            {
                if (__instance.TryPeek(out ArgumentCollection<TArgument> _instance))
                {
                    return _instance.method;
                }
                return null;
            }
        }

        public int Count
        {
            get
            {
                if (__instance.TryPeek(out ArgumentCollection<TArgument> _instance))
                {
                    return _instance.arguments.Count;
                }
                return 0;
            }
        }

        public ArgumentCollection<TArgument> FocusOn(string method)
        {
            return new ArgumentCollection<TArgument>(method);
        }

        public TArgument Peek()
        {
            if (__instance.TryPeek(out ArgumentCollection<TArgument> _instance))
            {
                return _instance.arguments.Peek();
            }
            return default(TArgument);
        }

        public void Push(TArgument argument)
        {
            if(__instance.Peek().Method == "base")
            {
                throw new InvalidOperationException();
            }

            __instance.Peek().arguments.Enqueue(argument);
        }

        public TArgument Pop()
        {
            return __instance.Peek().arguments.Dequeue();
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (__instance.IsNotNull())
                    {
                        __instance.Pop();

                        if (__instance.Count == 0)
                        {
                            __instance = null;
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArgumentCollection()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
