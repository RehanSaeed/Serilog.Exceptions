namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using static LogJsonOutputUtils;

    public class TaskCanceledExceptionDestructurerTest : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;

        public TaskCanceledExceptionDestructurerTest()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task TaskCanceledException_SimplePropertiesAreAttached()
        {
            // Arrange
            async Task<Exception> Wait(CancellationToken ct)
            {
                try
                {
                    await Task.Delay(500, ct).ConfigureAwait(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }

            this.cancellationTokenSource.CancelAfter(100);

            // Act
            var ex = await Wait(this.cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            var tce = ex.Should().BeOfType<TaskCanceledException>().Which;
            var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
            Assert_ContainsPropertyWithValue(exceptionDetails, "CancellationToken", "CancellationRequested: true");

            var taskProperty = ExtractProperty(exceptionDetails, "Task");
            var taskPropertyObject = taskProperty.Value.Should().BeOfType<JObject>().Which;
            Assert_ContainsPropertyWithValue(taskPropertyObject, "Status", "Canceled");
            Assert_ContainsPropertyWithValue(taskPropertyObject, "CreationOptions", "None");

        }

        public void Dispose() => this.cancellationTokenSource?.Dispose();
    }
}