using ReactivePlot.Base;
using ReactivePlot.Model;
using ReactivePlot.Time;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace ReactivePlot.Common
{
    public static class ObservableExtension
    {

        public static IDisposable Subscribe<TKey, R, TIn>(this IObservable<TIn> observable,
            CartesianModel<TKey, IDoublePoint<TKey>, R> model,
            Func<TIn, TKey> funcKey,
            Func<TIn, double> funcVar,
            Func<TIn, double> funcValue) where R : IDoublePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(funcKey(a), (IDoublePoint<TKey>)new DoublePoint<TKey>(funcVar(a), funcValue(a), funcKey(a))))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable Subscribe<TKey, R, TIn>(this IObservable<TIn> observable,
      TimeModel<TKey, ITimePoint<TKey>, R> model,
      Func<TIn, TKey> funcKey,
      Func<TIn, DateTime> funcVar,
      Func<TIn, double> funcValue) where R : ITimePoint<TKey>
        {
            return observable
                .Select(a => KeyValuePair.Create(funcKey(a), (ITimePoint<TKey>)new TimePoint<TKey>(funcVar(a), funcValue(a), funcKey(a))))
                .Subscribe(a =>
                model.OnNext(a));
        }

        public static IDisposable SubscribeCustom<TKey, R>(
            this IObservable<KeyValuePair<TKey, KeyValuePair<double, double>>> observable, 
            CartesianModel<TKey, IDoublePoint<TKey>, R> model) 
            where R : IDoublePoint<TKey>
        {
            return observable.Subscribe(model, a => a.Key, a => a.Value.Key, a => a.Value.Value);
        }

        public static IDisposable SubscribeCustom<TKey, R>(
            this IObservable<KeyValuePair<TKey, KeyValuePair<DateTime, double>>> observable, 
            TimeModel<TKey, ITimePoint<TKey>, R> model) 
            where R : ITimePoint<TKey>
        {
            return observable.Subscribe(model, a => a.Key, a => a.Value.Key, a => a.Value.Value);
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