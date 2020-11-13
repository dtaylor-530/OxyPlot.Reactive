using OxyPlot.Reactive.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OxyPlot.Reactive
{
    public static class ObservableExtension
    {
        public static IDisposable SubscribeCustom<TKey, R>(this IObservable<KeyValuePair<TKey, KeyValuePair<double, double>>> observable, CartesianModel<TKey, IDoublePoint<TKey>, R> model) where R : IDoublePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (IDoublePoint<TKey>)new DoublePoint<TKey>(a.Value.Key, a.Value.Value, a.Key)))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable SubscribeCustom<TKey, R>(this IObservable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> observable, TimeModel<TKey, ITimePoint<TKey>, R> model) where R : ITimePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(a.Key, (ITimePoint<TKey>)new TimePoint<TKey>(a.Value.Key, a.Value.Value, a.Key)))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable SubscribeCustom<TGroupKey, R, S>(this IObservable<KeyValuePair<TGroupKey, KeyValuePair<DateTime, double>>> observable, TimeModel<TGroupKey, string, R, S> model, Func<string>? keyFunc = null)
            where R : ITimePoint<string>
            where S : R
        {
            keyFunc ??= CreateKey;
            return observable
                .Select(a =>
                {
                    var timePoint = (ITimePoint<string>)new TimePoint<string>(a.Value.Key, a.Value.Value, keyFunc());
                    R r = default;
                    try
                    {
                        r = (R)timePoint;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error casting to " + nameof(R), ex);
                    }
                    return KeyValuePair.Create(a.Key, r);
                })
                .Subscribe(a =>
                model.OnNext(a));
        }

        //public static IDisposable SubscribeCustom2<TGroupKey, R, S>(this IObservable<KeyValuePair<TGroupKey, KeyValuePair<DateTime, (double x, double y)>>> observable, TimeModel<TGroupKey, string, R, S> model, Func<string>? keyFunc = null)
        //where R : ITimePoint<string>
        //where S : R
        //{
        //    keyFunc ??= CreateKey;
        //    return observable
        //        .Select(a => KeyValuePair.Create(a.Key, (R)(ITimePoint<string>)new TimeModelPoint<string, double>(a.Value.Key, a.Value.Value.x, a.Value.Value.y, keyFunc())))
        //        .Subscribe(a =>
        //        model.OnNext(a));
        //}


        public static IDisposable SubscribeCustom3<R, S, Y>(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeModel<string, R, S> model, Func<string>? keyFunc = null)
        where R : ITimeModelPoint<string, Y>
        where S : R
        {
            keyFunc ??= CreateKey;
            return observable
                   .Select(a =>
                   {
                       var timePoint = (ITimeModelPoint<string, Y>)new TimeModelPoint<string, Y>(a.Value.Key, a.Value.Value, default, keyFunc());
                       R r = default;
                       try
                       {
                           r = (R)timePoint;
                       }
                       catch (Exception ex)
                       {
                           throw new Exception("Error casting to " + nameof(R), ex);
                       }
                       return KeyValuePair.Create(a.Key, r);
                   })
                .Subscribe(a =>
                model.OnNext(a));
        }


        public static IDisposable SubscribeCustom4(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeModel<string, string, ITimePoint<string>, ITimeRangePoint<string>> model, Func<string>? keyFunc = null)
        {
            keyFunc ??= CreateKey;
            return observable
                   .Select(a =>
                   {
                       var timePoint = (ITimePoint<string>)new TimeStatsPoint<string>(a.Value.Key, a.Value.Value, default, keyFunc());
                       return KeyValuePair.Create(a.Key, timePoint);
                   })
                .Subscribe(a =>
                model.OnNext(a));
        }


        public static IDisposable SubscribeCustom4(this IObservable<KeyValuePair<string, KeyValuePair<DateTime, double>>> observable, TimeGroupOnTheFlyStatsModel<string> model, Func<string>? keyFunc = null)
        {
            keyFunc ??= CreateKey;
            return observable
                   .Select(a =>
                   {
                       var timePoint = (ITimeStatsPoint<string>)new TimeStatsPoint<string>(a.Value.Key, a.Value.Value, default, keyFunc());
                       return KeyValuePair.Create(a.Key, timePoint);
                   })
                .Subscribe(a =>
                model.OnNext(a));
        }

        private static string CreateKey() => string.Empty;
    }
}