using System;਍甀猀椀渀最 匀礀猀琀攀洀⸀䌀漀氀氀攀挀琀椀漀渀猀⸀䜀攀渀攀爀椀挀㬀ഀഀ
using System.ComponentModel;਍甀猀椀渀最 匀礀猀琀攀洀⸀䐀愀琀愀㬀ഀഀ
using System.Drawing;਍甀猀椀渀最 匀礀猀琀攀洀⸀吀攀砀琀㬀ഀഀ
using System.Windows.Forms;਍甀猀椀渀最 䐀攀瘀䔀砀瀀爀攀猀猀⸀堀琀爀愀䔀搀椀琀漀爀猀㬀ഀഀ
using ShomreiTorah.Common.Calendar;਍甀猀椀渀最 匀礀猀琀攀洀⸀䰀椀渀焀㬀ഀഀ
using DevExpress.XtraVerticalGrid.Events;਍甀猀椀渀最 䐀攀瘀䔀砀瀀爀攀猀猀⸀堀琀爀愀䈀愀爀猀㬀ഀഀ
using ShomreiTorah.Common;਍甀猀椀渀最 匀礀猀琀攀洀⸀䜀氀漀戀愀氀椀稀愀琀椀漀渀㬀ഀഀ
਍渀愀洀攀猀瀀愀挀攀 匀栀漀洀爀攀椀吀漀爀愀栀⸀匀挀栀攀搀甀氀攀猀⸀嘀攀爀椀昀椀攀爀 笀ഀഀ
	partial class MainForm : DevExpress.XtraEditors.XtraForm {਍ऀऀ瀀甀戀氀椀挀 䴀愀椀渀䘀漀爀洀⠀⤀ 笀ഀഀ
			InitializeComponent();਍ഀഀ
			yearItem.EditValue = DateTime.Now.Year;਍ഀഀ
		}਍ऀऀ瘀漀椀搀 唀瀀搀愀琀攀䐀愀琀愀⠀⤀ 笀 瘀䜀爀椀搀⸀䐀愀琀愀匀漀甀爀挀攀 㴀 䜀攀琀䐀愀礀猀⠀⤀⸀吀漀䄀爀爀愀礀⠀⤀㬀 紀ഀഀ
਍ऀऀ䤀䔀渀甀洀攀爀愀戀氀攀㰀䌀漀氀甀洀渀䌀漀洀瀀愀爀椀猀漀渀㸀 䜀攀琀䐀愀礀猀⠀⤀ 笀ഀഀ
			var year = Convert.ToInt32(yearItem.EditValue, CultureInfo.CurrentUICulture);਍ഀഀ
			HebrewDate start = new DateTime(year, 1, 1);਍ഀഀ
			var days = Enumerable.Range(0, DateTime.IsLeapYear(year) ? 366 : 365).Select(d => start + d).Where(d => d.Info.Isשבתיוםטוב).Select(d => new ColumnComparison(d));਍ഀഀ
			return showBoring.Checked ? days : days.Where(cc => cc.HasDifferences);਍ऀऀ紀ഀഀ
਍ऀऀ猀琀愀琀椀挀 爀攀愀搀漀渀氀礀 䘀漀渀琀 一漀爀洀愀氀䘀漀渀琀 㴀 䐀攀昀愀甀氀琀䘀漀渀琀㬀ഀഀ
		static readonly Font BoldFont = new Font(NormalFont, FontStyle.Bold);਍ऀऀ瀀爀椀瘀愀琀攀 瘀漀椀搀 瘀䜀爀椀搀开刀攀挀漀爀搀䌀攀氀氀匀琀礀氀攀⠀漀戀樀攀挀琀 猀攀渀搀攀爀Ⰰ 䜀攀琀䌀甀猀琀漀洀刀漀眀䌀攀氀氀匀琀礀氀攀䔀瘀攀渀琀䄀爀最猀 攀⤀ 笀ഀഀ
			var sr = vGrid.GetCellValue(e.Row, e.RecordIndex) as ValueReference;਍ऀऀऀ椀昀 ⠀猀爀 㴀㴀 渀甀氀氀⤀ 爀攀琀甀爀渀㬀ഀഀ
			var svc = sr.Reference;਍ഀഀ
			if (e.CellIndex == 1)	//Only new values can be bold਍ऀऀऀऀ攀⸀䄀瀀瀀攀愀爀愀渀挀攀⸀䘀漀渀琀 㴀 猀瘀挀⸀䤀猀䈀漀氀搀 㼀 䈀漀氀搀䘀漀渀琀 㨀 一漀爀洀愀氀䘀漀渀琀㬀ഀഀ
			e.Appearance.BackColor = svc.AreSame ? Color.PaleGreen : Color.PaleVioletRed;਍ऀऀ紀ഀഀ
