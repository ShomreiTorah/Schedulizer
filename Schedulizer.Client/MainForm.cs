using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using Microsoft.Office.Interop.PowerPoint.Extensions;
using Microsoft.Office.Interop.Word.Extensions;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Schedules.Export;
using ShomreiTorah.Schedules.WinClient.Properties;
using ShomreiTorah.WinForms.Controls;
using ShomreiTorah.WinForms.Forms;
using MsoBool = Microsoft.Office.Core.MsoTriState;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Word = Microsoft.Office.Interop.Word;

namespace ShomreiTorah.Schedules.WinClient {
	partial class MainForm : RibbonForm, IExportUIProvider {
		ScheduleContext context;
		CellLoader loader;

		public MainForm() {
			InitializeComponent();
			flashingButton = wbUpdateDirty.Links[0];
		}
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);

			Waiter.ExecAsync(ui => {
				ui.Caption = "Connecting to SQL Server...";
				context = new ScheduleContext(DB.Default);
				loader = new CellLoader(context);
				ui.Caption = "Loading data...";
				context.LoadCells(calendar.MonthStart.Last(DayOfWeek.Sunday), calendar.MonthStart.Last(DayOfWeek.Sunday) + 7 * 6);

				BeginInvoke(new Action(delegate {
					calendar.ContentRenderer = new Controls.SchedulizerCalendarContentRenderer(calendar.CalendarPainter, loader);
					calendar.Invalidate();

					UpdateCellPanel();

					if (IsWordRunning) {
						HandleWord();
					} else {
						wordBinderMenu.BeforePopup += wordBinderMenu_BeforePopup;
						//TODO: Timer?
					}
				}));
			}, "Loading", false);
		}

		///<summary>Releases the unmanaged resources used by the MainForm and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (context != null) context.Dispose();
				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error message")]
		void SaveDB() {
			Waiter.ExecAsync(() => context.SaveChanges(), "Saving schedule to SQL Server...");
		}
		private void doSave_ItemClick(object sender, ItemClickEventArgs e) { SaveDB(); }

		private void resetMonth_ItemClick(object sender, ItemClickEventArgs e) {
			if (DialogResult.Yes != XtraMessageBox.Show("Are you sure you want to recalculate the schedule for all of the days currently displayed?\r\nYou will lose any changes you have made to this schedule.",
														"Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
				return;
			var firstDate = calendar.MonthStart.Last(DayOfWeek.Sunday);
			var lastDate = firstDate + 42;
			for (HebrewDate date = firstDate; date < lastDate; date++) {
				if (!calendar.IsVisible(date)) continue;
				context.GetCell(date).Recalculate();
			}
			calendar.Invalidate();
		}
		private void calendar_SelectionChanged(object sender, EventArgs e) { UpdateCellPanel(); }
		void UpdateCellPanel() {
			if (enableAutoSave.Checked)
				SaveDB();
			if (calendar.SelectedDate.HasValue) {
				loader.LoadCell(calendar.SelectedDate.Value, cell => {
					valueGridPanel.Text = calendar.SelectedDate.Value.EnglishDate.ToLongDateString();
					cellEditor.Cell = cell;
				});
			} else {
				valueGridPanel.Text = "No date selected";
				cellEditor.Cell = null;
			}
			SetUpdateCellEnabled();
		}
		#region Word
		static Word.Application WordApp { get { return Office<Word.ApplicationClass>.App; } }
		static bool IsWordRunning { get { return Office<Word.ApplicationClass>.IsRunning; } }

		BarItem GetItem(WordBinder binder) {
			return wordBinderMenu.ItemLinks.Cast<BarItemLink>().First(link => link.Item.Tag == binder).Item;
		}
		Dictionary<string, WordBinder> openDocuments = new Dictionary<string, WordBinder>();


		void wordBinderMenu_BeforePopup(object sender, CancelEventArgs e) { HandleWord(); }

		bool handledWord;
		void HandleWord() { HandleWord(false); }
		void HandleWord(bool forceStart) {
			if (handledWord) return;
			if (!forceStart && !IsWordRunning)
				return;
			else {
				wordBinderMenu.BeforePopup -= wordBinderMenu_BeforePopup;
			}
			WordApp.DocumentOpen += AddWordDocument;
			WordApp.DocumentBeforeClose += WordApp_DocumentBeforeClose;
			WordApp.DocumentBeforeSave += WordApp_DocumentBeforeSave;

			foreach (Word.Document openDocument in WordApp.Documents) {
				AddWordDocument(openDocument);
			}
			handledWord = true;
		}

		void WordApp_DocumentBeforeSave(Word.Document document, ref bool SaveAsUI, ref bool Cancel) {
			WordBinder binder;
			if (!openDocuments.TryGetValue(document.DocID() ?? "", out binder)) return;
			GetItem(binder).Caption = document.Name;
			if (RibbonBinder == binder)
				wordBinderGroup.Text = document.Name;
		}

		void WordApp_DocumentBeforeClose(Word.Document document, ref bool Cancel) {
			WordBinder binder;
			if (!openDocuments.TryGetValue(document.DocID() ?? "", out binder)) return;

			if (RibbonBinder == binder) RibbonBinder = null;

			ribbon.Items.Remove(GetItem(binder));
			openDocuments.Remove(document.DocID());
			binder.Dispose();
		}

		void AddWordDocument(Word.Document document) {
			if (document == null || !WordBinder.IsSchedule(document)) return;

			AddWordBinder(WordBinder.BindDocument(context, document, this));
		}
		void AddWordBinder(WordBinder binder) {
			object docId = binder.Document.DocID();
			if (docId == null) {
				docId = Guid.NewGuid().ToString();
				binder.Document.Variables.Add("ID", ref docId);
			}
			if (openDocuments.ContainsKey(docId.ToString())) return;

			openDocuments.Add(docId.ToString(), binder);

			var item = new BarCheckItem {
				Caption = binder.Document.Name,
				Glyph = Resources.WordDocument16,
				Tag = binder
			};
			item.ItemClick += item_ItemClick;
			binder.IsDirtyChanged += binder_IsDirtyChanged;
			ribbon.Items.Add(item);
			wordBinderMenu.AddItem(item);
		}

		void binder_IsDirtyChanged(object sender, EventArgs e) {
			var binder = (WordBinder)sender;
			if (binder == RibbonBinder)
				wbUpdateDirty.Enabled = binder.IsDirty;
			else if (binder.IsDirty
				  && RibbonBinder == null || !RibbonBinder.IsDirty)
				RibbonBinder = binder;

			var item = (BarCheckItem)wordBinderMenu.ItemLinks.OfType<BarCheckItemLink>().First(bcil => bcil.Item.Tag == binder).Item;
			item.Appearance.ForeColor = binder.IsDirty ? Color.Red : Color.Empty;
		}

		void item_ItemClick(object sender, ItemClickEventArgs e) {
			RibbonBinder = e.Item.Tag as WordBinder;
		}
		static string GetDefaultWordPath(DateTime date) {
			return Path.Combine(Config.ReadAttribute("Schedules", "Path"),
					String.Format(CultureInfo.InvariantCulture, @"{0:yyyy}\Shomrei Torah Schedule {0:yyyy-MM}", date));
		}

		private void exportWord_ItemClick(object sender, ItemClickEventArgs e) {
			HandleWord(true);   //If I don't call this now, I'll end up with two binders for the new document.
			var binder = WordBinder.CreateDocument(context, calendar.MonthStart, 5, this);  //TODO: Error handling

			AddWordBinder(binder);
			RibbonBinder = binder;

			//Ugly hack to execute this after ExecAsync finishes
			//creating the document from the WeekCount property.
			Waiter.ExecAsync(delegate {
				BeginInvoke(new Action(delegate {

					var defaultPath = GetDefaultWordPath(calendar.MonthStart);

					if (DialogResult.Yes == XtraMessageBox.Show("Would you like to save this document?\r\n\r\nPath:  " + defaultPath,
																"Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
						Directory.CreateDirectory(Path.GetDirectoryName(defaultPath));

						if (ConfirmSave(defaultPath + ".docx"))
							binder.Document.SaveAs(defaultPath + ".docx", Word.WdSaveFormat.wdFormatXMLDocument);

						if (ConfirmSave(defaultPath + ".pdf"))
							binder.Document.SaveAs(defaultPath + ".pdf", Word.WdSaveFormat.wdFormatPDF);
					}
				}));
			}, "Busy");

		}

		private void findWord_ItemClick(object sender, ItemClickEventArgs e) {
			var openDoc = openDocuments.Values.FirstOrDefault(b => calendar.IsSameMonth(b.StartDate + (7 * b.WeekCount) / 2));
			if (openDoc != null) {
				RibbonBinder = openDoc;

				RibbonBinder.Document.Activate();
				WordApp.Activate();

				return;
			}
			var path = GetDefaultWordPath(calendar.MonthStart) + ".docx";
			if (File.Exists(path))
				OpenDocument(path);
			else
				exportWord.PerformClick();
		}

		///<summary>Confirms that a file should be saved.</summary>
		///<returns>True if the file should be saved; false if the user clicked no in the overwrite prompt.</returns>
		static bool ConfirmSave(string path) {
			if (!File.Exists(path))
				return true;

			if (DialogResult.Yes == XtraMessageBox.Show(path + " already exists.\r\nDo you want to overwrite it?",
														"Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
				File.Delete(path);
				return true;
			} else
				return false;
		}
		private void openWord_ItemClick(object sender, ItemClickEventArgs e) {
			string fileName;
			using (var openDialog = new OpenFileDialog {
				Filter = "Word Documents|*.docx;*.doc|All Files|*.*",
				InitialDirectory = Config.ReadAttribute("Schedules", "Path"),
				Title = "Open Schedule"
			}) {
				if (openDialog.ShowDialog() == DialogResult.Cancel) return;
				fileName = openDialog.FileName;
			}
			OpenDocument(fileName);
		}
		void OpenDocument(string fileName) {
			HandleWord(true);

			Word.Document document;
			try {
				document = WordApp.Documents.Open(fileName);
			} catch (COMException ex) {
				XtraMessageBox.Show("Couldn't open " + Path.GetFileName(fileName) + ".\r\n" + ex.Message,
									"Shomrei Torah Schedulizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			//The Open event handler should have created a binder
			WordBinder binder;
			if (!openDocuments.TryGetValue(document.DocID() ?? "", out binder)) {
				if (DialogResult.Yes == XtraMessageBox.Show("This document is not a schedule.  Do you want to close it?",
															"Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					DocumentExtensions.Close(document);
				return;
			}
			RibbonBinder = binder;

			document.Activate();
			WordApp.Activate();
		}

		#region Binder Group
		void SetUpdateCellEnabled() {
			wbUpdateDirty.Enabled = ribbonFlashTimer.Enabled = RibbonBinder != null && RibbonBinder.IsDirty;
			wbUpdateCell.Enabled = RibbonBinder != null
								&& calendar.SelectedDate.HasValue
								&& calendar.SelectedDate.Value >= RibbonBinder.StartDate
								&& calendar.SelectedDate.Value < RibbonBinder.StartDate + 7 * RibbonBinder.WeekCount;
		}

		readonly BarItemLink flashingButton;
		private void ribbonFlashTimer_Tick(object sender, EventArgs e) {
			ribbon.HighlightedItem = flashingButton.Enabled && ribbon.HighlightedItem == null ? flashingButton : null;
		}

		WordBinder ribbonBinder;
		WordBinder RibbonBinder {
			get { return ribbonBinder; }
			set {
				foreach (BarCheckItemLink menuItem in wordBinderMenu.ItemLinks) {
					((BarCheckItem)menuItem.Item).Checked = menuItem.Item.Tag == value;
				}

				ribbonBinder = value;
				wordBinderGroup.Visible = dividerGroup.Visible = (value != null);
				if (value != null) {
					wordBinderGroup.Text = value.Document.Name;
					wbWeekCountItem.EditValue = value.WeekCount;
				}

				SetUpdateCellEnabled();
			}
		}
		private void wbSavePdf_ItemClick(object sender, ItemClickEventArgs e) {
			if (String.IsNullOrEmpty(RibbonBinder.Document.Path)) {
				RibbonBinder.Document.Activate();
				WordApp.CommandBars.ExecuteMso("FileSaveAsPdfOrXps");
			} else {
				RibbonBinder.Document.SaveAs(Path.ChangeExtension(RibbonBinder.Document.FullName, ".pdf"), Word.WdSaveFormat.wdFormatPDF);
			}
		}
		private void wbUpdateCell_ItemClick(object sender, ItemClickEventArgs e) {
			loader.LoadCell(calendar.SelectedDate.Value, cell => {
				RibbonBinder.UpdateTitle(cell);
				RibbonBinder.UpdateTimes(cell);
			});
		}
		private void wbUpdate_ItemClick(object sender, ItemClickEventArgs e) {
			RibbonBinder.UpdateDocument();
		}

		private void wbUpdateDirty_ItemClick(object sender, ItemClickEventArgs e) {
			RibbonBinder.UpdateDirtyCells();
		}

		private void wbWeekCountItem_EditValueChanged(object sender, EventArgs e) {
			RibbonBinder.WeekCount = Convert.ToInt32(wbWeekCountItem.EditValue, CultureInfo.CurrentCulture);
		}
		private void wbActivate_ItemClick(object sender, ItemClickEventArgs e) {
			RibbonBinder.Document.Activate();
			WordApp.Activate();
		}
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error handling")]
		private void wbUploadPdf_ItemClick(object sender, ItemClickEventArgs e) {
			if (DateTime.Now < RibbonBinder.StartDate
			 && DateTime.Now > RibbonBinder.StartDate + RibbonBinder.WeekCount * 7
			 && DialogResult.No == XtraMessageBox.Show("Are you sure you want to upload a non-current schedule?",
													   "Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
				return;

			Waiter.ExecAsync(ui => {
				try {
					ui.Caption = "Saving PDF...";
					var pdfPath = String.IsNullOrEmpty(RibbonBinder.Document.Path) ? Path.GetTempFileName() : Path.ChangeExtension(RibbonBinder.Document.FullName, ".pdf");

					File.Delete(pdfPath);
					RibbonBinder.Document.SaveAs(pdfPath, Word.WdSaveFormat.wdFormatPDF);

					ui.Caption = "Uploading PDF...";

					var url = String.Format(
						CultureInfo.InvariantCulture,
						Config.ReadAttribute("Schedules", "PdfUri"),
						RibbonBinder.StartDate.EnglishDate.AddDays(14)
					);
					//Won't throw if the directory already exists
					FtpClient.Default.CreateDirectory(new Uri(Path.GetDirectoryName(url), UriKind.Relative));
					FtpClient.Default.UploadFile(new Uri(url, UriKind.Relative), pdfPath, ui);
				} catch (Exception ex) {
					BeginInvoke(new Action(delegate {
						XtraMessageBox.Show(ex.ToString(), "Shomrei Torah Schedulizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}));
				}
			}, "Uploading PDF", false);
		}
		#endregion
		#endregion

		#region PowerPoint

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error Handling")]
		private void exportPowerpoint_ItemClick(object sender, ItemClickEventArgs e) {
			Waiter.ExecAsync(ui => {
				ui.Caption = "Opening Announcements...";
				PowerPoint.Presentation announcements;
				try {
					var powerpoint = new PowerPoint.Application();
					powerpoint.Visible = MsoBool.msoTrue;
					var announcementsPath = String.Format(CultureInfo.InvariantCulture, Config.ReadAttribute("Schedules", "Announcements"), calendar.MonthStart.EnglishDate);

					if (!File.Exists(announcementsPath)) {
						XtraMessageBox.Show("Please create " + announcementsPath + " by copying from last year",
											"Shomrei Torah Schedulizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					announcements = powerpoint.Presentations.Items().FirstOrDefault(pres => pres.FullName == announcementsPath)
								 ?? powerpoint.Presentations.Open(announcementsPath, MsoBool.msoFalse, MsoBool.msoFalse, MsoBool.msoTrue);

				} catch (Exception ex) {
					BeginInvoke(new Action(delegate {
						XtraMessageBox.Show("Couldn't open the announcements:\r\n\r\n" + ex,
											"Shomrei Torah Schedulizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}));
					return;
				}
				if (ui.WasCanceled) return;

				var exporter = new PowerPointExporter(context, announcements);
				ui.Maximum = 5;
				var date = calendar.MonthStart.Next(DayOfWeek.Saturday);
				for (int week = 0; week < 6; week++) {
					if (ui.WasCanceled) return;

					if (date.Parsha != null) {
						ui.Caption = "Updating announcements for " + date.EnglishDate.ToString("MMMM d, yyyy", CultureInfo.CurrentCulture);
						ui.Progress = week;

						exporter.UpdateSlide(date);
					}
					date += 7;
				}
				announcements.Windows[1].Activate();
				announcements.Application.Activate();
			}, "Creating Announcements", true);
		}
		#endregion

		private void calendar_DateToolTip(object sender, CalendarToolTipEventArgs e) {
			if (loader.GetState(e.Date) != DataState.Ready)
				return;
			var warning = context.GetCell(e.Date).GetWarning();
			if (!String.IsNullOrEmpty(warning))
				e.ToolTipText.AppendLine().AppendLine("WARNING").AppendLine().AppendLine(warning);
		}
		protected override void OnClosing(CancelEventArgs e) {
			var dirtyBinder = openDocuments.Values.FirstOrDefault(wb => wb.IsDirty);
			if (dirtyBinder != null
			 && DialogResult.No == XtraMessageBox.Show(dirtyBinder.Document.Name + " has not been updated.\r\nAre you sure you want to exit?",
													   "Shomrei Torah Schedulizer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)) {
				e.Cancel = true;
				RibbonBinder = dirtyBinder;
			}

			base.OnClosing(e);
		}

		void IExportUIProvider.PerformOperation(Action<IProgressReporter> method, bool cancellable) {
			Waiter.ExecAsync(method, "Exporting", cancellable);
		}

		private void exportShulCloud_ItemClick(object sender, ItemClickEventArgs e) {
			ProgressWorker.Execute((progress) =>
				new ShulCloudExporter().ExportRange(progress, context, calendar.MonthStart.Last(DayOfWeek.Sunday), 6).GetAwaiter().GetResult()
			, true);
		}
	}
	static class Extensions {
		public static string DocID(this Word.Document document) {
			var variable = document.Variables.OfType<Word.Variable>().FirstOrDefault(v => v.Name == "ID");
			return variable == null ? null : variable.Value;
		}
	}
}