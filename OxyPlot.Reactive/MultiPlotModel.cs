#nullable enable
using MoreLinq;
using OxyPlot;
using OxyPlot.Reactive.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using e = System.Linq.Enumerable;

namespace OxyPlot.Reactive
{
    public abstract class MultiPlotModel<T, TR> : MultiPlotModelBase<T, (TR X, double Y)>
    {
        public MultiPlotModel(IDispatcher dispatcher, PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100) : base(dispatcher, plotModel, comparer, refreshRate)
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

    public abstract class MultiPlotModelBase<T, R> : IObserver<KeyValuePair<T, R>>, IObserver<bool>
    {
        private readonly IEqualityComparer<T>? comparer;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly IDispatcher dispatcher;
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected bool showAll;
        protected Dictionary<T, List<R>> DataPoints;

        public MultiPlotModelBase(IDispatcher dispatcher, PlotModel plotModel, IEqualityComparer<T>? comparer = null, int refreshRate = 100)
        {
            this.comparer = comparer;
            this.dispatcher = dispatcher ?? throw new ArgumentNullException("IDispatcher is null");
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

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(MultiDateTimeAccumulatedModel<T>)}");


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
            dispatcher.Invoke(() =>
            {
                while (plotModel.Series.Any(predicate.Invoke))
                    plotModel.Series.Remove(plotModel.Series.First(predicate.Invoke));
                DataPoints = GetDataPoints();
                plotModel.InvalidatePlot(true);
            });
        }

        protected virtual Dictionary<T, List<R>> GetDataPoints()
        {
            return comparer == default ?
                  new Dictionary<T, List<R>>() :
                new Dictionary<T, List<R>>(comparer);
        }
    }
}



