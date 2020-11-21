#nullable enable

using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace ReactivePlot.Base
{
    public abstract class SingleSeriesModel<T> : IObserver<KeyValuePair<T, double>>, IMixedScheduler
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly SynchronizationContext? context;
        protected readonly IScheduler? scheduler;
        protected readonly object lck = new object();
        protected List<XY<T>> DataPoints = new List<XY<T>>();

        public SingleSeriesModel(SynchronizationContext? context = null, IScheduler? scheduler = null)
        {
            if (scheduler == null)
                this.Context = context ?? SynchronizationContext.Current;
            else
                this.scheduler = scheduler;
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
        }

        protected virtual void ModifyPlotModel()
        {
        }

        public void OnNext(KeyValuePair<T, double> item)
        {
            if (item.Key != null)
                Task.Run(() => AddToDataPoints(item)).ToObservable().Subscribe(refreshSubject.OnNext);
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset() => Task.Run(() => RemoveByPredicate(a => true)).ToObservable().Subscribe(refreshSubject.OnNext);

        public void Remove(ISet<T> names) => Task.Run(() => RemoveByPredicate(s => names.Contains(s.X))).ToObservable().Subscribe(refreshSubject.OnNext);

        public void OnCompleted()
        {
        }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(SingleSeriesModel<T>)}");

        private void AddToDataPoints(KeyValuePair<T, double> item)
        {
            var newdp = new XY<T>(item.Key, item.Value);
            lock (lck)
            {
                DataPoints.Add(newdp);
            }
        }

        protected abstract void Refresh(IList<Unit> units);

        private void RemoveByPredicate(Predicate<XY<T>> predicate)
        {
            lock (lck)
            {
                foreach (var dataPoint in DataPoints.Where(a => predicate(a)))
                    DataPoints.Remove(dataPoint);
            }
        }

        public IScheduler? Scheduler => scheduler;

        public SynchronizationContext? Context { get; }
    }
}