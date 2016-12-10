using System.Collections.Generic;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using ArtemisWest.Mayfair.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;

namespace ArtemisWest.Mayfair.Shell.Controls
{
    public sealed class ProgressBarSyncBehavior : RegionBehavior, IHostAwareRegionBehavior, INotifyPropertyChanged
    {
        public static readonly string BehaviorKey = "ProgressBarSyncBehavior";
        private readonly ISet<IViewModelStatus> _vms = new HashSet<IViewModelStatus>();
        private ProgressBar _hostControl;
        private bool _hasBusyViewModels;

        public DependencyObject HostControl
        {
            get { return _hostControl; }
            set { _hostControl = value as ProgressBar; }
        }

        public bool HasBusyViewModels
        {
            get { return _hasBusyViewModels; }
            set
            {
                if (_hasBusyViewModels != value)
                {
                    _hasBusyViewModels = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override void OnAttach()
        {
            Region.Views.CollectionChanged += Views_CollectionChanged;

            _hostControl.DataContext = this;
            BindingOperations.SetBinding(_hostControl, UIElement.VisibilityProperty,
                new Binding("HasBusyViewModels")
                {
                    Converter = new BooleanToVisibilityConverter()
                });
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IViewModelStatus vm in e.NewItems)
                {
                    vm.PropertyChanged += ViewModel_PropertyChanged;
                    ProcessViewModel(vm);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IViewModelStatus vm in e.OldItems)
                {
                    vm.PropertyChanged -= ViewModel_PropertyChanged;
                    RemoveViewModel(vm);
                }
            }
        }


        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status" || e.PropertyName == string.Empty)
            {
                var vm = sender as IViewModelStatus;
                ProcessViewModel(vm);
            }
        }
        
        private void ProcessViewModel(IViewModelStatus vm)
        {
            _vms.Add(vm);
            RefreshState();
        }
        
        private void RemoveViewModel(IViewModelStatus vm)
        {
            _vms.Remove(vm);
            RefreshState();
        }

        private void RefreshState()
        {
            HasBusyViewModels = _vms.Any(vm => vm.Status.IsBusy);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}