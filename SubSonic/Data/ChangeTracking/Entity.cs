using SubSonic.Data.DynamicProxies;
using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Schema;
using SubSonic.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public void SetDbComputedProperties(IEntityProxy<TEntity> fromDb)
        {
            DynamicProxyBuilder<TEntity>.ProxyStub.SetDbComputedProperties(this, fromDb);
        }

        public void EnsureForeignKeys()
        {
            MethodInfo setForeignKeysInfo = typeof(DbContextAccessor).GetMethod(nameof(DbContextAccessor.SetForeignKeyProperty));

            Parallel.ForEach(Model.Properties, property =>
            {
                if (property.EntityPropertyType != DbEntityPropertyType.Navigation)
                {
                    return;
                }

                setForeignKeysInfo.MakeGenericMethod(Model.EntityModelType, property.PropertyType).Invoke(Accessor, new object[] { Data, Model.EntityModelType.GetProperty(property.PropertyName) });
            });
        }
    }

    public class Entity
        : IEntityProxy
    {
        internal DbContextAccessor Accessor => new DbContextAccessor(DbContext.Current);

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

        public Type ModelType
        {
            get
            {
                return Model.EntityModelType;
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

        private bool _isDeleted;

        public bool IsDeleted
        {
            get => Proxy?.IsDeleted ?? _isDeleted;
            set
            {
                if (Proxy.IsNotNull())
                {
                    Proxy.IsDeleted = value;
                }
                _isDeleted = value;
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

        public void SetKeyData(IEnumerable<object> keyData)
        {
            string[] keys = Model
                        .GetPrimaryKey()
                        .ToArray();

            for (int i = 0, n = keys.Length; i < n; i++)
            {
                Type.GetProperty(keys[i]).SetValue(Data, keyData.ElementAt(i));
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Entity);
        }

        public bool Equals(Entity right)
        {
            return this == right;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
