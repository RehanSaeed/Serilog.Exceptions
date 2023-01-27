namespace Serilog.Exceptions.Grpc.Destructurers;

using System;
using System.Collections.Generic;
using global::Grpc.Core;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;

/// <summary>
/// A destructurer for <see cref="RpcException"/>.
/// </summary>
/// <seealso cref="ExceptionDestructurer" />
public class RpcExceptionDestructurer : ExceptionDestructurer
{
    /// <inheritdoc />
    public override Type[] TargetTypes => new[] { typeof(RpcException) };

    /// <inheritdoc />
    public override void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
        base.Destructure(exception, propertiesBag, destructureException);

        var rpcException = (RpcException)exception;

#pragma warning disable CA1062 // Validate arguments of public methods
        propertiesBag.AddProperty(nameof(RpcException.Status.StatusCode), rpcException.Status.StatusCode);
        propertiesBag.AddProperty(nameof(RpcException.Status.Detail), rpcException.Status.Detail);

        foreach (var trailer in rpcException.Trailers)
        {
            if (trailer.IsBinary)
            {
                continue;
            }

            propertiesBag.AddProperty($"{nameof(RpcException.Trailers)}.{trailer.Key}", trailer.Value);
        }
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}
