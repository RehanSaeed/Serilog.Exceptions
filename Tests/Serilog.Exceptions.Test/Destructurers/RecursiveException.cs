namespace Serilog.Exceptions.Test.Destructurers
{
    using System;

    public class RecursiveException : Exception
    {
        public RecursiveNode Node { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class RecursiveNode
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string Name { get; set; }

        public RecursiveNode Child { get; set; }
    }
}