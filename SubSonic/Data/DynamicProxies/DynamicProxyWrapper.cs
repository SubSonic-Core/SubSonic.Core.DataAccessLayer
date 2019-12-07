using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    public class DynamicProxyWrapper
    {
        private readonly Type baseType;

        internal DynamicProxyWrapper(Type baseType)
        {
            this.baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
        }

        public bool IsElegibleForProxy => baseType.GetProperties().Any(property => property.GetMethod.IsVirtual);

        private Type proxyType;

        public Type Type
        {
            get
            {
                if(!IsElegibleForProxy)
                {
                    return null;
                }

                return proxyType ?? (proxyType = DynamicProxy.BuildDerivedTypeFrom(baseType));
            }
        }
    }
}
