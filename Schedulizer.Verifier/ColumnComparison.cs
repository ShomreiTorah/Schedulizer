using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Schedules.Verifier {
	class ColumnComparison {
		public ColumnComparison(HebrewDate date) {
			Date = date;

			var priorCalc = new OldמנחהCalculator(date - 1);
			PriorCell = new ReadOnlyCollection<ScheduleValue>(priorCalc.CalcTimes().ToArray());
			var calc = new OldמנחהCalculator(date);
			NewTitle = calc.CalcTitle();
			Cell = new ReadOnlyCollection<ScheduleValue>(calc.CalcTimes().ToArray());


			var oldHoliday = TimesCalculator.HolidayDates.FirstOrDefault(h => h.GetEnglishDate(date.HebrewYear) == date);

			OldTimes = TimesCalculator.GetDay(date, oldHoliday);

			OldTitle = OldTimes.Title;

			OldNotes = OldTimes.Notes;

			HasDifferences = HasAnyDifferences(
				ערב_שבת_Candle_Lighting = new ScheduleValueComparison(OldTimes.ערב_שבת_Candle_Lighting, PriorCell.Find("Candle Lighting")),

				ערב_שבת_מנחה = new PriorMinchaComparison(OldTimes.ערב_שבת_מנחה, PriorCell.Find("מנחה")),

				שבת_שחרית = new ScheduleValueComparison(OldTimes.שבת_שחרית, Cell.Find("שחרית")),
				שבת_סוף_זמן_קריאת_שמע = new ScheduleValueComparison(OldTimes.שבת_סוף_זמן_קריאת_שמע, Cell.Find("סזק״ש")),

				שבת_שיעור = new ShiurComparison(OldTimes.שבת_שיעור, Cell),

				שבת_מנחה = new ScheduleValueComparison(OldTimes.שבת_מנחה, Cell.Find("מנחה")),
				שבת_מעריב = new ScheduleValueComparison(OldTimes.שבת_מעריב, Cell.Find("מעריב"))
			);
		}

		public bool HasDifferences { get; private set; }

		public DateTime Date { get; private set; }

		public ReadOnlyCollection<ScheduleValue> PriorCell { get; private set; }
		public ReadOnlyCollection<ScheduleValue> Cell { get; private set; }

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public string NewTitle { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public string OldTitle { get; private set; }

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public string OldNotes { get; private set; }

		public ColumnInfo OldTimes { get; private set; }

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldערב_שבת_Candle_Lighting { get { return ערב_שבת_Candle_Lighting.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newערב_שבת_Candle_Lighting { get { return ערב_שבת_Candle_Lighting.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldערב_שבת_מנחה { get { return ערב_שבת_מנחה.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newערב_שבת_מנחה { get { return ערב_שבת_מנחה.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldשבת_שחרית { get { return שבת_שחרית.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newשבת_שחרית { get { return שבת_שחרית.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldשבת_סוף_זמן_קריאת_שמע { get { return שבת_סוף_זמן_קריאת_שמע.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newשבת_סוף_זמן_קריאת_שמע { get { return שבת_סוף_זמן_קריאת_שמע.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldשבת_שיעור { get { return שבת_שיעור.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newשבת_שיעור { get { return שבת_שיעור.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldשבת_מנחה { get { return שבת_מנחה.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newשבת_מנחה { get { return שבת_מנחה.NewString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Oldשבת_מעריב { get { return שבת_מעריב.OldString; } }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ValueReference Newשבת_מעריב { get { return שבת_מעריב.NewString; } }

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison ערב_שבת_Candle_Lighting { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison ערב_שבת_מנחה { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison שבת_שחרית { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison שבת_סוף_זמן_קריאת_שמע { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison שבת_שיעור { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison שבת_מנחה { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Data binding")]
		public ScheduleValueComparison שבת_מעריב { get; private set; }

		static bool HasAnyDifferences(params ScheduleValueComparison[] values) { return values.Any(svc => !svc.AreSame); }
	}
	class OldמנחהCalculator : ScheduleCalculator {
		public OldמנחהCalculator(HebrewDate date) : base(date) { }

		protected override TimeSpan Getערב٠שבת٠מנחה(TimeSpan defaultמנחה) {
			var candleLighting = Zmanim.Sunset - TimeSpan.FromMinutes(18);

			if (candleLighting < Time(7, 15, PM) || (Date + 1).Info.Is(Holiday.שבועות.Days[0]))
				return defaultמנחה;
			if (candleLighting < Time(7, 35, PM))
				return Time(6, 30, PM);
			else if (candleLighting < Time(7, 55, PM))
				return Time(6, 45, PM);
			else
				return Time(7, 00, PM);
		}
	}
	static class Extensions {
		public static IEnumerable<ScheduleValue> Find(this IEnumerable<ScheduleValue> values, string name) {
			return values.Where(v => v.Name == name);
		}
	}
}
