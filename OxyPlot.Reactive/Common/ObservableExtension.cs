using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OxyPlot.Reactive
{
    public static class ObservableExtension
    {
        public static IDisposable Subscribe<TKey, R>(this IObservable<KeyValuePair<TKey, KeyValuePair<double, double>>> observable, CartesianModel<TKey, IDoublePoint<TKey>, R> model) where R : IDoublePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (IDoublePoint<TKey>)new DoublePoint<TKey>(a.Value.Key, a.Value.Value, a.Key)))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable Subscribe<TKey, R>(this IObservable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> observable, TimeModel<TKey, ITimePoint<TKey>, R> model) where R : ITimePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<TKey>)new TimePoint<TKey>(a.Value.Key, a.Value.Value, a.Key)))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable Subscribe<TGroupKey, TKey>(this IObservable<KeyValuePair<TGroupKey, KeyValuePair<DateTime, double>>> observable, TimeGroupKeyModel<TGroupKey, TKey> model, Func<TKey> keyFunc)
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<TKey>)new TimePoint<TKey>(a.Value.Key, a.Value.Value, keyFunc())))
                .Subscribe(a =>
                model.OnNext(a));
        }
    }
}