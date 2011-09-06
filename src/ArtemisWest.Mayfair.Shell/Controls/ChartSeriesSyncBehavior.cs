using System.Windows;
using System.Collections.Specialized;
using System.Windows.Controls.DataVisualization.Charting;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;

namespace ArtemisWest.Mayfair.Shell.Controls
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
                foreach (Series newSeries in e.NewItems)
                {
                    this._hostControl.Series.Insert(newStartingIndex++, newSeries);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Series oldSeries in e.OldItems)
                {
                    this._hostControl.Series.Remove(oldSeries);
                }
            }
        }
    }
}