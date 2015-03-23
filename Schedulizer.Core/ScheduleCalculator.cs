using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;
using ShomreiTorah.Common.Calendar.Zmanim;

namespace ShomreiTorah.Schedules {
	public class ScheduleCalculator {
		public ScheduleCalculator(HebrewDate date) {
			Date = date;
			Holiday = Date.Info.Holiday;
			Zmanim = Date.GetZmanim();

			בדיקה = Holiday.פסח.Days.First().Date.GetDate(Date.HebrewYear) - 2;
			if (בדיקה.DayOfWeek == DayOfWeek.Friday)
				בדיקה--;
		}

		readonly HebrewDate בדיקה;

		public HebrewDate Date { get; private set; }
		public Holiday Holiday { get; private set; }
		public ZmanimInfo Zmanim { get; private set; }
		public bool Isיוםטוב { get { return HolidayCategory == HolidayCategory.דאריתא; } }
		public bool Isשבת { get { return Date.Info.Isשבת; } }
		public DayOfWeek DayOfWeek { get { return Date.DayOfWeek; } }
		public HolidayCategory HolidayCategory { get { return Holiday == null ? HolidayCategory.None : Holiday.Category; } }
		public string HolidayName { get { return Holiday == null ? null : Holiday.Name; } }

		DateTime SummerStart { get { return new DateTime(Date.EnglishDate.Year, 7, 1).Last(DayOfWeek.Saturday); } }
		DateTime LaborDay { get { return new DateTime(Date.EnglishDate.Year, 9, 1).Next(DayOfWeek.Monday); } }


		public virtual string CalcTitle() {
			var retVal = new StringBuilder();
			if (Isשבת && !String.IsNullOrEmpty(Date.Parsha))
				retVal.Append("פרשת ").AppendLine(Date.Parsha);
			if (Holiday != null)
				retVal.AppendLine(Holiday.Name);
			if (!String.IsNullOrEmpty(Date.Info.ראשחודשCaption))
				retVal.AppendLine(Date.Info.ראשחודשCaption);
			else if (Isשבת) {
				if (Date.HebrewMonth != HebrewMonth.אלול) {
					if (Date.HebrewDay == 29)
						retVal.Append("מחר חודש ").AppendLine((Date + 1).Info.ראשחודשMonth);

					else if (Date.HebrewDay >= 23)
						retVal.Append("מברכים ").AppendLine((Date + 15).HebrewMonthName);
				}
			}

			if ((DayOfWeek == DayOfWeek.Wednesday && (Date + 1).Info.Is(HolidayCategory.דאריתא) && (Date + 2).Info.Is(HolidayCategory.דאריתא))
			 || (DayOfWeek == DayOfWeek.Thursday && !(Date).Info.Is(HolidayCategory.דאריתא) && (Date + 1).Info.Is(HolidayCategory.דאריתא)))
				retVal.AppendLine("ערוב תבשילין");

			if ((Date + 2).Info.Is(Holiday.תשעה٠באב) && (Date + 1).Info.Isשבת)
				retVal.AppendLine("Bring shoes on ערב שבת");

			if (Date.EnglishDate.Month == 12 && Date.EnglishDate.Day == 4 + (DateTime.IsLeapYear(Date.EnglishDate.Year + 1) ? 1 : 0))
				retVal.AppendLine("ותן טל ומטר");

			// Check for a DST transition between midnight & 6AM
			if (Date.EnglishDate.IsDaylightSavingTime() && !Date.EnglishDate.AddHours(6).IsDaylightSavingTime())
				retVal.AppendLine("Daylight Saving Time Ends");
			if (!Date.EnglishDate.IsDaylightSavingTime() && Date.EnglishDate.AddHours(6).IsDaylightSavingTime())
				retVal.AppendLine("Daylight Saving Time Begins");

			if (Date == SummerStart)
				retVal.AppendLine("No סעדה שלישית");
			if (Date == LaborDay.Last(DayOfWeek.Saturday))
				retVal.AppendLine("סעדה שלישית Resumes");

			if (Holiday.Is(Holiday.סוכות)
				&& HolidayCategory == HolidayCategory.חולהמועד
				&& !(Date + 1).Info.Is(HolidayCategory.דאריתא)
				&& !Date.Info.Is(Holiday.סוכות.Days[5])
				&& (DayOfWeek == DayOfWeek.Monday || DayOfWeek == DayOfWeek.Thursday))
				retVal.AppendLine("At the Rav's סוכה");

			if (Date == בדיקה)
				retVal.AppendLine("בדיקת חמץ");

			return retVal.ToString().Trim();
		}

