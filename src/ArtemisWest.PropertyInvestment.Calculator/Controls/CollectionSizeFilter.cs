using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ArtemisWest.Mayfair.Infrastructure;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    public class CollectionSizeFilter : CollectionViewSource
    {
        int _count;
        ICollectionView _defaultView;
        HashSet<object> _toKeep;

        public CollectionSizeFilter()
        {
            Filter += OnFilter;
        }

        protected virtual void OnFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = _toKeep == null || _toKeep.Contains(e.Item);
        }

        protected override void OnSourceChanged(object oldSource, object newSource)
        {
            base.OnSourceChanged(oldSource, newSource);
            _defaultView = GetDefaultView(newSource);
            _count = _defaultView.SourceCollection.Count();
            LoadHashset();
        }

        public double MaxItemCount
        {
            get { return (double)GetValue(MaxItemCountProperty); }
            set { SetValue(MaxItemCountProperty, value); }
        }
        public static readonly DependencyProperty MaxItemCountProperty = DependencyProperty.Register("MaxItemCount", typeof(double), typeof(CollectionSizeFilter), new UIPropertyMetadata(1d, MaxItemCountProperty_Changed));

        private static void MaxItemCountProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var self = (CollectionSizeFilter)sender;
            self.LoadHashset();
        }

        private void LoadHashset()
        {
            if (_count <= MaxItemCount)
            {
                _toKeep = null;
            }
            else
            {
                _toKeep = new HashSet<object>();
                var gap = MaxItemCount - 1;
                var spacing = _count / gap;
                double nextIndex = 0d;
                int i = 0;
                foreach (var item in _defaultView.SourceCollection)
                {
                    if (i >= nextIndex)
                    {
                        _toKeep.Add(item);
                        nextIndex += spacing;
                    }
                    i++;
                }
            }
            if (View != null)
                View.Refresh();
        }
    }
}
