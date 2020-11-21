using OxyPlot.Reactive.DemoApp.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for BusyView.xaml
    /// </summary>
    public partial class BusyView
    {
        public BusyView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.PlotModel, v => v.ProgressRingContentControl1.Content).
                DisposeWith(disposable);

                this.OneWayBind(this.ViewModel, vm => vm.IsBusy, v => v.ProgressRingContentControl1.IsBusy).
                DisposeWith(disposable);
            });
        }
    }
}