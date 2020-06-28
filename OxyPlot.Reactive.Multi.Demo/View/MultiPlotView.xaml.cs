using Betfair.View.Base;
using Betfair.ViewModel.Base;
using Betfair.ViewModel.Profits;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Betfair.View.Profits
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : ReactiveUserControl<TestViewModel>
    {

        private ObservableCollection<object> observableCollection = new ObservableCollection<object>();
        public TestView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(
                        this.ViewModel,
                        vm => vm.SubViewModels,
                        v => v.ListBox1.ItemsSource)
                    .DisposeWith(disposables);

                //ListBox1.ItemsSource = observableCollection;

                (ViewModel.SubViewModels as INotifyCollectionChanged).CollectionChanged += TestView_CollectionChanged;

                this.RadioButtonList1.WhenAnyValue(a => a.Value).Subscribe(a =>
                { });

                ListBox1.ToChanges().Cast<TestSubViewModel>().Subscribe(a =>
                {

                });

                this.RadioButtonList1.WhenAnyValue(a => a.Value)
                            .Cast<WagerModifierType>()
                                  .CombineLatest(ListBox1.ToChanges().Cast<TestSubViewModel>(), (wagerMod, keyModel) => (wagerMod, keyModel.Name))
                     //.SubscribeOn(RxApp.MainThreadScheduler)
                     .Subscribe(b =>
                    {
                        this.ViewModel.WagerModifier = b.wagerMod;
                        sampleViewModelViewHost.ViewModel = ViewModel.SubViewModels.Single(a => a.Name == b.Name);
                    }, e =>
                         {
                    }).DisposeWith(disposables);
            });
        }

        private void TestView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                observableCollection.Add(e.NewItems.Cast<object>().FirstOrDefault());
        }
    }
}


