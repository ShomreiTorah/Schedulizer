using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Word.Extensions;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Schedules.Export {
	///<summary>Binds a portion of the schedule to a Word document.</summary>
	public class WordBinder : IDisposable {

		static CultureInfo Culture { get { return CultureInfo.CurrentCulture; } }
		static Application Word { get { return Office<ApplicationClass>.App; } }

		readonly IBindingList cellsBindingList, timesBindingList;
		private WordBinder(IExportUIProvider ui, ScheduleContext context, Document document, CustomXMLPart scheduleXml) {
			dirtyCells = new DirtyCellsCollection(this);
			uiProvider = ui;
			Context = context;
			Document = document;
			StartDate = new HebrewDate(DateTime.ParseExact(scheduleXml.SelectSingleNode("/Schedulizer/@StartDate").Text, "d", CultureInfo.InvariantCulture));
			שבתStyle = (ScheduleTable.get_Style() as Style).Table.Condition(WdConditionCode.wdLastColumn);

			cellsBindingList = ((IListSource)Context.CellSet).GetList() as IBindingList;
			timesBindingList = ((IListSource)Context.TimeSet).GetList() as IBindingList;
			cellsBindingList.ListChanged += Cells_ListChanged;
			timesBindingList.ListChanged += Times_ListChanged;
		}
		class EmptyProgressReporter : IProgressReporter {
			public string Caption { get; set; }
			public int Progress { get; set; }
			public int Maximum { get; set; }
			public bool WasCanceled { get { return false; } }
			public bool CanCancel { get; set; }
		}

		void PerformOperation(Action<IProgressReporter> operation, bool cancellable) {
			if (uiProvider == null)
				operation(new EmptyProgressReporter());
			else
				uiProvider.PerformOperation(operation, cancellable);
		}
		readonly IExportUIProvider uiProvider;

		///<summary>Releases all resources used by the WordBinder.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the WordBinder and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				cellsBindingList.ListChanged -= Cells_ListChanged;
				timesBindingList.ListChanged -= Times_ListChanged;
			}
		}

		readonly DirtyCellsCollection dirtyCells;
		class DirtyCellsCollection : Collection<ScheduleCell> {
			readonly WordBinder parent;
			public DirtyCellsCollection(WordBinder parent) { this.parent = parent; }

			protected override void ClearItems() {
				if (Count == 0) return;
				base.ClearItems();
				parent.OnIsDirtyChanged();
			}
			protected override void RemoveItem(int index) {
				if (Count == 0) return;
				base.RemoveItem(index);
				if (Count == 0)
					parent.OnIsDirtyChanged();
			}
			protected override void InsertItem(int index, ScheduleCell item) {
				if (Contains(item)) return;
				base.InsertItem(index, item);
				if (Count == 1) parent.OnIsDirtyChanged();
			}
		}
		///<summary>Indicates whether the any cells in the schedule have changed since the Word document was last updated.</summary>
		public bool IsDirty { get { return dirtyCells.Any(); } }
		///<summary>Occurs when the value of the IsDirty property is changed.</summary>
		public event EventHandler IsDirtyChanged;
		///<summary>Raises the IsDirtyChanged event.</summary>
		internal protected virtual void OnIsDirtyChanged() { OnIsDirtyChanged(EventArgs.Empty); }
		///<summary>Raises the IsDirtyChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnIsDirtyChanged(EventArgs e) {
			if (IsDirtyChanged != null)
				IsDirtyChanged(this, e);
		}
		///<summary>Gets or sets whether this instance will automatically update the Word document when the schedule changes.</summary>
		///<remarks>If this proeprty is false, the Word document should be updated manually using the UpdateDirtyCells method.</remarks>
		public bool AutoUpdate { get; set; }
		public void UpdateDirtyCells() {
			var dirtyCopy = dirtyCells.ToArray();	//Make a copy to enumerate over

			PerformOperation(ui => {
				ui.Maximum = dirtyCopy.Length;

				foreach (var cell in dirtyCopy) {
					ui.Caption = "Updating " + cell.EnglishDate.ToLongDateString();

					UpdateTitle(cell);
					UpdateTimes(cell);	//This removes it from dirtyCells
					ui.Progress++;

					if (ui.WasCanceled)
						break;
				}
			}, true);
		}

		void Cells_ListChanged(object sender, ListChangedEventArgs e) {
			if (e.ListChangedType == ListChangedType.ItemChanged) {
				var cell = cellsBindingList[e.NewIndex] as ScheduleCell;

				if (Document.SelectContentControlsByTag(cell.TitleTag()).Count == 0) return;

				dirtyCells.Add(cell);
				if (AutoUpdate)
					UpdateTitle(cell);
			}
		}

		void Times_ListChanged(object sender, ListChangedEventArgs e) {
			if (e.ListChangedType == ListChangedType.ItemDeleted) return;   //I can't get the deleted item, so there's nothing I can do here.
			var time = timesBindingList[e.NewIndex] as ScheduleTime;

			if (!Document.Bookmarks.Exists(time.Cell.ValuesTag()))
				return;

			dirtyCells.Add(time.Cell);
			if (AutoUpdate)
				UpdateTimes(time.Cell, Document.Bookmarks.Item(time.Cell.ValuesTag()).Range);
		}

		public void UpdateDocument() {
			var endDate = StartDate.EnglishDate.AddDays(7 * weekCount);

			PerformOperation(ui => {
				try {
					ui.Maximum = weekCount * 7;
					Word.ScreenUpdating = false;

					for (HebrewDate date = StartDate; date <= endDate; date++) {
						ui.Progress = (date - StartDate).Days;
						ui.Caption = String.Format(CultureInfo.CurrentUICulture, "Updating {0:D}", date.EnglishDate);
						var cell = Context.GetCell(date);
						UpdateTitle(cell);
						UpdateTimes(cell);
						if (ui.WasCanceled) break;
					}
				} finally { Word.ScreenUpdating = true; }
			}, true);
		}
		static string FixTitle(string title) {
			if (string.IsNullOrEmpty(title))
				return " ";	//Suppress the content control's default value prompt.
			return title.Replace("\r", "").Replace('\n', '\v');	//Replace newlines with soft newlines that can go in a content control.  (Avoids document corruption)
		}
		public void UpdateTitle(ScheduleCell cell) {
			if (!Contains(cell.EnglishDate)) return;

			var controls = Document.SelectContentControlsByTag(cell.TitleTag());
			if (controls.Count == 1)
				controls.Item(1).Range.Text = FixTitle(cell.Title);
		}
		public void UpdateTimes(ScheduleCell cell) {
			if (!Contains(cell.EnglishDate)) return;

			if (Document.Bookmarks.Exists(cell.ValuesTag())) {
				try {
					Word.ScreenUpdating = false;
					UpdateTimes(cell, Document.Bookmarks.Item(cell.ValuesTag()).Range);
				} finally { Word.ScreenUpdating = true; }
			} else
				dirtyCells.Remove(cell);	//If the cell had a bookmark, this was done by UpdateTimes
		}

		public static WordBinder CreateDocument(DateTime startDate, int weeks, IExportUIProvider ui) { return CreateDocument(ScheduleContext.Default, startDate, weeks, ui); }
		public static WordBinder CreateDocument(ScheduleContext context, DateTime startDate, int weeks, IExportUIProvider ui) {
			if (context == null) throw new ArgumentNullException("context");
			if (weeks <= 0) throw new ArgumentOutOfRangeException("weeks");
			var document = Word.Documents.Add(Config.ReadAttribute("Schedules", "TemplatePath"));

			startDate = startDate.Last(DayOfWeek.Sunday);

			var xml = document.CustomXMLParts.Add(new XElement("Schedulizer", new XAttribute("StartDate", startDate.ToString("d", CultureInfo.InvariantCulture))).ToString(), Type.Missing);
			var retVal = new WordBinder(ui, context, document, xml);

			var title = retVal.Document.SelectContentControlsByTag("Month");
			if (title.Count == 1)
				title.Item(1).Range.Text = startDate.AddDays((7 * weeks) / 2).ToString(@"MMMM \'yy", Culture);
			retVal.WeekCount = weeks;

			return retVal;
		}
		public static WordBinder BindDocument(ScheduleContext context, Document document, IExportUIProvider ui) {
			if (context == null) throw new ArgumentNullException("context");
			if (document == null) throw new ArgumentNullException("document");

			var xml = document.CustomXMLParts.Cast<CustomXMLPart>().FirstOrDefault(p => p.DocumentElement.BaseName == "Schedulizer");
			if (xml == null) throw new ArgumentException(document.Name + " is not a schedule", "document");

			var table = document.Tables[1];
			if (table.Columns.Count != 7) throw new ArgumentException(document.Name + " is not a schedule", "document");
			if (table.Rows.Count < 2) throw new ArgumentException(document.Name + " is not a schedule", "document");

			var retVal = new WordBinder(ui, context, document, xml) { weekCount = table.Rows.Count - 1 };

			return retVal;
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "COM Interface")]
		public static bool IsSchedule(Document document) {
			if (document == null) throw new ArgumentNullException("document");

			if (!document.CustomXMLParts.Cast<CustomXMLPart>().Any(p => p.DocumentElement.BaseName == "Schedulizer"))
				return false;

			var table = document.Tables[1];
			return table.Columns.Count == 7 && table.Rows.Count >= 2;
		}

		public bool Contains(DateTime date) { return date >= StartDate && date <= StartDate.EnglishDate.AddDays(WeekCount * 7); }

		int weekCount;
		public Document Document { get; private set; }

		Table ScheduleTable { get { return Document.Tables[1]; } }
		ConditionalStyle שבתStyle { get; set; }

		public ScheduleContext Context { get; private set; }
		public HebrewDate StartDate { get; private set; }
		public int WeekCount {
			get { return weekCount; }
			set {
				if (WeekCount == value) return;
				if (value <= 0) throw new ArgumentOutOfRangeException("value");

				var old = WeekCount;
				PerformOperation(ui => {
					try {
						Word.ScreenUpdating = false;
						if (value < WeekCount) {
							for (int w = weekCount; w > value; w--) {
								for (int c = 0; c < 7; c++)
									dirtyCells.Remove(Context.GetCell(StartDate + 7 * w + c));

								ScheduleTable.Rows[w + 1].Delete();	//Indexer is one-based.
								weekCount = w - 1;
								if (ui.WasCanceled) break;
							}
						} else {
							ui.Maximum = (value - WeekCount) * 7;
							try {
								for (int w = WeekCount; w < value; w++) {
									var row = ScheduleTable.Rows.Add();

									for (int c = 0; c < 7; c++) {
										var date = StartDate.AddDays(w * 7 + c);
										ui.Progress++;
										ui.Caption = String.Format(CultureInfo.CurrentUICulture, "Creating {0:D}", date.EnglishDate);

										ResetCell(row.Cells[c + 1], date);	 //Indexer is one-based.
										//Don't stop in this loop as it would
										//leave us in an  inconsistent state.
									}
									weekCount = w + 1;
									if (ui.WasCanceled) break;
								}
							} finally {
								ui.CanCancel = false;
								//I only do this after creating the table to prevent
								//Word from copying the formatting down to new rows.
								//If the user canceled, I colorize the weeks we did
								//before he canceled.
								if (שבתStyle != null) {
									//http://stackoverflow.com/questions/1230107/bug-in-word-2007-conditionalstyle
									//I worked around this bug by moving the selection out of the table.
									Document.Range().Offset(-1).Select();
									for (int w = old; w < WeekCount; w++) {
										var row = ScheduleTable.Rows[w + 2];
										for (int c = 0; c < 6; c++) {	//Skip שבת
											var date = StartDate + (7 * w + c);
											if (!date.Info.Is(HolidayCategory.דאריתא)) continue;

											var cell = row.Cells[c + 1];

											cell.Shading.Texture = שבתStyle.Shading.Texture;
											cell.Shading.BackgroundPatternColor = שבתStyle.Shading.BackgroundPatternColor;
											cell.Shading.ForegroundPatternColor = שבתStyle.Shading.ForegroundPatternColor;
										}
									}
								}
							}
						}
					} finally { Word.ScreenUpdating = true; }
				}, true);
			}
		}
		void ResetCell(Cell cell, HebrewDate date) {
			var scheduleCell = Context.GetCell(date);

			cell.Range.Delete();

			cell.Range.InsertAfter(date.EnglishDate.ToString("MMM d", Culture));
			cell.Range.InsertAfter(date.EnglishDate.Day.GetSuffix());

			Document.Range(cell.Range.Start, cell.Range.End - 1).ApplyStyle("English Date");
			Document.Range(cell.Range.End - 3, cell.Range.End - 1).Font.Superscript = 1;

			cell.Range.Offset(-1).InsertAlignmentTab(WdAlignmentTabAlignment.wdRight);

			cell.Range.Offset(-1).AppendText(date.ToString("M"), "Hebrew Date");

			cell.Range.InsertParagraphAfter();
			var titleControl = cell.Range.ContentControls.Add(WdContentControlType.wdContentControlText, cell.Range.Offset(-1));
			titleControl.MultiLine = true;
			titleControl.Tag = scheduleCell.TitleTag();
			titleControl.Range.Text = FixTitle(scheduleCell.Title);
			cell.Range.InsertParagraphAfter();

			titleControl.Range.ApplyStyle("Cell Title");

			var valuesRange = cell.Range.Offset(-1);

			UpdateTimes(scheduleCell, valuesRange);
		}
		void UpdateTimes(ScheduleCell scheduleCell, Range range) {
			var printedPairs = from t in scheduleCell.Times
							   group t by t.Name into g
							   orderby g.First().Time
							   select new {
								   Name = g.Key,
								   Value = g.OrderByDescending(t => t.Time).Select(t => t.TimeString).Join(", "),
								   IsBold = g.Any(t => t.IsBold),
							   };

			range.Text = "";
			bool first = true;
			foreach (var time in printedPairs) {
				if (!first) {
					range.Offset(0).InsertBreak(WdBreakType.wdLineBreak);
					range.End++;
				}
				first = false;

				range.AppendText(time.Value, time.IsBold ? "Bold Value Time" : "Value Time");

				range.Offset(0).InsertAlignmentTab(WdAlignmentTabAlignment.wdRight);
				range.End++;

				range.AppendText(time.Name, "Value Name");
			}
			//If I don't do this here, the style is lost when the cell is reset.
			range.ApplyStyle("Schedule Values");
			//Where it'll fit, I put in דף יומי
			if (printedPairs.Count() < 6) {		//I could optimize and not call Count
				range.Offset(0).InsertParagraph();
				range.End++;
				range.AppendText(scheduleCell.Date.Info.DafYomiString, "Daf");
			}

			range.Bookmarks.Add(scheduleCell.ValuesTag());
			dirtyCells.Remove(scheduleCell);
		}
	}
	static class WordExtensions {
		public static void AppendText(this Range range, string text, string styleName) {
			range.InsertAfter(text);
			range.Document.Range(range.End - text.Length, range.End).ApplyStyle(styleName);
		}
		public static Range Offset(this Range range, int offset) {
			if (offset < 0)
				offset = range.End + offset;
			else if (offset > 0)
				offset = range.Start + offset;
			else
				offset = range.End;
			return range.Document.Range(offset, offset);
		}

		public static void InsertAlignmentTab(this Range range, WdAlignmentTabAlignment alignment) {
			range.InsertAlignmentTab((int)alignment, (int)WdAlignmentTabRelative.wdMargin);
		}

		///<summary>Applies a style to a range if the style exists.</summary>
		public static void ApplyStyle(this Range range, string styleName) {
			try {
				range.Style(styleName);
			} catch (COMException ex) {
				if (Debugger.IsAttached)
					Debug.Assert(false, "Undefined style: " + styleName + "\r\n\r\n" + ex);
			}
		}

		public static string CCTag(this ScheduleCell cell) { return "Schedule Cell " + cell.EnglishDate.ToString("d", CultureInfo.InvariantCulture); }
		public static string TitleTag(this ScheduleCell cell) { return cell.CCTag() + " Title"; }
		public static string ValuesTag(this ScheduleCell cell) { return String.Format(CultureInfo.InvariantCulture, "ScheduleCell{0:yyyyMMdd}Values", cell.EnglishDate); }
	}
}
