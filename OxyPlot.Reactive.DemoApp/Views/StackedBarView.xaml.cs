﻿using ReactivePlot.Data.Factory;
using ReactivePlot.OxyPlot;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for StackedBarView.xaml
    /// </summary>
    public partial class StackedBarView : Page
    {
        private StackedBarModel sb1, sb2, sb3;

        public StackedBarView()
        {
            InitializeComponent();

            sb1 = new StackedBarModel(plotView1.Model ?? new OxyPlot.PlotModel());
            sb2 = new StackedBarModel(plotView2.Model ?? new OxyPlot.PlotModel());
            sb3 = new StackedBarModel(plotView3.Model ?? new OxyPlot.PlotModel());

            NewMethod1().Subscribe(sb1);
            NewMethod2().Subscribe(sb2);
            NewMethod3().Subscribe(sb3);
        }

        private IObservable<(string, string, double)> NewMethod1()
        {
            var get2 = new DataFactory().GetLine2().GetEnumerator();
            var observable2 = Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1).Take(1000);

            return observable2;
        }

        private IObservable<(string, string, double)> NewMethod2()
        {
            var get3 = new DataFactory().GetLine2().Take(100).ToObservable();
            return get3;
        }

        private IObservable<(string, string, double)> NewMethod3()
        {
            var get3 = new DataFactory().GetLine3().Take(60).ToObservable().SelectMany((a, i) => Observable.Return(a).Delay(TimeSpan.FromSeconds(i)));
            return get3;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sb1?.Reset();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            sb1?.OnNext(ToggleButton1.IsChecked ?? false);
            sb2?.OnNext(ToggleButton1.IsChecked ?? false);
            sb3?.OnNext(ToggleButton1.IsChecked ?? false);
            //MessageBox.Show("IsStacked is " + (ToggleButton1.IsChecked?.ToString()));
        }
    }
}