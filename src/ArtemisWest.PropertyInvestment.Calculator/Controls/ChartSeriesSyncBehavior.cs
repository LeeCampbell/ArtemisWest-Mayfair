using System.Windows;
using System.Collections.Specialized;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    public class ChartSeriesSyncBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        public static readonly string BehaviorKey = "ChartSeriesSyncBehavior";
        private Chart _hostControl;

        public DependencyObject HostControl
        {
            get { return this._hostControl; }
            set { this._hostControl = value as Chart; }
        }

        protected override void OnAttach()
        {
            base.Region.Views.CollectionChanged += this.Views_CollectionChanged;
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int newStartingIndex = e.NewStartingIndex;
                //HACK:
                foreach (CalculationViewModel newViewModel in e.NewItems)
                {
                    var newSeries = CreateLineSeries(newViewModel);

                    _hostControl.Series.Insert(newStartingIndex++, newSeries);
                }
            }
        }

        private static LineSeries CreateLineSeries(CalculationViewModel newViewModel)
        {
            var newSeries = new LineSeries
                                {
                                    DataContext = newViewModel,
                                    Title = newViewModel.Title,
                                    DependentValuePath = "Value",
                                    IndependentValuePath = "Date"
                                };

            var filter = new CollectionSizeFilter();
            BindingOperations.SetBinding(filter,
                                         CollectionSizeFilter.MaxItemCountProperty,
                                         new Binding("ActualWidth")
                                             {
                                                 RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Chart), 1),
                                                 Converter = new DivisionConverter(),
                                                 ConverterParameter = 25
                                             }
                                       );
            BindingOperations.SetBinding(filter,
                                         CollectionViewSource.SourceProperty,
                                         new Binding("ResultOverTime"));
            newSeries.Resources.Add("FilteredBalances", filter);


            BindingOperations.SetBinding(newSeries,
                                         DataPointSeries.ItemsSourceProperty,
                                         new Binding { Source = filter });
            return newSeries;
        }
    }
}