namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructurer for <see cref="OperationCanceledException"/>.
    /// </summary>
    public class OperationCanceledExceptionDestructurer : ExceptionDestructurer
    {
        private const string CancellationTokenTrue = "CancellationRequested: true";
        private const string CancellationTokenFalse = "CancellationRequested: false";

        private static readonly Type[] TargetExceptionTypes =
        {
            typeof(OperationCanceledException)
        };

        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => TargetExceptionTypes;

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var operationCancelledException = (OperationCanceledException)exception;
            propertiesBag.AddProperty(
                nameof(OperationCanceledException.CancellationToken),
                DestructureCancellationToken(operationCancelledException.CancellationToken));
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <summary>
        /// Destructures the cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The destructured cancellation token.</returns>
        internal static object DestructureCancellationToken(in CancellationToken cancellationToken) =>
            cancellationToken.IsCancellationRequested ? CancellationTokenTrue : CancellationTokenFalse;
    }
}