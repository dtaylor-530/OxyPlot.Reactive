using System.Reactive.Linq;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Reactive.Threading.Tasks;
using Betfair.ViewModel.Profits;
using Betfair.View.Base;

namespace Betfair.View.Profits
{
    /// <summary>
    /// Interaction logic for TestUserControl.xaml
    /// </summary>
    public partial class TestSubView : ReactiveUserControl<TestSubViewModel>
    {
        private readonly ListBox ListBox1;
        
        public TestSubView()
        {
            InitializeComponent();

            ListBox1 = this.Resources["ListBox1"] as ListBox;
            var dockHost = this.Resources["DockPanelHost"] as DependencyObject;
            var viewModelViewHost = this.Resources["ViewModelViewHost1"] as ViewModelViewHost;

            this.WhenActivated(disposables =>
            {
                //this.OneWayBind(this.ViewModel, vm => vm.ProfitModels, v => v.ListBox1.ItemsSource)
                //.DisposeWith(disposables);

                ListBox1.ItemsSource = ViewModel.ProfitModels;

                ViewModel.GetStatistics()
                   .ToObservable()
                   .ObserveOn(RxApp.MainThreadScheduler)
                   .Subscribe(a =>
                   {
                       var (mean, variance) = a;
                       Mean.Text = mean.ToString("N");
                       Variance.Text = variance.ToString("N");
                       TransitionControl.Visibility = Visibility.Visible;
                   }).DisposeWith(disposables);

                ListBox1.ToChanges().Cast<TestSubChartViewModel>().Subscribe(a =>
                {
                    viewModelViewHost.ViewModel = a;
                    TCC1.Content = viewModelViewHost;

                    a.Back.Subscribe(e =>
                    {
                        TCC1.Content = ListBox1;
                    }).DisposeWith(disposables);
                }).DisposeWith(disposables);


            });
        }
    }
}