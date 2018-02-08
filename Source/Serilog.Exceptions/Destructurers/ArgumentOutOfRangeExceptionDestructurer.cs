namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructurer for <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public class ArgumentOutOfRangeExceptionDestructurer : ArgumentExceptionDestructurer
    {
        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => new[]
        {
            typeof(ArgumentOutOfRangeException)
        };

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var argumentException = (ArgumentOutOfRangeException)exception;

            propertiesBag.AddProperty(nameof(ArgumentOutOfRangeException.ActualValue), argumentException.ActualValue);
        }
    }
}