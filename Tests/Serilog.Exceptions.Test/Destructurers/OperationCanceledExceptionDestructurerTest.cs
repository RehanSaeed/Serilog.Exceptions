namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Threading;
    using Xunit;
    using static LogJsonOutputUtils;

    public class OperationCanceledExceptionDestructurerTest : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        public OperationCanceledExceptionDestructurerTest() =>
            this.cancellationTokenSource = new CancellationTokenSource();

        [Fact]
        public void OperationCanceledException_CancellationTokenIsAttachedAsProperty()
        {
            this.cancellationTokenSource.Cancel();

            var oce = new OperationCanceledException(this.cancellationTokenSource.Token);

            Test_LoggedExceptionContainsProperty(oce, "CancellationToken", "CancellationRequested: true");
        }

        [Fact]
        public void OperationCanceledException_DisposedCancellationisAttachedAsProperty()
        {
            var token = this.cancellationTokenSource.Token;
            this.cancellationTokenSource.Dispose();

            var oce = new OperationCanceledException(token);

            Test_LoggedExceptionContainsProperty(oce, "CancellationToken", "CancellationRequested: false");
        }

        public void Dispose() => this.cancellationTokenSource?.Dispose();
    }
}
