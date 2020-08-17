using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SubSonic.Core;
using SubSonic.Extensions.SqlServer;
using SubSonic.Logging;
using SubSonic.Schema;
using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.CodeGenerator.Testing
{
    [TestFixture]
    public class GeneratorContextTests
    {
        private const string connection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DbSubSonic;Integrated Security=True";
        [Test]
        public void ShouldBeAbleToInstanciateTheGeneratorContext()
        {
            using var context = new SqlGeneratorContext(connection);

            Type providerFactoryType = typeof(SubSonicSqlClient);

            DbProviderFactories.TryGetFactory(providerFactoryType.Namespace, out DbProviderFactory factory);

            factory.Should().NotBeNull();
        }

        [Test]
        public void ShouldBeAbleToIdentifyEntityTypeAsView()
        {
            using var context = new SqlGeneratorContext(connection);

            IDbEntityModel model = context.Model.GetEntityModel<Models.Table>();

            model.DbObjectType.Should().Be(DbObjectTypeEnum.View);
        }

        [Test]
        public void DbSetCollectionForTablesGetSqlFromView()
        {
            using var context = new SqlGeneratorContext(connection);

            string expected = $@"SELECT [T1].[Catalog], [T1].[Schema], [T1].[Name]
FROM ({Models.Table.Query}) AS [T1]";
            
            Expression select = context.Tables.Expression;

            IDbQuery query = null;

            var logging = context.Instance.GetService<ISubSonicLogger<ISubSonicCollection<Models.Table>>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Table> builder = context.Instance.GetService<ISubSonicQueryProvider<Models.Table>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Be(expected);
        }

        [Test]
        public void DbSetCollectionForRelationshipsGetSqlFromView()
        {
            using var context = new SqlGeneratorContext(connection);

            string expected = $@"SELECT [T1].[TableName], [T1].[ColumnName], [T1].[ForiegnTableName], [T1].[ForiegnColumnName], [T1].[ConstraintName], [T1].[SchemaOwner]
FROM ({Models.Relationship.Query}) AS [T1]";

            Expression select = context.Relationships.Expression;

            IDbQuery query = null;

            var logging = context.Instance.GetService<ISubSonicLogger<ISubSonicCollection<Models.Table>>>();

            using (var perf = logging.Start("SQL Query Writer"))
            {
                FluentActions.Invoking(() =>
                {
                    ISubSonicQueryProvider<Models.Relationship> builder = context.Instance.GetService<ISubSonicQueryProvider<Models.Relationship>>();

                    query = builder.ToQuery(select);
                }).Should().NotThrow();
            }

            query.Sql.Should().NotBeNullOrEmpty();
            query.Sql.Should().Be(expected);
        }

        [Test]
        public void CanPullListOfTableObjectsFromDatabase()
        {
            using var context = new SqlGeneratorContext(connection, LogLevel.Trace);

            foreach (Models.Table table in context.Tables)
            {
                Console.WriteLine(table.ToString());
            }

            context.Tables.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void CanPullListOfColumnsObjectsFromDatabase()
        {
            using var context = new SqlGeneratorContext(connection, LogLevel.Trace);

            foreach (Models.Column column in context.Columns)
            {
                Console.WriteLine(column.ToString());
            }

            context.Columns.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void CanPullListOfRelationshipObjectsFromDatabase()
        {
            using var context = new SqlGeneratorContext(connection, LogLevel.Trace);

            foreach (Models.Relationship relationship in context.Relationships)
            {
                Console.WriteLine(relationship.ToString());
            }

            context.Relationships.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void CanNavigateRelationshipModel()
        {
            using var context = new SqlGeneratorContext(connection, LogLevel.Trace);

            foreach(Models.Table table in context.Tables)
            {
                table.WithOneRelationships.Load();
                table.WithManyRelationships.Load();

                Console.WriteLine(table);

                if (table.WithOneRelationships.Count > 0)
                {
                    foreach(Models.Relationship relationship in table.WithOneRelationships)
                    {
                        Console.WriteLine("{0} points to {1}".Format(relationship, relationship.ForiegnTable));
                    }
                }
                else
                {
                    Console.WriteLine("{0} no with one relationships detected".Format(table));
                }

                if (table.WithManyRelationships.Count > 0)
                {
                    foreach (Models.Relationship relationship in table.WithManyRelationships)
                    {
                        Console.WriteLine("{0} points to {1}".Format(relationship, relationship.Table));
                    }
                }
                else
                {
                    Console.WriteLine("{0} no with many relationships detected".Format(table));
                }

                Console.WriteLine();
            }
        }

        [Test]
        public void CanNavigateColumnModel()
        {
            using var context = new SqlGeneratorContext(connection, LogLevel.Trace);

            foreach (Models.Table table in context.Tables)
            {
                table.WithOneRelationships.Load();

                Console.WriteLine(table);

                if (table.Columns.Count > 0)
                {
                    foreach (Models.Column column in table.Columns.OrderBy(x => x.OrdinalPosition))
                    {
                        column.GetSqlDbType().ToString().ToUpperInvariant().Should().Be(column.DataType.ToUpperInvariant());

                        column.IsPrimaryKey.Should().Be(column.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase));

                        column.IsIdentity.Should().Be(column.IsPrimaryKey);

                        if (column.TableName == "Person" && column.ColumnName == "FullName")
                        {
                            column.IsComputed.Should().BeTrue();
                        }

                        column.GetClrType().Should().NotBeNull();

                        if (column.ColumnName.EndsWith("Date"))
                        {
                            column.GetClrType().Should().Be<DateTime>();
                        }

                        string simpleType = column.ToSimpleType();

                        if (column.GetClrType().IsValueType)
                        {
                            if (column.GetClrType() == typeof(int))
                            {
                                simpleType.Should().StartWith("int");
                            }
                            else if (column.GetClrType() == typeof(long))
                            {
                                simpleType.Should().StartWith("long");
                            }
                            else if (column.GetClrType() == typeof(bool))
                            {
                                simpleType.Should().StartWith("bool");
                            }
                            else if (column.GetClrType() == typeof(decimal))
                            {
                                simpleType.Should().StartWith("decimal");
                            }
                            else if (column.GetClrType() == typeof(double))
                            {
                                simpleType.Should().StartWith("double");
                            }
                            else if (column.GetClrType() == typeof(short))
                            {
                                simpleType.Should().StartWith("short");
                            }
                            else if (column.GetClrType() == typeof(float))
                            {
                                simpleType.Should().StartWith("float");
                            }
                            else if (column.GetClrType() == typeof(byte))
                            {
                                simpleType.Should().StartWith("byte");
                            }
                            else if (column.GetClrType() == typeof(byte[]))
                            {
                                simpleType.Should().StartWith("byte[]");
                            }

                            if (column.IsNullable)
                            {
                                simpleType.Should().EndWith("?");
                            }
                        }
                        else
                        {
                            if (column.GetClrType() == typeof(string))
                            {
                                simpleType.Should().Equals("string");
                            }
                            else if (column.GetClrType() == typeof(object))
                            {
                                simpleType.Should().Equals("object");
                            }
                        }

                        Console.WriteLine("{0} belongs to {1}".Format(column, column.Table));
                    }
                }
                else
                {
                    Console.WriteLine("{0} no columns detected".Format(table));
                }

                Console.WriteLine();
            }
        }
    }
}
