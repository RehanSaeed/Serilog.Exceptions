namespace Serilog.Exceptions.Benchmark
{
    public class Point
    {
        public Point() => this.Z = 3;

        public int X { get; set; }

        public int Y { get; set; }

#pragma warning disable IDE0052 // Remove unread private members
        private int Z { get; set; }
#pragma warning restore IDE0052 // Remove unread private members
    }
}
