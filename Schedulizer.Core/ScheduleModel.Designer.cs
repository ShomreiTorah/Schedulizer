//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Data.Objects.DataClasses.EdmSchemaAttribute()]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("ScheduleModel", "ScheduleTimesForeignKey", "ScheduleDates", global::System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(ShomreiTorah.Schedules.ScheduleCell), "ScheduleTimes", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(ShomreiTorah.Schedules.ScheduleTime))]

// Original file name:
// Generation date: 8/21/2009 6:30:51 PM
namespace ShomreiTorah.Schedules
{
    
    /// <summary>
    /// There are no comments for ScheduleContext in the schema.
    /// </summary>
    public partial class ScheduleContext : global::System.Data.Objects.ObjectContext
    {
        /// <summary>
        /// Initializes a new ScheduleContext object using the connection string found in the 'ScheduleContext' section of the application configuration file.
        /// </summary>
        public ScheduleContext() : 
                base("name=ScheduleContext", "ScheduleContext")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new ScheduleContext object.
        /// </summary>
        public ScheduleContext(string connectionString) : 
                base(connectionString, "ScheduleContext")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new ScheduleContext object.
        /// </summary>
        public ScheduleContext(global::System.Data.EntityClient.EntityConnection connection) : 
                base(connection, "ScheduleContext")
        {
            this.OnContextCreated();
        }
        partial void OnContextCreated();
        /// <summary>
        /// There are no comments for CellSet in the schema.
        /// </summary>
        public global::System.Data.Objects.ObjectQuery<ScheduleCell> CellSet
        {
            get
            {
                if ((this._CellSet == null))
                {
                    this._CellSet = base.CreateQuery<ScheduleCell>("[CellSet]");
                }
                return this._CellSet;
            }
        }
        private global::System.Data.Objects.ObjectQuery<ScheduleCell> _CellSet;
        /// <summary>
        /// There are no comments for TimeSet in the schema.
        /// </summary>
        public global::System.Data.Objects.ObjectQuery<ScheduleTime> TimeSet
        {
            get
            {
                if ((this._TimeSet == null))
                {
                    this._TimeSet = base.CreateQuery<ScheduleTime>("[TimeSet]");
                }
                return this._TimeSet;
            }
        }
        private global::System.Data.Objects.ObjectQuery<ScheduleTime> _TimeSet;
        /// <summary>
        /// There are no comments for CellSet in the schema.
        /// </summary>
        public void AddToCellSet(ScheduleCell scheduleCell)
        {
            base.AddObject("CellSet", scheduleCell);
        }
        /// <summary>
        /// There are no comments for TimeSet in the schema.
        /// </summary>
        public void AddToTimeSet(ScheduleTime scheduleTime)
        {
            base.AddObject("TimeSet", scheduleTime);
        }
    }
    /// <summary>
    /// A cell in the schedule.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="ScheduleModel", Name="ScheduleCell")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ScheduleCell : global::System.Data.Objects.DataClasses.EntityObject
    {
        /// <summary>
        /// Create a new ScheduleCell object.
        /// </summary>
        /// <param name="englishDate">Initial value of EnglishDate.</param>
        /// <param name="title">Initial value of Title.</param>
        public static ScheduleCell CreateScheduleCell(global::System.DateTime englishDate, string title)
        {
            ScheduleCell scheduleCell = new ScheduleCell();
            scheduleCell.EnglishDate = englishDate;
            scheduleCell.Title = title;
            return scheduleCell;
        }
        /// <summary>
        /// There are no comments for Property EnglishDate in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.DateTime EnglishDate
        {
            get
            {
                return this._EnglishDate;
            }
            private set
            {
                this.OnEnglishDateChanging(value);
                this.ReportPropertyChanging("EnglishDate");
                this._EnglishDate = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("EnglishDate");
                this.OnEnglishDateChanged();
            }
        }
        private global::System.DateTime _EnglishDate;
        partial void OnEnglishDateChanging(global::System.DateTime value);
        partial void OnEnglishDateChanged();
        /// <summary>
        /// There are no comments for Property Title in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this.OnTitleChanging(value);
                this.ReportPropertyChanging("Title");
                this._Title = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Title");
                this.OnTitleChanged();
            }
        }
        private string _Title;
        partial void OnTitleChanging(string value);
        partial void OnTitleChanged();
        /// <summary>
        /// There are no comments for Property Id in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        private global::System.Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this.OnIdChanging(value);
                this.ReportPropertyChanging("Id");
                this._Id = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Id");
                this.OnIdChanged();
            }
        }
        private global::System.Guid _Id;
        partial void OnIdChanging(global::System.Guid value);
        partial void OnIdChanged();
        /// <summary>
        /// There are no comments for Times in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("ScheduleModel", "ScheduleTimesForeignKey", "ScheduleTimes")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScheduleTime> Times
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScheduleTime>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleTimes");
            }
            private set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScheduleTime>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleTimes", value);
                }
            }
        }
    }
    /// <summary>
    /// A single time value within a cell.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="ScheduleModel", Name="ScheduleTime")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ScheduleTime : global::System.Data.Objects.DataClasses.EntityObject
    {
        /// <summary>
        /// Create a new ScheduleTime object.
        /// </summary>
        /// <param name="name">Initial value of Name.</param>
        /// <param name="sqlTime">Initial value of SqlTime.</param>
        /// <param name="lastModified">Initial value of LastModified.</param>
        public static ScheduleTime CreateScheduleTime(string name, global::System.DateTime sqlTime, global::System.DateTime lastModified)
        {
            ScheduleTime scheduleTime = new ScheduleTime();
            scheduleTime.Name = name;
            scheduleTime.SqlTime = sqlTime;
            scheduleTime.LastModified = lastModified;
            return scheduleTime;
        }
        /// <summary>
        /// There are no comments for Property Id in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        private global::System.Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this.OnIdChanging(value);
                this.ReportPropertyChanging("Id");
                this._Id = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Id");
                this.OnIdChanged();
            }
        }
        private global::System.Guid _Id;
        partial void OnIdChanging(global::System.Guid value);
        partial void OnIdChanged();
        /// <summary>
        /// There are no comments for Property Name in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.OnNameChanging(value);
                this.ReportPropertyChanging("Name");
                this._Name = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Name");
                this.OnNameChanged();
            }
        }
        private string _Name;
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        /// <summary>
        /// A DateTime containing the time for  this value.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.DateTime SqlTime
        {
            get
            {
                return this._SqlTime;
            }
            set
            {
                this.OnSqlTimeChanging(value);
                this.ReportPropertyChanging("SqlTime");
                this._SqlTime = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("SqlTime");
                this.OnSqlTimeChanged();
            }
        }
        private global::System.DateTime _SqlTime;
        partial void OnSqlTimeChanging(global::System.DateTime value);
        partial void OnSqlTimeChanged();
        /// <summary>
        /// There are no comments for Property IsBold in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBold
        {
            get
            {
                return this._IsBold;
            }
            set
            {
                this.OnIsBoldChanging(value);
                this.ReportPropertyChanging("IsBold");
                this._IsBold = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("IsBold");
                this.OnIsBoldChanged();
            }
        }
        private bool _IsBold;
        partial void OnIsBoldChanging(bool value);
        partial void OnIsBoldChanged();
        /// <summary>
        /// There are no comments for Property LastModified in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.DateTime LastModified
        {
            get
            {
                return this._LastModified;
            }
            private set
            {
                this.OnLastModifiedChanging(value);
                this.ReportPropertyChanging("LastModified");
                this._LastModified = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("LastModified");
                this.OnLastModifiedChanged();
            }
        }
        private global::System.DateTime _LastModified;
        partial void OnLastModifiedChanging(global::System.DateTime value);
        partial void OnLastModifiedChanged();
        /// <summary>
        /// There are no comments for Cell in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("ScheduleModel", "ScheduleTimesForeignKey", "ScheduleDates")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public ScheduleCell Cell
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScheduleCell>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleDates").Value;
            }
            private set
            {
                ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScheduleCell>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleDates").Value = value;
            }
        }
        /// <summary>
        /// There are no comments for Cell in the schema.
        /// </summary>
        [global::System.ComponentModel.BrowsableAttribute(false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityReference<ScheduleCell> CellReference
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScheduleCell>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleDates");
            }
            private set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedReference<ScheduleCell>("ScheduleModel.ScheduleTimesForeignKey", "ScheduleDates", value);
                }
            }
        }
    }
}
