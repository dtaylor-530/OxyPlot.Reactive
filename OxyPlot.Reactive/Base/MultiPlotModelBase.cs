#nullable enable

using MoreLinq;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{

    public abstract class MultiPlotModel2Base<TGroupKey, TKey, TR, TRS, TRS2> : MultiPlotModelBase<TGroupKey, TKey, TRS>, IObservable<TRS2>
    {
        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }

        public abstract IDisposable Subscribe(IObserver<TRS2> observer);
    }

    public abstract class MultiPlotModel2Base<TKey, TR, TRS, TRS2> : MultiPlotModelBase<TKey, TKey, TRS>, IObservable<TRS2>
    {
        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }

        public abstract IDisposable Subscribe(IObserver<TRS2> observer);
    }

    public abstract class MultiPlotModel2Base<TKey, TR> : MultiPlotModelBase<TKey, TKey, KeyValuePair<TR, double>>
    {
        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
        }

        public MultiPlotModel2Base(PlotModel plotModel, IEqualityComparer<TKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
        }
    }

    public abstract class MultiPlotModelBase<TGroupKey, TKey, TValue> : DataPointsModel<TGroupKey, TValue>, IObserver<KeyValuePair<TGroupKey, TValue>>, IObserver<bool>, IMixedScheduler
    {
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected bool showAll;

        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, IScheduler? scheduler = default) : this(plotModel, comparer, refreshRate)
        {
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100, SynchronizationContext? context = default) : this(plotModel, comparer, refreshRate)
        {
            this.context = context ?? SynchronizationContext.Current;
        }

        private MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<TGroupKey>? comparer = null, int refreshRate = 100) : base(comparer)
        {
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);

            if (plotModel?.PlotView?.ActualController is IController plotController)
                CongfigureBindings(plotController);
        }

        protected virtual void ModifyPlotModel()
        {
        }

        public virtual void OnNext(KeyValuePair<TGroupKey, TValue> item)
        {
            this.AddToDataPoints(new[] { item });
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(bool showAll)
        {
            this.showAll = showAll;
            refreshSubject.OnNext(Unit.Default);
        }

        public void Reset()
        {
            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (DataPoints)
                    DataPoints.Clear();

                lock (plotModel)
                {
                    plotModel.Series.Clear();
                    plotModel.InvalidatePlot(true);
                }
            });
        }

        public void Remove(ISet<TGroupKey> names)
        {
            (this as IMixedScheduler).ScheduleAction(() =>
            {
                lock (DataPoints)
                    RemoveFromDataPoints(names);
                lock (plotModel)
                {
                    RemoveFromSeries(s => names.Select(a => a.ToString()).Contains(s.Title));
                    plotModel.InvalidatePlot(true);
                }
            });

            void RemoveFromSeries(Predicate<Series.Series> pred)
            {
                while (plotModel.Series.Any(pred.Invoke))
                    plotModel.Series.Remove(plotModel.Series.First(pred.Invoke));
            }
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiPlotModelBase<TGroupKey, TKey, TValue>)}", error);

        protected abstract void Refresh(IList<Unit> units);

        protected void CongfigureBindings(IController pc)
        {
            pc.UnbindMouseDown(OxyMouseButton.Left);
            //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
            //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

            pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                             (view, controller, args) =>
                             controller.AddMouseManipulator(view, new TrackerManipulator1(view), args)));
        }

        IScheduler? IMixedScheduler.Scheduler => scheduler;

        SynchronizationContext? IMixedScheduler.Context => context;
    }
}