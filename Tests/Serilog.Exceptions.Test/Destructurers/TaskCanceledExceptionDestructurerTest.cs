namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static LogJsonOutputUtils;

    public class TaskCanceledExceptionDestructurerTest: IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;

        public TaskCanceledExceptionDestructurerTest()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        [Fact]
        public async Task TaskCanceledException_SimplePropertiesAreAttached()
        {
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

            var ex = await Wait(this.cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.NotNull(ex);
            var tce = Assert.IsType<TaskCanceledException>(ex);

            var exceptionDetails = LogAndDestructureException(tce);
        }

        public void Dispose() => this.cancellationTokenSource?.Dispose();
    }
}