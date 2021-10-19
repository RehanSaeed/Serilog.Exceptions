namespace Serilog.Exceptions.Filters
{
    using System;

    /// <summary>
    /// Abstraction over collection of filters that filters property is any of given filters alone would filter it.
    /// This is equivalent to OR over a set of booleans. Executes filters in the order they were passed to a
    /// constructor.
    /// </summary>
    public class CompositeExceptionPropertyFilter : IExceptionPropertyFilter
    {
        private readonly IExceptionPropertyFilter[] filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeExceptionPropertyFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <exception cref="ArgumentNullException">filters was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">filters was empty or filter at index {i} is <c>null</c>.</exception>
        public CompositeExceptionPropertyFilter(params IExceptionPropertyFilter[] filters)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(filters);
#else
            if (filters is null)
            {
                throw new ArgumentNullException(nameof(filters));
            }
#endif

            if (filters.Length == 0)
            {
                throw new ArgumentException(Resources.CannotBeEmpty, nameof(filters));
            }

            for (var i = 0; i < filters.Length; ++i)
            {
                if (filters[i] is null)
                {
                    throw new ArgumentException(
                        $"Cannot create composite exception properties filter, because filter at index {i} is null",
                        nameof(filters));
                }
            }

            this.filters = filters;
        }

        /// <inheritdoc />
        public bool ShouldPropertyBeFiltered(Exception exception, string propertyName, object? value)
        {
            for (var i = 0; i < this.filters.Length; ++i)
            {
                if (this.filters[i].ShouldPropertyBeFiltered(exception, propertyName, value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
