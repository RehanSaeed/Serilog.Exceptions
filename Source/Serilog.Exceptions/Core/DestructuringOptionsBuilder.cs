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

        public static readonly IExceptionPropertyFilter IgnoreStackTraceAndTargetIdExceptionFilter =

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

        public DestructuringOptionsBuilder()
        {
        }

        public string RootName => this.rootName;

        public int DestructuringDepth => this.destructuringDepth;

        public IEnumerable<IExceptionDestructurer> Destructurers => this.destructurers;

        public IExceptionPropertyFilter Filter => this.filter;

        public DestructuringOptionsBuilder WithDestructurers(IEnumerable<IExceptionDestructurer> destructurers)
        {
            this.destructurers.AddRange(destructurers);
            return this;
        }

        public DestructuringOptionsBuilder WithDefaultDestructurers()
        {
            return this.WithDestructurers(DefaultDestructurers);
        }

        public DestructuringOptionsBuilder WithFilter(IExceptionPropertyFilter filter)
        {
            this.filter = filter;
            return this;
        }

        public DestructuringOptionsBuilder WithIgnoreStackTraceAndTargetIdExceptionFilterFilter(IExceptionPropertyFilter filter)
        {
            return this.WithFilter(IgnoreStackTraceAndTargetIdExceptionFilter);
        }

        public DestructuringOptionsBuilder WithRootName(string rootName)
        {
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