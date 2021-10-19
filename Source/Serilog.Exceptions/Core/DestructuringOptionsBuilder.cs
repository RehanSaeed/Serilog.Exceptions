namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
            new SocketExceptionDestructurer(),
        };

        /// <summary>
        /// Filter that ignores <see cref="Exception.StackTrace"/> and Exception.TargetSite properties.
        /// Usually, they can be safely ignored, because Serilog attaches them tog <see cref="LogEvent"/> already.
        /// </summary>
        public static readonly IExceptionPropertyFilter IgnoreStackTraceAndTargetSiteExceptionFilter =

#if NET461 || NET472
            new IgnorePropertyByNameExceptionFilter(
                nameof(Exception.StackTrace),
                nameof(Exception.TargetSite));
#else
            new IgnorePropertyByNameExceptionFilter(
                nameof(Exception.StackTrace));
#endif

        private readonly List<IExceptionDestructurer> destructurers = new();

        /// <summary>
        /// Gets the name of the property which value will be filled with destructured exception.
        /// </summary>
        public string RootName { get; private set; } = "ExceptionDetail";

        /// <summary>
        /// Gets the maximum depth of destructuring to which reflection destructurer will proceed.
        /// </summary>
        public int DestructuringDepth { get; private set; } = 10;

        /// <summary>
        /// Gets a value indicating whether to disable the reflection based destructurer.
        /// </summary>
        public bool DisableReflectionBasedDestructurer { get; private set; }

        /// <summary>
        /// Gets a collection of destructurers that will be used to handle exception.
        /// </summary>
        public IEnumerable<IExceptionDestructurer> Destructurers => this.destructurers;

        /// <summary>
        /// Gets a global filter, that will be applied to every destructured property just before it is added to the result.
        /// </summary>
        public IExceptionPropertyFilter? Filter { get; private set; }

        /// <summary>
        /// Accumulates destructurers to be used by <see cref="ExceptionEnricher"/>.
        /// </summary>
        /// <param name="destructurers">Collection of destructurers.</param>
        /// <returns>Options builder for method chaining.</returns>
        public DestructuringOptionsBuilder WithDestructurers(IEnumerable<IExceptionDestructurer> destructurers)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(destructurers);
#else
            if (destructurers is null)
            {
                throw new ArgumentNullException(nameof(destructurers));
            }
#endif

            this.destructurers.AddRange(destructurers);
            return this;
        }

        /// <summary>
        /// Adds destructurers for a known set of exceptions from standard library.
        /// </summary>
        /// <returns>Options builder for method chaining.</returns>
        public DestructuringOptionsBuilder WithDefaultDestructurers() =>
            this.WithDestructurers(DefaultDestructurers);

        /// <summary>
        /// Sets a filter that will be used by <see cref="ExceptionEnricher"/>. Only one filter can be set, second
        /// invocation of this method throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>Options builder for method chaining.</returns>
        public DestructuringOptionsBuilder WithFilter(IExceptionPropertyFilter filter)
        {
            if (this.Filter is not null)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, Resources.FilterAlreadySet, nameof(CompositeExceptionPropertyFilter)));
            }

            this.Filter = filter;
            return this;
        }

        /// <summary>
        /// Sets a filter that will be used by <see cref="ExceptionEnricher"/>. The filter ignores
        /// <see cref="Exception.StackTrace"/> and Exception.TargetSite properties. Only one filter can
        /// be set, second invocation of this method throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>Options builder for method chaining.</returns>
        public DestructuringOptionsBuilder WithIgnoreStackTraceAndTargetSiteExceptionFilter() =>
            this.WithFilter(IgnoreStackTraceAndTargetSiteExceptionFilter);

        /// <summary>
        /// Sets a property name that will be used by <see cref="ExceptionEnricher"/>.
        /// </summary>
        /// <param name="rootName">The name of root property.</param>
        /// <returns>Options builder for method chaining.</returns>
        /// <exception cref="ArgumentException">Name cannot be null or empty.</exception>
        public DestructuringOptionsBuilder WithRootName(string rootName)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(rootName);
#else
            if (rootName is null)
            {
                throw new ArgumentNullException(nameof(rootName));
            }
#endif

            if (rootName.Length == 0)
            {
                throw new ArgumentException(Resources.CannotBeEmpty, nameof(rootName));
            }

            this.RootName = rootName;
            return this;
        }

        /// <summary>
        /// Sets a maximum destructuring depth that <see cref="ExceptionEnricher"/> will reach during destructuring of
        /// unknown exception type.
        /// </summary>
        /// <param name="destructuringDepth">Maximum depth, must be positive.</param>
        /// <returns>Options builder for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Given depth must be positive.</exception>
        public DestructuringOptionsBuilder WithDestructuringDepth(int destructuringDepth)
        {
            if (destructuringDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destructuringDepth),
                    destructuringDepth,
                    Resources.DestructuringDepthMustBeMoreThanZero);
            }

            this.DestructuringDepth = destructuringDepth;
            return this;
        }

        /// <summary>
        /// Disable reflection based destructurer.
        /// You may want to disable this destructurer if you need full control
        /// over the process of destructuring and want to provide all the destructurers yourself.
        /// </summary>
        /// <returns>Options builder for method chaining.</returns>
        public DestructuringOptionsBuilder WithoutReflectionBasedDestructurer()
        {
            this.DisableReflectionBasedDestructurer = true;
            return this;
        }
    }
}
