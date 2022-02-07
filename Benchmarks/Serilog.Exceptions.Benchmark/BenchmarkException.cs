namespace Serilog.Exceptions.Benchmark;

using System;
using System.Runtime.Serialization;

[Serializable]
public class BenchmarkException : Exception
{
    public BenchmarkException()
    {
    }

    public BenchmarkException(string message)
        : base(message)
    {
    }

    public BenchmarkException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected BenchmarkException(
      SerializationInfo info,
      StreamingContext context)
        : base(info, context)
    {
    }

    public string? ParamString { get; set; }

    public int ParamInt { get; set; }

    public Point? Point { get; set; }
}
