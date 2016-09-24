namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using static LogJsonOutputUtils;

    public class AggregateExceptionTest
    {
        [Fact]
        public void AggregateException_WithTwoArgumentExceptions_TheyAreSerializedInInnerExceptionsProperty()
        {
            var argumentException1 = new ArgumentException("MSG1", "testParamName1");
            var argumentException2 = new ArgumentException("MSG1", "testParamName2");
            var aggregateException = new AggregateException(argumentException1, argumentException2);

            JObject rootObject = LogAndDestructureException(aggregateException);
            JArray innerExceptions = ExtractInnerExceptionsProperty(rootObject);

            Assert.Equal(2, innerExceptions.Count);
            Assert_ContainsPropertyWithValue(Assert.IsType<JObject>(innerExceptions[0]), "ParamName", "testParamName1");
            Assert_ContainsPropertyWithValue(Assert.IsType<JObject>(innerExceptions[1]), "ParamName", "testParamName2");
        }
    }
}
