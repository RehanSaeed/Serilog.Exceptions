namespace Serilog.Exceptions.Test.Destructurers;

using Xunit;
using static LogJsonOutputUtils;

public class ArgumentExceptionDestructurerTest
{
    [Fact]
    public void ArgumentException_ParamNameIsAttachedAsProperty()
    {
        var argumentException = new ArgumentException("MSG", "testParamName");
        Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
    }
}
