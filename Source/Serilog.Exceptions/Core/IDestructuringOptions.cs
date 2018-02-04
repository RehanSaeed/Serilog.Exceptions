namespace Serilog.Exceptions.Core
{
    using System.Collections.Generic;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    public interface IDestructuringOptions
    {
        /// <summary>
        /// Name of the key dictionary to which destructured exception
        /// will be assigned. Default value is "ExceptionDetail".
        /// </summary>
        string RootName { get; }

        /// <summary>
        /// Depth at which reflection based destructurer will stop recursive
        /// process of children destructuring.
        /// </summary>
        int DestructuringDepth { get; }

        IEnumerable<IExceptionDestructurer> Destructurers { get; }

        IExceptionPropertyFilter Filter { get; }
    }
}