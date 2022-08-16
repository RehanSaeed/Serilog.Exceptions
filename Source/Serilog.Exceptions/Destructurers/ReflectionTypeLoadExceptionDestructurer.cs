namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections.Generic;
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
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        base.Destructure(exception, propertiesBag, destructureException);

        var reflectionTypeLoadException = (ReflectionTypeLoadException)exception;
        if (reflectionTypeLoadException.LoaderExceptions is not null)
        {
            propertiesBag.AddProperty(
                nameof(ReflectionTypeLoadException.LoaderExceptions),
                GetLoaderExceptionsValue(reflectionTypeLoadException.LoaderExceptions, destructureException));
        }
    }

    private static List<IReadOnlyDictionary<string, object?>> GetLoaderExceptionsValue(
        Exception?[] exceptions,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        var loaderExceptionValues = new List<IReadOnlyDictionary<string, object?>>();
        foreach (var exception in exceptions)
        {
            if (exception is not null)
            {
                var dictionary = destructureException(exception);
                if (dictionary is not null)
                {
                    loaderExceptionValues.Add(dictionary);
                }
            }
        }

        return loaderExceptionValues;
    }
}
