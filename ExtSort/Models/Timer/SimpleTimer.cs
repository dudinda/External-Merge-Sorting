using System.Diagnostics;

namespace ExtSort.Models.Timer
{
    public class SimpleTimer : IDisposable
    {
        private readonly string _description;
        private readonly Stopwatch _watch;

        public SimpleTimer(string description)
        {
            _description = description;
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Dispose()
        {
            _watch.Stop();
            var time = _watch.Elapsed.ToString("mm\\:ss\\.ff");
            Console.WriteLine($"{Environment.NewLine}Operation {_description} was completed in: {time}");
        }
    }
}
