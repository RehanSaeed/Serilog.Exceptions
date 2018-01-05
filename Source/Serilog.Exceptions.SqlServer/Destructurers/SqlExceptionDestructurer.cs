namespace Serilog.Exceptions.SqlServer.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using Serilog.Exceptions.Destructurers;

    public class SqlExceptionDestructurer : ExceptionDestructurer
    {
        public SqlExceptionDestructurer(List<string> ignoredProperties)
            : base(ignoredProperties)
        {
        }

        public override Type[] TargetTypes => new[] { typeof(SqlException) };

        public override void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var sqlException = (SqlException)exception;

            // Don't log ClientConnectionId because it's not supported on Mono.
            // data.Add(nameof(SqlException.ClientConnectionId), sqlException.ClientConnectionId);
            data.AddIfNotIgnored(nameof(SqlException.Class), sqlException.Class, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(SqlException.LineNumber), sqlException.LineNumber, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(SqlException.Number), sqlException.Number, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(SqlException.Server), sqlException.Server, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(SqlException.State), sqlException.State, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(SqlException.Errors), sqlException.Errors.Cast<SqlError>().ToList(), this.IgnoredProperties);
        }
    }
}