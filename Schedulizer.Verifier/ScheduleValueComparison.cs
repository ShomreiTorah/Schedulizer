using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ShomreiTorah.Common;

namespace ShomreiTorah.Schedules.Verifier {
	class ScheduleValueComparison {
		protected ScheduleValueComparison() { }
		public ScheduleValueComparison(string oldString, IEnumerable<ScheduleValue> newValues) {
			NewValues = new ReadOnlyCollection<ScheduleValue>(newValues == null ? new ScheduleValue[0] : newValues.ToArray());

			OldString = new ValueReference(oldString, this);
			NewString = new ValueReference(NewValues.Join("\n", t => t.TimeString), this);
		}

		public ValueReference OldString { get; protected set; }
		public ReadOnlyCollection<ScheduleValue> NewValues { get; protected set; }

		public ValueReference NewString { get; protected set; }
		public bool IsBold { get { return NewValues.Any(t => t.IsBold); } }

		public virtual bool AreSame { get { return OldString.String.Replace("\r", "") == NewString.String; } }
	}

	class ShiurComparison : ScheduleValueComparison {
		public ShiurComparison(string oldString, IEnumerable<ScheduleValue> newCell) {
			OldString = new ValueReference(oldString, this);

			string newName;
			if (oldString.Contains("דף יומי"))
				newName = "דף יומי";
			else if (oldString.Contains("דרשה"))
				newName = "דרשה";
			else
				newName = "שיעור";

			NewValues = new ReadOnlyCollection<ScheduleValue>(newCell.Where(sv => sv.Name == newName).ToArray());

			var newString = NewValues.Join("\n", t => t.TimeString);

			if (newName == "דף יומי")
				newString = "דף יומי " + newString;
			else if (newName == "דרשה")
				newString = newString + " דרשה";

			NewString = new ValueReference(newString, this);
		}
	}

	class PriorMinchaComparison : ScheduleValueComparison {
		public PriorMinchaComparison(string oldString, IEnumerable<ScheduleValue> newValues) : base(oldString, newValues) { }

		public override bool AreSame { get { return String.IsNullOrEmpty(OldString) || base.AreSame; } }
	}

	class StringReference<T> {
		public StringReference(string str, T reference) { String = str; Reference = reference; }

		public string String { get; private set; }

		public T Reference { get; private set; }

		public override string ToString() { return String; }

		public static implicit operator string(StringReference<T> sr) { return sr.String; }
	}
	class ValueReference : StringReference<ScheduleValueComparison> {
		public ValueReference(string str, ScheduleValueComparison reference) : base(str, reference) { }
	}
}
