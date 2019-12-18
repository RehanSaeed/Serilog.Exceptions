namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ArgumentExceptionDestructurerTest
    {
        [Fact]
        public void ArgumentException_ParamNameIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var argumentException = new ArgumentException("MSG", "testParamName");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }
    }
}
