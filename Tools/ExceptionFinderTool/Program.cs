namespace ExceptionFinderTool
{
    using System;
#if NET6_0_OR_GREATER
    using System.Globalization;
#endif
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            var stringBuilder = new StringBuilder();

            foreach (var exceptionType in AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetTypeInfo().IsPublic && typeof(Exception).IsAssignableFrom(x))
                .Select(x => x.FullName)
                .OrderBy(x => x))
            {
#if NET6_0_OR_GREATER
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"typeof({exceptionType}),");
#else
                stringBuilder.AppendLine($"typeof({exceptionType}),");
#endif
            }

            var types = stringBuilder.ToString();
            Console.WriteLine(types);
            Console.Read();
        }
    }
}
