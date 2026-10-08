using System.Diagnostics;

namespace X264GUIv2.Models
{
    public class CustomStopwatch
    {
        private readonly Stopwatch _stopwatch = new();
        public TimeSpan Offset { get; private set; }

        public TimeSpan Elapsed => Offset + _stopwatch.Elapsed;
        public void Start() => _stopwatch.Start();
        public void Stop() => _stopwatch.Stop();

        public void Reset()
        {
            _stopwatch.Reset();
            Offset = TimeSpan.Zero;
        }

        public void Add(TimeSpan time) => Offset += time;
        public void Subtract(TimeSpan time) => Offset -= time;
    }
}
