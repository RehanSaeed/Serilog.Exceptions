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
        /// Collection of exceptions types from standard library that do not have any custom property,
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
#if NET461
                        typeof(Microsoft.SqlServer.Server.InvalidUdtException),
                        typeof(System.AccessViolationException),
                        typeof(System.AppDomainUnloadedException),
                        typeof(System.ApplicationException),
#endif
                        typeof(System.ArithmeticException),
                        typeof(System.ArrayTypeMismatchException),
#if NET461
                        typeof(System.CannotUnloadAppDomainException),
#endif
                        typeof(System.Collections.Generic.KeyNotFoundException),
#if NET461
                        typeof(System.ComponentModel.Design.CheckoutException),
                        typeof(System.ComponentModel.InvalidAsynchronousStateException),
                        typeof(System.ComponentModel.InvalidEnumArgumentException),
                        typeof(System.Configuration.SettingsPropertyIsReadOnlyException),
                        typeof(System.Configuration.SettingsPropertyNotFoundException),
                        typeof(System.Configuration.SettingsPropertyWrongTypeException),
                        typeof(System.ContextMarshalException),
#endif
#if NET461
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
                        typeof(System.Data.OperationAbortedException),
                        typeof(System.Data.ReadOnlyException),
                        typeof(System.Data.RowNotInTableException),
                        typeof(System.Data.SqlTypes.SqlAlreadyFilledException),
                        typeof(System.Data.SqlTypes.SqlNotFilledException),
#endif
#if NET461
                        typeof(System.Data.StrongTypingException),
                        typeof(System.Data.SyntaxErrorException),
                        typeof(System.Data.VersionNotFoundException),
#endif
                        typeof(System.DataMisalignedException),
                        typeof(System.DivideByZeroException),
                        typeof(System.DllNotFoundException),
#if NET461
                        typeof(System.DuplicateWaitObjectException),
                        typeof(System.EntryPointNotFoundException),
#endif
                        typeof(System.Exception),
                        typeof(System.FieldAccessException),
                        typeof(System.FormatException),
                        typeof(System.IndexOutOfRangeException),
                        typeof(System.InsufficientExecutionStackException),
#if NET461
                        typeof(System.InsufficientMemoryException),
#endif
                        typeof(System.InvalidCastException),
                        typeof(System.InvalidOperationException),
                        typeof(System.InvalidProgramException),
                        typeof(System.InvalidTimeZoneException),
                        typeof(System.IO.DirectoryNotFoundException),
#if NET461
                        typeof(System.IO.DriveNotFoundException),
#endif
                        typeof(System.IO.EndOfStreamException),
#if NET461
                        typeof(System.IO.InternalBufferOverflowException),
#endif
                        typeof(System.IO.InvalidDataException),
                        typeof(System.IO.IOException),
#if NET461
                        typeof(System.IO.IsolatedStorage.IsolatedStorageException),
#endif
                        typeof(System.IO.PathTooLongException),
                        typeof(System.MemberAccessException),
                        typeof(System.MethodAccessException),
#if NET461
                        typeof(System.MulticastNotSupportedException),
#endif
                        typeof(System.Net.CookieException),
#if NET461
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
#if NET461
                        typeof(System.Reflection.CustomAttributeFormatException),
#endif
#if !NETSTANDARD1_3
                        typeof(System.Reflection.InvalidFilterCriteriaException),
                        typeof(System.Reflection.TargetException),
#endif
                        typeof(System.Reflection.TargetInvocationException),
                        typeof(System.Reflection.TargetParameterCountException),
                        typeof(System.Resources.MissingManifestResourceException),
                        typeof(System.Runtime.InteropServices.COMException),
                        typeof(System.Runtime.InteropServices.InvalidComObjectException),
                        typeof(System.Runtime.InteropServices.InvalidOleVariantTypeException),
                        typeof(System.Runtime.InteropServices.MarshalDirectiveException),
                        typeof(System.Runtime.InteropServices.SafeArrayRankMismatchException),
                        typeof(System.Runtime.InteropServices.SafeArrayTypeMismatchException),
                        typeof(System.Runtime.InteropServices.SEHException),
#if NET461
                        typeof(System.Runtime.Remoting.RemotingException),
                        typeof(System.Runtime.Remoting.RemotingTimeoutException),
                        typeof(System.Runtime.Remoting.ServerException),
                        typeof(System.Runtime.Serialization.SerializationException),
                        typeof(System.Security.Authentication.AuthenticationException),
                        typeof(System.Security.Authentication.InvalidCredentialException),
#endif
                        typeof(System.Security.Cryptography.CryptographicException),
#if NET461
                        typeof(System.Security.Cryptography.CryptographicUnexpectedOperationException),
                        typeof(System.Security.Policy.PolicyException),
#endif
                        typeof(System.Security.VerificationException),
#if NET461
                        typeof(System.Security.XmlSyntaxException),
                        typeof(System.StackOverflowException),
                        typeof(System.SystemException),
#endif
                        typeof(System.Threading.BarrierPostPhaseException),
                        typeof(System.Threading.LockRecursionException),
                        typeof(System.Threading.SemaphoreFullException),
                        typeof(System.Threading.SynchronizationLockException),
                        typeof(System.Threading.Tasks.TaskSchedulerException),
#if NET461
                        typeof(System.Threading.ThreadInterruptedException),
                        typeof(System.Threading.ThreadStartException),
                        typeof(System.Threading.ThreadStateException),
#endif
                        typeof(System.Threading.WaitHandleCannotBeOpenedException),
                        typeof(System.TimeoutException),
#if NET461
                        typeof(System.TimeZoneNotFoundException),
#endif
                        typeof(System.TypeAccessException),
#if NET461
                        typeof(System.TypeUnloadedException),
#endif
                        typeof(System.UnauthorizedAccessException),
                        typeof(System.UriFormatException)
                    };

#if NET461
                foreach (var dangerousType in GetNotHandledByMonoTypes())
                {
                    var type = Type.GetType(dangerousType);
                    if (type != null)
                    {
                        targetTypes.Add(type);
                    }
                }
#endif
                return targetTypes.ToArray();
            }
        }

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public virtual void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> innerDestructure)
        {
            propertiesBag.AddProperty("Type", exception.GetType().FullName);

            DestructureCommonExceptionProperties(
                exception,
                propertiesBag,
                innerDestructure,
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
            Func<Exception, IReadOnlyDictionary<string, object>> innerDestructure,
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

#if NET461
            if (exception.TargetSite != null)
            {
                propertiesBag.AddProperty(nameof(Exception.TargetSite), exception.TargetSite.ToString());
            }
#endif

            if (exception.InnerException != null)
            {
                propertiesBag.AddProperty(nameof(Exception.InnerException), innerDestructure(exception.InnerException));
            }
        }

        /// <summary>
        /// Get types that are currently not handled by mono and could raise a LoadTypeException.
        /// </summary>
        /// <returns>List of type names.</returns>
        private static string[] GetNotHandledByMonoTypes() =>
            new string[]
            {
                "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Diagnostics.Eventing.Reader.EventLogNotFoundException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Diagnostics.Eventing.Reader.EventLogReadingException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Diagnostics.Tracing.EventSourceException, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089",
                "System.Management.Instrumentation.InstanceNotFoundException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Management.Instrumentation.InstrumentationBaseException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Management.Instrumentation.InstrumentationException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            };
    }
}
