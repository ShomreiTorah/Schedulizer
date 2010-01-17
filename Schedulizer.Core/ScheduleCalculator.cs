using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;
using ShomreiTorah.Common.Calendar.Zmanim;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Schedules {
	public class ScheduleCalculator {
		public ScheduleCalculator(HebrewDate date) {
			Date = date;
			Holiday = Date.Info.Holiday;
			Zmanim = Date.GetZmanim();
		}

		public HebrewDate Date { get; private set; }
		public Holiday Holiday { get; private set; }
		public ZmanimInfo Zmanim { get; private set; }
		public bool Isיוםטוב { get { return HolidayCategory == HolidayCategory.דאריתא; } }
		public bool Isשבת { get { return Date.Info.Isשבת; } }
		public DayOfWeek DayOfWeek { get { return Date.DayOfWeek; } }
		public HolidayCategory HolidayCategory { get { return Holiday == null ? HolidayCategory.None : Holiday.Category; } }
		public string HolidayName { get { return Holiday == null ? null : Holiday.Name; } }


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

					else if (Date.HebrewDay > 23)
						retVal.Append("מברכים ").AppendLine((Date + 15).HebrewMonthName);
				}
			}

			return retVal.ToString().Trim();
		}

		#region Calculation Methods
		enum דףיומיType {
			None,
			Beforeשחרית,
			Beforeשיעור,
			Beforeמנחה,
			WeekNight
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
			} else if (DayOfWeek == DayOfWeek.Monday || DayOfWeek == DayOfWeek.Thursday)
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
				else if (Date.HebrewDay < 8)
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

		protected virtual TimeSpan Getערב٠שבת٠מנחה(TimeSpan defaultמנחה) {
			if ((Date + 1).Info.Is(Holiday.שבועות.Days[0]))
				return defaultמנחה;

			if (defaultמנחה > Time(7, 00, PM))
				return Time(7, 00, PM);
			else
				return defaultמנחה;
		}
		#endregion
		#endregion

		public virtual IEnumerable<ScheduleValue> CalcTimes() {
			if (HolidayCategory == HolidayCategory.תענית && !Holiday.Is(Holiday.תשעה٠באב))
				yield return new ScheduleValue("Fast Begins", Zmanim.Sunrise - TimeSpan.FromMinutes(72));

			string dafYomiString = "דף יומי";//I wanted to do this, but it doesn't fit in Word.  "דף יומי – " + Date.Info.DafYomiString;
			דףיומיType dafYomi;
			if (Isיוםטוב)
				dafYomi = דףיומיType.Beforeמנחה;
			else if (Isשבת) {
				if (Date.EnglishDate.Month == 7 || Date.EnglishDate.Month == 8)
					dafYomi = דףיומיType.None;
				else if (Date.EnglishDate.IsDaylightSavingTime())
					dafYomi = דףיומיType.Beforeשיעור;
				else
					dafYomi = דףיומיType.Beforeשחרית;

			} else if (HolidayCategory == HolidayCategory.תענית)
				dafYomi = דףיומיType.None;
			else if ((Date + 1).Info.Isשבתיוםטוב)
				dafYomi = דףיומיType.None;
			else if (Holiday.Is(Holiday.סוכות.Days[5]))	//No דף יומי on משנה תורה
				dafYomi = דףיומיType.None;
			else
				dafYomi = דףיומיType.WeekNight;

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
				} else if (Holiday.Is(Holiday.שבועות.Days.First()))
					yield return new ScheduleValue("שחרית", Time(4, 40, AM));

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
					var shacharis = Time(6, 30, AM);

					var selichosOffset = GetסליחותOffset();
					if (selichosOffset.HasValue)
						yield return new ScheduleValue("סליחות", shacharis - selichosOffset.Value, true);

					yield return new ScheduleValue("שחרית", Time(6, 30, AM));
				}
			} else if ((Holiday.Is(Holiday.פורים))) {
				yield return new ScheduleValue("שחרית", Time(7, 00, AM));
				yield return new ScheduleValue("מגילה", Time(7, 45, AM));
				yield return new ScheduleValue("מגילה", Time(9, 00, AM));
			} else if (Holiday.Is(Holiday.חנוכה)) {
				yield return new ScheduleValue("שחרית", Time(6, 30, AM));
			} else if ((Date - 1) == GetSelichosStart(Date.HebrewYear)) {
				yield return new ScheduleValue("סליחות", Time(1, 00, AM), true);
			} else if (DayOfWeek != DayOfWeek.Sunday) {
				var shacharis = GetWeekdayשחרית(out isשחריתBold);
				var selichosOffset = GetסליחותOffset();

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

			if (!hasLateCandleLighting && (Date + 1).Info.Isשבתיוםטוב)
				yield return new ScheduleValue("Candle Lighting", Zmanim.Sunset - TimeSpan.FromMinutes(18));

			if (Isשבת || Isיוםטוב) {
				#region שבת/יום טוב Afternoon
				//TODO: שבת/יום טוב Afternoon
				var defaultמנחה = GetDefaultערב٠שבת٠מנחה(Date - 1) - TimeSpan.FromMinutes(10);

				#region מנחה
				var isמנחהBold = false;
				var actualמנחה = defaultמנחה;
				if (Holiday.Is(Holiday.ראש٠השנה))
					actualמנחה -= TimeSpan.FromMinutes(15);
				else if (Holiday.Is(Holiday.יום٠כיפור))
					actualמנחה -= TimeSpan.FromMinutes(110);
				else if (!Isשבת)
					actualמנחה += TimeSpan.FromMinutes(5);
				else if (HolidayCategory == HolidayCategory.חולהמועד) {	//שבת חול המועד מנחה is early to allow for סעודה שלישית.
					actualמנחה -= TimeSpan.FromMinutes(15);
					isמנחהBold = true;
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


				if (!Isיוםטוב || DayOfWeek != DayOfWeek.Friday) {	//On יום טוב ערב שבת, we have מעריב right after מנחה.
					if (Holiday.Is(Holiday.ראש٠השנה.Days.First()))	//ראש השנה מעריב is longer, so we start 10 minutes earlier.
						yield return new ScheduleValue("מעריב", defaultמנחה + TimeSpan.FromMinutes(70));
					else if (Isיוםטוב && !(Date + 1).Info.Isשבתיוםטוב)	//Because we don't say ויהי נועם, we daven מעריב five minutes later.
						yield return new ScheduleValue("מעריב", defaultמנחה + TimeSpan.FromMinutes(85));
					else
						yield return new ScheduleValue("מעריב", defaultמנחה + TimeSpan.FromMinutes(80));
				}
				if (Holiday.Is(Holiday.סוכות.Days[7]))			//הקפות are 20 minutes after מעריב
					yield return new ScheduleValue("הקפות", defaultמנחה + TimeSpan.FromMinutes(100));

				if ((Date + 1).Info.Is(Holiday.פורים)) {
					yield return new ScheduleValue("מגילה", defaultמנחה + TimeSpan.FromMinutes(95));
					yield return new ScheduleValue("מגילה", defaultמנחה + TimeSpan.FromMinutes(95 + 90));
					yield return new ScheduleValue("מסיבה", Time(10, 30, PM));
				}
				#endregion
			} else if (Holiday.Is(Holiday.סוכות.Days[5])) {
				yield return new ScheduleValue("מעריב", Time(9, 00, PM));
				yield return new ScheduleValue("משנה תורה", Time(9, 15, PM));
			} else if ((Date + 1).Info.Isשבתיוםטוב) {
				if ((Date + 1).Info.Is(Holiday.יום٠כיפור)) {
					yield return new ScheduleValue("מנחה", Time(3, 00, PM));
					yield return new ScheduleValue("תפילה זכה", Time(6, 15, PM));
					yield return new ScheduleValue("כל נדרי", Time(6, 30, PM));
				} else {
					if ((Date + 1).Info.Is(Holiday.חנוכה))
						yield return new ScheduleValue("מנחה", Time(3, 00, PM));
					//ערב שבת חנוכה has two מנחהs
					yield return new ScheduleValue("מנחה", Getערב٠שבת٠מנחה(GetDefaultערב٠שבת٠מנחה(Date)));
				}
			} else if (Holiday.Is(Holiday.פורים)) {//And not Friday
				yield return new ScheduleValue("מנחה", Time(3, 00, PM));
				yield return new ScheduleValue("מעריב", Time(9, 00, PM));
				dafYomi = דףיומיType.None;
			} else if (HolidayName == "תענית אסתר") {
				var mincha = (Zmanim.Sunset - TimeSpan.FromMinutes(30)).RoundUp();

				if (TimeZoneInfo.Local.IsDaylightSavingTime(Date)	//פורים is on Sunday
				 || (Date + 1).Info.Is(Holiday.פורים)) {				//פורים is on Tuesday, Thursday, or Friday
					yield return new ScheduleValue("מנחה", mincha);
					yield return new ScheduleValue("מעריב", mincha + TimeSpan.FromHours(1));
				}

				if ((Date + 1).Info.Is(Holiday.פורים)) {				//פורים is on Tuesday, Thursday, or Friday
					yield return new ScheduleValue("מגילה", mincha + TimeSpan.FromMinutes(75));
					yield return new ScheduleValue("מגילה", mincha + TimeSpan.FromMinutes(75 + 90));
					yield return new ScheduleValue("מסיבה", Time(10, 30, PM));
				}
			}

			if (hasLateCandleLighting) {
				yield return new ScheduleValue("Candle Lighting", Zmanim.Sunset + TimeSpan.FromMinutes(72));
			}

			if (dafYomi == דףיומיType.WeekNight) {
				if (DayOfWeek == DayOfWeek.Sunday || Date < new DateTime(2009, 11, 2))
					yield return new ScheduleValue(dafYomiString, Time(9, 00, PM));
				else {
					yield return new ScheduleValue("עמוד יומי", Time(8, 40, PM));
					//No מעריב...  Waaah...
					//yield return new ScheduleValue("מעריב", Time(9, 10, PM));
					yield return new ScheduleValue(dafYomiString, Time(9, 30, PM));
				}
			}

			//TODO: תענית בכורות / ערב פסח
			//TODO: מנין on תענית

			if ((Date + 1).Info.Is(Holiday.תשעה٠באב))
				yield return new ScheduleValue("Sunset", Zmanim.Sunset);
			if (HolidayCategory == HolidayCategory.תענית)
				yield return new ScheduleValue("Fast Ends", Zmanim.Sunset + TimeSpan.FromMinutes(45));


			yield break;
		}
	}
}
