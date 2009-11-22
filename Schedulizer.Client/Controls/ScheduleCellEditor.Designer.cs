namespace ShomreiTorah.Schedules.WinClient.Controls {
	partial class ScheduleCellEditor {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			DevExpress.XtraGrid.StyleFormatCondition styleFormatCondition1 = new DevExpress.XtraGrid.StyleFormatCondition();
			DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			this.isBoldColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.isBoldEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
			this.grid = new DevExpress.XtraGrid.GridControl();
			this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.nameColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.nameEdit = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
			this.timeColumn = new DevExpress.XtraGrid.Columns.GridColumn();
			this.timeEdit = new DevExpress.XtraEditors.Repository.RepositoryItemTimeEdit();
			this.title = new DevExpress.XtraEditors.MemoEdit();
			this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
			this.nullLabel = new DevExpress.XtraEditors.LabelControl();
			this.timeContextMenu = new DevExpress.XtraBars.PopupMenu(this.components);
			this.menuRecalc = new DevExpress.XtraBars.BarButtonItem();
			this.menuDelete = new DevExpress.XtraBars.BarButtonItem();
			this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
			this.menuRecalcTitle = new DevExpress.XtraBars.BarButtonItem();
			((System.ComponentModel.ISupportInitialize)(this.isBoldEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nameEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timeEdit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.title.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
			this.splitContainerControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.timeContextMenu)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
			this.SuspendLayout();
			// 
			// isBoldColumn
			// 
			this.isBoldColumn.Caption = "Bold";
			this.isBoldColumn.ColumnEdit = this.isBoldEdit;
			this.isBoldColumn.FieldName = "IsBold";
			this.isBoldColumn.Name = "isBoldColumn";
			this.isBoldColumn.OptionsColumn.FixedWidth = true;
			this.isBoldColumn.OptionsColumn.ShowInCustomizationForm = false;
			this.isBoldColumn.Visible = true;
			this.isBoldColumn.VisibleIndex = 2;
			this.isBoldColumn.Width = 20;
			// 
			// isBoldEdit
			// 
			this.isBoldEdit.AutoHeight = false;
			this.isBoldEdit.Name = "isBoldEdit";
			// 
			// grid
			// 
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.MainView = this.gridView;
			this.grid.Name = "grid";
			this.grid.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.timeEdit,
            this.nameEdit,
            this.isBoldEdit});
			this.grid.Size = new System.Drawing.Size(385, 518);
			this.grid.TabIndex = 3;
			this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
			this.grid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.grid_MouseClick);
			this.grid.ProcessGridKey += new System.Windows.Forms.KeyEventHandler(this.grid_ProcessGridKey);
			// 
			// gridView
			// 
			this.gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.nameColumn,
            this.timeColumn,
            this.isBoldColumn});
			styleFormatCondition1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			styleFormatCondition1.Appearance.Options.UseFont = true;
			styleFormatCondition1.ApplyToRow = true;
			styleFormatCondition1.Column = this.isBoldColumn;
			styleFormatCondition1.Condition = DevExpress.XtraGrid.FormatConditionEnum.Equal;
			styleFormatCondition1.Value1 = true;
			this.gridView.FormatConditions.AddRange(new DevExpress.XtraGrid.StyleFormatCondition[] {
            styleFormatCondition1});
			this.gridView.GridControl = this.grid;
			this.gridView.Name = "gridView";
			this.gridView.OptionsCustomization.AllowFilter = false;
			this.gridView.OptionsCustomization.AllowGroup = false;
			this.gridView.OptionsCustomization.AllowSort = false;
			this.gridView.OptionsMenu.EnableColumnMenu = false;
			this.gridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
			this.gridView.OptionsView.ShowColumnHeaders = false;
			this.gridView.OptionsView.ShowGroupPanel = false;
			this.gridView.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
			this.gridView.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.timeColumn, DevExpress.Data.ColumnSortOrder.Ascending)});
			this.gridView.VertScrollTipFieldName = "Name";
			// 
			// nameColumn
			// 
			this.nameColumn.Caption = "Title";
			this.nameColumn.ColumnEdit = this.nameEdit;
			this.nameColumn.FieldName = "Name";
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.True;
			this.nameColumn.Visible = true;
			this.nameColumn.VisibleIndex = 0;
			this.nameColumn.Width = 103;
			// 
			// nameEdit
			// 
			this.nameEdit.AutoHeight = false;
			toolTipTitleItem1.Text = "Delete";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Deletes this time.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.nameEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Delete, "", -1, true, true, true, DevExpress.XtraEditors.ImageLocation.MiddleCenter, global::ShomreiTorah.Schedules.WinClient.Properties.Resources.Delete16, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject1, "", null, superToolTip1)});
			this.nameEdit.Name = "nameEdit";
			this.nameEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.timeEdit_ButtonClick);
			// 
			// timeColumn
			// 
			this.timeColumn.AppearanceCell.Options.UseTextOptions = true;
			this.timeColumn.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.timeColumn.Caption = "Time";
			this.timeColumn.ColumnEdit = this.timeEdit;
			this.timeColumn.DisplayFormat.FormatString = "t";
			this.timeColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
			this.timeColumn.FieldName = "SqlTime";
			this.timeColumn.Name = "timeColumn";
			this.timeColumn.OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.False;
			this.timeColumn.OptionsColumn.FixedWidth = true;
			this.timeColumn.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
			this.timeColumn.SortMode = DevExpress.XtraGrid.ColumnSortMode.Value;
			this.timeColumn.Visible = true;
			this.timeColumn.VisibleIndex = 1;
			// 
			// timeEdit
			// 
			this.timeEdit.Appearance.Options.UseTextOptions = true;
			this.timeEdit.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.timeEdit.AutoHeight = false;
			this.timeEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
			this.timeEdit.DisplayFormat.FormatString = "t";
			this.timeEdit.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
			this.timeEdit.EditFormat.FormatString = "t";
			this.timeEdit.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
			this.timeEdit.Mask.EditMask = "t";
			this.timeEdit.Name = "timeEdit";
			this.timeEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.timeEdit_ButtonClick);
			// 
			// title
			// 
			this.title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.title.EditValue = "";
			this.title.Location = new System.Drawing.Point(0, 0);
			this.title.Name = "title";
			this.title.Properties.Appearance.Options.UseTextOptions = true;
			this.title.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.title.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.title.Properties.NullValuePrompt = "Select a date";
			this.title.Properties.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.title.Properties.BeforeShowMenu += new DevExpress.XtraEditors.Controls.BeforeShowMenuEventHandler(this.title_Properties_BeforeShowMenu);
			this.title.Size = new System.Drawing.Size(385, 50);
			this.title.TabIndex = 2;
			this.title.EditValueChanged += new System.EventHandler(this.title_EditValueChanged);
			// 
			// splitContainerControl1
			// 
			this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerControl1.Horizontal = false;
			this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerControl1.Name = "splitContainerControl1";
			this.splitContainerControl1.Panel1.Controls.Add(this.title);
			this.splitContainerControl1.Panel1.MinSize = 50;
			this.splitContainerControl1.Panel1.Text = "Panel1";
			this.splitContainerControl1.Panel2.Controls.Add(this.grid);
			this.splitContainerControl1.Panel2.Text = "Panel2";
			this.splitContainerControl1.Size = new System.Drawing.Size(385, 576);
			this.splitContainerControl1.SplitterPosition = 50;
			this.splitContainerControl1.TabIndex = 4;
			this.splitContainerControl1.Text = "splitContainerControl1";
			// 
			// nullLabel
			// 
			this.nullLabel.Appearance.Options.UseTextOptions = true;
			this.nullLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.nullLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.nullLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nullLabel.Location = new System.Drawing.Point(0, 0);
			this.nullLabel.Name = "nullLabel";
			this.nullLabel.Size = new System.Drawing.Size(385, 576);
			this.nullLabel.TabIndex = 5;
			this.nullLabel.Text = "Please select a date in the calendar";
			// 
			// timeContextMenu
			// 
			this.timeContextMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.menuRecalc),
            new DevExpress.XtraBars.LinkPersistInfo(this.menuDelete)});
			this.timeContextMenu.Manager = this.barManager1;
			this.timeContextMenu.Name = "timeContextMenu";
			// 
			// menuRecalc
			// 
			this.menuRecalc.Caption = "Recalculate";
			this.menuRecalc.Glyph = global::ShomreiTorah.Schedules.WinClient.Properties.Resources.Recalc16;
			this.menuRecalc.Id = 1;
			this.menuRecalc.Name = "menuRecalc";
			this.menuRecalc.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.menuRecalc_ItemClick);
			// 
			// menuDelete
			// 
			this.menuDelete.Caption = "Delete";
			this.menuDelete.Glyph = global::ShomreiTorah.Schedules.WinClient.Properties.Resources.Delete16;
			this.menuDelete.Id = 0;
			this.menuDelete.Name = "menuDelete";
			this.menuDelete.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.menuDelete_ItemClick);
			// 
			// barManager1
			// 
			this.barManager1.DockControls.Add(this.barDockControlTop);
			this.barManager1.DockControls.Add(this.barDockControlBottom);
			this.barManager1.DockControls.Add(this.barDockControlLeft);
			this.barManager1.DockControls.Add(this.barDockControlRight);
			this.barManager1.DockManager = this.dockManager1;
			this.barManager1.Form = this;
			this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.menuDelete,
            this.menuRecalc,
            this.menuRecalcTitle});
			this.barManager1.MaxItemId = 3;
			// 
			// dockManager1
			// 
			this.dockManager1.Form = this;
			this.dockManager1.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl"});
			// 
			// menuRecalcTitle
			// 
			this.menuRecalcTitle.Caption = "&Reset Title";
			this.menuRecalcTitle.Id = 2;
			this.menuRecalcTitle.Name = "menuRecalcTitle";
			// 
			// ScheduleCellEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerControl1);
			this.Controls.Add(this.nullLabel);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.Name = "ScheduleCellEditor";
			this.Size = new System.Drawing.Size(385, 576);
			((System.ComponentModel.ISupportInitialize)(this.isBoldEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nameEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timeEdit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.title.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
			this.splitContainerControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.timeContextMenu)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl grid;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView;
		private DevExpress.XtraGrid.Columns.GridColumn nameColumn;
		private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit nameEdit;
		private DevExpress.XtraGrid.Columns.GridColumn timeColumn;
		private DevExpress.XtraEditors.Repository.RepositoryItemTimeEdit timeEdit;
		private DevExpress.XtraGrid.Columns.GridColumn isBoldColumn;
		private DevExpress.XtraEditors.MemoEdit title;
		private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit isBoldEdit;
		private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
		private DevExpress.XtraEditors.LabelControl nullLabel;
		private DevExpress.XtraBars.PopupMenu timeContextMenu;
		private DevExpress.XtraBars.BarButtonItem menuRecalc;
		private DevExpress.XtraBars.BarButtonItem menuDelete;
		private DevExpress.XtraBars.BarManager barManager1;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		private DevExpress.XtraBars.Docking.DockManager dockManager1;
		private DevExpress.XtraBars.BarButtonItem menuRecalcTitle;
	}
}
