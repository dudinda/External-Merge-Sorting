using System.Diagnostics;

namespace ExtSort.Models.Timer 
{
    public class IntervalScope : IDisposable 
    {
        private readonly int _intervalSec;
        private readonly Stopwatch _watch;

        public IntervalScope(int intervalSec)
        {
            _intervalSec = intervalSec;
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void WriteLine(string message) 
        {
            if (_watch.Elapsed.Seconds > _intervalSec) 
            {
                _watch.Restart();
                Console.WriteLine(message);
            }
        }

        public void Dispose() 
        {
            _watch.Stop();
        }
    }
}
