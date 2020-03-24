using OxyPlotEx.DemoApp;
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
    public partial class StackedBarView : Page
    {
        ViewModel.StackedBarModel sb;

        public StackedBarView()
        {
            InitializeComponent();

            sb = new ViewModel.StackedBarModel(new DispatcherX(this.Dispatcher));
            plotView1.Model = sb.Model;
            NewMethod1().Subscribe(sb);

            var sb2 = new ViewModel.StackedBarModel(new DispatcherX(this.Dispatcher));
            plotView2.Model = sb2.Model;
            NewMethod().Subscribe(sb2);

        }

        private IObservable<(string, string, double)> NewMethod1()
        {
            var get2 = new DataFactory().GetLine2().GetEnumerator();
            var observable2 = Observable.Interval(TimeSpan.FromMilliseconds(1)).Select(t =>
            {
                get2.MoveNext();
                return get2.Current;
            }).Skip(1);

            return observable2;
        }

        private IObservable<(string, string, double)> NewMethod()
        {
            var get3 = new DataFactory().GetLine2().Take(100).ToObservable();
            return get3;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sb.Reset();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show((ToggleButton1.IsChecked?.ToString()));
            sb.OnNext(ToggleButton1.IsChecked ?? false);

        }
    }
}
