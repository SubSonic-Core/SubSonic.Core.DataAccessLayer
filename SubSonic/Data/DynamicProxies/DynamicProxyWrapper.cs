using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    internal class DynamicProxyWrapper
    {
        private readonly Type baseType;

        public DynamicProxyWrapper(Type baseType)
        {
            this.baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
        }

        public bool IsElegibleForProxy => baseType.GetProperties().Any(property => property.GetMethod.IsVirtual);

        private Type proxyType;

        public Type ProxyType
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
