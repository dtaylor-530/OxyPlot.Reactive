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
using System.Threading;
using e = System.Linq.Enumerable;

namespace ReactivePlot.Base
{
    public abstract class SingleSeriesBaseModel : IMixedScheduler
    {
        protected readonly ISubject<Unit> refreshSubject = new Subject<Unit>();
        protected readonly SynchronizationContext? context;
        protected readonly IScheduler? scheduler;
        private ISinglePlotModel plotModel;

        public SingleSeriesBaseModel(ISinglePlotModel plotModel, SynchronizationContext? context = null, IScheduler? scheduler = null)
        {
            if (scheduler == null)
                this.Context = context ?? SynchronizationContext.Current;
            else
                this.scheduler = scheduler;
            ModifyPlotModel();
            refreshSubject.Buffer(TimeSpan.FromMilliseconds(1000)).Where(e.Any).Subscribe(Refresh);
            this.plotModel = plotModel;
        }

        protected virtual void ModifyPlotModel()
        {
        }

        public abstract void Reset();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error) => throw new NotImplementedException($"Error in {this.GetType().Name}");

        protected abstract void Add();

        protected virtual void Refresh(IList<Unit> units)
        {
            Add();
            plotModel.Add(units);
            plotModel.Invalidate(true);
        }

        public abstract void Clear();

        public IScheduler? Scheduler => scheduler;

        public SynchronizationContext? Context { get; }
    }
}