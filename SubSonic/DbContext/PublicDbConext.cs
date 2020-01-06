using System;
using System.Data;

using System.Collections.Generic;

namespace SubSonic
{
    using Infrastructure;
    using Infrastructure.Schema;
    using Linq;

    public partial class DbContext
    {
        public DbContextOptions Options { get; }

        public DbModel Model { get; }

        public static DbContext Current => ServiceProvider.GetService<DbContext>();

        public DbSetCollection<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return Instance.GetService<DbSetCollection<TEntity>>();
        }

        public DbDatabase Database => Instance.GetService<DbDatabase>();

        public IServiceProvider Instance => ServiceProvider;

        public bool SaveChanges()
        {
            bool result = false;

            try
            {
                using (SharedDbConnectionScope Scope = UseSharedDbConnection())
                {
                    foreach (var dataset in Cache)
                    {
                        IDbEntityModel model = Model.GetEntityModel(dataset.Key);

                        var insert = dataset.Value.Where(x => x.IsNew);
                        var update = dataset.Value.Where(x => !x.IsNew && x.IsDirty);

                        if (insert.Count() > 0)
                        {
                            SaveChanges(DbQueryType.Insert, model, insert);
                        }

                        if (update.Count() > 0)
                        {
                            SaveChanges(DbQueryType.Update, model, update);
                        }
                    }
                }
                result = true;
            }
            finally
            {

            }

            return result;
        }

        protected virtual void SaveChanges(DbQueryType queryType, IDbEntityModel model, IEnumerable<IEntityProxy> data)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            DbCommandQuery command = model.Commands[queryType];

            if (command is null || command.CommandType == CommandType.Text)
            {
                throw new NotImplementedException();
            }
            else if (command.CommandType == CommandType.StoredProcedure)
            {
                object procedure = Activator.CreateInstance(command.StoredProcedureType, data);

                Database.ExecuteStoredProcedure(procedure);

                data.ForEach(x =>
                {
                    if (x.KeyData.Count() == 0 || x.KeyData.IsSameAs(new object[] { 0 }))
                    {
                        Cache.Remove(x.ModelType, x);

                        return;
                    }

                    x.IsNew = false;
                    x.IsDirty = false;
                });
            }
        }

        /// <summary>
        /// Use a shared connection to get data faster and reduce connection overhead
        /// </summary>
        /// <returns></returns>
        public SharedDbConnectionScope UseSharedDbConnection() => Instance.GetService<SharedDbConnectionScope>();
    }
}
