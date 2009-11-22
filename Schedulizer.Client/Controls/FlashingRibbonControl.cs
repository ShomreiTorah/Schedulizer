using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Ribbon.ViewInfo;
using DevExpress.XtraBars;
using System.Drawing;

namespace ShomreiTorah.Schedules.WinClient.Controls {
	class FlashingRibbonControl : RibbonControl {
		class MyRibbonViewInfo : RibbonViewInfo {
			public MyRibbonViewInfo(RibbonControl ribbon) : base(ribbon) { }
			public new RibbonItemViewInfo FindItem(IRibbonItem item, Rectangle bounds) {
				return base.FindItem(item, bounds);
			}
		}
		MyRibbonViewInfo viewInfo;
		protected override RibbonViewInfo CreateViewInfo() {
			return viewInfo = new MyRibbonViewInfo(this);
		}


		BarItemLink highlightedItem;

		static readonly Point InvalidPoint = new Point(-1234, -1234);
		public BarItemLink HighlightedItem {
			get { return highlightedItem; }
			set {
				if (HighlightedItem != null && viewInfo.HotObject is FakeHitInfo)
					viewInfo.HotObject = null;

				highlightedItem = value;

				if (viewInfo.HotObject == null
				|| (viewInfo.HotObject.Item == null && viewInfo.HotObject.PageGroup == null && viewInfo.HotObject.Page == null)) {
					if (HighlightedItem == null)
						viewInfo.HotObject = null;
					else {
						var myHit = new FakeHitInfo(viewInfo.FindItem(HighlightedItem, highlightedItem.Bounds));
						viewInfo.HotObject = myHit;
					}
				}
			}
		}
		class FakeHitInfo : RibbonHitInfo {
			public FakeHitInfo(RibbonItemViewInfo info) {
				SetItem(info, RibbonHitTest.Item);
				base.PageGroup = info.Owner as RibbonPageGroup;
				HitPoint = InvalidPoint;
			}
		}
	}
}
