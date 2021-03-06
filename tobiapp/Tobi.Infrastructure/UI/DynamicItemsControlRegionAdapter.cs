﻿using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Regions;

namespace Tobi.Infrastructure.UI
{
    /// <summary>
    /// See Microsoft.Practices.Composite.Presentation.Regions.ItemsControlRegionAdapter
    /// </summary>
    public class DynamicItemsControlRegionAdapter : RegionAdapterBase<ItemsControl>
    {
        public DynamicItemsControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
            // Nothing else to do here.
        }

        protected override void Adapt(IRegion region, ItemsControl regionTarget)
        {
            AddItems(regionTarget, region.ActiveViews.ToList());

            region.ActiveViews.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    AddItems(regionTarget, e.NewItems);
                }
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    RemoveItems(regionTarget, e.OldItems);
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        private static void AddItems(ItemsControl control, IList newItems)
        {
            if (newItems == null)
            {
                return;
            }

            foreach (var item in newItems)
            {
                control.Items.Add(item);
            }

            var dependencyItem = control as DependencyObject;
            if (dependencyItem == null)
            {
                return;
            }

            RegionManager.SetRegionName(dependencyItem, RegionManager.GetRegionName(dependencyItem));
        }

        private static void RemoveItems(ItemsControl control, IList oldItems)
        {
            if (oldItems == null)
            {
                return;
            }

            foreach (var item in oldItems)
            {
                control.Items.Remove(item);
            }
        }
    }
}
