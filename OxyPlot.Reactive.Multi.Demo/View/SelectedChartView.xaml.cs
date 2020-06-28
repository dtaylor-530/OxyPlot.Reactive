using Betfair.ViewModel.Profits;
using Endless;
using MoreLinq;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Betfair.View.Base;
namespace Betfair.View.Profits
{
    /// <summary>
    /// Interaction logic for TestSelectedChartView.xaml
    /// </summary>
    public partial class TestSelectedChartView : ReactiveUserControl<TestSubChartViewModel>
    {
        public TestSelectedChartView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.Collection, v => v.DataGrid1.ItemsSource)
                .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Model, v => v.PlotView1.Model)
                    .DisposeWith(disposables);

                this.ViewModel.WhenAnyValue(a => a.Selected).Where(b => b != null).Subscribe(d =>
                {
                    var n = this.ViewModel.Collection.Index().Where(a => (a.Value.Key, a.Value.DateTime) == (d.Key, d.DateTime)).Cached();

                    if(n.Count()==1)
                    {
                        var index = n.Single().Key;
                        DataGrid1.SelectedIndex = index;
                        DataGrid1.ScrollIntoView(DataGrid1.Items[index]);
                    }
                    else
                    {

                    }
                }).DisposeWith(disposables);

                //this.Back1.Events().MouseLeftButtonDown.Do(a=>a.Handled =true ).Select(a=>Unit.Default).InvokeCommand(ViewModel, vm => vm.Back)
                this.Back1.ToClicks().Select(a => Unit.Default).Subscribe(a=> ViewModel.Back.Execute(a).Subscribe().DisposeWith(disposables))
                .DisposeWith(disposables);

            });

        }
    }
}
