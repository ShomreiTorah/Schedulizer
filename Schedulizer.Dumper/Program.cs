using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShomreiTorah.Common.Calendar;

namespace ShomreiTorah.Schedules.Dumper {
	class Program {
		static void Main(string[] args) {
			for (var date = new HebrewDate(new DateTime(2015, 1, 1)); date.EnglishDate.Year <= 2020; date++) {
				var calc = new ScheduleCalculator(date);
				Console.WriteLine("---- " + date.EnglishDate.ToShortDateString()
								+ " " + date.DayOfWeek
								+ ": " + date.ToString("d") + " " + calc.CalcTitle());
				foreach (var time in calc.CalcTimes()) {
					Console.WriteLine((time.Name + ":").PadRight(10) + time.TimeString + (time.IsBold ? " **" : ""));
				}
				Console.WriteLine();
				Console.WriteLine();
			}
			if (!Console.IsOutputRedirected) {
				Console.ReadKey();
			}
		}
	}
}
