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
        /// process of children destructuring. Historically and by default it is 10.
        /// </summary>
        int DestructuringDepth { get; }

        /// <summary>
        /// Collection of destructurers that will be used to destructure
        /// incoming exceptions. If none of the destructurers can handle given
        /// type of exception, a generic, reflection-based destructurer will be
        /// used.
        /// </summary>
        IEnumerable<IExceptionDestructurer> Destructurers { get; }

        /// <summary>
        /// Optional filter, that will evaluate and possibly reject
        /// each destructured property just before they are about
        /// to be written to a result structure. If no filter is set
        /// no properties are going to be rejected. Filter is applied
        /// to every property regardless of which destructurer was used.
        /// </summary>
        IExceptionPropertyFilter Filter { get; }
    }
}