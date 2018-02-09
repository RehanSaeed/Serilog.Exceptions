namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructurer for <see cref="ReflectionTypeLoadException"/>.
    /// </summary>
    public class ReflectionTypeLoadExceptionDestructurer : ExceptionDestructurer
    {
        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => new[] { typeof(ReflectionTypeLoadException) };

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var reflectionTypeLoadException = (ReflectionTypeLoadException)exception;

            if (reflectionTypeLoadException.LoaderExceptions != null)
            {
                propertiesBag.AddProperty(
                    nameof(ReflectionTypeLoadException.LoaderExceptions),
                    reflectionTypeLoadException.LoaderExceptions.Select(destructureException).ToList());
            }
        }
    }
}