਍ऀऀ瀀爀椀瘀愀琀攀 瘀漀椀搀 䐀愀琀愀匀漀甀爀挀攀䌀栀愀渀最攀搀⠀漀戀樀攀挀琀 猀攀渀搀攀爀Ⰰ 䔀瘀攀渀琀䄀爀最猀 攀⤀ 笀 唀瀀搀愀琀攀䐀愀琀愀⠀⤀㬀 紀ഀഀ
		private void showBoring_CheckedChanged(object sender, ItemClickEventArgs e) { UpdateData(); }਍ഀഀ
		private void vGrid_FocusedRecordChanged(object sender, IndexChangedEventArgs e) {਍ऀऀऀ椀昀 ⠀攀⸀一攀眀䤀渀搀攀砀 㰀 　⤀ 笀ഀഀ
				newCellsPanel.Hide();਍ऀऀऀ紀 攀氀猀攀 笀ഀഀ
				var cc = (ColumnComparison)vGrid.GetRecordObject(e.NewIndex);਍ഀഀ
				var priorDate = cc.Date.AddDays(-1);਍ഀഀ
				newCellsPanel.Text = cc.Date.ToLongDateString() + " " + new ScheduleCalculator(cc.Date).CalcTitle();਍ऀऀऀऀ渀攀眀䌀攀氀氀猀䜀爀椀搀⸀䐀愀琀愀匀漀甀爀挀攀 㴀 ⠀ഀഀ
					from t in cc.PriorCell਍ऀऀऀऀऀ最爀漀甀瀀 琀 戀礀 琀⸀一愀洀攀 椀渀琀漀 最ഀഀ
					orderby g.First().Time਍ऀऀऀऀऀ猀攀氀攀挀琀 渀攀眀 笀ഀഀ
						Name = g.Key,਍ऀऀऀऀऀऀ嘀愀氀甀攀 㴀 最⸀匀攀氀攀挀琀⠀琀 㴀㸀 琀⸀吀椀洀攀匀琀爀椀渀最⤀⸀䨀漀椀渀⠀∀Ⰰ ∀⤀Ⰰഀഀ
						IsBold = g.Any(t => t.IsBold),਍ऀऀऀऀऀऀ䐀愀礀 㴀 瀀爀椀漀爀䐀愀琀攀ഀഀ
					}਍ऀऀऀ   ⤀⸀䌀漀渀挀愀琀⠀ഀഀ
					from t in cc.Cell਍ऀऀऀऀऀ最爀漀甀瀀 琀 戀礀 琀⸀一愀洀攀 椀渀琀漀 最ഀഀ
					orderby g.First().Time਍ऀऀऀऀऀ猀攀氀攀挀琀 渀攀眀 笀ഀഀ
						Name = g.Key,਍ऀऀऀऀऀऀ嘀愀氀甀攀 㴀 最⸀匀攀氀攀挀琀⠀琀 㴀㸀 琀⸀吀椀洀攀匀琀爀椀渀最⤀⸀䨀漀椀渀⠀∀Ⰰ ∀⤀Ⰰഀഀ
						IsBold = g.Any(t => t.IsBold),਍ऀऀऀऀऀऀ䐀愀礀 㴀 挀挀⸀䐀愀琀攀ഀഀ
					}਍ऀऀऀऀ⤀⸀吀漀䄀爀爀愀礀⠀⤀㬀ഀഀ
਍ऀऀऀऀ渀攀眀䌀攀氀氀猀倀愀渀攀氀⸀匀栀漀眀⠀⤀㬀ഀഀ
			}਍ऀऀ紀ഀഀ
	}਍紀