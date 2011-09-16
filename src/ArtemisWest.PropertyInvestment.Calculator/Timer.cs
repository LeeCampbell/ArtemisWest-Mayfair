using System;
using System.Diagnostics;

namespace ArtemisWest.PropertyInvestment.Calculator
{
    public sealed class Timer : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _watch;

        public Timer(string name)
        {
            _name = name;
            _watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _watch.Stop();
            Console.WriteLine("{0} took {1}", _name, _watch.Elapsed);
        }
    }
}