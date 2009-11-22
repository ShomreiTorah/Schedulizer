using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;
using ShomreiTorah.Common.Calendar.Zmanim;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShomreiTorah.Schedules {
	public partial class ScheduleContext {
		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static readonly ScheduleContext Instance = new ScheduleContext(DB.Default);
		}
		//If the property is set before it is first read, DB.Default will never be called
		static ScheduleContext defaultOverride;

		///<summary>Gets or sets the default ScheduleContext instance.</summary>
		public static ScheduleContext Default {
			get { return defaultOverride ?? DefaultContainer.Instance; }
			set { defaultOverride = value; }
		}

		readonly bool shouldCloseConnection;
		CellDictionary loadedCells;

		public ScheduleContext(DBConnector database) : this(CreateEntityConnection(database)) { shouldCloseConnection = true; }

		partial void OnContextCreated() {
			_CellSet = CellSet.Include("Times");
			loadedCells = new CellDictionary(this);
		}

		static EntityConnection CreateEntityConnection(DBConnector database) {
			var workspace = new MetadataWorkspace();

			EdmItemCollection csdl;
			StoreItemCollection ssdl;
			StorageMappingItemCollection msl;

			using (var stream = typeof(ScheduleContext).Assembly.GetManifestResourceStream("ScheduleModel.csdl"))
			using (var reader = XmlReader.Create(stream))
				csdl = new EdmItemCollection(Enumerable.Repeat(reader, 1));
			using (var stream = typeof(ScheduleContext).Assembly.GetManifestResourceStream("ScheduleModel.ssdl")) {
				XDocument ssdlDoc;
				using (var reader = XmlReader.Create(stream))
					ssdlDoc = XDocument.Load(reader);

				ssdlDoc.Root.Attribute("Provider").Value = database.Factory.GetType().Namespace;
				using (var reader = ssdlDoc.CreateReader())
					ssdl = new StoreItemCollection(Enumerable.Repeat(reader, 1));
			}

			using (var stream = typeof(ScheduleContext).Assembly.GetManifestResourceStream("ScheduleModel.msl"))
			using (var reader = XmlReader.Create(stream))
				msl = new StorageMappingItemCollection(csdl, ssdl, Enumerable.Repeat(reader, 1));

			workspace.RegisterItemCollection(csdl);
			workspace.RegisterItemCollection(ssdl);
			workspace.RegisterItemCollection(msl);

			var connection = database.OpenConnection();
			connection.Close();	//EntityConnection requires that the connection be closed
			return new EntityConnection(workspace, connection);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (shouldCloseConnection) {
					Connection.Close();
					Connection.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		class CellDictionary : KeyedCollection<DateTime, ScheduleCell> {
			ScheduleContext context;
			public CellDictionary(ScheduleContext context) { this.context = context; }
			protected override void InsertItem(int index, ScheduleCell item) {
				base.InsertItem(index, item);
				item.Context = context;
			}

			protected override DateTime GetKeyForItem(ScheduleCell item) { return item.Date.EnglishDate.Date; }
			public bool TryGetValue(DateTime date, out ScheduleCell cell) {
				if (Dictionary != null)
					return Dictionary.TryGetValue(date.Date, out cell);
				else {
					return null != (cell = this.FirstOrDefault(c => c.Date.EnglishDate.Date == date.Date));
				}
			}
		}
		///<summary>Checks whether the cell for the given date has already been loaded from SQL Server.</summary>
		public bool IsLoaded(DateTime date) { return loadedCells.Contains(date.Date); }
		///<summary>Gets the ScheduleCell that describes a given date.</summary>
		public ScheduleCell GetCell(DateTime date) {
			date = date.Date;
			ScheduleCell retVal;

			if (loadedCells.TryGetValue(date, out retVal))
				return retVal;

			retVal = CellSet.FirstOrDefault(c => c.EnglishDate == date);

			if (retVal == null)
				retVal = CreateCell(date);

			loadedCells.Add(retVal);

			return retVal;
		}
		ScheduleCell CreateCell(DateTime date) {
			ScheduleCell retVal;
			retVal = new ScheduleCell(date);
			retVal.Recalculate();
			AddToCellSet(retVal);
			return retVal;
		}
		static readonly MemberComparer<ScheduleCell> cellDateComparer = new MemberComparer<ScheduleCell>(c => c.EnglishDate.Date);

		///<summary>Loads all of the schedule cells  for a range of dates.</summary>
		public void LoadCells(DateTime from, DateTime to) {
			for (from = from.Date; from <= to; from = from.AddDays(1)) {
				if (!IsLoaded(from)) break;
			}
			for (to = to.Date; to >= from; to = to.AddDays(-1)) {
				if (!IsLoaded(to)) break;
			}
			if (from > to) return;		//Everything is already loaded

			var newCells = CellSet
				.Where(c => c.EnglishDate >= from && c.EnglishDate <= to)
				.AsEnumerable()	//LINQ to Entities cannot handle Except(IComparer)
				.Except(loadedCells, cellDateComparer)
				.ToArray();

			loadedCells.AddRange(newCells);

			//If not all of the cells are in the database, calculate the
			//missing ones now instead of hitting the DB for each one in
			//GetCell individually.
			if (newCells.Length < (to - from).Days) {
				for (var date = from; date <= to; date = date.AddDays(1)) {
					if (loadedCells.Contains(date)) continue;
					loadedCells.Add(CreateCell(date));
				}
			}
		}
	}

	public sealed partial class ScheduleCell {
		public ScheduleCell() { Id = Guid.NewGuid(); }
		public ScheduleCell(DateTime date) : this() { EnglishDate = date; }

		partial void OnEnglishDateChanging(DateTime value) {
#if CODE_ANALYSIS
			value.ToString();
#endif
			OnPropertyChanging("Date");
			OnPropertyChanging("Holiday");
			OnPropertyChanging("Isיוםטוב");
			OnPropertyChanging("Isשבת");
			OnPropertyChanging("HolidayCategory");
		}
		partial void OnEnglishDateChanged() {
			//There's nothing wrong with setting the field here.
			_EnglishDate = SetValidValue(EnglishDate.Date);
			Date = new HebrewDate(EnglishDate);
			Holiday = Date.Info.Holiday;

			OnPropertyChanged("Date");
			OnPropertyChanged("Holiday");
			OnPropertyChanged("Isיוםטוב");
			OnPropertyChanged("Isשבת");
			OnPropertyChanged("HolidayCategory");
		}

		public HebrewDate Date { get; private set; }
		public Holiday Holiday { get; private set; }
		public bool Isיוםטוב { get { return HolidayCategory == HolidayCategory.דאריתא; } }
		public bool Isשבת { get { return Date.Info.Isשבת; } }
		public HolidayCategory HolidayCategory { get { return Holiday == null ? HolidayCategory.None : Holiday.Category; } }
		public ScheduleContext Context { get; set; }

		public override string ToString() {
			if (String.IsNullOrEmpty(Title))
				return Date.EnglishDate.ToLongDateString();
			else
				return Date.EnglishDate.ToLongDateString() + " - " + Title;
		}

		public void Recalculate() { Recalculate(new ScheduleCalculator(Date)); }
		public void Recalculate(ScheduleCalculator calc) {
			if (calc == null) throw new ArgumentNullException("calc");

			var newTitle = calc.CalcTitle();
			if (Title != newTitle)	//Believe it or not, the setter doesn't check this
				Title = newTitle;

			//foreach (var time in Times.ToArray())
			//    Context.DeleteObject(time);

			//AddTimes(calc.CalcTimes());

			//return;
			var unprocessedOldTimes = Times.ToList();
			var unprocessedNewTimes = calc.CalcTimes().ToList();

			//Ignore all times that haven't changed (by removing them from the lists)
			for (int i = unprocessedOldTimes.Count - 1; i >= 0; i--) {
				var oldTime = unprocessedOldTimes[i];
				var newTime = unprocessedNewTimes.FindIndex(sv => sv == oldTime);
				if (newTime >= 0) {
					unprocessedOldTimes.RemoveAt(i);
					unprocessedNewTimes.RemoveAt(newTime);
				}
			}

			//Remove all remaining times that weren't identical
			foreach (var oldTime in unprocessedOldTimes) {
				Context.DeleteObject(oldTime);
			}

			//Add all the new times that weren't identical
			AddTimes(unprocessedNewTimes);

			Debug.Assert(Times.OrderBy(t => t.Time).Select(t => (ScheduleValue)t).SequenceEqual(calc.CalcTimes().OrderBy(t => t.Time)));

			////Delete any old times that aren't there any more
			//for (int i = unprocessedOldTimes.Count - 1; i >= 0; i--) {
			//    var oldTime = unprocessedOldTimes[i];
			//    if (!unprocessedNewTimes.Any(nt => oldTime.Name == nt.Name)) {
			//        Context.DeleteObject(oldTime);
			//        unprocessedOldTimes.RemoveAt(i);
			//    }
			//}

			////Add any new times that weren't there before.
			//foreach (var newTime in unprocessedNewTimes) {
			//    if (!unprocessedOldTimes.Any(ot => newTime.Name == ot.Name))
			//        AddTime(newTime);
			//}

			/////////////////////

			//var oldTimes = Times.ToArray();
			//var newTimes = calc.CalcTimes().ToArray();

			////First, delete all of the added custom times

			//foreach (var time in oldTimes.Where(ot => !newTimes.Any(nt => nt.Name == ot.Name)))
			//    Context.DeleteObject(time);

			////Then, reset all of the changed custom times.
			//foreach (var oldTime in Times) {	//Loop through what remains in Times after deletion
			//    var newTime=newTimes.First(nt=>nt.Name==
			//}

			////Finally, re-add any deleted times
			//AddTimes(newTimes.Where(nt => !oldTimes.Any(ot => nt.Name == ot.Name)));
		}

		#region Times
		public void AddTimes(IEnumerable<ScheduleValue> values) { foreach (var value in values) AddTime(value); }
		public ScheduleTime AddTime(string name, TimeSpan time) { return AddTime(name, time, false); }
		public ScheduleTime AddTime(ScheduleValue value) { return AddTime(value.Name, value.Time, value.IsBold); }
		public ScheduleTime AddTime(string name, TimeSpan time, bool isBold) {
			var retVal = new ScheduleTime(this, name, time, isBold);
			Times.Add(retVal);
			return retVal;
		}
		#endregion

	}
	public sealed partial class ScheduleTime {
		public ScheduleTime() {
			Id = Guid.NewGuid();
			SqlTime = DateTime.Now;	//Bugfix; otherwise, when times are added by the grid, they default to 1/1/0001, which doesn't fit in SQL Server
		}
		public ScheduleTime(ScheduleCell cell, string name, TimeSpan time, bool isBold) : this() { Cell = cell; Name = name; Time = time; IsBold = isBold; }

		///<summary>The time for this value.</summary>
		public TimeSpan Time {
			get { return SqlTime.TimeOfDay; }
			set {
				if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
					throw new ArgumentOutOfRangeException("value");
				SqlTime = Cell.EnglishDate + value;
			}
		}
		partial void OnSqlTimeChanging(DateTime value) {
#if CODE_ANALYSIS
			value.ToString();
#endif
			OnPropertyChanging("Time");
		}

		partial void OnSqlTimeChanged() {
			//If you look at the generated source for the SqlTime setter,
			//you'll see that there's nothing wrong with this. SqlTime is
			//databound to the grid.
			if (Cell != null)
				_SqlTime = SetValidValue(Cell.EnglishDate + _SqlTime.TimeOfDay);
			OnPropertyChanged("Time");
		}

		protected override void OnPropertyChanged(string property) {
			base.OnPropertyChanged(property);
			if (property != "LastModified")
				LastModified = DateTime.Now;
		}

		///<summary>Gets the time for this value in string form.</summary>
		public string TimeString { get { return Time.ToString("h:mm", CultureInfo.CurrentCulture); } }

		public override string ToString() { return Name + ": " + TimeString; }

		public static implicit operator ScheduleValue(ScheduleTime time) { return time == null ? new ScheduleValue() : new ScheduleValue(time.Name, time.Time, time.IsBold); }
	}
	class MemberComparer<T> : IEqualityComparer<T> {
		readonly Func<T, object> memberGetter;

		public MemberComparer(Func<T, object> memberGetter) { this.memberGetter = memberGetter; }

		public bool Equals(T x, T y) { return memberGetter(x).Equals(memberGetter(y)); }
		public int GetHashCode(T obj) { return memberGetter(obj).GetHashCode(); }
	}
}
