namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ExceptionDestructurer : IExceptionDestructurer
    {
        public virtual Type[] TargetTypes
        {
            get
            {
                return new Type[]
                    {
                        typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException),
                        typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException),
                        typeof(Microsoft.SqlServer.Server.InvalidUdtException),
                        typeof(System.AccessViolationException),
                        typeof(System.AppDomainUnloadedException),
                        typeof(System.ApplicationException),
                        typeof(System.ArithmeticException),
                        typeof(System.ArrayTypeMismatchException),
                        typeof(System.CannotUnloadAppDomainException),
                        typeof(System.Collections.Generic.KeyNotFoundException),
                        typeof(System.ComponentModel.Design.CheckoutException),
                        typeof(System.ComponentModel.InvalidAsynchronousStateException),
                        typeof(System.ComponentModel.InvalidEnumArgumentException),
                        typeof(System.Configuration.SettingsPropertyIsReadOnlyException),
                        typeof(System.Configuration.SettingsPropertyNotFoundException),
                        typeof(System.Configuration.SettingsPropertyWrongTypeException),
                        typeof(System.ContextMarshalException),
                        typeof(System.Data.Common.DbException),
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
                        typeof(System.Data.SqlTypes.SqlNullValueException),
                        typeof(System.Data.SqlTypes.SqlTruncateException),
                        typeof(System.Data.SqlTypes.SqlTypeException),
                        typeof(System.Data.StrongTypingException),
                        typeof(System.Data.SyntaxErrorException),
                        typeof(System.Data.VersionNotFoundException),
                        typeof(System.DataMisalignedException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogInvalidDataException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogNotFoundException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException),
                        typeof(System.Diagnostics.Eventing.Reader.EventLogReadingException),
#if !NET40
                        typeof(System.Diagnostics.Tracing.EventSourceException),
#endif
                        typeof(System.DivideByZeroException),
                        typeof(System.DllNotFoundException),
                        typeof(System.DuplicateWaitObjectException),
                        typeof(System.EntryPointNotFoundException),
                        typeof(System.Exception),
                        typeof(System.FieldAccessException),
                        typeof(System.FormatException),
                        typeof(System.IndexOutOfRangeException),
                        typeof(System.InsufficientExecutionStackException),
                        typeof(System.InsufficientMemoryException),
                        typeof(System.InvalidCastException),
                        typeof(System.InvalidOperationException),
                        typeof(System.InvalidProgramException),
                        typeof(System.InvalidTimeZoneException),
                        typeof(System.IO.DirectoryNotFoundException),
                        typeof(System.IO.DriveNotFoundException),
                        typeof(System.IO.EndOfStreamException),
                        typeof(System.IO.InternalBufferOverflowException),
                        typeof(System.IO.InvalidDataException),
                        typeof(System.IO.IOException),
                        typeof(System.IO.IsolatedStorage.IsolatedStorageException),
                        typeof(System.IO.PathTooLongException),
                        typeof(System.Management.Instrumentation.InstanceNotFoundException),
                        typeof(System.Management.Instrumentation.InstrumentationBaseException),
                        typeof(System.Management.Instrumentation.InstrumentationException),
                        typeof(System.MemberAccessException),
                        typeof(System.MethodAccessException),
                        typeof(System.MulticastNotSupportedException),
                        typeof(System.Net.CookieException),
                        typeof(System.Net.NetworkInformation.PingException),
                        typeof(System.Net.ProtocolViolationException),
                        typeof(System.NotImplementedException),
                        typeof(System.NotSupportedException),
                        typeof(System.NullReferenceException),
                        typeof(System.OutOfMemoryException),
                        typeof(System.OverflowException),
                        typeof(System.PlatformNotSupportedException),
                        typeof(System.RankException),
                        typeof(System.Reflection.AmbiguousMatchException),
                        typeof(System.Reflection.CustomAttributeFormatException),
                        typeof(System.Reflection.InvalidFilterCriteriaException),
                        typeof(System.Reflection.TargetException),
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
                        typeof(System.Runtime.Remoting.RemotingException),
                        typeof(System.Runtime.Remoting.RemotingTimeoutException),
                        typeof(System.Runtime.Remoting.ServerException),
                        typeof(System.Runtime.Serialization.SerializationException),
                        typeof(System.Security.Authentication.AuthenticationException),
                        typeof(System.Security.Authentication.InvalidCredentialException),
                        typeof(System.Security.Cryptography.CryptographicException),
                        typeof(System.Security.Cryptography.CryptographicUnexpectedOperationException),
                        typeof(System.Security.Policy.PolicyException),
                        typeof(System.Security.VerificationException),
                        typeof(System.Security.XmlSyntaxException),
                        typeof(System.StackOverflowException),
                        typeof(System.SystemException),
                        typeof(System.Threading.BarrierPostPhaseException),
                        typeof(System.Threading.LockRecursionException),
                        typeof(System.Threading.SemaphoreFullException),
                        typeof(System.Threading.SynchronizationLockException),
                        typeof(System.Threading.Tasks.TaskSchedulerException),
                        typeof(System.Threading.ThreadInterruptedException),
                        typeof(System.Threading.ThreadStartException),
                        typeof(System.Threading.ThreadStateException),
                        typeof(System.Threading.WaitHandleCannotBeOpenedException),
                        typeof(System.TimeoutException),
                        typeof(System.TimeZoneNotFoundException),
                        typeof(System.TypeAccessException),
                        typeof(System.TypeUnloadedException),
                        typeof(System.UnauthorizedAccessException),
                        typeof(System.UriFormatException)
                    };
            }
        }

        public virtual void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> innerDestructure)
        {
            data.Add("Type", exception.GetType().FullName);

            if (exception.Data.Count != 0)
            {
                data.Add(
                    nameof(Exception.Data), 
                    exception.Data
                        .Cast<DictionaryEntry>()
                        .Where(k => k.Key is string)
                        .ToDictionary(e => (string)e.Key, e => e.Value));
            }

            if (!string.IsNullOrEmpty(exception.HelpLink))
            {
                data.Add(nameof(Exception.HelpLink), exception.HelpLink);
            }

#if !NET40
            if (exception.HResult != 0)
            {
                data.Add(nameof(Exception.HResult), exception.HResult);
            }
#endif

            data.Add(nameof(Exception.Message), exception.Message);
            data.Add(nameof(Exception.Source), exception.Source);
            data.Add(nameof(Exception.StackTrace), exception.StackTrace);

            if (exception.TargetSite != null)
            {
                data.Add(nameof(Exception.TargetSite), exception.TargetSite.ToString());
            }

            if (exception.InnerException != null)
            {
                data.Add(nameof(Exception.InnerException), innerDestructure(exception.InnerException));
            }
        }
    }
}
