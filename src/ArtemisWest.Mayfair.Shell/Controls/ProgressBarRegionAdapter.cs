using System;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.Mayfair.Shell.Controls
{
    public sealed class ProgressBarRegionAdapter : RegionAdapterBase<ProgressBar>
    {
        public ProgressBarRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        #region Overrides of RegionAdapterBase<Chart>

        protected override void AttachBehaviors(IRegion region, ProgressBar regionTarget)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            var regionBehavior = new ProgressBarSyncBehavior { HostControl = regionTarget };
            region.Behaviors.Add(ProgressBarSyncBehavior.BehaviorKey, regionBehavior);
            base.AttachBehaviors(region, regionTarget);

        }

        protected override void Adapt(IRegion region, ProgressBar regionTarget)
        {
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        #endregion
    }
}
