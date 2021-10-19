namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Base class for more specific destructurers.
    /// It destructures all the standard properties that every <see cref="Exception"/> has.
    /// </summary>
    public class ExceptionDestructurer : IExceptionDestructurer
    {
        /// <summary>
        /// Gets a collection of exceptions types from standard library that do not have any custom property,
        /// so they can be destructured using generic exception destructurer.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public virtual Type[] TargetTypes
#pragma warning restore CA1819 // Properties should not return arrays
        {
            get
            {
                var targetTypes = new List<Type>
                    {
#pragma warning disable IDE0001 // Simplify Names
#if NET461 || NET472
                        typeof(Microsoft.SqlServer.Server.InvalidUdtException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.AccessViolationException),
                        typeof(System.AppDomainUnloadedException),
                        typeof(System.ApplicationException),
                        typeof(System.ArithmeticException),
                        typeof(System.ArrayTypeMismatchException),
                        typeof(System.CannotUnloadAppDomainException),
#endif
                        typeof(System.Collections.Generic.KeyNotFoundException),
#if !NETSTANDARD1_3
                        typeof(System.ComponentModel.Design.CheckoutException),
                        typeof(System.ComponentModel.InvalidAsynchronousStateException),
                        typeof(System.ComponentModel.InvalidEnumArgumentException),
#endif
#if NET461 || NET472
                        typeof(System.Configuration.SettingsPropertyIsReadOnlyException),
                        typeof(System.Configuration.SettingsPropertyNotFoundException),
                        typeof(System.Configuration.SettingsPropertyWrongTypeException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.ContextMarshalException),

                        typeof(System.Data.ConstraintException),
                        typeof(System.Data.DataException),
                        typeof(System.Data.DeletedRowInaccessibleException),
                        typeof(System.Data.DuplicateNameException),
                        typeof(System.Data.EvaluateException),
                        typeof(System.Data.InRowChangingEventException),
                        typeof(System.Data.InvalidConstraintException),
                        typeof(System.Data.InvalidExpressionException),
                        typeof(System.Data.MissingPrimaryKeyException),
                        typeof(System.Data.NoNullAllowedException),
#endif
#if NET461 || NET472
                        typeof(System.Data.OperationAbortedException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.Data.ReadOnlyException),
                        typeof(System.Data.RowNotInTableException),
                        typeof(System.Data.SqlTypes.SqlAlreadyFilledException),
                        typeof(System.Data.SqlTypes.SqlNotFilledException),
                        typeof(System.Data.StrongTypingException),
                        typeof(System.Data.SyntaxErrorException),
                        typeof(System.Data.VersionNotFoundException),
#endif
#if NET461 || NET472
                        typeof(System.Diagnostics.Eventing.Reader.EventLogInvalidDataException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogNotFoundException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogReadingException),
#endif
                        typeof(System.Diagnostics.Tracing.EventSourceException),
                        typeof(System.DataMisalignedException),
                        typeof(System.DivideByZeroException),
                        typeof(System.DllNotFoundException),
#if !NETSTANDARD1_3
                        typeof(System.DuplicateWaitObjectException),
                        typeof(System.EntryPointNotFoundException),
#endif
                        typeof(System.Exception),
                        typeof(System.FieldAccessException),
                        typeof(System.FormatException),
                        typeof(System.IndexOutOfRangeException),
                        typeof(System.InsufficientExecutionStackException),
#if !NETSTANDARD1_3
                        typeof(System.InsufficientMemoryException),
#endif
                        typeof(System.InvalidCastException),
                        typeof(System.InvalidOperationException),
                        typeof(System.InvalidProgramException),
                        typeof(System.InvalidTimeZoneException),
                        typeof(System.IO.DirectoryNotFoundException),
#if !NETSTANDARD1_3
                        typeof(System.IO.DriveNotFoundException),
#endif
                        typeof(System.IO.EndOfStreamException),
#if !NETSTANDARD1_3
                        typeof(System.IO.InternalBufferOverflowException),
#endif
                        typeof(System.IO.InvalidDataException),
                        typeof(System.IO.IOException),
#if !NETSTANDARD1_3
                        typeof(System.IO.IsolatedStorage.IsolatedStorageException),
#endif
                        typeof(System.IO.PathTooLongException),
#if NET461 || NET472
                        typeof(System.Management.Instrumentation.InstanceNotFoundException),
                        typeof(System.Management.Instrumentation.InstrumentationBaseException),
                        typeof(System.Management.Instrumentation.InstrumentationException),
#endif
                        typeof(System.MemberAccessException),
                        typeof(System.MethodAccessException),
#if !NETSTANDARD1_3
                        typeof(System.MulticastNotSupportedException),
#endif
                        typeof(System.Net.CookieException),
#if !NETSTANDARD1_3
                        typeof(System.Net.NetworkInformation.PingException),
                        typeof(System.Net.ProtocolViolationException),
#endif
                        typeof(System.NotImplementedException),
                        typeof(System.NotSupportedException),
                        typeof(System.NullReferenceException),
                        typeof(System.OutOfMemoryException),
                        typeof(System.OverflowException),
                        typeof(System.PlatformNotSupportedException),
                        typeof(System.RankException),
                        typeof(System.Reflection.AmbiguousMatchException),
#if !NETSTANDARD1_3
                        typeof(System.Reflection.CustomAttributeFormatException),
                        typeof(System.Reflection.InvalidFilterCriteriaException),
                        typeof(System.Reflection.TargetException),
#endif
                        typeof(System.Reflection.TargetInvocationException),
                        typeof(System.Reflection.TargetParameterCountException),
                        typeof(System.Resources.MissingManifestResourceException),
#if NET5_0_OR_GREATER || NETSTANDARD2_1
                        typeof(System.Runtime.AmbiguousImplementationException),
#endif
                        typeof(System.Runtime.InteropServices.COMException),
                        typeof(System.Runtime.InteropServices.InvalidComObjectException),
                        typeof(System.Runtime.InteropServices.InvalidOleVariantTypeException),
                        typeof(System.Runtime.InteropServices.MarshalDirectiveException),
                        typeof(System.Runtime.InteropServices.SafeArrayRankMismatchException),
                        typeof(System.Runtime.InteropServices.SafeArrayTypeMismatchException),
                        typeof(System.Runtime.InteropServices.SEHException),
#if NET461 || NET472
                        typeof(System.Runtime.Remoting.RemotingException),
                        typeof(System.Runtime.Remoting.RemotingTimeoutException),
                        typeof(System.Runtime.Remoting.ServerException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.Runtime.Serialization.SerializationException),
                        typeof(System.Security.Authentication.AuthenticationException),
                        typeof(System.Security.Authentication.InvalidCredentialException),
#endif
                        typeof(System.Security.Cryptography.CryptographicException),
#if !NETSTANDARD1_3
                        typeof(System.Security.Cryptography.CryptographicUnexpectedOperationException),
#endif
#if NET461 || NET472
                        typeof(System.Security.Policy.PolicyException),
#endif
                        typeof(System.Security.VerificationException),
#if NET461 || NET472
                        typeof(System.Security.XmlSyntaxException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.StackOverflowException),
                        typeof(System.SystemException),
#endif
                        typeof(System.Threading.BarrierPostPhaseException),
                        typeof(System.Threading.LockRecursionException),
                        typeof(System.Threading.SemaphoreFullException),
                        typeof(System.Threading.SynchronizationLockException),
                        typeof(System.Threading.Tasks.TaskSchedulerException),
#if !NETSTANDARD1_3
                        typeof(System.Threading.ThreadInterruptedException),
                        typeof(System.Threading.ThreadStartException),
                        typeof(System.Threading.ThreadStateException),
#endif
                        typeof(System.Threading.WaitHandleCannotBeOpenedException),
                        typeof(System.TimeoutException),
#if !NETSTANDARD1_3
                        typeof(System.TimeZoneNotFoundException),
#endif
                        typeof(System.TypeAccessException),
#if !NETSTANDARD1_3
                        typeof(System.TypeUnloadedException),
#endif
                        typeof(System.UnauthorizedAccessException),
                        typeof(System.UriFormatException),
                    };
#pragma warning restore IDE0001 // Simplify Names

                return targetTypes.ToArray();
            }
        }

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public virtual void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
        {
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

            propertiesBag.AddProperty("Type", exception.GetType().FullName);

            DestructureCommonExceptionProperties(
                exception,
                propertiesBag,
                destructureException,
                data => data.ToStringObjectDictionary());
        }

        /// <summary>
        /// Destructures public properties of <see cref="Exception"/>.
        /// Omits less frequently used ones if they are null.
        /// </summary>
        /// <param name="exception">The exception that will be destructured.</param>
        /// <param name="propertiesBag">The bag when destructured properties will be put.</param>
        /// <param name="innerDestructure">Function that can be used to destructure inner exceptions if there are any.</param>
        /// <param name="destructureDataProperty">Injected function for destructuring <see cref="Exception.Data"/>.</param>
        internal static void DestructureCommonExceptionProperties(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object?>?> innerDestructure,
            Func<System.Collections.IDictionary, object> destructureDataProperty)
        {
            if (exception.Data.Count != 0)
            {
                propertiesBag.AddProperty(nameof(Exception.Data), destructureDataProperty(exception.Data));
            }

            if (!string.IsNullOrEmpty(exception.HelpLink))
            {
                propertiesBag.AddProperty(nameof(Exception.HelpLink), exception.HelpLink);
            }

            if (exception.HResult != 0)
            {
                propertiesBag.AddProperty(nameof(Exception.HResult), exception.HResult);
            }

            propertiesBag.AddProperty(nameof(Exception.Message), exception.Message);
            propertiesBag.AddProperty(nameof(Exception.Source), exception.Source);
            propertiesBag.AddProperty(nameof(Exception.StackTrace), exception.StackTrace);

#if !NETSTANDARD1_3
            if (exception.TargetSite is not null)
            {
                propertiesBag.AddProperty(nameof(Exception.TargetSite), exception.TargetSite.ToString());
            }
#endif

            if (exception.InnerException is not null)
            {
                propertiesBag.AddProperty(nameof(Exception.InnerException), innerDestructure(exception.InnerException));
            }
        }
    }
}
