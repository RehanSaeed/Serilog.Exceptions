namespace Serilog.Exceptions.Test.Destructurers;

using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using static LogJsonOutputUtils;

public sealed class TaskCanceledExceptionDestructurerTest : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();

    [Fact]
    public void TaskCanceledException_SimplePropertiesAreAttached()
    {
        this.cancellationTokenSource.Cancel();
        var task = Task.FromCanceled(this.cancellationTokenSource.Token);

        var ex = new TaskCanceledException(task);

        var tce = Assert.IsType<TaskCanceledException>(ex);
        var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
        Assert_ContainsPropertyWithValue(exceptionDetails, nameof(TaskCanceledException.CancellationToken), "CancellationRequested: true");

        var taskProperty = ExtractProperty(exceptionDetails, nameof(TaskCanceledException.Task));
        var taskPropertyObject = Assert.IsType<JObject>(taskProperty.Value);
        Assert_ContainsPropertyWithValue(taskPropertyObject, nameof(Task.Status), nameof(TaskStatus.Canceled));
        Assert_ContainsPropertyWithValue(taskPropertyObject, nameof(Task.CreationOptions), nameof(TaskCreationOptions.None));
    }

    [Fact]
    public void TaskCanceledException_TaskWithSomeCreationOptions_TheyAreDestructured()
    {
        var task = new Task(() => { }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

        var ex = new TaskCanceledException(task);

        var tce = Assert.IsType<TaskCanceledException>(ex);
        var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
        var taskProperty = ExtractProperty(exceptionDetails, nameof(TaskCanceledException.Task));
        var taskPropertyObject = Assert.IsType<JObject>(taskProperty.Value);
        Assert_ContainsPropertyWithValue(taskPropertyObject, nameof(Task.CreationOptions), $"{nameof(TaskCreationOptions.PreferFairness)}, {nameof(TaskCreationOptions.LongRunning)}");
    }

    [Fact]
    public void TaskCanceledException_TaskNull()
    {
        var ex = new TaskCanceledException();

        var tce = Assert.IsType<TaskCanceledException>(ex);
        var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(tce));
        Assert_ContainsPropertyWithValue(exceptionDetails, nameof(TaskCanceledException.Task), null);
    }

    [Fact]
    public void FaultedTaskCanceledException_SimplePropertiesAreAttached()
    {
        var innerException = new Exception("Inner exception message");
        var task = Task.FromException(innerException);
        var ex = new TaskCanceledException(task);

        var exceptionDetails = ExtractExceptionDetails(LogAndDestructureException(ex));

        Assert_ContainsPropertyWithValue(exceptionDetails, "CancellationToken", "CancellationRequested: false");

        var taskProperty = ExtractProperty(exceptionDetails, nameof(TaskCanceledException.Task));
        var taskPropertyObject = Assert.IsType<JObject>(taskProperty.Value);
        Assert_ContainsPropertyWithValue(taskPropertyObject, nameof(Task.Status), nameof(TaskStatus.Faulted));
        Assert_ContainsPropertyWithValue(taskPropertyObject, nameof(Task.CreationOptions), nameof(TaskCreationOptions.None));
        var taskException = ExtractProperty(taskPropertyObject, nameof(Task.Exception));
        var taskExceptionObject = Assert.IsType<JObject>(taskException.Value);

        var typeOfTaskException = ExtractProperty(taskExceptionObject, "Type");
        var jsonProperty = Assert.IsType<JProperty>(typeOfTaskException);
        var jsonValue = Assert.IsType<JValue>(jsonProperty.Value);
        var value = Assert.IsType<string>(jsonValue.Value);
        Assert.Equal("System.AggregateException", value);
    }

    public void Dispose() => this.cancellationTokenSource?.Dispose();
}
