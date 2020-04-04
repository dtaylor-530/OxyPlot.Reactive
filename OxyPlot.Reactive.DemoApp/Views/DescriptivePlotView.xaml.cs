using OxyPlot;
using OxyPlot.Wpf;
using OxyPlotEx.DemoApp;
using OxyPlotEx.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OxyPlotEx.DemoAppCore.Pages
{
    /// <summary>
    /// Interaction logic for StackedBarView.xaml
    /// </summary>
    public partial class DescriptivePlotView : Page
    {
        private readonly DescriptivePlotModel sb;
        private readonly DescriptiveMultiPlotModel sb2;

        //ViewModel.StackedBarModel sb;

        public DescriptivePlotView()
        {
            InitializeComponent();

            //sb = new ViewModel.StackedBarModel(new DispatcherX(this.Dispatcher));
            //plotView1.Model = sb.Model;
            //NewMethod1().Subscribe(sb);
            plotView1.Model = new OxyPlot.PlotModel();
            sb = new ViewModel.DescriptivePlotModel(new DispatcherX(this.Dispatcher), plotView1.Model);
            NewMethod().Subscribe(sb);

            CongfigureBindings(plotView1.ActualController);

            plotView2.Model = new PlotModel();
            sb2 = new DescriptiveMultiPlotModel(new DispatcherX(this.Dispatcher), plotView2.Model);
            NewMethod2().Subscribe(sb2);

            CongfigureBindings2(plotView2.ActualController);




        }

        private void CongfigureBindings(IPlotController pc)
        {
            pc.UnbindMouseDown(OxyMouseButton.Left);
            pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
            pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

            pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                         (view, controller, args) =>
                            controller.AddMouseManipulator(view, new WpbTrackerManipulator(view), args)));
        }

        private void CongfigureBindings2(IPlotController pc)
        {
            pc.UnbindMouseDown(OxyMouseButton.Left);
            pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
            pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

            pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                         (view, controller, args) =>
                            controller.AddMouseManipulator(view, new WpbTrackerManipulator2(view), args)));
        }

        //private IObservable<(string, string, double)> NewMethod1()
        //{
        //    var get2 = new DataFactory().GetLine2().GetEnumerator();
        //    var observable2 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
        //    {
        //        get2.MoveNext();
        //        return get2.Current;
        //    }).Skip(1);

        //    return observable2;
        //}

        private IObservable<CustomDataPoint> NewMethod()
        {
            var get3 = new DataFactory().GetLine2().Take(100).ToObservable();
            return get3.Select((g, i) => new CustomDataPoint(i, g.Item3, g.Item1));
        }

        private IObservable<KeyValuePair<string, IDataPointProvider>> NewMethod2()
        {
            var get3 = new DataFactory().GetLine2(25).Take(100).ToObservable();
            return get3.Select((g, i) => KeyValuePair.Create(g.Item2, (IDataPointProvider) new CustomDataPoint(i, g.Item3, g.Item1)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //sb.Reset();
        }


        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ToggleButton1.IsChecked ?? false)
                sb.OnNext("Description");
            else
                sb.OnNext("X={2},\nY={4},\nAdditionalInfo={Description}");
        }
    }

    public class CustomDataPoint : IDataPointProvider
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Description { get; set; }
        public DataPoint GetDataPoint() => new DataPoint(X, Y);

        public CustomDataPoint(double x, double y, string description)
        {
            X = x;
            Y = y;
            Description = description;
        }
    }
}
