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
        private static readonly Type[] TargetExceptionTypes =
        {
            typeof(OperationCanceledException)
        };

        public override Type[] TargetTypes => TargetExceptionTypes;

        public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object>> innerDestructure)
        {
            base.Destructure(exception, propertiesBag, innerDestructure);
            var oce = (OperationCanceledException)exception;
            propertiesBag.AddProperty(nameof(OperationCanceledException.CancellationToken), DestructureCancellationToken(oce.CancellationToken));
        }

        internal static object DestructureCancellationToken(in CancellationToken ct)
        {
            return ct.IsCancellationRequested
                ? "CancellationRequested: true"
                : "CancellationRequested: false";
        }
    }
}