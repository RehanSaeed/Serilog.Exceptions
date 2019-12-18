namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ArgumentOutOfRangeExceptionDestructurerTest
    {
        [Fact]
        public void ArgumentOfOutRangeException_ParamNameIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var argumentException = new ArgumentOutOfRangeException("testParamName");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }

        [Fact]
        public void ArgumentOfOutRangeException_ActualValueIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var argumentException = new ArgumentOutOfRangeException("testParamName", "ACTUAL_VALUE", "MSG");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(argumentException, "ActualValue", "ACTUAL_VALUE");
        }
    }
}
