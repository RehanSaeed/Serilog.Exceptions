namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections.Generic;
using Serilog.Exceptions.Core;

/// <summary>
/// Destructurer for <see cref="ArgumentException"/>.
/// </summary>
public class ArgumentExceptionDestructurer : ExceptionDestructurer
{
    /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
    public override Type[] TargetTypes =>
    [
        typeof(ArgumentException),
        typeof(ArgumentNullException)
    ];

    /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
    public override void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
        var argumentException = (ArgumentException)exception;
        propertiesBag.AddProperty(nameof(ArgumentException.ParamName), argumentException.ParamName);
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}
