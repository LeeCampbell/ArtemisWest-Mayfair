using System.Reactive.Concurrency;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public interface ISchedulerProvider
    {
        IScheduler Background { get; }
        IScheduler Foreground { get; }
    }
}