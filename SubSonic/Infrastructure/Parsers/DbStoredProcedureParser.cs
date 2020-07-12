using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SubSonic.Infrastructure
{
    using Infrastructure.Factory;
    using Infrastructure.Schema;
    using Linq;

    internal static class DbStoredProcedureParser
    {
        private static readonly Parsers.Helper helper = new Parsers.Helper();

        public static DbStoredProcedure ParseStoredProcedure(object procedure)
        {
            IEnumerable<DbStoredProcedureParameter> parameters = ParseStoredProcedureParameters(procedure);

            return new DbStoredProcedure(
                GenerateSql(procedure, parameters),
                GenerateName(procedure),
                GetIsNonQuery(procedure),
                GenerateSqlParameters(procedure, parameters));

        }

        private static string GenerateName(object procedure)
        {
            return helper.StoreProcedureName(procedure.GetType());
        }

        private static bool GetIsNonQuery(object procedure)
        {
            return helper.IsNonQuery(procedure.GetType());
        }

        private static string GenerateSql(object procedure, IEnumerable<DbStoredProcedureParameter> parameters)
        { 
            int CountOfReturnParameters = parameters.Count(x => x.Direction == ParameterDirection.ReturnValue);
            if (CountOfReturnParameters == 0)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "EXEC {0} {1}",
                    helper.StoreProcedureName(procedure.GetType()),
                    string.Join(", ", parameters.Select(p => $"@{p.Name} = @{p.Name}{(p.Direction == ParameterDirection.Output ? " out" : "")}")));
            }
            else if (CountOfReturnParameters == 1)
            {
                DbStoredProcedureParameter parameter = parameters.Single(x => x.Direction == ParameterDirection.ReturnValue);

                return string.Format(
                    CultureInfo.CurrentCulture,
@"DECLARE @{0} [{1}];
EXEC @{0} = {2} {3}",
                    parameter.Name,
                    TypeConvertor.ToSqlDbType(parameter.DbType),
                    helper.StoreProcedureName(procedure.GetType()),
                    string.Join(", ", parameters.Where(x => x.Direction != ParameterDirection.ReturnValue).Select(p => $"@{p.Name} = @{p.Name}{(p.Direction == ParameterDirection.Output ? " out" : "")}")));
            }
            else
            {
                throw new InvalidOperationException(/*can not have more than one return parameter*/);
            }
        }

        private static DbParameter[] GenerateSqlParameters(object procedure, IEnumerable<DbStoredProcedureParameter> parameters)
        {
            List<DbParameter> Parameters = new List<DbParameter>();

            foreach (DbStoredProcedureParameter oInfo in parameters)
            {
                IDbEntityModel model = null;

                object value;
                if (oInfo.IsUserDefinedTable)
                {
                    if (SubSonicContext.DbModel.TryGetEntityModel(oInfo.PropertyInfo.PropertyType.GetQualifiedType(), out model))
                    {
                        value = helper.GetUserDefinedTableValue(model, oInfo.PropertyInfo, procedure);
                    }
                    else
                    {
                        value = helper.GetUserDefinedTableValue(oInfo.PropertyInfo, procedure);
                    }
                }
                else
                {
                    value = oInfo.PropertyInfo.GetValue(procedure);
                }

                Parameters.Add(CreateParameter(
                      oInfo.Name
                    , value
                    , oInfo.IsMandatory
                    , oInfo.Size
                    , oInfo.IsUserDefinedTable
                    , oInfo.IsUserDefinedTable ? (model?.QualifiedName ?? helper.GetUserDefinedTableType(oInfo.PropertyInfo)) : null
                    //, oInfo.DbType
                    , oInfo.Direction));
            }

            return Parameters.ToArray();
        }

        private static DbParameter CreateParameter(string name, object value, bool mandatory, int size,
                                           bool isUserDefinedTableParameter, string udtType, ParameterDirection direction)
        {
            if (SubSonicContext.ServiceProvider.GetService<DbProviderFactory>() is SubSonicDbProvider client)
            {
                return client.CreateParameter(name, value, mandatory, size, isUserDefinedTableParameter, udtType, direction);
            }

            throw new NotSupportedException();
        }

        private static IEnumerable<DbStoredProcedureParameter> ParseStoredProcedureParameters(object procedure)
        {
            Collection<DbStoredProcedureParameter> parameters = new Collection<DbStoredProcedureParameter>();

            foreach(PropertyInfo info in procedure.GetType().GetProperties())
            {
                DbParameterAttribute attribute = info.GetCustomAttribute<DbParameterAttribute>();

                if (attribute.IsNotNull())
                {
                    parameters.Add(new DbStoredProcedureParameter()
                    {
                        Name = attribute.IsNotNull(x => x.Name, info.Name),
                        IsUserDefinedTable = helper.IsUserDefinedTableParameter(info),
                        PropertyInfo = info,
                        IsMandatory = helper.ParameterIsMandatory(attribute.Options),
                        DbType = attribute.IsNotNull(x => x.DbType, info.PropertyType.GetDbType()),
                        Direction = attribute.IsNotNull(x => x.Direction, ParameterDirection.Input),
                        Size = attribute.IsNotNull(x => x.Size)
                    });
                }
            }

            return parameters;
        }
    }
}
