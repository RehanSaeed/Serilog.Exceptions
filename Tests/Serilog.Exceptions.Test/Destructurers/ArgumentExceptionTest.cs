namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ArgumentExceptionTest
    {
        [Fact]
        public void ArgumentException_ParamNameIsAttachedAsProperty()
        {
            var argumentException = new ArgumentException("MSG", "testParamName");
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }
    }
}
