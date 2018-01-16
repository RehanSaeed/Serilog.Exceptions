namespace Serilog.Exceptions.Filters
{
    using System;

    /// <summary>
    /// Filters the exception properties based only on their name. If exception property
    /// matches any of provided property names, exception property is ignored altogether.
    /// Comparison method is exact case-sensitive.
    /// </summary>
    public class IgnorePropertyByNameExceptionFilter : IExceptionPropertyFilter
    {
        /// <summary>
        /// The usage of array instead of HashSet is dictated by the assumption
        /// that there will be only small number of properties to ignore and for such
        /// case array is much faster than HashSet.
        /// </summary>
        private readonly string[] propertiesToIgnore;

        public IgnorePropertyByNameExceptionFilter(params string[] propertiesToIgnore)
        {
            this.propertiesToIgnore = propertiesToIgnore;
        }

        public bool ShouldPropertyBeFiltered(Type exceptionType, string propertyName, object value)
        {
            if (this.propertiesToIgnore == null)
            {
                return false;
            }

            for (int i = 0; i < this.propertiesToIgnore.Length; i++)
            {
                if (this.propertiesToIgnore[i].Equals(propertyName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}