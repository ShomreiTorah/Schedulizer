using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Schedules.WinClient {
	static class ScheduleVerifier {
		public static string GetWarning(this ScheduleCell cell) {
			if (cell.HolidayCategory == HolidayCategory.תענית)
				return "Please check all fast days carefully";

			if ((cell.Date + 1).Info.Is(Holiday.פסח.Days.First()))
				return "Please add חמץ times";

			if (cell.Holiday.Is(Holiday.סוכות.Days[2]))
				return "Please ask the Rav when the שמחת בית השואבה is.  (It's usually 9:00)";

			var duplicateTimes = cell.Times.GroupBy(st => st.Time).Where(g => g.Has(2));
			if (duplicateTimes.Any())
				return "This date has multiple entries for the same time:\r\n  • "
					+ duplicateTimes
						.SelectMany(g => g)
						.OrderBy(st => st.Time)
						.Join("\r\n  • ", st => st.ToString());

			return null;
		}
	}
}