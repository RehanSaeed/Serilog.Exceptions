namespace Serilog.Exceptions.MsSqlServer.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.SqlClient;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;

    /// <summary>
    /// A destructurer for <see cref="SqlException"/>.
    /// </summary>
    /// <seealso cref="ExceptionDestructurer" />
    public class SqlExceptionDestructurer : ExceptionDestructurer
    {
        /// <inheritdoc />
        public override Type[] TargetTypes => new[] { typeof(SqlException) };

        /// <inheritdoc />
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var sqlException = (SqlException)exception;
            propertiesBag.AddProperty(nameof(SqlException.ClientConnectionId), sqlException.ClientConnectionId);
            propertiesBag.AddProperty(nameof(SqlException.Class), sqlException.Class);
            propertiesBag.AddProperty(nameof(SqlException.LineNumber), sqlException.LineNumber);
            propertiesBag.AddProperty(nameof(SqlException.Number), sqlException.Number);
            propertiesBag.AddProperty(nameof(SqlException.Server), sqlException.Server);
            propertiesBag.AddProperty(nameof(SqlException.State), sqlException.State);
            propertiesBag.AddProperty(nameof(SqlException.Errors), sqlException.Errors.Cast<SqlError>().ToArray());
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
