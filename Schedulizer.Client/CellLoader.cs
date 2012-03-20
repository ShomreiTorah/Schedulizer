using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ShomreiTorah.Schedules.WinClient {
	class CellLoader {
		class DateRange : IComparable<DateRange> {
			public DateRange(DateTime from, DateTime to) {
				if (from > to)
					throw new ArgumentOutOfRangeException("to");
				From = from.Date;
				To = to.Date;
			}

			public DateTime From { get; private set; }
			public DateTime To { get; private set; }

			public DataState State { get; set; }

			public int CompareTo(DateRange other) {
				if (other == null)
					return -1;
				return From.CompareTo(other.From);
			}

			///<summary>Occurs when the range's contents finish loading.</summary>
			public event EventHandler Loaded;
			///<summary>Raises the Loaded event.</summary>
			internal protected virtual void OnLoaded() { OnLoaded(EventArgs.Empty); }
			///<summary>Raises the Loaded event.</summary>
			///<param name="e">An EventArgs object that provides the event data.</param>
			internal protected virtual void OnLoaded(EventArgs e) {
				if (Loaded != null)
					Loaded(this, e);
				Loaded = null;
			}

		}

		readonly List<DateRange> ranges = new List<DateRange>();

		public ScheduleContext Context { get; set; }

		public CellLoader(ScheduleContext context) {
			Context = context;
		}

		public void LoadCell(DateTime date, Action<ScheduleCell> callback) {
			var range = GetRange(date);
			if (range == null) {
				LoadRange(date, date);
				range = GetRange(date);
			}
			if (range.State == DataState.Ready)
				callback(Context.GetCell(date));
			else if (range.State == DataState.Loading)
				range.Loaded += delegate {
					if (range.State == DataState.Ready)
						callback(Context.GetCell(date));
				};

		}

		public DataState GetState(DateTime date) {
			var range = GetRange(date);
			if (range == null)
				throw new InvalidOperationException(date + " has not been requested");
			return range.State;
		}
		private DateRange GetRange(DateTime date) {
			date = date.Date;
			var startIndex = ranges.BinarySearch(new DateRange(date, date));
			if (startIndex >= 0)
				return ranges[startIndex];
			for (int i = Math.Max(0, ~startIndex - 1); i < ranges.Count; i++) {
				var r = ranges[i];

				if (r.From <= date && r.To >= date)
					return r;

				if (r.From > date)
					break;
			}
			return null;
		}

		public void LoadRange(DateTime from, DateTime to) {
			from = from.Date; to = to.Date;
			var startIndex = ranges.BinarySearch(new DateRange(from, to));
			if (startIndex >= 0 && ranges[startIndex].To >= to)
				return;
			for (int i = Math.Max(0, Math.Max(startIndex, ~startIndex - 1)); i < ranges.Count; i++) {
				var r = ranges[i];
				if (r.To < from)
					continue;	//We haven't reached the desired range
				if (r.From <= from && r.To >= to)
					return;		//We found the entire range
				if (r.From > to)
					break;		//We overshot

				//If either end of this range is inside of the range we want, avoid it
				if (r.To >= from && r.To < to) {
					if (r.From > from)		//If we find a island that was already loaded, split in half
						LoadRange(from, r.From.AddDays(-1));
					from = r.To.AddDays(1);
				}
				if (r.From <= to && r.From > from) {
					if (r.To < to)
						LoadRange(r.To.AddDays(1), to);
					to = r.From.AddDays(-1);
				}
			}

			var syncContext = SynchronizationContext.Current;

			var range = new DateRange(from, to);
			ranges.Insert(~ranges.BinarySearch(range), range);
			Waiter.ExecAsync(delegate {
				try {
					Context.LoadCells(from, to);
					range.State = DataState.Ready;
				} catch {
					range.State = DataState.Error;
					throw;
				} finally {
					syncContext.Post(delegate {
						OnDataLoaded();
						range.OnLoaded();
					}, null);
				}
			}, "Loading " + from.ToShortDateString() + " through " + to.ToShortDateString());
		}

		///<summary>Occurs when schedule data is loaded from the database.</summary>
		public event EventHandler DataLoaded;
		///<summary>Raises the DataLoaded event.</summary>
		internal protected virtual void OnDataLoaded() { OnDataLoaded(EventArgs.Empty); }
		///<summary>Raises the DataLoaded event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnDataLoaded(EventArgs e) {
			if (DataLoaded != null)
				DataLoaded(this, e);
		}
	}

	enum DataState {
		Loading,
		Ready,
		Error
	}
}