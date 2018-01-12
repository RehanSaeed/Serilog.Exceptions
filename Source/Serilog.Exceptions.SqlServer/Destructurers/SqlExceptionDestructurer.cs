namespace Serilog.Exceptions.SqlServer.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using Serilog.Exceptions.Destructurers;

    public class SqlExceptionDestructurer : ExceptionDestructurer
    {
        public override Type[] TargetTypes => new[] { typeof(SqlException) };

        public override void Destructure(Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var sqlException = (SqlException)exception;

            propertiesBag.AddProperty(nameof(SqlException.ClientConnectionId), sqlException.ClientConnectionId);
            propertiesBag.AddProperty(nameof(SqlException.Class), sqlException.Class);
            propertiesBag.AddProperty(nameof(SqlException.LineNumber), sqlException.LineNumber);
            propertiesBag.AddProperty(nameof(SqlException.Number), sqlException.Number);
            propertiesBag.AddProperty(nameof(SqlException.Server), sqlException.Server);
            propertiesBag.AddProperty(nameof(SqlException.State), sqlException.State);
            propertiesBag.AddProperty(nameof(SqlException.Errors), sqlException.Errors.Cast<SqlError>().ToArray());
        }
    }
}