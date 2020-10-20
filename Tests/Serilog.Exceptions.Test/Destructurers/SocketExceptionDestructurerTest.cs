namespace Serilog.Exceptions.Test.Destructurers
{
    using System.Net.Sockets;
    using Xunit;
    using static LogJsonOutputUtils;

    public class SocketExceptionDestructurerTest
    {
        [Fact]
        public void SocketException_SocketErrorCodeIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var socketException = new SocketException((int)SocketError.HostUnreachable);
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(socketException, nameof(SocketException.SocketErrorCode), nameof(SocketError.HostUnreachable));
        }

        [Fact]
        public void SocketException_SocketErrorCodeDocumentationIsAttachedAsProperty()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var socketException = new SocketException((int)SocketError.ConnectionRefused);
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(socketException, nameof(SocketException.SocketErrorCode) + "Documentation", "The remote host is actively refusing a connection.");
        }

        [Fact]
        public void SocketException_SocketErrorCodeDocumentationWhenSocketErrorCodeUnknown()
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            var socketException = new SocketException(42);
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            Test_LoggedExceptionContainsProperty(socketException, nameof(SocketException.SocketErrorCode) + "Documentation", "Unknown SocketErrorCode value 42");
        }
    }
}
