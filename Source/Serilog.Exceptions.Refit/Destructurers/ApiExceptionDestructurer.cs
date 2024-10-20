namespace Serilog.Exceptions.Refit.Destructurers;

using global::Refit;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;

/// <summary>
/// A destructurer for the Refit <see cref="ApiException"/>.
/// </summary>
/// <seealso cref="ExceptionDestructurer" />
/// <remarks>
/// Initializes a new instance of the <see cref="ApiExceptionDestructurer"/> class.
/// </remarks>
/// <param name="destructureCommonExceptionProperties">Destructure common public Exception properties or not.</param>
/// <param name="destructureHttpContent">Destructure the HTTP body. This is left optional due to possible security and log size concerns.</param>
public class ApiExceptionDestructurer(bool destructureCommonExceptionProperties = true, bool destructureHttpContent = false) :
    ExceptionDestructurer
{
    /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
    public override Type[] TargetTypes => [typeof(ApiException), typeof(ValidationApiException)];

    /// <inheritdoc />
    public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        if (destructureCommonExceptionProperties)
        {
            base.Destructure(exception, propertiesBag, destructureException);
        }
        else
        {
            // Argument checks are usually done in <see cref="ExceptionDestructurer.Destructure"/>
            // but as we didn't call this method we need to do the checks here.
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(propertiesBag);
            ArgumentNullException.ThrowIfNull(destructureException);
#else
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (propertiesBag is null)
            {
                throw new ArgumentNullException(nameof(propertiesBag));
            }

            if (destructureException is null)
            {
                throw new ArgumentNullException(nameof(destructureException));
            }
#endif
        }

#pragma warning disable CA1062 // Validate arguments of public methods
        var apiException = (ApiException)exception;
        if (destructureHttpContent)
        {
            propertiesBag.AddProperty(nameof(ApiException.Content), apiException.Content);
        }

        propertiesBag.AddProperty(nameof(ApiException.Uri), apiException.Uri);
        propertiesBag.AddProperty(nameof(ApiException.StatusCode), apiException.StatusCode);
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}
