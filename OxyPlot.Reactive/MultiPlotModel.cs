using MoreLinq;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using e = System.Linq.Enumerable;

namespace OxyPlotEx.ViewModel
{
    public abstract class MultiPlotModel<T, TR> : MultiPlotModelBase<T, (TR X, double Y)>
    {
        public MultiPlotModel(IDispatcher dispatcher, PlotModel plotModel) : base(dispatcher, plotModel)
        {
        }

        public MultiPlotModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : base(dispatcher, model, comparer)
        {
        }
    }

    public abstract class MultiPlotModelBase<T, R> : IObserver<KeyValuePair<T, R>>
    {
        private readonly IEqualityComparer<T> comparer;
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly IDispatcher dispatcher;
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();

        protected Dictionary<T, List<R>> DataPoints;

        public MultiPlotModelBase(IDispatcher dispatcher, PlotModel plotModel)
        {
            if (plotModel == null)
                throw new Exception("PlotModel is null");        
            if (dispatcher == null)
                throw new Exception("IDispatcher is null");    
   
            this.dispatcher = dispatcher;
            this.plotModel = plotModel;
            ModifyPlotModel();
            DataPoints = GetDataPoints();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
        }
        public MultiPlotModelBase(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : this(dispatcher, model)
        {
            if (comparer == null)
                throw new Exception("PlotModel is null");
            this.comparer = comparer;
        }

        protected virtual void ModifyPlotModel() { }


        public bool ShowAll { get; set; } = false;

        public void OnNext(KeyValuePair<T, R> item)
        {
            if (item.Key != null)
                Task.Run(() => AddToDataPoints(item)).ToObservable().Subscribe(refreshSubject.OnNext);
        }

        protected virtual double Combine(double x0, double x1) => x1;

        public void Reset() => RemoveByPredicate(a => true);

        public void Remove(ISet<string> names) => RemoveByPredicate(s => names.Contains(s.Title));

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {nameof(MultiLineModelAccumulated<T>)}");

        private void AddToDataPoints(KeyValuePair<T, R> item)
        {
            var newdp = item.Value;
            lock (lck)
            {
                if (!DataPoints.ContainsKey(item.Key))
                    DataPoints[item.Key] = new List<R>();
                DataPoints[item.Key].Add(newdp);
            }
        }

        protected abstract void Refresh(IList<Unit> units);

        private void RemoveByPredicate(Predicate<OxyPlot.Series.Series> predicate)
        {
            dispatcher.Invoke(() =>
            {
                while (plotModel.Series.Any(predicate.Invoke))
                    plotModel.Series.Remove(plotModel.Series.First(predicate.Invoke));
                DataPoints = GetDataPoints();
                plotModel.InvalidatePlot(true);
            });
        }

        private Dictionary<T, List<R>> GetDataPoints()
        {
            return comparer == default ?
                  new Dictionary<T, List<R>>() :
                new Dictionary<T, List<R>>(comparer);
        }
    }
}



