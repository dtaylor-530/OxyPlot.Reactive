#nullable enable

using Kaos.Collections;
using MoreLinq;
using ReactivePlot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ReactivePlot.Base
{
    public abstract class MultiSeriesModel<TKey, TVar, TType> : MultiSeriesModel<TKey, TVar, TType, TType>, IObserver<int>
          where TType : IDoublePoint<TKey, TVar>, IGroupKey<TKey>
        where TVar : IComparable<TVar>
    {
        public MultiSeriesModel(IMultiPlotModel<TType> model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class MultiSeriesModel<TKey, TVar, TType, TType3> : MultiSeriesMinMaxModel<TKey, TKey, TVar, TType, TType3>, IObserver<int>
        //where TType3 : TType
       where TType : IDoublePoint<TKey, TVar>
        where TVar : IComparable<TVar>
    {
        public MultiSeriesModel(IMultiPlotModel<TType3> model, TVar max, TVar min, IEqualityComparer<TKey>? comparer = null, IScheduler? scheduler = null) : base(model, max, min, comparer, scheduler: scheduler)
        {
        }
    }

    public abstract class MultiSeriesMinMaxModel<TGroupKey, TKey, TVar, TType, TType3> :
        MultiSeriesModel<TGroupKey, TKey, TVar, TType, TType3>
          //where TType3 : TType
          //   where TType : IDoublePoint<TKey, TVar>
          where TType : IVar<TVar>
        where TVar : IComparable<TVar>
    {

        public MultiSeriesMinMaxModel(IMultiPlotModel<TType3> plotModel, TVar max, TVar min, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
            base(plotModel, comparer, scheduler: scheduler)
        {
            this.Max = max;
            this.Min = min;
        }

        protected TVar Min { get; set; }
        protected TVar Max { get; set; }


        protected override void AddToDataPoints(IEnumerable<KeyValuePair<TGroupKey, TType>> items)
        {
            Min = CalculateMin(items);
            Max = CalculateMax(items);
            base.AddToDataPoints(items);
        }

        protected abstract TVar CalculateMin(IEnumerable<KeyValuePair<TGroupKey, TType>> items);

        protected abstract TVar CalculateMax(IEnumerable<KeyValuePair<TGroupKey, TType>> items);

    }

    /// <summary>
    /// Allows the combination of points
    /// </summary>
    /// <typeparam name="TGroupKey"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVar"></typeparam>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TType3"></typeparam>
    public abstract class MultiSeriesModel<TGroupKey, TKey, TVar, TType, TType3> :
        MultiSeriesBaseModel<TGroupKey, TKey, TVar, TType, TType3>,
        IObserver<bool>
        //where TType3 : TType        
      
        where TType : IVar<TVar>
        where TVar : IComparable<TVar>
    {
        protected bool showAll;
        private const string AllSeriesTitle = "All";

        public MultiSeriesModel(IMultiPlotModel<TType3> plotModel, IEqualityComparer<TGroupKey>? comparer = null, IScheduler? scheduler = null) :
        base(plotModel, comparer, scheduler: scheduler)
        {
        }


        protected override async Task AddAllPointsToSeries(KeyValuePair<TGroupKey, ICollection<TType>>[] dataPoints)
        {
            await base.AddAllPointsToSeries(dataPoints);

            if (showAll || pointsSubject.HasObservers)
            {
                _ = await Task.Run(() =>
                {
                    return CreateMany(dataPoints).ToArray();
                }).ContinueWith(async points =>
                {
                    var taskPoints = await points;

                    if (showAll)
                        plotModel.AddSeries(taskPoints, AllSeriesTitle, dataPoints.Length);

                    if (pointsSubject.HasObservers)
                    {
                        pointsSubject.OnNext(taskPoints);
                    }
                });
            }
            else
            {
                _ = plotModel.RemoveSeries(AllSeriesTitle);
            }
        }


        protected virtual IEnumerable<TType3> CreateMany(KeyValuePair<TGroupKey, ICollection<TType>>[] keyValues)
        {
            return Create(keyValues.SelectMany(a => a.Value).OrderBy(a => a.Var).Scan((a, b) => CreatePoint(a, b)));
        }
        protected abstract TType CreatePoint(TType xy0, TType xy);

        protected override ICollection<TType> CreateCollection()
        {
            // Represents a collection of non-distinct items
            return new RankedBag<TType>(Comparer<TType>.Create((a, b) => a.Var.CompareTo(b.Var)));
        }


        public void OnNext(bool value)
        {
            throw new NotImplementedException();
        }
    }
}