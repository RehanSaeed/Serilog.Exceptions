namespace Serilog.Exceptions.Test.Destructurers;

using System;
using Xunit;
using static LogJsonOutputUtils;

public class ArgumentNullExceptionDestructurerTest
{
    [Fact]
    public void ArgumentNullException_ParamNameIsAttachedAsProperty()
    {
        var argumentException = new ArgumentNullException("testParamName", "MSG");
        Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
    }
}
