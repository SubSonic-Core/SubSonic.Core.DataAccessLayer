using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using SubSonic.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class Entity<TEntity>
        : Entity
        , IEntityProxy<TEntity>
    {
        public Entity(TEntity data) 
            : base(data)
        {
        }

        public new TEntity Data => (TEntity)base.Data;
    }

    public class Entity
        : IEntityProxy
    {
        public Entity(object data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public object Data { get; }

        protected Type Type => Data.GetType();

        protected IEntityProxy Proxy
        {
            get
            {
                if (Data is IEntityProxy proxy)
                {
                    return proxy;
                }
                return null;
            }
        }

        protected IDbEntityModel Model
        {
            get
            {
                return DbContext.DbModel.GetEntityModel(Proxy.IsNotNull() ? Type.BaseType : Type);
            }
        }

        public IEnumerable<object> KeyData
        {
            get
            {
                string[] keys = Model
                        .GetPrimaryKey()
                        .ToArray();

                return Type.GetProperties()
                    .Where(property => keys.Any(key => key.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(property => property.GetValue(Data, null))
                    .ToArray();
            }
        }

        private bool _isDirty;

        public bool IsDirty
        {
            get => Proxy?.IsDirty ?? _isDirty;
            set
            {
                if (Proxy.IsNotNull())
                {
                    Proxy.IsDirty = value;
                }
                _isDirty = value;
            }
        }

        private bool _isNew;

        public bool IsNew
        {
            get => Proxy?.IsNew ?? _isNew;
            set
            {
                if (Proxy.IsNotNull())
                {
                    Proxy.IsNew = value;
                }
                _isNew = value;
            }
        }

        public void OnPropertyChange(IEntityProxy proxy)
        {
            if (proxy.IsNotNull())
            {
                proxy.IsDirty = true;
            }

            IsDirty = true;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Entity);
        }

        public bool Equals(Entity right)
        {
            return this == right;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (left is null && right is null)
            {
                return true;
            }
            else if (left is null || right is null)
            {
                return false;
            }

            return left.Data.GetHashCode() == right.Data.GetHashCode();
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}
