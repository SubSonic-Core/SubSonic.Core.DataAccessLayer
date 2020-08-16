using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    public class DynamicProxyWrapper<TEntity>
        : DynamicProxyWrapper
    {
        internal DynamicProxyWrapper(SubSonicContext dbContext) 
            : base(typeof(TEntity), dbContext)
        {
        }

        public override Type Type
        {
            get
            {
                if (!IsElegibleForProxy)
                {
                    return null;
                }

#pragma warning disable IDE0074 // Use compound assignment
                return ProxyType ?? (ProxyType = DynamicProxy.BuildDerivedTypeFrom<TEntity>(DbContext));
#pragma warning restore IDE0074 // Use compound assignment
            }
        }
}

    public abstract class DynamicProxyWrapper
    {
        internal DynamicProxyWrapper(Type baseType, SubSonicContext dbContext)
        {
            BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        
        protected Type BaseType { get; }

        protected SubSonicContext DbContext { get; }

        public bool IsElegibleForProxy => DbContext.Options.EnableProxyGeneration;

        protected Type ProxyType { get; set; }

        public abstract Type Type { get; }
    }
}
