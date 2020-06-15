using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Extensions.Test
{
    public class AlteredState<TInterface, TType>
        : IDisposable
        where TInterface: class
        where TType: class, TInterface, new()
    {
        private bool disposedValue;
        private TInterface current;
        private TType archived;
        private object state;

        public AlteredState(TInterface current, object state)
        {
            this.current = current ?? throw new ArgumentNullException(nameof(current));
            this.state = state ?? throw new ArgumentNullException(nameof(state));

            Initialize();
        }

        private void Initialize()
        {
            archived = new TType();

            foreach (PropertyInfo property in typeof(TType).GetProperties())
            {
                if (property.CanWrite)
                {
                    property.SetValue(archived, property.GetValue(current));
                }
            }
        }

        public AlteredState<TInterface, TType> Apply()
        {
            foreach(PropertyInfo property in state.GetType().GetProperties())
            {
                typeof(TType).GetProperty(property.Name).SetValue(current, property.GetValue(state));
            }

            return this;
        }

        public void Restore()
        {
            foreach (PropertyInfo property in typeof(TType).GetProperties())
            {
                if (property.CanWrite)
                {
                    property.SetValue(current, property.GetValue(archived));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Restore();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AlteredState()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
