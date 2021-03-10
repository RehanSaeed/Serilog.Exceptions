namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructurer for <see cref="SocketException"/>.
    /// </summary>
    public class SocketExceptionDestructurer : ExceptionDestructurer
    {
        // obtained from https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketerror
        private static readonly IReadOnlyDictionary<SocketError, string> SocketErrorDocumentationBySocketError =
            new Dictionary<SocketError, string>
                {
                    { SocketError.AccessDenied, "An attempt was made to access a Socket in a way that is forbidden by its access permissions." },
                    { SocketError.AddressAlreadyInUse, "Only one use of an address is normally permitted." },
                    { SocketError.AddressFamilyNotSupported, "The address family specified is not supported. This error is returned if the IPv6 address family was specified and the IPv6 stack is not installed on the local machine. This error is returned if the IPv4 address family was specified and the IPv4 stack is not installed on the local machine." },
                    { SocketError.AddressNotAvailable, "The selected IP address is not valid in this context." },
                    { SocketError.AlreadyInProgress, "The nonblocking Socket already has an operation in progress." },
                    { SocketError.ConnectionAborted, "The connection was aborted by the .NET Framework or the underlying socket provider." },
                    { SocketError.ConnectionRefused, "The remote host is actively refusing a connection." },
                    { SocketError.ConnectionReset, "The connection was reset by the remote peer." },
                    { SocketError.DestinationAddressRequired, "A required address was omitted from an operation on a Socket." },
                    { SocketError.Disconnecting, "A graceful shutdown is in progress." },
                    { SocketError.Fault, "An invalid pointer address was detected by the underlying socket provider." },
                    { SocketError.HostDown, "The operation failed because the remote host is down." },
                    { SocketError.HostNotFound, "No such host is known. The name is not an official host name or alias." },
                    { SocketError.HostUnreachable, "There is no network route to the specified host." },
                    { SocketError.InProgress, "A blocking operation is in progress." },
                    { SocketError.Interrupted, "A blocking Socket call was canceled." },
                    { SocketError.InvalidArgument, "An invalid argument was supplied to a Socket member." },
                    { SocketError.IOPending, "The application has initiated an overlapped operation that cannot be completed immediately." },
                    { SocketError.IsConnected, "The Socket is already connected." },
                    { SocketError.MessageSize, "The datagram is too long." },
                    { SocketError.NetworkDown, "The network is not available." },
                    { SocketError.NetworkReset, "The application tried to set KeepAlive on a connection that has already timed out." },
                    { SocketError.NetworkUnreachable, "No route to the remote host exists." },
                    { SocketError.NoBufferSpaceAvailable, "No free buffer space is available for a Socket operation." },
                    { SocketError.NoData, "The requested name or IP address was not found on the name server." },
                    { SocketError.NoRecovery, "The error is unrecoverable or the requested database cannot be located." },
                    { SocketError.NotConnected, "The application tried to send or receive data, and the Socket is not connected." },
                    { SocketError.NotInitialized, "The underlying socket provider has not been initialized." },
                    { SocketError.NotSocket, "A Socket operation was attempted on a non-socket." },
                    { SocketError.OperationAborted, "The overlapped operation was aborted due to the closure of the Socket." },
                    { SocketError.OperationNotSupported, "The address family is not supported by the protocol family." },
                    { SocketError.ProcessLimit, "Too many processes are using the underlying socket provider." },
                    { SocketError.ProtocolFamilyNotSupported, "The protocol family is not implemented or has not been configured." },
                    { SocketError.ProtocolNotSupported, "The protocol is not implemented or has not been configured." },
                    { SocketError.ProtocolOption, "An unknown, invalid, or unsupported option or level was used with a Socket." },
                    { SocketError.ProtocolType, "The protocol type is incorrect for this Socket." },
                    { SocketError.Shutdown, "A request to send or receive data was disallowed because the Socket has already been closed." },
                    { SocketError.SocketError, "An unspecified Socket error has occurred." },
                    { SocketError.SocketNotSupported, "The support for the specified socket type does not exist in this address family." },
                    { SocketError.Success, "The Socket operation succeeded." },
                    { SocketError.SystemNotReady, "The network subsystem is unavailable." },
                    { SocketError.TimedOut, "The connection attempt timed out, or the connected host has failed to respond." },
                    { SocketError.TooManyOpenSockets, "There are too many open sockets in the underlying socket provider." },
                    { SocketError.TryAgain, "The name of the host could not be resolved. Try again later." },
                    { SocketError.TypeNotFound, "The specified class was not found." },
                    { SocketError.VersionNotSupported, "The version of the underlying socket provider is out of range." },
                    { SocketError.WouldBlock, "An operation on a nonblocking socket cannot be completed immediately." },
                };

        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => new[]
        {
            typeof(SocketException),
        };

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var socketException = (SocketException)exception;
            propertiesBag.AddProperty(nameof(SocketException.SocketErrorCode), socketException.SocketErrorCode);
            if (SocketErrorDocumentationBySocketError.TryGetValue(socketException.SocketErrorCode, out var documentation))
            {
                propertiesBag.AddProperty(nameof(SocketException.SocketErrorCode) + "Message", documentation);
            }
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
