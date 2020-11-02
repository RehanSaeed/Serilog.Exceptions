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
            var socketException = new SocketException((int)SocketError.HostUnreachable);
            Test_LoggedExceptionContainsProperty(socketException, nameof(SocketException.SocketErrorCode), nameof(SocketError.HostUnreachable));
        }

        [Fact]
        public void SocketException_SocketErrorCodeDocumentationIsAttachedAsProperty()
        {
            var socketException = new SocketException((int)SocketError.ConnectionRefused);
            Test_LoggedExceptionContainsProperty(socketException, nameof(SocketException.SocketErrorCode) + "Message", "The remote host is actively refusing a connection.");
        }

        [Fact]
        public void SocketException_SocketErrorCodeDocumentationWhenSocketErrorCodeUnknown()
        {
            var socketException = new SocketException(42);
            Test_LoggedExceptionDoesNotContainProperty(socketException, nameof(SocketException.SocketErrorCode) + "Message");
        }
    }
}
