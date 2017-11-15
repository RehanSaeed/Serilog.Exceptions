namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ExceptionDestructurer : IExceptionDestructurer
    {
        public ExceptionDestructurer(List<string> ignoredProperties)
        {
            this.IgnoredProperties = ignoredProperties;
        }

        public virtual Type[] TargetTypes
        {
            get
            {
                var targetTypes = new List<Type>
                    {
                        typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException),
                        typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException),
#if NET45
                        typeof(Microsoft.SqlServer.Server.InvalidUdtException),
                        typeof(System.AccessViolationException),
                        typeof(System.AppDomainUnloadedException),
                        typeof(System.ApplicationException),
#endif
                        typeof(System.ArithmeticException),
                        typeof(System.ArrayTypeMismatchException),
#if NET45
                        typeof(System.CannotUnloadAppDomainException),
#endif
                        typeof(System.Collections.Generic.KeyNotFoundException),
#if NET45
                        typeof(System.ComponentModel.Design.CheckoutException),
                        typeof(System.ComponentModel.InvalidAsynchronousStateException),
                        typeof(System.ComponentModel.InvalidEnumArgumentException),
                        typeof(System.Configuration.SettingsPropertyIsReadOnlyException),
                        typeof(System.Configuration.SettingsPropertyNotFoundException),
                        typeof(System.Configuration.SettingsPropertyWrongTypeException),
                        typeof(System.ContextMarshalException),
#endif
                        typeof(System.Data.Common.DbException),
#if NET45
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
                        typeof(System.Data.SqlTypes.SqlNullValueException),
                        typeof(System.Data.SqlTypes.SqlTruncateException),
                        typeof(System.Data.SqlTypes.SqlTypeException),
#if NET45
                        typeof(System.Data.StrongTypingException),
                        typeof(System.Data.SyntaxErrorException),
                        typeof(System.Data.VersionNotFoundException),
#endif
                        typeof(System.DataMisalignedException),
                        typeof(System.DivideByZeroException),
                        typeof(System.DllNotFoundException),
#if NET45
                        typeof(System.DuplicateWaitObjectException),
                        typeof(System.EntryPointNotFoundException),
#endif
                        typeof(System.Exception),
                        typeof(System.FieldAccessException),
                        typeof(System.FormatException),
                        typeof(System.IndexOutOfRangeException),
                        typeof(System.InsufficientExecutionStackException),
#if NET45
                        typeof(System.InsufficientMemoryException),
#endif
                        typeof(System.InvalidCastException),
                        typeof(System.InvalidOperationException),
                        typeof(System.InvalidProgramException),
                        typeof(System.InvalidTimeZoneException),
                        typeof(System.IO.DirectoryNotFoundException),
#if NET45
                        typeof(System.IO.DriveNotFoundException),
#endif
                        typeof(System.IO.EndOfStreamException),
#if NET45
                        typeof(System.IO.InternalBufferOverflowException),
#endif
                        typeof(System.IO.InvalidDataException),
                        typeof(System.IO.IOException),
#if NET45
                        typeof(System.IO.IsolatedStorage.IsolatedStorageException),
#endif
                        typeof(System.IO.PathTooLongException),
                        typeof(System.MemberAccessException),
                        typeof(System.MethodAccessException),
#if NET45
                        typeof(System.MulticastNotSupportedException),
#endif
                        typeof(System.Net.CookieException),
#if NET45
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
#if NET45
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
#if NET45
                        typeof(System.Runtime.Remoting.RemotingException),
                        typeof(System.Runtime.Remoting.RemotingTimeoutException),
                        typeof(System.Runtime.Remoting.ServerException),
                        typeof(System.Runtime.Serialization.SerializationException),
                        typeof(System.Security.Authentication.AuthenticationException),
                        typeof(System.Security.Authentication.InvalidCredentialException),
#endif
                        typeof(System.Security.Cryptography.CryptographicException),
#if NET45
                        typeof(System.Security.Cryptography.CryptographicUnexpectedOperationException),
                        typeof(System.Security.Policy.PolicyException),
#endif
                        typeof(System.Security.VerificationException),
#if NET45
                        typeof(System.Security.XmlSyntaxException),
                        typeof(System.StackOverflowException),
                        typeof(System.SystemException),
#endif
                        typeof(System.Threading.BarrierPostPhaseException),
                        typeof(System.Threading.LockRecursionException),
                        typeof(System.Threading.SemaphoreFullException),
                        typeof(System.Threading.SynchronizationLockException),
                        typeof(System.Threading.Tasks.TaskSchedulerException),
#if NET45
                        typeof(System.Threading.ThreadInterruptedException),
                        typeof(System.Threading.ThreadStartException),
                        typeof(System.Threading.ThreadStateException),
#endif
                        typeof(System.Threading.WaitHandleCannotBeOpenedException),
                        typeof(System.TimeoutException),
#if NET45
                        typeof(System.TimeZoneNotFoundException),
#endif
                        typeof(System.TypeAccessException),
#if NET45
                        typeof(System.TypeUnloadedException),
#endif
                        typeof(System.UnauthorizedAccessException),
                        typeof(System.UriFormatException)
                    };

#if NET45
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

        public List<string> IgnoredProperties { get; set; }

        public virtual void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> innerDestructure)
        {
            data.AddIfNotIgnored("Type", exception.GetType().FullName, this.IgnoredProperties);

            if (exception.Data.Count != 0)
            {
                data.AddIfNotIgnored(nameof(Exception.Data), exception.Data.ToStringObjectDictionary(this.IgnoredProperties), this.IgnoredProperties);
            }

            if (!string.IsNullOrEmpty(exception.HelpLink))
            {
                data.AddIfNotIgnored(nameof(Exception.HelpLink), exception.HelpLink, this.IgnoredProperties);
            }

            if (exception.HResult != 0)
            {
                data.AddIfNotIgnored(nameof(Exception.HResult), exception.HResult, this.IgnoredProperties);
            }

            data.AddIfNotIgnored(nameof(Exception.Message), exception.Message, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(Exception.Source), exception.Source, this.IgnoredProperties);
            data.AddIfNotIgnored(nameof(Exception.StackTrace), exception.StackTrace, this.IgnoredProperties);

#if NET45
            if (exception.TargetSite != null)
            {
                data.AddIfNotIgnored(nameof(Exception.TargetSite), exception.TargetSite.ToString(), this.IgnoredProperties);
            }
#endif

            if (exception.InnerException != null)
            {
                data.AddIfNotIgnored(nameof(Exception.InnerException), innerDestructure(exception.InnerException), this.IgnoredProperties);
            }
        }

        /// <summary>
        /// Get types that are currently not handled by mono and could raise a LoadTypeException.
        /// </summary>
        /// <returns>List of type names.</returns>
        private static string[] GetNotHandledByMonoTypes()
        {
            return new string[]
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
}
