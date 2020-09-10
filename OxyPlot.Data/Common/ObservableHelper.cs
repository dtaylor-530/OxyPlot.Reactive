using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OxyPlot.Data.Common
{
    public static class ObservableHelper
    {
        // James World
        //http://www.zerobugbuild.com/?p=323
        ///The events should be output at a maximum rate specified by a TimeSpan, but otherwise as soon as possible.
        public static IObservable<T> Pace<T>(this IObservable<T> source, TimeSpan rate)
        {
            var paced = source.Select(i => Observable.Empty<T>()
                                      .Delay(rate)
                                      .StartWith(i)).Concat();

            return paced;
        }

        public static IObservable<(double min, double max)> ToMinMax<T>(this IObservable<T> source, Func<T, double> func)
        {
            return source.Select(func).Scan((min: double.MaxValue, max: double.MinValue), (a, b) =>
                {
                    return (Math.Min(a.min, b), Math.Max(a.max, b));
                }).Skip(1);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/7597773/does-reactive-extensions-support-rolling-buffers?noredirect=1
        /// answered Sep 30 '11 at 0:32
        /// Enigmativity
        /// </summary>
        public static IObservable<IEnumerable<T>> BufferWithInactivity<T>(
    this IObservable<T> source,
    TimeSpan inactivity,
    int maximumBufferSize)
        {
            return Observable.Create<IEnumerable<T>>(o =>
            {
                var gate = new object();
                var buffer = new List<T>();
                var mutable = new SerialDisposable();
                var subscription = (IDisposable)null;
                var scheduler = Scheduler.Default;

                Action dump = () =>
                {
                    var bts = buffer.ToArray();
                    buffer = new List<T>();
                    if (o != null)
                    {
                        o.OnNext(bts);
                    }
                };

                Action dispose = () =>
                {
                    if (subscription != null)
                    {
                        subscription.Dispose();
                    }
                    mutable.Dispose();
                };

                Action<Action<IObserver<IEnumerable<T>>>> onErrorOrCompleted =
                    onAction =>
                    {
                        lock (gate)
                        {
                            dispose();
                            dump();
                            if (o != null)
                            {
                                onAction(o);
                            }
                        }
                    };

                Action<Exception> onError = ex =>
                    onErrorOrCompleted(x => x.OnError(ex));

                Action onCompleted = () => onErrorOrCompleted(x => x.OnCompleted());

                Action<T> onNext = t =>
                {
                    lock (gate)
                    {
                        buffer.Add(t);
                        if (buffer.Count == maximumBufferSize)
                        {
                            dump();
                            mutable.Disposable = Disposable.Empty;
                        }
                        else
                        {
                            mutable.Disposable = scheduler.Schedule(inactivity, () =>
                            {
                                lock (gate)
                                {
                                    dump();
                                }
                            });
                        }
                    }
                };

                subscription =
                    source
                        .ObserveOn(scheduler)
                        .Subscribe(onNext, onError, onCompleted);

                return () =>
                {
                    lock (gate)
                    {
                        o = null;
                        dispose();
                    }
                };
            });
        }
    }
}