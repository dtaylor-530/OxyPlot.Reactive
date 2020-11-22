#nullable enable

using System;
using System.Reactive.Concurrency;
using System.Threading;

namespace ReactivePlot.Common
{
    public interface IMixedScheduler
    {
        public IScheduler? Scheduler { get; }

        public SynchronizationContext? Context { get; }

        public void ScheduleAction<TR>(Action<TR> action, TR arg)
        {
            if (Scheduler != null)
                Scheduler.Schedule(() =>
                {
                    action(arg);
                });
            else if (Context != null)
                Context.Post(obj =>
                {
                    if (obj is TR oarg)
                        action(oarg);
                    else throw new Exception("fsdd");
                }, arg);
        }

        public IDisposable? ScheduleAction(Action action)
        {
            if (Scheduler != null)
                return Scheduler.Schedule(() =>
                {
                    action();
                });
            else if (Context != null)
                Context.Send(obj =>
                {
                    action();
                }, null);
            return null;
        }
    }
}