		#region Calculation Methods
		enum דףיומיType {
			None,
			Beforeשחרית,
			Beforeשיעור,
			Beforeמנחה,
			WeekNight,
			NightAlone
		}

		protected const long AM = 0, PM = 12;
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ampm")]
		protected static TimeSpan Time(int hours, int minutes, long ampm) {
			if (hours == 12) {
				if (ampm == AM)
					hours = 0;
				else
					hours = 12;
				ampm = 0;
			}
			return new TimeSpan((int)ampm + hours, minutes, 0);
		}

		#region שחרית
		static readonly TimeSpan Defaultשבת٠יוםטוב٠שחרית = Time(8, 30, AM);
		static readonly TimeSpan timeUntilשמע = TimeSpan.FromMinutes(54);

		TimeSpan Getשבת٠יוםטוב٠שחרית(out bool isBold) {
			var latest = (Zmanim.סוף٠זמן٠קריאת٠שמע٠גרא - timeUntilשמע)
				.RoundDown();

			isBold = latest < Defaultשבת٠יוםטוב٠שחרית;
			return isBold ? latest : Defaultשבת٠יוםטוב٠שחרית;
		}
		TimeSpan GetWeekdayשחרית(out bool isBold) {
			isBold = false;
			if (Date.Info.Isראשחודש) {
				isBold = true;
				return Time(6, 30, AM);
			} else if (Holiday.Is(Holiday.חנוכה))
				return Time(6, 30, AM);
			else if (DayOfWeek == DayOfWeek.Monday || DayOfWeek == DayOfWeek.Thursday)
				return Time(6, 35, AM);
			else
				return Time(6, 45, AM);
		}
		static HebrewDate GetSelichosStart(int hebrewYear) {
			var rh = Holiday.ראש٠השנה.Days.First().Date.GetDate(hebrewYear + 1);
			return (rh - 4).Last(DayOfWeek.Saturday);
		}
		TimeSpan? GetסליחותOffset() {
			if (Date.HebrewMonth == HebrewMonth.תשרי) {
				if (Date.HebrewDay == 9)
					return TimeSpan.FromMinutes(15);
				else if (Date.HebrewDay <= 8)
					return TimeSpan.FromMinutes(35);
			} else if (Date.HebrewMonth == HebrewMonth.אלול) {
				if ((Date + 1).Info.Is(Holiday.ראש٠השנה))
					return TimeSpan.FromHours(1.5);
				else if (Date > GetSelichosStart(Date.HebrewYear))
					return TimeSpan.FromMinutes(30);
			}
			return null;
		}
		#endregion

		#region מנחה

		static TimeSpan GetDefaultערב٠שבת٠מנחה(HebrewDate date) {
			return (date.GetZmanim().Sunset - TimeSpan.FromMinutes(18)).RoundDown() + TimeSpan.FromMinutes(5);	//Always add, even if it's already a multiple of 5.
		}

		protected virtual TimeSpan Getערב٠שבת٠מנחה(TimeSpan defaultמנחה, out bool isEarly) {
			isEarly = false;
			if ((Date + 1).Info.Is(HolidayCategory.דאריתא) && !Date.Info.Is(Holiday.פסח[6]))
				return defaultמנחה;
			else if (Date.Info.Is(HolidayCategory.תענית))
				return defaultמנחה - TimeSpan.FromMinutes(15);
			else if (defaultמנחה > Time(7, 20, PM)) {
				isEarly = true;
				return Time(7, 00, PM);
			} else
				return defaultמנחה;
		}
		#endregion
		#endregion

