using System.Reactive.Concurrency;
using ArtemisWest.Mayfair.Infrastructure;

namespace ArtemisWest.Mayfair.Shell
{
    public sealed class SchedulerProvider : ISchedulerProvider
    {
        private readonly DispatcherScheduler _dispatcherScheduler;

        public SchedulerProvider()
            : this(DispatcherScheduler.Instance)
        {
        }

        private SchedulerProvider(DispatcherScheduler dispatcherScheduler)
        {
            _dispatcherScheduler = dispatcherScheduler;
        }


        public IScheduler CurrentThread
        {
            get { return Scheduler.CurrentThread; }
        }

        public IScheduler Immediate
        {
            get { return Scheduler.Immediate; }
        }

        public IScheduler NewThread
        {
            get { return Scheduler.NewThread; }
        }

        public IScheduler TaskPool
        {
            get { return Scheduler.TaskPool; }
        }

        public IScheduler ThreadPool
        {
            get { return Scheduler.ThreadPool; }
        }

        public IScheduler Dispatcher
        {
            get { return _dispatcherScheduler; }
        }
    }
}