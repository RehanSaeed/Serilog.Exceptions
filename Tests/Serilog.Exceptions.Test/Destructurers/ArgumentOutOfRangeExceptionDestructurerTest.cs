namespace Serilog.Exceptions.Test.Destructurers;

using System;
using Xunit;
using static LogJsonOutputUtils;

public class ArgumentOutOfRangeExceptionDestructurerTest
{
    [Fact]
    public void ArgumentOfOutRangeException_ParamNameIsAttachedAsProperty()
    {
        var argumentException = new ArgumentOutOfRangeException("testParamName");
        Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
    }

    [Fact]
    public void ArgumentOfOutRangeException_ActualValueIsAttachedAsProperty()
    {
        var argumentException = new ArgumentOutOfRangeException("testParamName", "ACTUAL_VALUE", "MSG");
        Test_LoggedExceptionContainsProperty(argumentException, "ActualValue", "ACTUAL_VALUE");
    }
}
