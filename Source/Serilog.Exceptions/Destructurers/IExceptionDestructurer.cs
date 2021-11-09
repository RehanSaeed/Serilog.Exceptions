namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections.Generic;
using Serilog.Exceptions.Core;

/// <summary>
/// Interface that all exception destructurers that want to be registered must implement. Exception destructurer
/// must explicitly declare which types it can destructure using <see cref="TargetTypes"/> property.
/// </summary>
public interface IExceptionDestructurer
{
    /// <summary>
    /// Gets a collection of types of exception that the destructurer can handle.
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
    Type[] TargetTypes { get; }
#pragma warning restore CA1819 // Properties should not return arrays

    /// <summary>
    /// Destructures given <paramref name="exception"/>. It's properties are to be put in
    /// <paramref name="propertiesBag"/>.
    /// </summary>
    /// <param name="exception">The exception that will be destructured.</param>
    /// <param name="propertiesBag">The bag when destructured properties will be put.</param>
    /// <param name="destructureException">Function that can be used to destructure inner exceptions if there are
    /// any.</param>
    void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException);
}
