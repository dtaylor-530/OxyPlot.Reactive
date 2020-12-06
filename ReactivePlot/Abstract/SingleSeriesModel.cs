#nullable enable

using ReactivePlot.Common;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{
    public class SingleSeriesModel : SingleSeriesModel<double>, IObserver<double>
    {
        public SingleSeriesModel(ISinglePlotModel<double> plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(plotModel, context, scheduler)
        {
        }
    }

    public class SingleSeriesModel<T> : SingleSeriesModel<T, T>
    {
        public SingleSeriesModel(ISinglePlotModel<T> plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(plotModel, context, scheduler)
        {

        }

        protected override T Convert(T tr)
        {
            return tr;
        }
    }

    public abstract class SingleSeriesModel<TIn, TPlot> : SingleSeriesBaseModel, IObserver<TIn>
    {
        protected ICollection<TPlot> DataPoints = new List<TPlot>();
        protected Queue<TIn> PointsQueue = new Queue<TIn>();
        protected readonly ISinglePlotModel<TPlot> plotModel;

        public SingleSeriesModel(ISinglePlotModel<TPlot> plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null) : base(plotModel, context, scheduler)
        {
            this.plotModel = plotModel;
        }

        public void OnNext(TIn item)
        {
            PointsQueue.Enqueue(item);
            refreshSubject.OnNext(Unit.Default);
        }

        public override void Reset() => Task.Run(() => RemoveByPredicate(a => true)).ToObservable().Subscribe(refreshSubject.OnNext);

        protected override void Add()
        {
            lock (PointsQueue)
                lock (DataPoints)
                {
                    while (PointsQueue.TryDequeue(out var item))
                    {
                        DataPoints.Add(Convert(item));
                    }
                }
        }

        protected abstract TPlot Convert(TIn tr);

        protected override void Refresh(IList<Unit> units)
        {
            Add();

            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (plotModel)
                lock (DataPoints)
                {
                    //plotModel.Clear();
                    plotModel.Add(Array.AsReadOnly(DataPoints.ToArray()));
                    plotModel.Invalidate(true);
                }
            });
        }

        private void RemoveByPredicate(Predicate<TPlot> predicate)
        {
            lock (DataPoints)
            {
                foreach (var dataPoint in DataPoints.Where(a => predicate(a)))
                    DataPoints.Remove(dataPoint);
            }
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}