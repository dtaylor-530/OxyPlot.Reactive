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
    public abstract class MultiPlotModel<T,TR> : IObserver<KeyValuePair<T, (TR x, double value)>>
    {

        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly IDispatcher dispatcher;
        protected readonly PlotModel plotModel;
        protected readonly object lck = new object();
        protected readonly IEqualityComparer<T> comparer;
        protected Dictionary<T, List<DataPoint<TR>>> DataPoints;

        public MultiPlotModel(IDispatcher dispatcher, PlotModel plotModel)
        {
            this.dispatcher = dispatcher;
            this.plotModel = plotModel;
            ModifyPlotModel();
            DataPoints = GetDataPoints();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(100)).Where(e.Any).Subscribe(Refresh);
        }

        public MultiPlotModel(IDispatcher dispatcher, PlotModel model, IEqualityComparer<T> comparer) : this(dispatcher, model)
        {
            this.comparer = comparer;
        }

        protected virtual void ModifyPlotModel() { }
        

        public bool ShowAll { get; set; } = false;

        public void OnNext(KeyValuePair<T, (TR x, double value)> item)
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


        private void AddToDataPoints(KeyValuePair<T, (TR x, double value)> item)
        {
            var newdp = new DataPoint<TR>(item.Value.x, item.Value.value);
            lock (lck)
            {
                if (!DataPoints.ContainsKey(item.Key))
                    DataPoints[item.Key] = new List<DataPoint<TR>>();
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


        private Dictionary<T, List<DataPoint<TR>>> GetDataPoints()
        {
            return comparer == default ?
                new Dictionary<T, List<DataPoint<TR>>>() :
                new Dictionary<T, List<DataPoint<TR>>>(comparer);
        }
    }
}



