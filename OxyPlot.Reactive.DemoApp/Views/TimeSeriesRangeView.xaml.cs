using Itenso.TimePeriod;
using OxyPlot.Data.Common;
using OxyPlot.Data.Factory;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using static OxyPlot.Data.Factory.TimeDataSource;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for MultiDateTime2View.xaml
    /// </summary>
    public partial class MultiDateTimeRangeView : UserControl
    {
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty EndProperty = DependencyProperty.Register("End", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register("StartDate", typeof(DateTime), typeof(MultiDateTimeRangeView), new PropertyMetadata(default(DateTime)));
        public static readonly DependencyProperty EndDateProperty = DependencyProperty.Register("EndDate", typeof(DateTime), typeof(MultiDateTimeRangeView), new PropertyMetadata(default(DateTime)));
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty MinDateProperty = DependencyProperty.Register("MinDate", typeof(DateTime), typeof(MultiDateTimeRangeView), new PropertyMetadata(default));
        public static readonly DependencyProperty MaxDateProperty = DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(MultiDateTimeRangeView), new PropertyMetadata(default));
        public static readonly DependencyProperty TimeSpanProperty = DependencyProperty.Register("TimeSpan", typeof(TimeSpan), typeof(MultiDateTimeRangeView), new PropertyMetadata(default));
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty TimeValueProperty = DependencyProperty.Register("TimeValue", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(double), typeof(MultiDateTimeRangeView), new PropertyMetadata(0d));

        public MultiDateTimeRangeView()
        {
            InitializeComponent();

            var model = new TimeRangeModel<string>(PlotView1.Model ??= new PlotModel());
            var model2 = new TimeRangeModel<string>(PlotView2.Model ??= new PlotModel());
            var model3 = new TimeRangeModel<string>(PlotView3.Model ??= new PlotModel());
     
            var obs = TimeDataSource.Observe1000()
                .Pace(TimeSpan.FromSeconds(1))
                .Publish().RefCount();
            Random random = new Random();
            obs.SubscribeCustom(model);
            obs.SubscribeCustom(model2);
            obs.SubscribeCustom(model3);

            obs.Scan(new HashSet<DateTime>(), (a, b) => { a.Add(b.Value.Key); return a; })
                             .ObserveOnDispatcher()
               .SubscribeOnDispatcher()
               .Subscribe(c =>
                {
                    if (Value == c.Count - 1)
                        Value = c.Count;
                    Count = c.Count;
                });

            obs.ToMinMax(a => FromDateTime(a.Value.Key))
               .ObserveOnDispatcher()
               .SubscribeOnDispatcher()
               .Subscribe(a =>
               {
                   if (TimeValue == Max - Min || TimeValue == 0)
                   {
                       TimeValue = a.max - a.min;
                   }
                   var diff1 = Start - Min;
                   var diff2 = Max - End;
                   Min = a.min;
                   Max = a.max;
                   Start = Min + diff1;
                   End = Max - diff2;
                   MinDate = ToDateTime(Min);
                   MaxDate = ToDateTime(Max);
                   Time = Max - Min;

                   TimeSpan = ToTimeSpan(TimeValue);
                   StartDate = ToDateTime(Start);
                   EndDate = ToDateTime(End);
                   model.OnNext(new TimeRange(StartDate, EndDate));
                   model2.OnNext(ToTimeSpan(TimeValue));
               });

            this.WhenAnyValue(a => a.Value).Select(a => (int)a).Subscribe(model3.OnNext);
        }

        public double Start
        {
            get { return (double)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        public double End
        {
            get { return (double)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public DateTime StartDate
        {
            get { return (DateTime)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        public DateTime EndDate
        {
            get { return (DateTime)GetValue(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public double TimeValue
        {
            get { return (double)GetValue(TimeValueProperty); }
            set { SetValue(TimeValueProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Count
        {
            get { return (double)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }


    }

}