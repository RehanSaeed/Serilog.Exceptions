namespace Serilog.Exceptions.Benchmark
{
    using System;

    public class Point
    {
        public Point() => this.Z = 3;
        public int X { get; set; }
        public int Y { get; set; }
        private int Z { get; set; }
    }

    public class BenchmarkException : Exception
    {
        public string ParamString { get; set; }
        public int ParamInt { get; set; }
        public Point Point { get; set; }
    }
}
