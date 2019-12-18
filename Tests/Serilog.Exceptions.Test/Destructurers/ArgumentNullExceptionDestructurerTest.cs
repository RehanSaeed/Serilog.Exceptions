namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ArgumentNullExceptionDestructurerTest
    {
        [Fact]
        public void ArgumentNullException_ParamNameIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var argumentException = new ArgumentNullException("testParamName", "MSG");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }
    }
}
