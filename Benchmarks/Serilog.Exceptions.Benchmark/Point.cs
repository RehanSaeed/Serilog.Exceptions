namespace Serilog.Exceptions.Benchmark
{
    public class Point
    {
        public Point() => this.Z = 3;

        public int X { get; set; }

        public int Y { get; set; }

        private int Z { get; set; }
    }
}
