#nullable enable
using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{
    public abstract class SinglePlotModel<T> : IObserver<KeyValuePair<T, double>>
    {

        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly PlotModel plotModel;
        protected readonly IScheduler scheduler;
        protected readonly object lck = new object();
        protected List<DataPoint<T>> DataPoints = new List<DataPoint<T>>();

        public SinglePlotModel( PlotModel plotModel, IScheduler? scheduler=null)
        {
            this.plotModel = plotModel;
            this.scheduler = scheduler?? Scheduler.Default;
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
        }

        protected virtual void ModifyPlotModel() { }

        public void OnNext(KeyValuePair<T, double> item)
        {
            if (item.Key != null)
                Task.Run(() => AddToDataPoints(item)).ToObservable().Subscribe(refreshSubject.OnNext);
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset() => Task.Run(() => RemoveByPredicate(a => true)).ToObservable().Subscribe(refreshSubject.OnNext);

        public void Remove(ISet<T> names) => Task.Run(() => RemoveByPredicate(s => names.Contains(s.X))).ToObservable().Subscribe(refreshSubject.OnNext);

        public void OnCompleted() { }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(SinglePlotModel<T>)}");


        private void AddToDataPoints(KeyValuePair<T, double> item)
        {
            var newdp = new DataPoint<T>(item.Key, item.Value);
            lock (lck)
            {
                DataPoints.Add(newdp);
            }
        }


        protected abstract void Refresh(IList<Unit> units);

        private void RemoveByPredicate(Predicate<DataPoint<T>> predicate)
        {

            lock (lck)
            {
                foreach (var dataPoint in DataPoints.Where(a => predicate(a)))
                    DataPoints.Remove(dataPoint);
            }

        }



    }
}



