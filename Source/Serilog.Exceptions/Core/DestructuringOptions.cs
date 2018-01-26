namespace Serilog.Exceptions.Core
{
    using System;

    public class DestructuringOptions : IDestructuringOptions
    {
        private readonly string rootName;

        public DestructuringOptions(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
            {
                throw new ArgumentException("Empty string was provided as root name", nameof(rootName));
            }

            this.rootName = rootName;
        }

        public string RootName => this.rootName;
    }
}