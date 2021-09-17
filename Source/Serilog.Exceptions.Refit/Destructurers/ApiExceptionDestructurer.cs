namespace Serilog.Exceptions.Refit.Destructurers
{
    using System;
    using System.Collections.Generic;
    using global::Refit;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;

    /// <summary>
    /// A destructurer for the Refit <see cref="ApiException"/>.
    /// </summary>
    /// <seealso cref="ExceptionDestructurer" />
    public class ApiExceptionDestructurer : ExceptionDestructurer
    {
        private readonly bool destructureCommonExceptionProperties = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExceptionDestructurer"/> class.
        /// </summary>
        /// <param name="destructureCommonExceptionProperties">Destructure common public Exception properties or not.</param>
        public ApiExceptionDestructurer(bool destructureCommonExceptionProperties = true) => this.destructureCommonExceptionProperties = destructureCommonExceptionProperties;

        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => new[] { typeof(ApiException) };

        /// <inheritdoc />
        public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
        {
            if (this.destructureCommonExceptionProperties)
            {
                base.Destructure(exception, propertiesBag, destructureException);
            }
            else
            {
                // Argument checks are usually done in <see cref="ExceptionDestructurer.Destructure"/>
                // but as we didn't call this method we need to do the checks here.
                if (exception is null)
                {
                    throw new ArgumentNullException(nameof(propertiesBag));
                }

                if (propertiesBag is null)
                {
                    throw new ArgumentNullException(nameof(propertiesBag));
                }

                if (destructureException is null)
                {
                    throw new ArgumentNullException(nameof(destructureException));
                }
            }

#pragma warning disable CA1062 // Validate arguments of public methods
            var apiException = (ApiException)exception;
            propertiesBag.AddProperty(nameof(ApiException.Uri), apiException.Uri);
            propertiesBag.AddProperty(nameof(ApiException.Content), apiException.Content);
            propertiesBag.AddProperty(nameof(ApiException.StatusCode), apiException.StatusCode);
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
