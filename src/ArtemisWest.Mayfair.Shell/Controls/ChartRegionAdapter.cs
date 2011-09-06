using System;
using System.Windows.Controls.DataVisualization.Charting;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.Mayfair.Shell.Controls
{
    public sealed class ChartRegionAdapter : RegionAdapterBase<Chart>
    {
        public ChartRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        #region Overrides of RegionAdapterBase<Chart>

        protected override void AttachBehaviors(IRegion region, Chart regionTarget)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            var regionBehavior = new ChartSeriesSyncBehavior { HostControl = regionTarget };
            region.Behaviors.Add(ChartSeriesSyncBehavior.BehaviorKey, regionBehavior);
            base.AttachBehaviors(region, regionTarget);

        }

        protected override void Adapt(IRegion region, Chart regionTarget)
        {
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        #endregion
    }
}
