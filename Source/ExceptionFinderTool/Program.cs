namespace ExceptionFinderTool
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class Program
    {
        public static void Main(string[] args)
        {
            var stringBuilder = new StringBuilder();

            foreach (var exceptionType in GetAllAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetTypeInfo().IsPublic && x.Name.EndsWith("Exception"))
                .Select(x => x.FullName)
                .OrderBy(x => x))
            {
                stringBuilder.AppendLine($"typeof({exceptionType}),");
            }

            var types = stringBuilder.ToString();
            Console.WriteLine(types);
            Console.Read();
        }

        private static Assembly[] GetAllAssemblies()
        {
#if NET45
            return AppDomain.CurrentDomain.GetAssemblies();
#else
            return new Assembly[]
            {
                Assembly.Load(new AssemblyName("System.AppContext")),
                Assembly.Load(new AssemblyName("System.Collections")),
                Assembly.Load(new AssemblyName("System.Collections.Concurrent")),
                Assembly.Load(new AssemblyName("System.Console")),
                Assembly.Load(new AssemblyName("System.Diagnostics.Debug")),
                Assembly.Load(new AssemblyName("System.Diagnostics.Tools")),
                Assembly.Load(new AssemblyName("System.Diagnostics.Tracing")),
                Assembly.Load(new AssemblyName("System.Globalization")),
                Assembly.Load(new AssemblyName("System.Globalization.Calendars")),
                Assembly.Load(new AssemblyName("System.IO")),
                Assembly.Load(new AssemblyName("System.IO.Compression")),
                Assembly.Load(new AssemblyName("System.IO.Compression.ZipFile")),
                Assembly.Load(new AssemblyName("System.IO.FileSystem")),
                Assembly.Load(new AssemblyName("System.IO.FileSystem.Primitives")),
                Assembly.Load(new AssemblyName("System.Linq")),
                Assembly.Load(new AssemblyName("System.Linq.Expressions")),
                Assembly.Load(new AssemblyName("System.Net.Http")),
                Assembly.Load(new AssemblyName("System.Net.Primitives")),
                Assembly.Load(new AssemblyName("System.Net.Sockets")),
                Assembly.Load(new AssemblyName("System.ObjectModel")),
                Assembly.Load(new AssemblyName("System.Reflection")),
                Assembly.Load(new AssemblyName("System.Reflection.Extensions")),
                Assembly.Load(new AssemblyName("System.Reflection.Primitives")),
                Assembly.Load(new AssemblyName("System.Resources.ResourceManager")),
                Assembly.Load(new AssemblyName("System.Runtime")),
                Assembly.Load(new AssemblyName("System.Runtime.Extensions")),
                Assembly.Load(new AssemblyName("System.Runtime.Handles")),
                Assembly.Load(new AssemblyName("System.Runtime.InteropServices")),
                Assembly.Load(new AssemblyName("System.Runtime.InteropServices.RuntimeInformation")),
                Assembly.Load(new AssemblyName("System.Runtime.Numerics")),
                Assembly.Load(new AssemblyName("System.Security.Cryptography.Algorithms")),
                Assembly.Load(new AssemblyName("System.Security.Cryptography.Encoding")),
                Assembly.Load(new AssemblyName("System.Security.Cryptography.Primitives")),
                Assembly.Load(new AssemblyName("System.Security.Cryptography.X509Certificates")),
                Assembly.Load(new AssemblyName("System.Text.Encoding")),
                Assembly.Load(new AssemblyName("System.Text.Encoding.Extensions")),
                Assembly.Load(new AssemblyName("System.Text.RegularExpressions")),
                Assembly.Load(new AssemblyName("System.Threading")),
                Assembly.Load(new AssemblyName("System.Threading.Tasks")),
                Assembly.Load(new AssemblyName("System.Threading.Timer")),
                Assembly.Load(new AssemblyName("System.Xml.ReaderWriter")),
                Assembly.Load(new AssemblyName("System.Xml.XDocument"))
            };
#endif
        }
    }
}
