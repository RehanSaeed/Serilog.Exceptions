namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Text.RegularExpressions;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Xunit;
    using static LogJsonOutputUtils;

    public class RegexMatchTimeoutExceptionDestructurerTests
    {
        [Fact]
        public void RegexMatchTimeoutException_ParamsAttachedAsProperties()
        {
            var exception = new RegexMatchTimeoutException("input", "pattern", TimeSpan.FromSeconds(1));

            var optionsBuilder = new DestructuringOptionsBuilder()
                .WithDestructurers(new IExceptionDestructurer[]
                {
                    new RegexMatchTimeoutExceptionDestructurer(),
                });

            var loggedExceptionDetails = ExtractExceptionDetails(LogAndDestructureException(exception, optionsBuilder));

            Assert_ContainsPropertyWithValue(loggedExceptionDetails, nameof(RegexMatchTimeoutException.Input), exception.Input);
            Assert_ContainsPropertyWithValue(loggedExceptionDetails, nameof(RegexMatchTimeoutException.Pattern), exception.Pattern);
            Assert_ContainsPropertyWithValue(loggedExceptionDetails, nameof(RegexMatchTimeoutException.MatchTimeout), exception.MatchTimeout.ToString("c"));
        }
    }
}
