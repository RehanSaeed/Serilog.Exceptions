namespace Serilog.Exceptions.Core
{
    using System;

    public class ExceptionFilterIgnoringByName : IExceptionPropertyFilter
    {
        private readonly string[] propertiesToIgnore;

        public ExceptionFilterIgnoringByName(string[] propertiesToIgnore)
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