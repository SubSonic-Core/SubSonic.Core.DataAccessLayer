using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Tests.DAL
{
    using Infrastructure.Schema;
    using Extensions.Test;
    using SubSonic.Infrastructure;
    using System.Linq;

    public class DbTestCase<TModel>
        : IDbTestCase
        where TModel: class
    {
        public DbTestCase(bool withUDTT, string expected)
        {
            if (expected.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(expected));
            }

            UseDefinedTableType = withUDTT;
            Expectation = expected;
        }

        public IDbEntityModel EntityModel => DbContext.DbModel.GetEntityModel<TModel>();

        public ISubSonicCollection DataSet => DbContext.Current.Set<TModel>();

        public bool UseDefinedTableType { get; }

        public string Expectation { get; }

        public void Insert(IEnumerable<IEntityProxy> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<IEntityProxy> entities)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<IEntityProxy> entities)
        {
            if (DataSet is ISubSonicCollection<TModel> dataSet)
            {
                foreach(IEntityProxy proxy in entities)
                {
                    if (proxy is IEntityProxy<TModel> entity)
                    {
                        dataSet.Remove(entity.Data);
                    }
                }
            }
        }

        public IEntityProxy FindByID(params object[] keyData)
        {
            if (DataSet is ISubSonicDbCollection<TModel> dataSet)
            {
                return (IEntityProxy)dataSet.FindByID(keyData).Single();
            }

            return null;
        }

        public IEnumerable FetchAll()
        {
            IQueryable<TModel> data = null;

            if (DataSet is ISubSonicDbCollection<TModel> dataSet)
            {
                dataSet.Clear();

                data = dataSet.Load();
            }

            return data;
        }

        public int Count()
        {
            if (DataSet is ISubSonicCollection<TModel> dataSet)
            {
                return dataSet.Count();
            }

            return default(int);
        }
    }
}
