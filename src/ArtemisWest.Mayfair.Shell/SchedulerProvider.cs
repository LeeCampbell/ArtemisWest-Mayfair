using System.Reactive.Concurrency;
using ArtemisWest.Mayfair.Infrastructure;

namespace ArtemisWest.Mayfair.Shell
{
    public sealed class SchedulerProvider : ISchedulerProvider
    {
        private readonly DispatcherScheduler _dispatcherScheduler;

        public SchedulerProvider()
        {
            _dispatcherScheduler = DispatcherScheduler.Current;
        }

        public IScheduler Background => TaskPoolScheduler.Default;

        public IScheduler Foreground => _dispatcherScheduler;
    }
}