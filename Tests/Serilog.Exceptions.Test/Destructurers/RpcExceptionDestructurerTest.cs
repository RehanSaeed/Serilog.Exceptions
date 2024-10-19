namespace Serilog.Exceptions.Test.Destructurers;

using global::Grpc.Core;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Grpc.Destructurers;
using Xunit;
using static LogJsonOutputUtils;

public class RpcExceptionDestructurerTest
{
    [Fact]
    public void RpcException_StatusCodeIsLoggedAsProperty()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new RpcExceptionDestructurer()]);
        var rpcException = new RpcException(new Status(StatusCode.Aborted, string.Empty));

        Test_LoggedExceptionContainsProperty(rpcException, nameof(RpcException.Status.StatusCode), nameof(StatusCode.Aborted), options);
    }

    [Fact]
    public void RpcException_StatusDetailIsLoggedAsProperty()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new RpcExceptionDestructurer()]);
        var testDetail = "details";
        var rpcException = new RpcException(new Status(StatusCode.Aborted, testDetail));

        Test_LoggedExceptionContainsProperty(rpcException, nameof(RpcException.Status.Detail), testDetail, options);
    }

    [Fact]
    public void RpcException_TrailersAreLoggedAsProperty()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new RpcExceptionDestructurer()]);
        const string stringTrailerKey1 = "key1";
        const string stringTrailerValue1 = "stringTrailerValue1";
        const string stringTrailerKey2 = "key2";
        const string stringTrailerValue2 = "stringTrailerValue2";
        var metadata = new Metadata { { stringTrailerKey1, stringTrailerValue1 }, { stringTrailerKey2, stringTrailerValue2 } };

        var rpcException = new RpcException(new Status(StatusCode.Aborted, string.Empty), metadata);

        Test_LoggedExceptionContainsProperty(rpcException, $"{nameof(RpcException.Trailers)}.{stringTrailerKey1}", stringTrailerValue1, options);
        Test_LoggedExceptionContainsProperty(rpcException, $"{nameof(RpcException.Trailers)}.{stringTrailerKey2}", stringTrailerValue2, options);
    }

    [Fact]
    public void RpcException_BinaryTrailersAreNotLoggedAsProperty()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new RpcExceptionDestructurer()]);
        const string stringTrailerKey1 = "key-bin";
        var metadata = new Metadata { { stringTrailerKey1, [1]} };

        var rpcException = new RpcException(new Status(StatusCode.Aborted, string.Empty), metadata);

        Test_LoggedExceptionDoesNotContainProperty(rpcException, $"{nameof(RpcException.Trailers)}.{stringTrailerKey1}", options);
    }
}
