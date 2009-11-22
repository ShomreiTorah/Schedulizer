using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Interop.PowerPoint.Extensions;
using System.Globalization;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common;
using MsoBool = Microsoft.Office.Core.MsoTriState;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Schedules.Export {
	public class PowerPointExporter {
		public PowerPointExporter(ScheduleContext context, Presentation presentation) {
			if (context == null) throw new ArgumentNullException("context");
			if (presentation == null) throw new ArgumentNullException("presentation");

			Context = context;
			Presentation = presentation;
			defaultLayout = GetLayout("Announcements");
		}

		readonly CustomLayout defaultLayout;
		public Presentation Presentation { get; private set; }
		public ScheduleContext Context { get; private set; }

		///<summary>Gets the slide for the given date, creating it if necessary.</summary>
		public Slide GetSlide(DateTime date) {
			bool created;
			return GetSlide(date, out created);
		}
		Slide GetSlide(DateTime date, out bool created) {
			var slideName = date.ToString("D", CultureInfo.InvariantCulture);

			var retVal = Presentation.Slides.Items().FirstOrDefault(s => s.Name == slideName);

			created = retVal == null;
			if (retVal == null) {
				int index;

				for (index = Presentation.Slides.Count; index > 0; index--) {
					DateTime otherDate;
					if (DateTime.TryParseExact(Presentation.Slides[index].Name, "D", CultureInfo.InvariantCulture, DateTimeStyles.None, out otherDate)
					 && otherDate < date)
						break;
				}
				retVal = Presentation.Slides.AddSlide(index + 1, defaultLayout);
				retVal.Name = slideName;
				UpdateSlide(retVal);
			}
			return retVal;
		}
		public void UpdateSlide(DateTime date) {
			bool created;
			var slide = GetSlide(date, out created);
			if (!created)
				UpdateSlide(slide);
		}
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "COM Interface")]
		public void UpdateSlide(Slide slide) {
			if (slide == null) throw new ArgumentNullException("slide");

			DateTime englishDate;
			if (!DateTime.TryParseExact(slide.Name, "D", CultureInfo.InvariantCulture, DateTimeStyles.None, out englishDate))
				throw new ArgumentException("Bad slide name: " + slide.Name, "slide");
			var date = new HebrewDate(englishDate);
			if (!date.Info.Isשבת || date.Parsha == null)
				throw new InvalidOperationException(englishDate.ToLongDateString() + " shouldn't have announcements");

			var cell = Context.GetCell(date);

			slide.Shapes.Title.TextFrame.TextRange.Text = "פרשת " + cell.Date.Parsha;

			var dateRange = slide.Shapes[2].TextFrame.TextRange;

			var englishDateText = englishDate.ToString("MMM d", CultureInfo.CurrentCulture);
			dateRange.Text = englishDateText + englishDate.Day.GetSuffix() + "\t" + date.ToString("M");
			dateRange.Characters(englishDateText.Length + 1, 2).Font.Superscript = MsoBool.msoTrue;

			var printedPairs = from t in cell.Times
							   group t by t.Name into g
							   orderby g.First().Time
							   select new {
								   Name = g.Key,
								   Value = g.Select(t => t.TimeString).Join(" / "),
								   IsBold = g.Any(t => t.IsBold),
							   };

			var timesRange = slide.Shapes[3].TextFrame.TextRange;
			timesRange.Text = "";
			var lineNumber = 0;
			foreach (var pair in printedPairs) {
				timesRange.InsertAfter(pair.Name + "\t");
				var valueStart = timesRange.Length;

				timesRange.InsertAfter(pair.Value + "\n");

				if (pair.IsBold) {
					var boldRange = timesRange.Characters(valueStart + 1, pair.Value.Length);
					boldRange.Font.Bold = MsoBool.msoTrue;
					boldRange.Font.Color.RGB = 255;
				}

				lineNumber++;
				if (lineNumber == 3)
					timesRange.InsertAfter("\n");
			}
		}

		///<summary>Gets the CustomLayout with the specified name.</summary>
		///<param name="layoutName">The name of the layout to look for.</param>
		///<returns>The PowerPoint.CustomLayout object.</returns>
		///<remarks>The indexer for PowerPoint.CustomLayouts does not accept strings.
		///Therefore, I wrote this function to search it for the given layout.  
		///The enumerator for PowerPoint.CustomLayouts
		///returns an unknown System._ComObject that cannot be
		///casted to PowerPoint.CustomLayouts. (QueryInterface() returns unsupported)</remarks>
		private CustomLayout GetLayout(string layoutName) {
			for (int n = 1; n <= Presentation.SlideMaster.CustomLayouts.Count; n++) {
				if (Presentation.SlideMaster.CustomLayouts[n].Name == layoutName)
					return Presentation.SlideMaster.CustomLayouts[n];
			}
			throw new ArgumentException("Layout " + layoutName + " not found.", "layoutName");
		}
	}
}
