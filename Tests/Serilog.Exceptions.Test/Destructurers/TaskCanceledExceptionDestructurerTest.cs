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
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        [Fact]
        public void TaskCanceledException_SimplePropertiesAreAttached()
        {
            // Arrange
            this.cancellationTokenSource.Cancel();
            var task = Task.FromCanceled(this.cancellationTokenSource.Token);

            // Act
            var ex = new TaskCanceledException(task);

            // Assert
            var tce = ex.Should().BeOfType<TaskCanceledException>().Which;
            var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
            Assert_ContainsPropertyWithValue(exceptionDetails, "CancellationToken", "CancellationRequested: true");

            var taskProperty = ExtractProperty(exceptionDetails, "Task");
            var taskPropertyObject = taskProperty.Value.Should().BeOfType<JObject>().Which;
            Assert_ContainsPropertyWithValue(taskPropertyObject, "Status", "Canceled");
            Assert_ContainsPropertyWithValue(taskPropertyObject, "CreationOptions", "None");
        }

        [Fact]
        public void TaskCanceledException_TaskNull()
        {
            // Arrange

            // Act
            var ex = new TaskCanceledException();

            // Assert
            var tce = ex.Should().BeOfType<TaskCanceledException>().Which;
            var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
            Assert_ContainsPropertyWithValue(exceptionDetails, "Task", "null");
        }

        [Fact]
        public void FaultedTaskCanceledException_SimplePropertiesAreAttached()
        {
            // Arrange
            var innerException = new Exception("Inner exception message");
            var task = Task.FromException(innerException);
            var ex = new TaskCanceledException(task);

            // Act
            var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(ex));

            // Assert
            Assert_ContainsPropertyWithValue(exceptionDetails, "CancellationToken", "CancellationRequested: false");

            var taskProperty = ExtractProperty(exceptionDetails, "Task");
            var taskPropertyObject = taskProperty.Value.Should().BeOfType<JObject>().Which;
            Assert_ContainsPropertyWithValue(taskPropertyObject, "Status", "Faulted");
            Assert_ContainsPropertyWithValue(taskPropertyObject, "CreationOptions", "None");
            var taskException = ExtractProperty(taskPropertyObject, "Exception");
            var taskExceptionObject = taskException.Should().BeOfType<JProperty>()
                .Which.Value.Should().BeOfType<JObject>()
                .Which;

            var typeOfTaskException = ExtractProperty(taskExceptionObject, "Type");
            typeOfTaskException.Should().BeOfType<JProperty>()
                .Which.Value.Should().BeOfType<JValue>()
                .Which.Value.Should().BeOfType<string>()
                .Which.Should().Be("System.AggregateException");
        }

        public void Dispose() => this.cancellationTokenSource?.Dispose();
    }
}