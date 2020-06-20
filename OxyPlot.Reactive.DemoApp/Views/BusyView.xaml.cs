using OxyPlot.Reactive.DemoApp.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
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

namespace OxyPlot.Reactive.DemoApp.Views
{
    /// <summary>
    /// Interaction logic for BusyView.xaml
    /// </summary>
    public partial class BusyView : ReactiveUserControl<BusyViewModel>
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