		public virtual IEnumerable<ScheduleValue> CalcTimes() {
			if (HolidayCategory == HolidayCategory.תענית && !Holiday.Is(Holiday.תשעה٠באב))
				yield return new ScheduleValue("Fast Begins", Zmanim.Sunrise - TimeSpan.FromMinutes(72));


			string dafYomiString = "דף יומי";//I wanted to do this, but it doesn't fit in Word.  "דף יומי – " + Date.Info.DafYomiString;
			דףיומיType dafYomi;
			if (Isיוםטוב || (Isשבת && HolidayCategory == HolidayCategory.חולהמועד))
				dafYomi = דףיומיType.Beforeמנחה;
			else if (Isשבת) {
				if ((Date - 1).Info.Is(Holiday.סוכות.Days.Last()))
					dafYomi = דףיומיType.Beforeמנחה;			//On שבת אסרו חג סוכות, there is no שיעור, since people are likely to go away
				else if (HolidayName == "שבת שובה" ||
						 (Date > new HebrewDayOfYear(HebrewMonth.ניסן, 6) && Date < new HebrewDayOfYear(HebrewMonth.ניסן, 14)))
					dafYomi = דףיומיType.Beforeשחרית;
				else if (Date.EnglishDate.IsDaylightSavingTime())
					dafYomi = דףיומיType.Beforeשיעור;
				else
					dafYomi = דףיומיType.Beforeשחרית;

			} else if ((Date + 1).Info.Isשבתיוםטוב || (Date + 1).Info.Is(Holiday.תשעה٠באב))
				dafYomi = דףיומיType.None;
			else if (HolidayCategory == HolidayCategory.תענית)
				dafYomi = דףיומיType.NightAlone;
			else if (Date >= SummerStart && Date <= LaborDay)
				dafYomi = דףיומיType.NightAlone;
			else
				dafYomi = דףיומיType.WeekNight;

			//TODO: שובה
			if (dafYomi == דףיומיType.Beforeשחרית)
				yield return new ScheduleValue(dafYomiString, Time(7, 30, AM));

			#region שחרית
			bool isשחריתBold;

			if (Holiday.Is(Holiday.ראש٠השנה)) {
				yield return new ScheduleValue("שחרית", Time(7, 45, AM), true);
				yield return new ScheduleValue("קידוש", Time(11, 00, AM));
				if (!Isשבת)
					yield return new ScheduleValue("שופר", Time(11, 15, AM));
			} else if (Holiday.Is(Holiday.יום٠כיפור)) {
				yield return new ScheduleValue("שחרית", Time(7, 50, AM), true);
			} else if (Isשבת || Isיוםטוב) {
				//These days have a second, early, שחרית
				if (Holiday.Is(Holiday.סוכות.Days.Last())) {
					yield return new ScheduleValue("שחרית", Time(6, 30, AM));
					yield return new ScheduleValue("קידוש", Time(10, 00, AM));
					yield return new ScheduleValue("הקפות", Time(10, 20, AM));
				} else if (Holiday.Is(Holiday.שבועות.Days.First())) {
					yield return new ScheduleValue("שחרית", Zmanim.Sunrise - TimeSpan.FromMinutes(50));
					yield return new ScheduleValue("Sunrise", Zmanim.Sunrise);
				}

				if ((Date + 1).Info.Is(Holiday.פסח.Days.First()) && Isשבת)
					yield return new ScheduleValue("שחרית", Time(6, 45, AM), true);
				else
					yield return new ScheduleValue("שחרית", Getשבת٠יוםטוב٠שחרית(out isשחריתBold), isשחריתBold);
			} else if (HolidayCategory == HolidayCategory.חולהמועד) {
				yield return new ScheduleValue("שחרית", Time(8, 00, AM));
			} else if (HolidayCategory == HolidayCategory.תענית) {
				if (Holiday.Is(Holiday.תשעה٠באב))
					yield return new ScheduleValue("שחרית", Time(8, 15, AM));
				else {
					var shacharis = DayOfWeek == DayOfWeek.Sunday ? Time(8, 00, AM) : Time(6, 30, AM);

					var selichosOffset = GetסליחותOffset();
					if (selichosOffset.HasValue)
						yield return new ScheduleValue("סליחות", shacharis - selichosOffset.Value, true);

					yield return new ScheduleValue("שחרית", shacharis);
				}
			} else if ((Holiday.Is(Holiday.פורים))) {
				TimeSpan shacharis = DayOfWeek == DayOfWeek.Friday ? Time(7, 00, AM) : Time(7, 30, AM);

				yield return new ScheduleValue("שחרית", shacharis);
				yield return new ScheduleValue("מגילה", shacharis + TimeSpan.FromMinutes(45));
				yield return new ScheduleValue("מגילה", shacharis + TimeSpan.FromHours(2));
			} else if (Holiday.Is(Holiday.חנוכה) && DayOfWeek == DayOfWeek.Sunday) {
				// Weekday חנוכה is returned later to catch נץ
				yield return new ScheduleValue("שחרית", Time(8, 00, AM));
			} else if ((Date - 1) == GetSelichosStart(Date.HebrewYear)) {
				yield return new ScheduleValue("סליחות", Time(1, 00, AM), true);
				yield return new ScheduleValue("שחרית", Time(8, 00, AM));
			} else if (Date == LaborDay || DayOfWeek == DayOfWeek.Sunday) {

				var selichosOffset = GetסליחותOffset();
				if (selichosOffset.HasValue) {
					yield return new ScheduleValue("סליחות", Time(8, 00, AM) - selichosOffset.Value, true);
					yield return new ScheduleValue("שחרית", Time(8, 00, AM));
				} else if (Date >= SummerStart && Date <= LaborDay) {
					yield return new ScheduleValue("שחרית", Time(8, 00, AM));
				} else {
					yield return new ScheduleValue("שחרית", Time(7, 45, AM));
					if (Date >= new DateTime(2013, 11, 10))
						yield return new ScheduleValue("סדר לימוד", Time(9, 00, AM));
				}
			} else {
				var shacharis = GetWeekdayשחרית(out isשחריתBold);
				var selichosOffset = GetסליחותOffset();

				if (shacharis >= Zmanim.Sunrise - TimeSpan.FromMinutes(29) && shacharis <= Zmanim.Sunrise - TimeSpan.FromMinutes(22))
					yield return new ScheduleValue("נץ", Zmanim.Sunrise);

				if (selichosOffset.HasValue)
					yield return new ScheduleValue("סליחות", shacharis - selichosOffset.Value, true);
				yield return new ScheduleValue("שחרית", shacharis, isשחריתBold);
			}
			#endregion

			#region יזכור
			if (Isיוםטוב) {
				if (Holiday == Holiday.יום٠כיפור)
					yield return new ScheduleValue("יזכור (Approx)", Time(12, 00, PM));

				else if (HolidayName == "שמיני עצרת")
					yield return new ScheduleValue("יזכור (Approx)", Isשבת ? Time(11, 00, AM) : Time(10, 30, AM));

				else if (Holiday == Holiday.פסח.Days.Last() || Holiday == Holiday.שבועות.Days.Last())
					yield return new ScheduleValue("יזכור (Approx)", Isשבת ? Time(10, 45, AM) : Time(10, 30, AM));
			}
			#endregion

			#region סוף זמן קריאת שמע
			yield return new ScheduleValue("סזק״ש", Zmanim.סוף٠זמן٠קריאת٠שמע٠מ٠א);
			yield return new ScheduleValue("סזק״ש", Zmanim.סוף٠זמן٠קריאת٠שמע٠גרא);
			#endregion

			bool hasLateCandleLighting = Date.Info.Isשבתיוםטוב
				&& Date.DayOfWeek != DayOfWeek.Friday
				&& (Date + 1).Info.Isשבתיוםטוב;

			bool isEarlyמנחה;
			var normalמנחה = Getערב٠שבת٠מנחה(GetDefaultערב٠שבת٠מנחה(Date), out isEarlyמנחה);

			if (!hasLateCandleLighting && (Date + 1).Info.Isשבתיוםטוב) {
				//When we have early מנחה, we list a separate
				//candle lighting for people who choose early
				//שבת.  However, if the two candle lightings
				//are too close, only show one.
				var hasTwoCandleLightings = isEarlyמנחה && (Zmanim.Sunset - TimeSpan.FromMinutes(18)) > Time(7, 32, PM);

				var candlesName = hasTwoCandleLightings ? "Candles" : "Candle Lighting";

				yield return new ScheduleValue(candlesName, Zmanim.Sunset - TimeSpan.FromMinutes(18));
				if (hasTwoCandleLightings)
					yield return new ScheduleValue(candlesName, Time(7, 30, PM));
			}

			if (Isשבת || Isיוםטוב) {
				#region שבת/יום טוב Afternoon
				var defaultמנחה = GetDefaultערב٠שבת٠מנחה(Date - 1) - TimeSpan.FromMinutes(10);

				#region מנחה
				var isמנחהBold = false;
				var actualמנחה = defaultמנחה;
				if (Holiday.Is(Holiday.ראש٠השנה))
					actualמנחה -= TimeSpan.FromMinutes(15);
				else if (Holiday.Is(Holiday.יום٠כיפור))
					actualמנחה -= TimeSpan.FromMinutes(110);
				else if ((Date + 1).Info.Is(Holiday.תשעה٠באב)) {
					actualמנחה -= TimeSpan.FromMinutes(80);
					isמנחהBold = true;
				} else if (!Isשבת)
					actualמנחה += TimeSpan.FromMinutes(5);
				else if (HolidayCategory == HolidayCategory.חולהמועד) {	//שבת חול המועד מנחה is early to allow for סעודה שלישית.
					actualמנחה -= TimeSpan.FromMinutes(15);
					isמנחהBold = true;
				} else if (Date >= SummerStart && Date < LaborDay.Last(DayOfWeek.Saturday)) {
					// During the summer time, until the שבת before Labor Day (Labor
					// Day weekend), there is no סעודה שלישית, we have מנחה early.
					if (actualמנחה > Time(7, 50, PM))
						actualמנחה = Time(7, 30, PM);
					else if (actualמנחה > Time(7, 30, PM))
						actualמנחה = Time(7, 00, PM);
					else
						actualמנחה = Time(6, 45, PM);

					isמנחהBold = Date == SummerStart;
				}
				#endregion

				#region שיעור
				var shiurTime = actualמנחה - TimeSpan.FromHours(1);
				var isדרשה = false;
				if (Date > new HebrewDayOfYear(HebrewMonth.ניסן, 6) && Date < new HebrewDayOfYear(HebrewMonth.ניסן, 14)) {
					//שבת ערב פסח is not שבת הגדול
					isדרשה = true;
					shiurTime -= TimeSpan.FromMinutes(15);
				} else if (HolidayName == "שבת שובה") {
					isדרשה = true;
					shiurTime -= TimeSpan.FromMinutes(10);
				}
				#endregion

				if (dafYomi == דףיומיType.Beforeשיעור)
					yield return new ScheduleValue(dafYomiString, shiurTime - TimeSpan.FromHours(1));

				if (dafYomi == דףיומיType.Beforeמנחה)
					yield return new ScheduleValue(dafYomiString, actualמנחה - TimeSpan.FromHours(1));
				else		//When דף יומי isn't right before מנחה, there is a שיעור
					yield return new ScheduleValue(isדרשה ? "דרשה" : "שיעור", shiurTime);

				yield return new ScheduleValue("מנחה", actualמנחה, isמנחהBold);
				if (Holiday.Is(Holiday.יום٠כיפור))
					yield return new ScheduleValue("נעילה", actualמנחה + TimeSpan.FromMinutes(90));


				TimeSpan? maariv = null;
				if (DayOfWeek != DayOfWeek.Friday) {				//On יום טוב ערב שבת, we have מעריב right after מנחה.
					if (Holiday.Is(Holiday.ראש٠השנה.Days.First()))	//ראש השנה מעריב is longer, so we start 10 minutes earlier.
						maariv = defaultמנחה + TimeSpan.FromMinutes(70);
					else if (Isיוםטוב && !(Date + 1).Info.Isשבתיוםטוב)	//Because we don't say ויהי נועם, we daven מעריב five minutes later.
						maariv = defaultמנחה + TimeSpan.FromMinutes(85);
					else if (Isיוםטוב)
						maariv = (Zmanim.Sunset + TimeSpan.FromMinutes(54)).RoundDown();
					else
						maariv = defaultמנחה + TimeSpan.FromMinutes(80);
					if (Holiday.Is(Holiday.פסח[1]) || Holiday.Is(Holiday.פסח[2]))
						maariv -= TimeSpan.FromMinutes(10);			// מעריב on סדר night starts early due to הלל
				}
				if (Holiday.Is(Holiday.סוכות.Days[7])) {
					maariv += TimeSpan.FromMinutes(10);		// No clue why
					yield return new ScheduleValue("הקפות", maariv.Value + TimeSpan.FromMinutes(15));
				}
				if (maariv != null)
					yield return new ScheduleValue("מעריב", maariv.Value);

				if (Holiday.Is(Holiday.סוכות.Days[5]))
					yield return new ScheduleValue("משנה תורה", maariv.Value + TimeSpan.FromMinutes(80));

				if ((Date + 1).Info.Is(Holiday.פורים)) {	//10 minute delay for women to come to Shul
					yield return new ScheduleValue("מגילה", defaultמנחה + TimeSpan.FromMinutes(95 + 10));
					yield return new ScheduleValue("מגילה", defaultמנחה + TimeSpan.FromMinutes(95 + 10 + 90));
					yield return new ScheduleValue("מסיבה", Time(10, 00, PM));
				}
				#endregion
			} else if ((Date + 1).Info.Is(Holiday.יום٠כיפור)) {
				yield return new ScheduleValue("מנחה", Time(3, 00, PM));

				var kolNidrei = GetDefaultערב٠שבת٠מנחה(Date);

				yield return new ScheduleValue("תפילה זכה", kolNidrei - TimeSpan.FromMinutes(10));
				yield return new ScheduleValue("כל נדרי", kolNidrei);
			} else if ((Date + 1).Info.Isשבתיוםטוב) {
				if ((Date + 1).Info.Is(Holiday.חנוכה))
					yield return new ScheduleValue("מנחה", Time(3, 00, PM));
				//no else; ערב שבת חנוכה has two מנחהs
				yield return new ScheduleValue("מנחה", normalמנחה);

				if (!(Date + 1).Info.Is(Holiday.חנוכה)) {
					if (Zmanim.Sunset < Time(5, 05, PM))
						yield return new ScheduleValue("חומש שיעור", Time(8, 00, PM));
					else if (Zmanim.Sunset < Time(5, 15, PM))
						yield return new ScheduleValue("חומש שיעור", Time(8, 15, PM), true);
					else if (Zmanim.Sunset < Time(5, 40, PM))
						yield return new ScheduleValue("חומש שיעור", Time(8, 30, PM), true);
				}
				if ((Date + 1).Info.Is(HolidayCategory.דאריתא) && DayOfWeek != DayOfWeek.Friday) {
					var maariv = normalמנחה + TimeSpan.FromMinutes(65);
					if ((Date + 1).Info.Is(Holiday.ראש٠השנה))
						maariv = normalמנחה + TimeSpan.FromMinutes(50);
					else if ((Date + 1).Info.Is(Holiday.פסח.Days.First()))
						maariv -= TimeSpan.FromMinutes(10);			// מעריב on סדר night starts early due to הלל
					if (Holiday != Holiday.פסח[6])
						yield return new ScheduleValue("מעריב", maariv);
					if (Holiday.Is(Holiday.סוכות.Days[6]))
						yield return new ScheduleValue("הקפות", maariv + TimeSpan.FromMinutes(15));
				}
			} else if (Holiday.Is(Holiday.סוכות.Days[5])) {
				yield return new ScheduleValue(dafYomiString, Time(8, 00, PM));
				yield return new ScheduleValue("מעריב", Time(9, 00, PM));
				yield return new ScheduleValue("משנה תורה", Time(9, 15, PM));
				dafYomi = דףיומיType.None;
			} else if (Holiday.Is(Holiday.סוכות) && (DayOfWeek == DayOfWeek.Monday || DayOfWeek == DayOfWeek.Thursday)) {	// This must be after the checks for ערב? יום טוב and משנה תורה
				yield return new ScheduleValue(dafYomiString, Time(8, 00, PM));
				yield return new ScheduleValue("שמחת בית השואבה", Time(9, 00, PM));
				yield return new ScheduleValue("מעריב", Time(10, 00, PM));
				dafYomi = דףיומיType.None;
			} else if (Holiday.Is(Holiday.פורים)) {//And not Friday
				yield return new ScheduleValue("מנחה", Time(3, 00, PM));
				//yield return new ScheduleValue("מעריב", Time(9, 00, PM));
				dafYomi = דףיומיType.NightAlone;
			} else if (HolidayName == "תענית אסתר") {
				var mincha = (Zmanim.Sunset - TimeSpan.FromMinutes(30)).RoundDown();

				if (TimeZoneInfo.Local.IsDaylightSavingTime(Date)		//פורים is on Sunday
					|| (Date + 1).Info.Is(Holiday.פורים)) {				//פורים is on Tuesday, Thursday, or Friday
					yield return new ScheduleValue("מנחה", mincha);
					yield return new ScheduleValue("מעריב", mincha + TimeSpan.FromHours(1));
				}

				if ((Date + 1).Info.Is(Holiday.פורים)) {				//פורים is on Tuesday, Thursday, or Friday
					yield return new ScheduleValue("מגילה", mincha + TimeSpan.FromMinutes(75));
					yield return new ScheduleValue("מגילה", mincha + TimeSpan.FromMinutes(75 + 90));

					if (TimeZoneInfo.Local.IsDaylightSavingTime(Date) || Date.DayOfWeek != DayOfWeek.Friday)
						yield return new ScheduleValue("מסיבה", Time(10, 00, PM));
					else
						yield return new ScheduleValue("מסיבה", Time(10, 30, PM));
				}
			} else if ((Date + 1).Info.Is(Holiday.תשעה٠באב)) {
				//ערב תשעה בעב
				yield return new ScheduleValue("מנחה", Zmanim.Sunset.RoundUp(TimeSpan.FromMinutes(15)) - TimeSpan.FromMinutes(90));
				yield return new ScheduleValue("מעריב", Zmanim.Sunset.RoundUp() + TimeSpan.FromMinutes(30));
				dafYomi = דףיומיType.None;

			} else if (Holiday.Is(Holiday.תשעה٠באב)) {
				yield return new ScheduleValue("מנחה", Time(1, 40, PM));
				yield return new ScheduleValue("חצות", Zmanim.חצות);
				yield return new ScheduleValue("מעריב", Zmanim.Sunset.RoundUp() + TimeSpan.FromMinutes(30));
				dafYomi = דףיומיType.None;
			} else if (HolidayCategory == HolidayCategory.תענית		//On a weekday, עשרה בטבת is too early for מנחה/מעריב.
					&& (HolidayName != "עשרה בטבת" || DayOfWeek == DayOfWeek.Sunday)) {
				var mincha = (Zmanim.Sunset - TimeSpan.FromMinutes(30)).RoundDown();
				if (mincha >= Time(7, 00, PM) || DayOfWeek == DayOfWeek.Sunday) {
					yield return new ScheduleValue(dafYomiString, mincha - TimeSpan.FromMinutes(30));
					dafYomi = דףיומיType.None;
				}
				yield return new ScheduleValue("מנחה", mincha);
				yield return new ScheduleValue("מעריב", mincha + TimeSpan.FromHours(1));
			}

			if (hasLateCandleLighting) {
				yield return new ScheduleValue("Candle Lighting", Zmanim.Sunset + TimeSpan.FromMinutes(72));
			}
			if (dafYomi == דףיומיType.NightAlone) {
				yield return new ScheduleValue(dafYomiString, Time(9, 00, PM));
			} else if (dafYomi == דףיומיType.WeekNight) {
				var hasמשנהברורה = Date.EnglishDate.Year >= 2013
				 && (Date > LaborDay || Date <= SummerStart)
				 && HolidayCategory != HolidayCategory.חולהמועד;

				if (DayOfWeek == DayOfWeek.Sunday || Date == בדיקה) {
					yield return new ScheduleValue("מנחה", Zmanim.Sunset - TimeSpan.FromMinutes(15));
					yield return new ScheduleValue("מעריב", Zmanim.Sunset + TimeSpan.FromMinutes(3));

					//בדיקת חמץ night has מנחה/מעריב, but not דף יומי
					if (Date != בדיקה)
						yield return new ScheduleValue(dafYomiString, Time(9, 00, PM));
				} else {
					if (hasמשנהברורה)
						yield return new ScheduleValue("משנה ברורה", Time(8, 45, PM));
					yield return new ScheduleValue(dafYomiString, Time(9, 15, PM));
					yield return new ScheduleValue("מעריב", Time(9, 00, PM));
				}
			}

			//TODO: תענית בכורות / ערב פסח

			if ((Date + 1).Info.Is(Holiday.תשעה٠באב))
				yield return new ScheduleValue("Sunset", Zmanim.Sunset);
			if (HolidayCategory == HolidayCategory.תענית && DayOfWeek != DayOfWeek.Friday)
				yield return new ScheduleValue("Fast Ends", Zmanim.Sunset + TimeSpan.FromMinutes(45));

			yield break;
		}
	}
}