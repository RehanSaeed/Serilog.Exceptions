namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Serilog.Exceptions.Core;

/// <summary>
/// Destructurer for <see cref="RegexMatchTimeoutException"/>.
/// </summary>
public class RegexMatchTimeoutExceptionDestructurer : ExceptionDestructurer
{
    /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
    public override Type[] TargetTypes => new Type[]
    {
            typeof(RegexMatchTimeoutException),
    };

    /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
    public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        base.Destructure(exception, propertiesBag, destructureException);

        var typedException = (RegexMatchTimeoutException)exception;
        propertiesBag.AddProperty(nameof(RegexMatchTimeoutException.Input), typedException.Input);
        propertiesBag.AddProperty(nameof(RegexMatchTimeoutException.Pattern), typedException.Pattern);
        propertiesBag.AddProperty(nameof(RegexMatchTimeoutException.MatchTimeout), typedException.MatchTimeout.ToString("c"));
    }
}
