using System.Reactive.Concurrency;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public interface ISchedulerProvider
    {
        IScheduler CurrentThread { get; }
        IScheduler Immediate { get; }
        IScheduler NewThread { get; }
        IScheduler TaskPool { get; }
        IScheduler ThreadPool { get; }
        IScheduler Dispatcher { get; }
    }
}