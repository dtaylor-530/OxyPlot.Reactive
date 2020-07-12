#nullable enable
using MoreLinq;
using OxyPlot;
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
    public abstract class MultiPlotModel<T, TR> : MultiPlotModelBase<T, KeyValuePair<TR, double>>
    {
        public MultiPlotModel(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, IScheduler? scheduler = null) : base(plotModel, comparer, refreshRate, scheduler)
        {
            if (plotModel?.PlotView?.ActualController is IController plotController)
                CongfigureBindings(plotController);

            static void CongfigureBindings(IController pc)
            {
                pc.UnbindMouseDown(OxyMouseButton.Left);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

                pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                             (view, controller, args) =>
                                controller.AddMouseManipulator(view, new TrackerManipulator1(view), args)));
            }
        }

        public MultiPlotModel(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, SynchronizationContext? context = null) : base(plotModel, comparer, refreshRate, context)
        {
            if (plotModel?.PlotView?.ActualController is IController plotController)
                CongfigureBindings(plotController);

            static void CongfigureBindings(IController pc)
            {
                pc.UnbindMouseDown(OxyMouseButton.Left);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
                //pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

                pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                             (view, controller, args) =>
                                controller.AddMouseManipulator(view, new TrackerManipulator1(view), args)));
            }
        }
    }

    public abstract class MultiPlotModelBase<T, R> : IObserver<KeyValuePair<T, R>>, IObserver<bool>, IMixedScheduler
    {
        private readonly IEqualityComparer<T>? comparer;
        private readonly SynchronizationContext? context;
        public IScheduler? scheduler;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected bool showAll;
        protected Dictionary<T, ICollection<R>> DataPoints;

        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, IScheduler? scheduler = default)
        {
            this.comparer = comparer;
            this.scheduler = scheduler ?? Scheduler.CurrentThread;
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            ModifyPlotModel();
            DataPoints = GetDataPoints();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);
        }

        public MultiPlotModelBase(PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100, SynchronizationContext? context = default)
        {
            this.comparer = comparer;
            this.context = context ?? SynchronizationContext.Current;
            this.plotModel = plotModel ?? throw new ArgumentNullException("PlotModel is null");
            ModifyPlotModel();
            DataPoints = GetDataPoints();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(refreshRate)).Where(e.Any).Subscribe(Refresh);
        }

        protected virtual void ModifyPlotModel() { }

        public void OnNext(KeyValuePair<T, R> item)
        {
            AddToDataPoints(item);
            refreshSubject.OnNext(Unit.Default);
        }

        public void OnNext(bool showAll)
        {
            this.showAll = showAll;
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset() => RemoveByPredicate(a => true);

        public void Remove(ISet<string> names) => RemoveByPredicate(s => names.Contains(s.Title));

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new Exception($"Error in {nameof(MultiPlotModelBase<T,R>)}",error);


        protected abstract void Refresh(IList<Unit> units);

        protected virtual void AddToDataPoints(KeyValuePair<T, R> item)
        {
            var newdp = item.Value;
            lock (lck)
            {
                if (!DataPoints.ContainsKey(item.Key))
                    DataPoints[item.Key] = new List<R>();
                DataPoints[item.Key].Add(newdp);
            }
        }

        protected virtual void RemoveByPredicate(Predicate<Series.Series> predicate)
        {
            (this as IMixedScheduler).ScheduleAction(Sdf, predicate);

            void Sdf(Predicate<Series.Series> pred)
            {
                lock (lck)
                {
                    while (plotModel.Series.Any(pred.Invoke))
                        plotModel.Series.Remove(plotModel.Series.First(pred.Invoke));
                    DataPoints = GetDataPoints();
                    plotModel.InvalidatePlot(true);
                }
            }
        }

        protected virtual Dictionary<T, ICollection<R>> GetDataPoints()
        {
            return comparer == default ?
                  new Dictionary<T, ICollection<R>>() :
                new Dictionary<T, ICollection<R>>(comparer);
        }

        IScheduler? IMixedScheduler.Scheduler => scheduler;
        SynchronizationContext? IMixedScheduler.Context => context;
    }
}



