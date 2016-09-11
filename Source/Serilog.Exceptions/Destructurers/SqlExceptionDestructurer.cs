namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    public class SqlExceptionDestructurer : ExceptionDestructurer
    {
#if NET45
        private readonly bool clientConnectionIdIsAvailable;

        public SqlExceptionDestructurer()
        {
            this.clientConnectionIdIsAvailable = typeof(SqlException).GetProperty("ClientConnectionId") != null;
        }
#endif

        public override Type[] TargetTypes
        {
            get { return new Type[] { typeof(SqlException) }; }
        }

        public override void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var sqlException = (SqlException)exception;
#if NET45
            if (this.clientConnectionIdIsAvailable)
            {
#endif
                data.Add(nameof(SqlException.ClientConnectionId), sqlException.ClientConnectionId);
#if NET45
            }
#endif
            data.Add(nameof(SqlException.Class), sqlException.Class);
            data.Add(nameof(SqlException.LineNumber), sqlException.LineNumber);
            data.Add(nameof(SqlException.Number), sqlException.Number);
            data.Add(nameof(SqlException.Server), sqlException.Server);
            data.Add(nameof(SqlException.State), sqlException.State);
            data.Add(nameof(SqlException.Errors), sqlException.Errors.Cast<SqlError>().ToList());
        }
    }
}