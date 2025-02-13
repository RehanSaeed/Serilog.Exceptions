namespace Serilog.Exceptions.Benchmark;

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

    public string? ParamString { get; set; }

    public int ParamInt { get; set; }

    public Point? Point { get; set; }
}
