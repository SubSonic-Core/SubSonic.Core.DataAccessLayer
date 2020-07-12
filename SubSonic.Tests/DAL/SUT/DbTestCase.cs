using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL
{
    using Extensions.Test;
    using Infrastructure.Schema;

    public class DbTestCase<TModel>
        : IDbTestCase
        where TModel: class
    { 
        private readonly Expression<Func<TModel, bool>> selector;

        public DbTestCase(bool withUDTT, string expected)
        {
            if (expected.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(expected));
            }

            UseDefinedTableType = withUDTT;
            Expectation = expected;
        }

        public DbTestCase(bool withUDTT, string expected, Expression<Func<TModel, bool>> selector)
            : this(withUDTT, expected)
        {
            this.selector = selector;
        }

        public IDbEntityModel EntityModel => SubSonicContext.DbModel.GetEntityModel<TModel>();

        public ISubSonicCollection DataSet => SubSonicContext.Current.Set<TModel>();

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
            if (DataSet is ISubSonicSetCollection<TModel> dataSet)
            {
                return (IEntityProxy)dataSet.FindByID(keyData);
            }

            return null;
        }

        public IEnumerable FetchAll()
        {
            IQueryable data = null;

            if (DataSet is ISubSonicCollection<TModel> dataSet)
            {
                dataSet.Clear();

                if (!(selector is null))
                {
                    dataSet = (ISubSonicCollection<TModel>)dataSet.Where(selector);
                }

                data = dataSet.Load();
            }

            return data;
        }

        public int Count()
        {
            if (DataSet is ISubSonicCollection<TModel> dataSet)
            {
                if (selector is null)
                {
                    return dataSet.Count();
                }
                else
                {
                    return dataSet.Count(selector);
                }
            }

            return default(int);
        }
    }
}
