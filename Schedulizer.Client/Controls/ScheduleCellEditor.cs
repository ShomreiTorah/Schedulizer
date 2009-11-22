using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace ShomreiTorah.Schedules.WinClient.Controls {
	partial class ScheduleCellEditor : XtraUserControl {
		public ScheduleCellEditor() {
			InitializeComponent();
		}

		ScheduleCell cell;
		ScheduleCalculator cellCalculator;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ScheduleCell Cell {
			get { return cell; }
			set {
				if (Cell != null)
					Cell.PropertyChanged -= Cell_PropertyChanged;

				cell = null;

				splitContainerControl1.Visible = (value != null);
				nullLabel.Visible = (value == null);

				if (value != null) {
					cellCalculator = new ScheduleCalculator(value.Date);
					value.PropertyChanged += Cell_PropertyChanged;
					title.Text = value.Title;
					grid.DataSource = value.Times;
				}

				cell = value;	//By setting this last, I avoid title_EditValueChanged
			}
		}


		void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "Title")
				title.Text = Cell.Title;
			else if (e.PropertyName == "Date")
				cellCalculator = new ScheduleCalculator(Cell.Date);
		}
		private void title_EditValueChanged(object sender, EventArgs e) {
			if (Cell != null)
				Cell.Title = title.Text;
		}

		private void timeEdit_ButtonClick(object sender, ButtonPressedEventArgs e) { DeleteTime(); }

		void DeleteTime() {
			if (gridView.FocusedRowHandle == GridControl.NewItemRowHandle) {
				gridView.CancelUpdateCurrentRow();
				return;
			}

			var time = gridView.GetFocusedRow() as ScheduleTime;
			if (time == null)
				return;


			if (DialogResult.No == XtraMessageBox.Show("Are you sure you want to delete " + time.Name + "?",
													   "Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			time.Cell.Context.DeleteObject(time);
		}

		private void grid_ProcessGridKey(object sender, KeyEventArgs e) {
			if (e.KeyData == Keys.Delete) {
				if (!(gridView.ActiveEditor is TextEdit))
					DeleteTime();
			} else if (e.KeyData == (Keys.Control | Keys.B)) {
				var time = gridView.GetFocusedRow() as ScheduleTime;
				if (time == null) return;

				time.IsBold = !time.IsBold;
				e.Handled = true;
			}
		}

		#region Context Menus
		///<summary>The time on which the context menu was invoked, or null if it was invoked on the background.</summary>
		ScheduleTime menuTime;
		private void grid_MouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				var hitInfo = gridView.CalcHitInfo(e.Location);
				if (hitInfo.InRow) {
					if (hitInfo.RowHandle == GridControl.NewItemRowHandle)
						return;
					menuTime = gridView.GetRow(hitInfo.RowHandle) as ScheduleTime;
					menuDelete.Caption = "&Delete " + menuTime.Name;
					menuRecalc.Caption = "&Recalculate " + menuTime.Name;
					menuRecalc.Enabled = cellCalculator.CalcTimes().Any(t => t.Name.Equals(menuTime.Name, StringComparison.CurrentCultureIgnoreCase));
				} else if (hitInfo.HitTest == GridHitTest.EmptyRow) {
					menuTime = null;
					menuDelete.Caption = "&Delete All";
					menuRecalc.Caption = "&Recalculate All";
				} else
					return;
				timeContextMenu.ShowPopup(grid.PointToScreen(e.Location));
			}
		}

		private void menuRecalc_ItemClick(object sender, ItemClickEventArgs e) {
			if (menuTime == null)
				Cell.Recalculate();
			else {
				foreach (var time in Cell.Times.Where(t => t.Name.Equals(menuTime.Name, StringComparison.CurrentCultureIgnoreCase)).ToArray())
					Cell.Times.Remove(time);

				Cell.AddTimes(cellCalculator.CalcTimes().Where(t => t.Name.Equals(menuTime.Name, StringComparison.CurrentCultureIgnoreCase)));
			}
		}

		private void menuDelete_ItemClick(object sender, ItemClickEventArgs e) {
			if (menuTime == null) {
				if (DialogResult.Yes == XtraMessageBox.Show("Are you sure you want to delete all of the times from this cell?",
															"Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					Cell.Times.Clear();
			} else {
				if (DialogResult.Yes == XtraMessageBox.Show("Are you sure you want to delete " + menuTime.Name + "?",
															"Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					Cell.Times.Remove(menuTime);
			}
		}

		bool addedTitleMenu;
		private void title_Properties_BeforeShowMenu(object sender, BeforeShowMenuEventArgs e) {
			if (!addedTitleMenu) {
				e.Menu.Items.Add(new DXMenuItem("&Reset Title", menuRecalcTitle_ItemClick) { BeginGroup = true });
			}
			addedTitleMenu = true;
		}

		private void menuRecalcTitle_ItemClick(object sender, EventArgs e) {
			Cell.Title = cellCalculator.CalcTitle();
		}
		#endregion
	}
}
