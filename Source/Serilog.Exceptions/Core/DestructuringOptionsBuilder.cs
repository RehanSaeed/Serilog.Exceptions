namespace Serilog.Exceptions.Core
{
    using System;

    using System.Collections.Generic;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    public class DestructuringOptionsBuilder : IDestructuringOptions
    {
        public static readonly IExceptionDestructurer[] DefaultDestructurers =
        {
            new ExceptionDestructurer(),
            new ArgumentExceptionDestructurer(),
            new ArgumentOutOfRangeExceptionDestructurer(),
            new AggregateExceptionDestructurer(),
            new ReflectionTypeLoadExceptionDestructurer()
        };

        public static readonly IExceptionPropertyFilter IgnoreStackTraceAndTargetSiteExceptionFilter =

#if NET45
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

        public string RootName => this.rootName;

        public int DestructuringDepth => this.destructuringDepth;

        public IEnumerable<IExceptionDestructurer> Destructurers => this.destructurers;

        public IExceptionPropertyFilter Filter => this.filter;

        public DestructuringOptionsBuilder WithDestructurers(IEnumerable<IExceptionDestructurer> destructurers)
        {
            if (destructurers == null)
            {
                throw new ArgumentNullException(nameof(destructurers), "Cannot add null destructurers collection");
            }

            this.destructurers.AddRange(destructurers);
            return this;
        }

        public DestructuringOptionsBuilder WithDefaultDestructurers() =>
            this.WithDestructurers(DefaultDestructurers);

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

        public DestructuringOptionsBuilder WithIgnoreStackTraceAndTargetSiteExceptionFilter() =>
            this.WithFilter(IgnoreStackTraceAndTargetSiteExceptionFilter);

        public DestructuringOptionsBuilder WithRootName(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
            {
                throw new ArgumentException("Cannot accept null or empty root property name", nameof(rootName));
            }

            this.rootName = rootName;
            return this;
        }

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