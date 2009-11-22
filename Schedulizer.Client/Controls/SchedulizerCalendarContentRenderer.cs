using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Controls;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;
using ShomreiTorah.Schedules.WinClient.Properties;

namespace ShomreiTorah.Schedules.WinClient.Controls {
	class SchedulizerCalendarContentRenderer : BaseContentRenderer {
		static CultureInfo Culture { get { return CultureInfo.CurrentUICulture; } }
		public SchedulizerCalendarContentRenderer(ICalendarPainter painter, ScheduleContext context)
			: base(painter) {
			Context = context;
		}

		public ScheduleContext Context { get; private set; }
		protected override void DrawContent() {
			if (ContentBounds.Width < 45) {
				if (Calendar.Mode == CalendarType.English)
					DrawString(Date.EnglishDate.Day.ToString(Culture));
				else
					DrawString(Date.HebrewDay.ToHebrewString(HebrewNumberFormat.LetterQuoted), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
			} else { DrawWithDetails(); }
		}
		void DrawWithDetails() {
			DrawString(Date.ToString(ContentBounds.Width > 90 || (ContentBounds.Width > 60 && Calendar.Mode == CalendarType.English) ? "M" : "%d"), TextFormatFlags.RightToLeft);

			string englishStr = Date.EnglishDate.ToString("%d", Culture);
			if (ContentBounds.Width > 60 && Calendar.Mode == CalendarType.Hebrew)
				englishStr = Date.EnglishDate.ToString("MMM d", Culture);
			if (ContentBounds.Width > 90) {
				englishStr = Date.EnglishDate.ToString("M", Culture);
				if (TextRenderer.MeasureText(englishStr, Font).Width > ContentBounds.Width / 2)
					englishStr = Date.EnglishDate.ToString("MMM d", Culture);
			}

			DrawString(englishStr, TextFormatFlags.Right);
			var dateBottom = ContentBounds.Y + MeasureText(englishStr, false).Height;
			if (ContentBounds.Width > 60 && ContentBounds.Height >= 50) {
				if (!Context.IsLoaded(Date))
					Context.LoadCells(Calendar.MonthStart.Last(DayOfWeek.Sunday), Calendar.MonthStart.Last(DayOfWeek.Sunday) + 7 * 6);

				var cell = Context.GetCell(Date);

				if (!String.IsNullOrEmpty(cell.GetWarning())) {
					Graphics.DrawImage(Resources.Warning16, ContentBounds.Left + 3, dateBottom + 3);
				}

				int y = ContentBounds.Bottom;
				var printedPairs = (from t in cell.Times
									group t by t.Name into g
									orderby g.First().Time descending
									select new {
										Name = g.Key,
										Value = g.Select(t => t.TimeString).Join(", "),
										IsBold = g.Any(t => t.IsBold),
									}).ToArray();

				var lineHeight = MeasureText("abc", true).Height;

				if (y - lineHeight * printedPairs.Length > dateBottom + MeasureText(cell.Title, false).Height) {
					foreach (var line in printedPairs) {
						var height = MeasureText(line.Name, false).Height;
						var timeWidth = MeasureText(line.Value, line.IsBold).Width;

						y -= height;

						var lineBounds = new Rectangle(ContentBounds.X, y, ContentBounds.Width, height);
						var nameBounds = lineBounds;
						nameBounds.Width -= timeWidth;

						DrawString(line.Name, false, nameBounds, TextFormatFlags.EndEllipsis);
						DrawString(line.Value, line.IsBold, lineBounds, TextFormatFlags.Right);
					}
				}

				DrawString(cell.Title, false, new Rectangle(ContentBounds.X, dateBottom, ContentBounds.Width, y - dateBottom),
						   TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
			}
		}

		Size MeasureText(string text, bool bold) {
			return TextRenderer.MeasureText(Graphics, text, bold ? Calendar.TodayFont : Font);
		}
		protected void DrawString(string text, bool bold, Rectangle bounds, TextFormatFlags flags) {
			TextRenderer.DrawText(Graphics, text, bold ? Calendar.TodayFont : Font, bounds, TextColor, flags | TextFormatFlags.PreserveGraphicsTranslateTransform);
		}
	}
}
