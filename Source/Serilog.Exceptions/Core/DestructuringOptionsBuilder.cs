namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using Serilog.Events;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    /// <summary>
    /// Accumulates destructuring options to be used by the <see cref="ExceptionEnricher"/>.
    /// </summary>
    public class DestructuringOptionsBuilder : IDestructuringOptions
    {
        /// <summary>
        /// Default set of destructurers. Destructurers cover all of the exceptions from standard library.
        /// </summary>
        public static readonly IExceptionDestructurer[] DefaultDestructurers =
        {
            new ExceptionDestructurer(),
            new ArgumentExceptionDestructurer(),
            new ArgumentOutOfRangeExceptionDestructurer(),
            new AggregateExceptionDestructurer(),
            new ReflectionTypeLoadExceptionDestructurer(),
            new OperationCanceledExceptionDestructurer(),
            new TaskCanceledExceptionDestructurer(),
        };

        /// <summary>
        /// Filter that ignores <see cref="Exception.StackTrace"/> and <see cref="Exception.TargetSite"/> properties.
        /// Usually, they can be safely ignored, because Serilog attaches them tog <see cref="LogEvent"/> already.
        /// </summary>
        public static readonly IExceptionPropertyFilter IgnoreStackTraceAndTargetSiteExceptionFilter =

#if NET46
            new IgnorePropertyByNameExceptionFilter(
                nameof(Exception.StackTrace),
                nameof(Exception.TargetSite));
#else
            new IgnorePropertyByNameExceptionFilter(
                nameof(Exception.StackTrace));
#endif

        private readonly List<IExceptionDestructurer> destructurers = new List<IExceptionDestructurer>();
        private string rootName = "ExceptionDetail";
        private int destructuringDepth = 10;
        private IExceptionPropertyFilter filter;

        /// <summary>
        /// Name of the property which value will be filled with destructured exception.
        /// </summary>
        public string RootName => this.rootName;

        /// <summary>
        /// Maximum depth of destructuring to which reflection destructurer will proceed.
        /// </summary>
        public int DestructuringDepth => this.destructuringDepth;

        /// <summary>
        /// Collection of destructurers that will be used to handle exception.
        /// </summary>
        public IEnumerable<IExceptionDestructurer> Destructurers => this.destructurers;

        /// <summary>
        /// Global filter, that will be applied to every destructured property just before it is added to the result.
        /// </summary>
        public IExceptionPropertyFilter Filter => this.filter;

        /// <summary>
        /// Accumulates destructurers to be used by <see cref="ExceptionEnricher"/>.
        /// </summary>
        /// <param name="destructurers">Collection of destructurers</param>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithDestructurers(IEnumerable<IExceptionDestructurer> destructurers)
        {
            if (destructurers == null)
            {
                throw new ArgumentNullException(nameof(destructurers), "Cannot add null destructurers collection");
            }

            this.destructurers.AddRange(destructurers);
            return this;
        }

        /// <summary>
        /// Adds destructurers for a known set of exceptions from standard library.
        /// </summary>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithDefaultDestructurers() =>
            this.WithDestructurers(DefaultDestructurers);

        /// <summary>
        /// Sets a filter that will be used by <see cref="ExceptionEnricher"/>
        /// Only one filter can be set, second invocation of this method throws <exception cref="InvalidOperationException">invalid operation exception</exception>
        /// </summary>
        /// <param name="filter">The filter</param>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithFilter(IExceptionPropertyFilter filter)
        {
            if (this.filter != null)
            {
                throw new InvalidOperationException(
                    $"Filter was already set, only one filter can be configured. Use {nameof(CompositeExceptionPropertyFilter)} or other aggregate to combine filters");
            }

            this.filter = filter;
            return this;
        }

        /// <summary>
        /// Sets a filter that will be used by <see cref="ExceptionEnricher"/>.
        /// The filter ignores <see cref="Exception.StackTrace"/> and <see cref="Exception.TargetSite"/> properties.
        /// Only one filter can be set, second invocation of this method throws <exception cref="InvalidOperationException">invalid operation exception</exception>
        /// </summary>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithIgnoreStackTraceAndTargetSiteExceptionFilter() =>
            this.WithFilter(IgnoreStackTraceAndTargetSiteExceptionFilter);

        /// <summary>
        /// Sets a property name that will be used by <see cref="ExceptionEnricher"/>.
        /// Name cannot be <exception cref="ArgumentException">null or empty</exception>.
        /// </summary>
        /// <param name="rootName">The name of root property</param>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithRootName(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
            {
                throw new ArgumentException("Cannot accept null or empty root property name", nameof(rootName));
            }

            this.rootName = rootName;
            return this;
        }

        /// <summary>
        /// Sets a maximum destructuring depth that <see cref="ExceptionEnricher"/> will reach during destructuring of unknown exception type.
        /// Given depth must <exception cref="ArgumentOutOfRangeException">be positive</exception>.
        /// </summary>
        /// <param name="destructuringDepth">Maximum depth, must be positive</param>
        /// <returns>Options builder for method chaining</returns>
        public DestructuringOptionsBuilder WithDestructuringDepth(int destructuringDepth)
        {
            if (destructuringDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destructuringDepth),
                    destructuringDepth,
                    "Destructuring depth must be positive");
            }

            this.destructuringDepth = destructuringDepth;
            return this;
        }
    }
}