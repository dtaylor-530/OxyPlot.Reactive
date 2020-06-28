using Betfair.ViewModel.Profits;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Betfair.View.Profits
{
    /// <summary>
    /// Interaction logic for TestCbar_l1.xaml
    /// </summary>
    public partial class TestSubChartView : ReactiveUserControl<TestSubChartViewModel>
    {
        public TestSubChartView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.Title, v => v.TextBlock1.Content)
                .DisposeWith(disposables);
                //this.OneWayBind(this.ViewModel, vm => vm.Model, v => v.PlotView1.Model)
                //        .DisposeWith(disposables);


                this.PlotView1.Model = ViewModel.Model;
                //this.OneWayBind(this.ViewModel, vm => vm.Collection, v => v.DataGrid1.ItemsSource);


            });
        }
    }
}
