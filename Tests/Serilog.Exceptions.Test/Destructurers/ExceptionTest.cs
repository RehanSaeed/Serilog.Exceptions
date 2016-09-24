namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ExceptionTest
    {
        [Fact]
        public void ApplicationException_ContainsMessage()
        {
            var applicationException = new ApplicationException("MSG");
            Test_LoggedExceptionContainsProperty(applicationException, "Message", "MSG");
        }

        [Fact]
        public void ApplicationException_ContainsHelpLink()
        {
            var applicationException = new ApplicationException() { HelpLink = "HELP LINK" };
            Test_LoggedExceptionContainsProperty(applicationException, "HelpLink", "HELP LINK");
        }

        [Fact]
        public void ApplicationException_ContainsSource()
        {
            var applicationException = new ApplicationException() { Source = "SOURCE" };
            Test_LoggedExceptionContainsProperty(applicationException, "Source", "SOURCE");
        }

        [Fact]
        public void ApplicationException_WithoutStackTrace_ContainsNullStackTrace()
        {
            var applicationException = new ApplicationException();
            Test_LoggedExceptionContainsProperty(applicationException, "StackTrace", null);
        }

        [Fact]
        public void ApplicationException_ContainsData()
        {
            var applicationException = new ApplicationException();
            applicationException.Data["SOMEKEY"] = "SOMEVALUE";

            JObject rootObject = LogAndDestructureException(applicationException);
            JObject exceptionDetail = ExtractExceptionDetails(rootObject);

            JProperty dataProperty = Assert.Single(exceptionDetail.Properties(), x => x.Name == "Data");
            JObject dataObject = Assert.IsType<JObject>(dataProperty.Value);

            JProperty someKeyProperty = Assert.Single(dataObject.Properties(), x => x.Name == "SOMEKEY");
            JValue someKeyValue = Assert.IsType<JValue>(someKeyProperty.Value);
            Assert.Equal("SOMEVALUE", someKeyValue.Value);
        }

        [Fact]
        public void ApplicationException_WithStackTrace_ContainsStackTrace()
        {
            try
            {
                throw new ApplicationException();
            }
            catch (ApplicationException ex)
            {
                Test_LoggedExceptionContainsProperty(ex, "StackTrace", ex.StackTrace.ToString());
            }
        }

        [Fact]
        public void ApplicationException_ContainsType()
        {
            var applicationException = new ApplicationException();
            Test_LoggedExceptionContainsProperty(applicationException, "Type", "System.ApplicationException");
        }
    }
}
