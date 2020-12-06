#nullable enable
using OnTheFlyStats;
using ReactivePlot.Base;
using ReactivePlot.Model;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace ReactivePlot.Common
{
    public static class Observable2Extension
    {
        public static IDisposable Subscribe<TGroupKey, T, R, S>(
            this IObservable<T> observable,
            TimeMinMaxModel<TGroupKey, string, R, S> model, Func<T, R> funcR, Func<T, TGroupKey> fTGroupKey, Func<T, string>? keyFunc = null)
    where R : ITimePoint<string>
    where S : R
        {
            keyFunc ??= new Func<T, string>(a => CreateKey());
            return observable
                .Select(a =>
                {
                    var timePoint = funcR(a);
                    return KeyValuePair.Create(fTGroupKey(a), timePoint);
                })
                .Subscribe(a => model.OnNext(a));
        }

        public static IDisposable Subscribe<TGroupKey, T, S>(
    this IObservable<T> observable,
    TimeMinMaxModel<TGroupKey, string, ITimePoint<string>, S> model, Func<T, DateTime> fts, Func<T, double> fr, Func<T, TGroupKey> fTGroupKey, Func<T, string>? keyFunc = null)
where S : ITimePoint<string>
        {
            keyFunc ??= new Func<T, string>(a => CreateKey());
            return observable
                .Select(a =>
                {
                    var timePoint = (ITimePoint<string>)new TimePoint<string>(fts(a), fr(a), keyFunc(a));
                    return KeyValuePair.Create(fTGroupKey(a), timePoint);
                })
                .Subscribe(a => model.OnNext(a));
        }


        public static IDisposable Subscribe<T>(
            this IObservable<T> observable,
            TimeOnTheFlyStatsModel model, Func<T, DateTime> fts, Func<T, double> fr, Func<T, string> fTGroupKey, Func<T, string>? keyFunc = null)
        {
            var stats = new Stats();
            keyFunc ??= new Func<T, string>(a => CreateKey());
            return observable
                .Select(a =>
                {
                    var timePoint = (ITimeModelPoint<string, Stats>)new TimeStatsPoint<string>(fts(a), fr(a), stats, keyFunc(a));
                    return KeyValuePair.Create(fTGroupKey(a), timePoint);
                })
                .Subscribe(a => model.OnNext(a));
        }


        private static string CreateKey() => string.Empty;
    }
}
