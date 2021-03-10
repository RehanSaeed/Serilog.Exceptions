namespace Serilog.Exceptions.Core
{
    using System.Collections.Generic;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    /// <summary>
    /// Represents all the configuration options user can specify to influence the destructuring process.
    /// </summary>
    public interface IDestructuringOptions
    {
        /// <summary>
        /// Gets the name of the key dictionary to which destructured exception will be assigned. Default is
        /// <c>"ExceptionDetail"</c>.
        /// </summary>
        string RootName { get; }

        /// <summary>
        /// Gets the depth at which reflection based destructurer will stop recursive process of children destructuring.
        /// Default is <c>10</c>.
        /// </summary>
        int DestructuringDepth { get; }

        /// <summary>
        /// Gets the collection of destructurers that will be used to destructure incoming exceptions. If none of the
        /// destructurers can handle given type of exception, a generic, reflection-based destructurer will be used.
        /// </summary>
        IEnumerable<IExceptionDestructurer> Destructurers { get; }

        /// <summary>
        /// Gets the optional filter that will evaluate and possibly reject each destructured property just before
        /// they are about to be written to a result structure. If no filter is set no properties are going to be
        /// rejected. Filter is applied to every property regardless of which destructurer was used.
        /// </summary>
        IExceptionPropertyFilter? Filter { get; }

        /// <summary>
        /// Gets a value indicating whether to disable the reflection based destructurer.
        /// You may want to disable this destructurer if you need full control
        /// over the process of destructuring and want to provide all the destructurers yourself.
        /// </summary>
        bool DisableReflectionBasedDestructurer { get; }
    }
}
