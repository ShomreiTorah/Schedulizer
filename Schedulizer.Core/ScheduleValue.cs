using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using ShomreiTorah.Common;

namespace ShomreiTorah.Schedules {
	///<summary>A single time in a ScheduleElement.</summary>
	public struct ScheduleValue : IEquatable<ScheduleValue> {
		public ScheduleValue(string name, TimeSpan time) : this(name, time, false) { }
		public ScheduleValue(string name, TimeSpan time, bool isBold)
			: this() {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (Time < TimeSpan.Zero || Time >= TimeSpan.FromDays(1))
				throw new ArgumentOutOfRangeException("time");

			Name = name;
			Time = time;
			IsBold = isBold;
		}

		///<summary>Gets the name of this time, or null of this value is unnamed.</summary>
		public string Name { get; private set; }
		///<summary>Gets the time for this value, or TimeSpan.Zero if this value has no time.</summary>
		public TimeSpan Time { get; private set; }

		///<summary>Gets whether this time should be displayed in bold.</summary>
		public bool IsBold { get; private set; }


		///<summary>Gets whether this ScheduleValue is empty.</summary>
		public bool IsEmpty { get { return Name == null; } }

		///<summary>Gets the time for this value in string form.</summary>
		public string TimeString {
			get {
				var time = Time;
				if (time.TotalHours > 12)	// Convert PM to AM.
					time -= TimeSpan.FromHours(12);
				return time.ToString(@"h\:mm", CultureInfo.CurrentCulture);
			}
		}

		#region Equality
		public override bool Equals(object obj) { return obj is ScheduleValue && Equals((ScheduleValue)obj); }
		public override int GetHashCode() {
			//http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
			var hash = 17;
			hash = hash * 23 + Name.GetHashCode();
			hash = hash * 23 + Time.GetHashCode();
			hash = hash * 23 + IsEmpty.GetHashCode();
			return hash;
		}

		public bool Equals(ScheduleValue other) { return Name == other.Name && Time == other.Time && IsBold == other.IsBold; }

		public static bool operator ==(ScheduleValue first, ScheduleValue second) { return first.Equals(second); }
		public static bool operator !=(ScheduleValue first, ScheduleValue second) { return !first.Equals(second); }
		#endregion

		public override string ToString() { return Name + ": " + TimeString; }
	}
}