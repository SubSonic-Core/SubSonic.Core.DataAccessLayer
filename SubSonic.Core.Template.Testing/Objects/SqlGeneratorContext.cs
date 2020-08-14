using Microsoft.Extensions.Logging;
using SubSonic.Extensions.SqlServer;

namespace SubSonic.CodeGenerator
{
    public class SqlGeneratorContext
        : GeneratorContext
    {
        public SqlGeneratorContext(string connection)
            : base(connection, LogLevel.Debug) { }

        public SqlGeneratorContext(string connection, LogLevel logLevel)
           : base(connection, logLevel) { }

        protected override void OnDbConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnDbConfiguring(builder);

            builder
                .UseSqlClient((config, options) =>
                {
                    config.ConnectionString = ConnectionString;
                });
        }
    }
}
