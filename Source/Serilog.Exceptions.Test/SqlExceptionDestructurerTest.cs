namespace Serilog.Exceptions.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;
    using Destructurers;
    using Xunit;

    public class SqlExceptionDestructurerTest
    {
        [Fact]
        public void Destructure_ClientConnectionIdAvailable()
        {
            var data = new Dictionary<string, object>();
            var clientConnectionId = Guid.NewGuid();

            var sqlErrorCtor = typeof(SqlError).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int) }, null);
            var sqlError = sqlErrorCtor.Invoke(new object[] { 42, (byte)0, (byte)0, "local-server", "connection not responding", "GetSum41", 65 });

            var sqlErrorCollectionCtor = typeof(SqlErrorCollection).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            var sqlErrorCollection = sqlErrorCollectionCtor.Invoke(null);
            var sqlErrorCollectionAdd = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            sqlErrorCollectionAdd.Invoke(sqlErrorCollection, new[] { sqlError });

            var sqlExCtor = typeof(SqlException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) }, null);
            var exception = (SqlException)sqlExCtor.Invoke(new object[] { "Sql exception testing", sqlErrorCollection, null, clientConnectionId });

            var sqlExDestructurer = new SqlExceptionDestructurer();

            sqlExDestructurer.Destructure(exception, data, null);

            Assert.NotNull(data);
            if (data.ContainsKey("ClientConnectionId"))
            {
                Assert.Equal(clientConnectionId, data["ClientConnectionId"]);
            }
        }
    }
}
