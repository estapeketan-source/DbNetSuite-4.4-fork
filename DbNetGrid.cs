using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Net;
using System.Drawing;
using DbNetLink.Data;
#if (!x64)
using System.Web.UI.DataVisualization.Charting;
#endif


namespace DbNetLink.DbNetSuite
{
    public class DbNetGrid : GridEditControl
    {
        public enum InlineEditToolbarOptions
        {
            Top,
            Bottom,
            TopAndBottom
        }

        public enum InlineEditToolbarButtonAlignment
        {
            Left,
            Right
        }

        public enum FilterColumnModeValues
        {
            Simple,
            Composite,
            Combined
        }
        public enum GridOverflowValues
        {
            Expand,
            Scroll
        }
        public enum BuildModes
        {
            Display,
            Html,
            Word,
            Excel,
            Copy,
            Print,
            DataSource,
            Pdf
        }
        public enum UpdateModes
        {
            Row,
            Page
        }
        public enum MultiRowSelectLocations
        {
            Left,
            Right
        };

        internal BuildModes BuildMode = BuildModes.Display;

        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Adds a Caption element to the top of the grid table.")
        ]
        public string Caption = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the chart button to the toolbar.")
        ]
        public bool Chart = false;

        [
        CategoryAttribute("Output"),
        Description("Supplies configuration information for the chart.")
        ]
        public Dictionary<string, object> ChartConfig;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the Column Picker dialog button to the toolbar.")
        ]
        public bool ColumnPicker = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the runtime configuration button to the toolbar.")
        ]
        public bool Config = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Copy button to the toolbar.")
        ]
        public bool Copy = true;

        [
        CategoryAttribute("Edit"),
        DefaultValue("0px"),
        Description("Sets the height of the Edit Dialog.")
        ]
        public string EditDialogHeight = "";

        [
        CategoryAttribute("Edit"),
        DefaultValue(InlineEditToolbarOptions.Bottom),
        Description("Configures the position of the inline edit toolbars.")
        ]

        public InlineEditToolbarOptions InlineEditToolbarLocation = InlineEditToolbarOptions.Bottom;
        [
        CategoryAttribute("Edit"),
        DefaultValue(InlineEditToolbarButtonAlignment.Right),
        Description("Configures the alignment of the inline edit toolbar buttons.")
        ]
        public InlineEditToolbarButtonAlignment InlineEditToolbarButtonLocation = InlineEditToolbarButtonAlignment.Right;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Export button to the toolbar")
        ]
        public bool Save = true;

        private string _ExportFileName = String.Empty;
        [
        CategoryAttribute("Output"),
        DefaultValue(true),
        Description("The name of the exported file")
        ]
        public string ExportFileName
        {
            set { _ExportFileName = value; }
            get { return (_ExportFileName == String.Empty ? this.Id : _ExportFileName); }
        }

        private string _ExportFolder = String.Empty;
        [
        CategoryAttribute("Output"),
        DefaultValue(true),
        Description("The name of the web folder into which the exported file is saved")
        ]
        public string ExportFolder
        {
            set { _ExportFolder = value; }
            get { return _ExportFolder; }
        }

        [
        CategoryAttribute("Totals"),
        DefaultValue("Final Totals"),
        Description("Label applied to the final totals row for column aggregation")
        ]
        public string FinalTotalLabel = "";

        [
        CategoryAttribute("Functionality"),
        DefaultValue(FilterColumnModeValues.Simple),
        Description("Defines how filter columns operate. Has no effect unless at least one column has its Filter property set to true.")
        ]
        public FilterColumnModeValues FilterColumnMode = FilterColumnModeValues.Simple;

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Applies an order by to the data that cannot be modified or removed from the client. FixedOrderBy will always be applied before any additional client ordering.")
        ]
        public string FixedOrderBy = "";

        [
        CategoryAttribute("Display"),
        DefaultValue(0),
        Description("Freezes the first n columns against which the remaining columns can be scrolled horizontally.")
        ]
        public int FrozenColumns = 0;

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Specifies the 'group by' having clause.")
        ]
        public string Having = "";

        [
        CategoryAttribute("Layout"),
        DefaultValue(GridOverflowValues.Expand),
        Description("Defines what will happen when a grid cannot fit within a supplied height or width. Has no effect unless a value is supplied for either height or width.")
        ]
        public GridOverflowValues GridOverflow = GridOverflowValues.Expand;

        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Groups the values in all the non-aggregated columns.")
        ]
        public bool GroupBy = false;

        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Include hidden columns when grouping selected rows.")
        ]
        public bool GroupByHiddenColumns = false;

        [
        CategoryAttribute("Layout"),
        DefaultValue(""),
        Description("Sets the approximate height of the grid.")
        ]
        public string Height = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds a mail-merge option to the toolbar")
        ]
        public bool MailMerge = false;

        internal object MailMergeDocument;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Specifies defined Word documents to be merged with the data source")
        ]
        public Dictionary<string, object> MailMergeDocuments;

        [
        CategoryAttribute("Behavior"),
        DefaultValue(false),
        Description("Adds checkboxes to the right edge of the grid which allows multiple rows to be selected. The Delete operation is applied to all selected rows.")
        ]
        public bool MultiRowSelect = false;

        [
        CategoryAttribute("Behavior"),
        DefaultValue(MultiRowSelectLocations.Right),
        Description("Defines the location of the multi-row selection checkboxes.")
        ]
        public MultiRowSelectLocations MultiRowSelectLocation = MultiRowSelectLocations.Right;

        [
        CategoryAttribute("Sorting"),
        DefaultValue(false),
        Description("Suppresses any sorting of the data.")
        ]
        public bool NoSort = false;

        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Optimizes the data selection algorithm for very large datasets")
        ]
        public bool OptimizeForLargeDataSet = false;

        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Optimizes the grid export for very large datasets")
        ]
        public bool OptimizeExportForLargeDataSet = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the Output Current Page Only option to the toolbar. Has no effect unless Export is set to true.")
        ]
        public bool OutputPageSelect = false;

        [
        CategoryAttribute("Output"),
        DefaultValue(0),
        Description("Splits output into pages of specified size ff set to a value greater than 0.")
        ]
        public int OutputPageSize = 0;

        [
        CategoryAttribute("Layout"),
        DefaultValue(20),
        Description("Defines the number of rows that will be displayed by the grid on each page.")
        ]
        public int PageSize = 20;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Print button to the toolbar.")
        ]
        public bool Print = true;

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("To use a stored procedure instead of a table or view provide the name of the procedure, this will override the FromPart property.")
        ]
        public string ProcedureName = "";

        [
        CategoryAttribute("Database"),
        Description("A collection of parameters to be used when working with a stored procedure.")
        ]
        public Dictionary<string, object> ProcedureParameters = new Dictionary<string, object>();

        [
        Category("Toolbar"),
        DefaultValue(ToolbarOptions.Top),
        Description("Controls the location of the toolbar")
        ]
        public ToolbarOptions ToolbarLocation = ToolbarOptions.Top;

        [
        CategoryAttribute("Edit"),
        DefaultValue(UpdateModes.Row),
        Description("Determines the mode of record editing for the grid")
        ]
        public UpdateModes UpdateMode = UpdateModes.Row;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Update button to the toolbar.")
        ]
        public bool UpdateRow = true;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Opens the view dialog for the currently selected row.")
        ]
        public bool View = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds a print button to the view dialog")
        ]
        public bool ViewPrint = true;

        [
        CategoryAttribute("Layout"),
        DefaultValue("0px"),
        Description("Sets the width of the grid.")
        ]
        public string Width = "";

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Modfies the select statement e.g. top 100 or distinct")
        ]
        public string SelectModifier = "";

        [
        CategoryAttribute("Layout"),
        DefaultValue(""),
        Description("Sets the height of the View Dialog.")
        ]
        public string ViewDialogHeight = "";

        [
        CategoryAttribute("Layout"),
        DefaultValue(""),
        Description("Sets the width of the View Dialog.")
        ]
        public string ViewDialogWidth = "";

        [
        CategoryAttribute("Layout"),
        DefaultValue(1),
        Description("The number of columns in the default view dialog layout")
        ]
        public int ViewLayoutColumns = 1;

        const string NullValueToken = "@@null@@";

        internal string SaveOptions = "HTML,Word,Excel,XML,CSV,PDF";
        internal bool OutputCurrentPage = false;
        internal bool PageUpdate = false;
        internal bool NestedGrid = false;
        internal int InsertedRecordIndex = -1;
        internal int TotalRows = -1;
        internal int TotalPages = -1;
        internal ArrayList ColumnFilterSql = new ArrayList();
        internal Dictionary<string, object> ColumnFilterParams = new Dictionary<string, object>();
        internal string SaveType = "";
        internal string MailMergeRequestKey = "MailMergeRequest";

        internal SortedList TotalColumnCurrentValues = new SortedList();
        internal SortedList SavedColumnCurrentValues = new SortedList();
        internal SortedList HeaderColumnCurrentValue = new SortedList();
        internal ArrayList TotalBreakRowCollection = new ArrayList();

        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            if (!Req.ContainsKey("method"))
                return;

            if (Req["method"].ToString() == "build-html-export-grid")
                if (this.SaveType == "datasource")
                {
                    Req = (Dictionary<string, object>)this.Context.Application[this.MailMergeRequestKey];
                    this.DeserialiseRequest();
                    this.Context.Application.Remove(this.MailMergeRequestKey);
                    Req["method"] = "build-html-export-grid";
                    this.SaveType = "datasource";
                }

            switch (Req["method"].ToString())
            {
                case "load-data":
                case "build-html-export-grid":
                case "export-grid":
                case "build-chart":
                case "export-chart":
                case "get-single-row":
                case "validate-batch-update":
                case "batch-update":
                case "column-filter":
                case "get-data-array":
                case "view-record":
                case "save-grid":
                    this.OpenConnection();
                    break;
            }

            switch (Req["method"].ToString())
            {
                case "load-data":
                    try
                    {
                        ConfigureColumns();
                        if (this.CurrentPage == -1)
                        {
                            BuildToolbar();
                            if (SearchPanelId != "")
                                Resp["searchPanel"] = BuildSearchPanel();

                            Resp["dragIcon"] = BuildDragIcon();

                            this.CurrentPage = 1;
                        }
                        Resp["grid"] = BuildGrid();
                        ClientProperties["columns"] = SerialiseColumns();
                    }
                    catch (Exception ex)
                    {
                        Resp["errorMessage"] = ex.Message;
                        Resp["stackTrace"] = ex.StackTrace;
                        Resp["source"] = ex.Source;
                        Resp["commandInfo"] = Database.CommandErrorInfo();
                        break;
                    }
                    break;
                case "build-html-export-grid":
                    Resp["html"] = BuildHtmlExportGrid().ToString();
                    break;
                case "get-data-array":
                    GetDataArray();
                    break;
                case "grid-css":
                    Resp["css"] = GridCSS();
                    break;
                case "view-css":
                    Resp["css"] = ViewCSS();
                    break;
                case "export-grid":
                    ExportGrid();
                    break;
                case "save-grid":
                    SaveGrid();
                    break;
                case "export-chart":
                    ExportChart();
                    break;
                case "build-chart":
#if (!x64)
                    BuildChart();
#endif
                    break;
                case "stream-chart":
                    StreamChart();
                    break;
                case "store-config":
                    this.Context.Application[this.MailMergeRequestKey] = Req;
                    break;
                case "validate-filter-params":
                    ValidateColumnFilterParameters();
                    break;
                case "edit-dialog":
                    Resp["html"] = BuildEditDialog();
                    break;
                case "get-single-row":
                    GetSingleRow();
                    break;
                case "validate-batch-update":
                    ValidateBatchUpdate();
                    break;
                case "batch-update":
                    BatchUpdate();
                    break;
                case "pdf-settings-dialog":
                    Resp["html"] = BuildPdfSettingsDialog();
                    break;
                case "config-dialog":
                    Resp["html"] = BuildConfigDialog();
                    break;
                case "column-picker-dialog":
                    Resp["html"] = BuildColumnPickerDialog();
                    break;
                case "chart-config-dialog":
                    Resp["html"] = BuildChartConfigDialog();
                    break;
                case "chart-dialog":
                    Resp["html"] = BuildChartDialog();
                    break;
                case "column-filter":
                    BuildColumnFilterSql();
                    break;
                case "validate-upload":
                    ValidateUpload();
                    break;
                case "upload":
                case "ajax-upload":
                    Upload();
                    break;
                case "view-dialog":
                    Resp["html"] = BuildViewDialog();
                    break;
                case "view-record":
                    ViewRecord();
                    break;
                case "image-data":
                    ImageData();
                    break;
            }

            this.CloseConnection();

            switch (Req["method"].ToString())
            {
                case "export-grid":
                case "export-chart":
                case "thumbnail":
                case "stream-chart":
                case "upload":
                    break;
                default:
                    context.Response.Write(JSON.Serialize(Resp));
                    break;
            }
        }

        ///////////////////////////////////////////////
        internal void BuildOutputOptions(TableRow ParentRow)
        ///////////////////////////////////////////////
        {
            TableCell C;

            if (this.Save)
            {
                AddToolButton(ParentRow, "save", "save", "SaveTheGridInTheSelectedFormat");

                C = new TableCell();
                ParentRow.Controls.Add(C);
                DropDownList ET = new DropDownList();
                C.Controls.Add(ET);
                ET.ID = AssignID("saveType");

                foreach (string Option in this.SaveOptions.Split(','))
                {
#if (x64)
                    if (Option.ToLower() == "pdf")
                        continue;
#endif
                    ET.Items.Add(new System.Web.UI.WebControls.ListItem(Option, Option.ToLower()));
                }

                C = new TableCell();
                ParentRow.Controls.Add(C);
                C.Text = "&nbsp;";
            }
            if (this.Chart)
            {
                Assembly DV = Assembly.LoadWithPartialName("System.Web.DataVisualization");
                if (DV != null)
                    AddToolButton(ParentRow, "chart", "Chart", "ChartGrid");
            }
            if (this.Print)
                AddToolButton(ParentRow, "print", "printer", "PrintGrid");

            if (this.Copy)
            {
                switch (this.GetBrowser())
                {
                    case "msie":
                    case "edge":
                        AddToolButton(ParentRow, "copy", "copy", "CopyGridToClipboard");
                        break;
                    default:
                        //         AddToolButton(ParentRow, "xcopy", "copy", "CopyGridToClipboard");
                        break;
                }
            }

            C = new TableCell();
            ParentRow.Controls.Add(C);
            HtmlGenericControl F = new HtmlGenericControl("iframe");
            C.Controls.Add(F);
            F.ID = AssignID("outputFrame");
            F.Attributes.Add("name", AssignID("outputFrame"));
            F.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            F.Style.Add(HtmlTextWriterStyle.Top, "-1000px");
            F.Style.Add(HtmlTextWriterStyle.Left, "-1000px");
            this.MakeIframe508Compliant(F, "Empty");

            if (this.OutputPageSelect)
            {
                C = new TableCell();
                ParentRow.Controls.Add(C);
                System.Web.UI.WebControls.Table T = new System.Web.UI.WebControls.Table();
                T.CellPadding = 0;
                T.CellSpacing = 0;
                C.Controls.Add(T);
                TableRow TR = new TableRow();
                T.Controls.Add(TR);
                TableCell TC = new TableCell();
                TR.Controls.Add(TC);
                HtmlInputCheckBox CB = new HtmlInputCheckBox();
                TC.Controls.Add(CB);

                CB.ID = this.AssignID("outputPageSelect");
                TC = new TableCell();
                TC.Text = Translate("OutputCurrentPageOnly");
                TR.Controls.Add(TC);
            }
            if (this.MailMerge && this.GetBrowser().Equals("msie"))
            {
                C = new TableCell();
                ParentRow.Controls.Add(C);
                C.Text = "&nbsp;";

                AddToolButton(ParentRow, "mailMerge", "MailMerge", "MergeData");

                if (this.MailMergeDocuments is Dictionary<string, object>)
                {
                    Dictionary<string, object> Options = this.MailMergeDocuments as Dictionary<string, object>;
                    if (Options.Count > 0)
                    {
                        C = new TableCell();
                        ParentRow.Controls.Add(C);
                        DropDownList ET = new DropDownList();
                        C.Controls.Add(ET);
                        ET.ID = AssignID("mailMergeDocument");

                        foreach (string Key in Options.Keys)
                        {
                            System.Web.UI.WebControls.ListItem LI = new System.Web.UI.WebControls.ListItem(Key, Options[Key].ToString());
                            ET.Items.Add(LI);
                        }

                        if (Options.Count == 1)
                            ET.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                }
            }
        }

        ///////////////////////////////////////////////
        internal string BuildGrid()
        ///////////////////////////////////////////////
        {
            System.Web.UI.WebControls.Table T = BuildGridTable();
            if (this.OutputPageSize > 0)
            {
                switch (this.BuildMode)
                {
                    case BuildModes.Html:
                    case BuildModes.Excel:
                    case BuildModes.Word:
                    case BuildModes.Print:
                    case BuildModes.Pdf:
                        return PagedTable(T);
                }
            }

            return RenderControlToString(T);
        }

        ///////////////////////////////////////////////
        internal string PagedTable(System.Web.UI.WebControls.Table T)
        ///////////////////////////////////////////////
        {
            HtmlGenericControl D = new HtmlGenericControl("div");

            System.Web.UI.WebControls.Table Page = new System.Web.UI.WebControls.Table();
            Page.CssClass = T.CssClass;

            while (T.Rows.Count > 1)
            {
                if (Page.Rows.Count == 0)
                    Page.Rows.Add(CopyTableRow(T.Rows[0]));

                if (Page.Rows.Count <= this.OutputPageSize)
                {
                    Page.Rows.Add(T.Rows[1]);
                }
                else
                {
                    if (D.Controls.Count > 0)
                    {
                        HtmlGenericControl P = new HtmlGenericControl("p");
                        P.Style.Add("page-break-before", "always");
                        D.Controls.Add(P);
                    }
                    D.Controls.Add(Page);
                    Page = new System.Web.UI.WebControls.Table();
                    Page.CssClass = T.CssClass;
                }
            }

            if (Page.Rows.Count > 1)
            {
                if (D.Controls.Count > 0)
                {
                    HtmlGenericControl P = new HtmlGenericControl("p");
                    P.Style.Add("page-break-before", "always");
                    D.Controls.Add(P);
                }
                D.Controls.Add(Page);
            }
            return RenderControlToString(D);
        }

        ///////////////////////////////////////////////
        internal TableRow CopyTableRow(TableRow Row)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();

            R.CssClass = Row.CssClass;
            foreach (TableCell Cell in Row.Cells)
            {
                TableCell C = new TableCell();

                if (Cell is TableHeaderCell)
                {
                    C = new TableHeaderCell();
                }

                C.CssClass = Cell.CssClass;
                C.Text = Cell.Text;


                foreach (string key in Cell.Attributes.Keys)
                {
                   C.Attributes.Add(key, Cell.Attributes[key]);
                }

                R.Cells.Add(C);
            }

            return R;
        }

        ///////////////////////////////////////////////
        internal string BuildDragIcon()
        ///////////////////////////////////////////////
        {
            System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
            I.ID = this.AssignID("dragInsertionImg");
            I.ImageUrl = GetImageUrl("GreenDown.png");
            I.CssClass = "drag-insertion-icon";
            I.Style.Add(HtmlTextWriterStyle.Display, "none");
            I.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            I.Style.Add(HtmlTextWriterStyle.ZIndex, Int32.MaxValue.ToString());
            return RenderControlToString(I);
        }

        ///////////////////////////////////////////////
        internal System.Web.UI.WebControls.Table BuildGridTable()
        ///////////////////////////////////////////////
        {
            if (this.OrderBy == "")
                if (this.FixedOrderBy == "" && ((this.TotalBreakColumns().Count + this.ColumnList(ColumnPropertyNames.GroupHeader.ToString()).Count + this.ColumnList(ColumnPropertyNames.ClearDuplicateValue.ToString()).Count) == 0))
                    this.OrderBy = DefaultOrderBy();

            System.Web.UI.WebControls.Table GT = new System.Web.UI.WebControls.Table();
            GT.ID = AssignID("datatable");
            GT.Caption = this.Caption;
            GT.CssClass = "dbnetgrid";

            if (this.GetTheme() == UI.Themes.Bootstrap)
                GT.CssClass += " table-condensed";

            GT.Attributes.Add("buildMode", this.BuildMode.ToString().ToLower());

            int firstRecord = 0;
            int lastRecord = 0;
            int totalRows = 0;

            GetLookupTables();

            AddInlineEditToolbar(GT);
            GT.Rows.Add(BuildHeaderRow());

            foreach (GridColumn column in Columns)
            {
                if (column.Filter && this.BuildMode == BuildModes.Display)
                {
                    GT.Rows.Add(BuildFilterRow());
                    break;
                }
            }

            QueryCommandConfig Query;

            if (this.GroupBy)
                this.OptimizeForLargeDataSet = false;

            if (!UniversalEmptyFilter())
            {
                if (BuildMode == BuildModes.Display && this.OptimizeForLargeDataSet && this.ProcedureName == "")
                {
                    Query = BuildSQL(QueryBuildModes.Count);

                    Database.ExecuteSingletonQuery(Query);
                    /*
                    try
                    {
                        Database.ExecuteSingletonQuery(Query);
                    }
                    catch (Exception Ex)
                    {
                        ThrowException(Ex.Message, Database.CommandErrorInfo());
                    }
                    */

                    totalRows = Convert.ToInt32(Database.ReaderValue(0));
                }
            }

            Query = BuildSQL();

            if (Req.ContainsKey("primaryKey"))
                SelectPageForInsertedRecord(Query);

            if (this.CurrentPage < 1)
                this.CurrentPage = 1;

            if (Navigation == false || ToolbarLocation == ToolbarOptions.Hidden)
                this.PageSize = Int32.MaxValue;

            firstRecord = (PageSize * (this.CurrentPage - 1)) + 1;
            lastRecord = (firstRecord + (PageSize - 1));

            if (this.BuildMode != BuildModes.Display && !this.OutputCurrentPage)
            {
                firstRecord = 1;
                lastRecord = Int32.MaxValue;
            }

            int recordCounter = 0;
            int rowCounter = 0;

            if (BuildMode == BuildModes.Display && this.OptimizeForLargeDataSet && Database.Database == DatabaseType.SqlServer)
            {
                lastRecord++;
                BuildOptimisedPageSQL(Query, firstRecord, lastRecord);
                recordCounter = firstRecord;
            }

            foreach (GridColumn GC in this.Columns)
                if (GC.TotalBreak && (GC.Display || GC.GroupHeader))
                    this.TotalColumnCurrentValues.Add(GC.ColumnName, null);

            foreach (GridColumn GC in this.Columns)
                if (GC.ClearDuplicateValue || GC.GroupHeader)
                    this.SavedColumnCurrentValues.Add(GC.ColumnName, null);


            if (!UniversalEmptyFilter())
            {
                Database.ExecuteQuery(Query);

                while (Database.Reader.Read())
                {
                    recordCounter++;
                    if (recordCounter >= firstRecord && recordCounter <= lastRecord)
                    {
                        if (TotalColumnCurrentValues.Count > 0)
                            AddTotalRows(GT, false);
                        else
                            CheckForChangedValues();

                        AddGroupHeaderRows(GT);

                        TableRow GridRow = BuildDataRow(rowCounter);
                        GT.Rows.Add(GridRow);
                        rowCounter++;

                        if (recordCounter == this.InsertedRecordIndex)
                            GridRow.Attributes.Add("insertedRecord", "true");
                    }

                    if (totalRows > 0 && recordCounter > lastRecord)
                    {
                        recordCounter = totalRows;
                        break;
                    }

                    if (this.ProcedureName != "")
                    {
                        if (this.AggregateColumns().Count > 0)
                        {
                            foreach (GridColumn C in this.AggregateColumns())
                            {
                                int Ord = Database.Reader.GetOrdinal(C.ColumnName);
                                if (!Database.Reader.IsDBNull(Ord))
                                {
                                    if (C.AggregateValue == null)
                                        C.AggregateValue = 0;

                                    C.AggregateValue = Convert.ToDouble(C.AggregateValue) + Convert.ToDouble(Database.Reader.GetValue(Ord));

                                }
                            }
                        }
                    }
                }
            }

            if (this.BuildMode == BuildModes.Display)
            {
                foreach (TableRow Row in GT.Rows)
                {
                    ListDictionary AuditInfo = null;
                    if (Audit != AuditModes.None || this.AuditColumns().Count > 0)
                    {
                        if (Row.Attributes["dataRowIndex"] != null)
                        {
                            Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)this.PrimaryKeyList[Convert.ToInt32(Row.Attributes["dataRowIndex"])];
                            AuditInfo = AuditData(FromPart, PrimaryKey);
                            if (Audit != AuditModes.None)
                                AssignAuditInfo(Row, AuditInfo);
                        }
                    }

                    foreach (TableCell Cell in Row.Cells)
                        if (Cell.Controls.Count > 0)
                            if (Cell.Controls[0] is EditField)
                            {
                                EditField EF = (Cell.Controls[0] as EditField);
                                if (EF.InputControl is TextBox)
                                    if (
                                        EF.Column.DataType == "String" ||
                                        EF.Column.EditControlType == EditField.ControlType.SuggestLookup ||
                                        EF.Column.EditControlType == EditField.ControlType.AutoCompleteLookup ||
                                        EF.Column.EditControlType == EditField.ControlType.TextBox ||
                                        EF.Column.EditControlType == EditField.ControlType.TextBoxSearchLookup)
                                        EF.InputControl.Attributes["size"] = EF.Column.MaxTextLength.ToString();

                                if (AuditInfo != null)
                                    EF.AssignAuditInfo(AuditInfo);

                            }


                }
            }

            if (totalRows > 0)
                this.TotalRows = totalRows;
            else
                this.TotalRows = recordCounter;

            double pages = (double)recordCounter / (double)PageSize;
            this.TotalPages = (int)Math.Ceiling(pages);
            if (recordCounter == 0)
                this.CurrentPage = 0;

            ClientProperties["totalRows"] = this.TotalRows;
            ClientProperties["orderBy"] = this.OrderBy;
            ClientProperties["totalPages"] = this.TotalPages;
            ClientProperties["currentPage"] = this.CurrentPage;
            ClientProperties["primaryKeyList"] = this.PrimaryKeyList;

            if (this.ProcedureName == "")
                ClientProperties["query"] = Query;

            if (rowCounter > 0)
            {
                AddTotalRows(GT, true);
                AddInlineEditToolbar(GT);
            }

            AssignAggregateValues();

            return GT;
        }


        ///////////////////////////////////////////////
        private void CheckForChangedValues()
        ///////////////////////////////////////////////
        {
            foreach (GridColumn C in this.Columns)
                if (C.ClearDuplicateValue || C.GroupHeader)
                    if (this.SavedColumnCurrentValues[C.ColumnName] == null)
                        this.SavedColumnCurrentValues[C.ColumnName] = Database.ReaderValue(C.ColumnName).ToString();

            foreach (GridColumn C in this.Columns)
            {
                if (C.ClearDuplicateValue || C.GroupHeader)
                {
                    string DbValue = Database.ReaderValue(C.ColumnName).ToString();

                    if (this.SavedColumnCurrentValues[C.ColumnName].ToString() != DbValue)
                    {
                        SetColumnPrintFlags(C);
                        this.SavedColumnCurrentValues[C.ColumnName] = DbValue;
                    }
                }
            }
        }

        ///////////////////////////////////////////////
        private void SelectPageForInsertedRecord(QueryCommandConfig sql)
        ///////////////////////////////////////////////
        {
            Database.ExecuteQuery(sql);

            int recordCounter = 0;

            while (Database.Reader.Read())
            {
                recordCounter++;

                if (this.IsMatchingPrimaryKey((Dictionary<string, object>)Req["primaryKey"]))
                {
                    this.CurrentPage = ((int)Math.Ceiling((double)recordCounter / (double)PageSize));
                    this.InsertedRecordIndex = recordCounter;
                    break;
                }
            }

            Database.Reader.Close();
        }

        ///////////////////////////////////////////////
        public void GetSingleRow()
        ///////////////////////////////////////////////
        {
            GetLookupTables();

            QueryCommandConfig Query = new QueryCommandConfig();

            Query.Sql = "select " + BuildSelectPart(QueryBuildModes.Normal) + " from " + FromPart;
            AddPrimaryKeyFilter(Query, (Dictionary<string, object>)Req["primaryKey"]);

            try
            {
                Database.ExecuteQuery(Query);
            }
            catch (Exception ex)
            {
                ThrowException(ex.Message, Database.CommandErrorInfo());
            }

            if (Database.Reader.Read())
            {
                Control row = BuildDataRow(Convert.ToInt32(Req["rowIndex"]));

                Resp["ok"] = true;
                Resp.Add("html", Shared.RenderControlToString(row));

                if (this.AggregateColumns().Count > 0)
                {
                    row = BuildAggregateRow(null);
                    AssignAggregateValues();
                    Resp.Add("totalsHtml", Shared.RenderControlToString(row));
                }
            }
            else
            {
                Resp.Add("ok", false);
            }
        }


        ///////////////////////////////////////////////
        public void ViewRecord()
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Query = new QueryCommandConfig();

            Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)Req["primaryKey"];
            Query.Sql = "select " + BuildSelectPart(QueryBuildModes.View) + " from " + FromPart;
            AddPrimaryKeyFilter(Query, PrimaryKey);

            Dictionary<string, object> Data = new Dictionary<string, object>();

            try
            {
                if (Database.ExecuteSingletonQuery(Query))
                {
                    foreach (GridColumn C in Columns)
                    {
                        string Key = C.ColumnName.ToLower();
                        object Value = Database.ReaderValue(C.ColumnName);
                        string FileName = String.Empty;

                        if (C.EditControlType == EditField.ControlType.Upload || C.DataType == "Byte[]")
                        {
                            Byte[] Bytes;

                            if (Value is Byte[])
                                Bytes = (Byte[])Value;
                            else
                                Bytes = GetFileData(C, Value.ToString());

                            if (C.UploadFileNameColumn != String.Empty)
                                FileName = Database.Reader[C.UploadFileNameColumn].ToString();

                            System.Web.UI.WebControls.Image TI;

                            if (DataUriSupported())
                                TI = new DataUriImage(this, Bytes, C, FileName, false);
                            else
                                TI = new ThumbnailImage(this, C, PrimaryKey, Bytes, "");

                            Data[Key] = TI.ImageUrl.Replace("method=thumbnail", "method=stream");
                        }
                        else if (C.Lookup != "")
                            Data[Key] = LookupValue(C, Value);
                        else
                            Data[Key] = FormattedValue(Value, C);
                    }

                    Resp["record"] = Data;
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex.Message, Database.CommandErrorInfo());
            }

        }


        ///////////////////////////////////////////////
        internal string BuildEditDialog()
        ///////////////////////////////////////////////
        {
            EditDialog ED = new EditDialog(this);
            return RenderControlToString(ED.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildViewDialog()
        ///////////////////////////////////////////////
        {
            ViewDialog D = new ViewDialog(this);
            D.TemplatePath = Req["templatePath"].ToString();
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        private TableHeaderRow BuildHeaderRow()
        ///////////////////////////////////////////////
        {
            TableHeaderRow HR = new TableHeaderRow();
            HR.TableSection = TableRowSection.TableHeader;

            HR.CssClass = "header-row";

            ConvertOrderByToOrdinal();

            foreach (GridColumn column in Columns)
            {
                if (!column.Display && this.BuildMode != BuildModes.DataSource)
                    continue;

                if (!column.GridData)
                    continue;

                TableHeaderCell HC = new TableHeaderCell();
                HR.Controls.Add(HC);

                if (this.BuildMode == BuildModes.DataSource)
                {
                    HC.Text = column.Label;
                    continue;
                }

                HC.CssClass = "header-cell";
                HC.Attributes.Add("columnIndex", column.ColumnIndex.ToString());
                HC.Attributes.Add("columnKey", column.ColumnKey);
                HC.Attributes.Add("columnName", column.ColumnName);
                HC.Attributes.Add("sortable", IsColumnSortable(column));
                HC.Attributes.Add("dbDataType", column.DbDataType);
                //HC.Style.Value = column.Style;

                if (column.TotalBreak)
                    HC.Attributes.Add("totalBreak", "true");

                if (column.Width != "")
                    HC.Width = new Unit(column.Width);

                if (this.BuildMode != BuildModes.Display)
                {
                    HC.Text = column.Label;
                    continue;
                }

                System.Web.UI.WebControls.Table T = new System.Web.UI.WebControls.Table();
                T.CssClass = "header-cell-table";
                T.Width = new Unit("100%");
                HC.Controls.Add(T);
                T.CellPadding = 0;
                T.CellSpacing = 0;

                TableRow R = new TableRow();
                //              R.TableSection = TableRowSection.TableHeader;
                T.Controls.Add(R);

                TableCell C = new TableCell();
                R.Controls.Add(C);
                C.Text = column.Label + "&nbsp;&nbsp;";
                C.Width = Unit.Percentage(100);

                C = new TableCell();
                R.Controls.Add(C);

                if (this.BuildMode == BuildModes.Display && this.NoSort == false)
                {
                    Match M = Regex.Match(this.OrderBy, "^" + (column.ColumnIndex + 1).ToString() + " (asc|desc)");

                    if (M.Success)
                    {
                        C.CssClass = M.Groups[1].Value.ToLower() + "-sort-sequence-image";
                        C.Text = "&nbsp;";
                    }
                }

            }

            AddNestedSelectCell(HR);
            AddAuditCells(HR);

            return HR;
        }


        ///////////////////////////////////////////////
        private string IsColumnSortable(GridColumn C)
        ///////////////////////////////////////////////
        {
            string Sortable = "true";

            switch (Database.Database)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServerCE:
                    switch (C.DbDataType.ToLower())
                    {
                        case "image":
                        case "ntext":
                        case "text":
                        case "geography":
                        case "geometry":
                            Sortable = "false";
                            break;
                        case "xml":
                        case "25":
                            Sortable = (C.XmlElementName != String.Empty).ToString().ToLower();
                            break;
                    }
                    break;
            }

            return Sortable;
        }

        ///////////////////////////////////////////////
        private void ConvertOrderByToOrdinal()
        ///////////////////////////////////////////////
        {
            string[] OrderByParts = this.OrderBy.Split(',');
            List<string> OrderByArray = new List<string>();

            foreach (string OrderByPart in OrderByParts)
            {
                string S = OrderByPart.Trim();
                string OB = S.Split(' ')[0].ToLower();
                foreach (GridColumn column in Columns)
                {
                    if (!column.Display)
                        continue;

                    string CE = column.ColumnExpression.ToLower();

                    if (OB.IndexOf(".") == -1)
                        CE = CE.Split('.')[CE.Split('.').Length - 1];

                    if (OB == CE)
                    {
                        S = (column.ColumnIndex + 1).ToString() + (OrderByPart.ToLower().EndsWith(" desc") ? " desc" : " asc");
                        break;
                    }
                }

                OrderByArray.Add(S);
            }

            this.OrderBy = String.Join(",", OrderByArray.ToArray());
        }

        ///////////////////////////////////////////////
        private void AddNestedSelectCell(TableRow R)
        ///////////////////////////////////////////////
        {
            if (this.BuildMode != BuildModes.Display)
                return;

            string ClassName = R.Cells[0].CssClass;
            TableCell C;

            if (MultiRowSelect)
            {
                if (R is TableHeaderRow)
                    C = new TableHeaderCell();
                else
                    C = new TableCell();

                if (MultiRowSelectLocation == MultiRowSelectLocations.Right)
                    R.Controls.Add(C);
                else
                    R.Controls.AddAt(0, C);

                C.CssClass = ClassName;
                C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

                switch (ClassName)
                {
                    case "data-cell":
                    case "header-cell":
                        HtmlInputCheckBox CB = new HtmlInputCheckBox();
                        C.Controls.Add(CB);
                        if (R is TableHeaderRow)
                            CB.ID = "selectAllCheckBox";
                        else
                            CB.Attributes.Add("class", "multi-select-checkbox");
                        break;
                }
            }

            if (this.NestedGrid)
            {
                if (R is TableHeaderRow)
                    C = new TableHeaderCell();
                else
                    C = new TableCell();
                R.Controls.AddAt(0, C);
                C.CssClass = ClassName;

                if (C.CssClass == "data-cell")
                {
                    System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
                    C.Controls.Add(I);
                    I.CssClass = "expand-button";

                    if (this.GetTheme() == UI.Themes.Bootstrap)
                        I.ImageUrl = GetImageUrl("bootstrap.expand.png");
                    else
                        I.ImageUrl = GetImageUrl("Expand.gif");

                    I = new System.Web.UI.WebControls.Image();
                    C.Controls.Add(I);
                    I.CssClass = "collapse-button";
                    if (this.GetTheme() == UI.Themes.Bootstrap)
                        I.ImageUrl = GetImageUrl("bootstrap.collapse.png");
                    else
                        I.ImageUrl = GetImageUrl("Collapse.gif");

                    I.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
            }
        }

        ///////////////////////////////////////////////
        private void AssignAuditInfo(TableRow R, ListDictionary AuditInfo)
        ///////////////////////////////////////////////
        {
            string[] Columns = { "created", "updated" };
            string[] Suffixes = { "", "_by" };

            foreach (string Column in Columns)
            {
                if (!AuditInfo.Contains("_row_" + Column))
                    continue;

                ListDictionary AuditData = (ListDictionary)AuditInfo["_row_" + Column];

                foreach (string Suffix in Suffixes)
                {
                    if (!AuditData.Contains("updated" + Suffix))
                        continue;

                    if (AuditData["updated" + Suffix].ToString() == "")
                        continue;

                    TableCell C;

                    if (Column == "updated")
                        C = R.Cells[R.Cells.Count - 1];
                    else
                        C = R.Cells[R.Cells.Count - 2];

                    string ClassName = Column + Suffix.Replace("_", "-") + "-audit";
                    HtmlGenericControl Div = (HtmlGenericControl)FindControlByClassName(ClassName, C);

                    if (Div == null)
                        continue;

                    Div.InnerText = AuditData["updated" + Suffix].ToString();
                    Div.Style.Remove(HtmlTextWriterStyle.Display);

                    System.Web.UI.WebControls.Image I = (System.Web.UI.WebControls.Image)FindControlByClassName("audit-history", C);

                    if (I != null)
                        I.Style.Remove(HtmlTextWriterStyle.Display);

                }
            }
        }

        ///////////////////////////////////////////////
        private void AddAuditCells(TableRow R)
        ///////////////////////////////////////////////
        {
            if (this.Audit == AuditModes.None || (UpdateMode == UpdateModes.Row && this.InlineEditColumns().Count == 0))
                return;

            string[] Columns = { "Created", "Updated" };

            foreach (string Column in Columns)
            {
                string ClassName = R.Cells[0].CssClass;
                TableCell C;

                if (R is TableHeaderRow)
                    C = new TableHeaderCell();
                else
                    C = new TableCell();

                R.Controls.Add(C);

                C.CssClass = ClassName;

                switch (ClassName)
                {
                    case "data-cell":
                        HtmlTable T = new HtmlTable();
                        T.Attributes.Add("class", "row-audit-info");
                        C.Controls.Add(T);
                        T.CellPadding = 0;
                        T.CellSpacing = 0;
                        HtmlTableRow TR = new HtmlTableRow();
                        T.Rows.Add(TR);

                        HtmlTableCell TC = new HtmlTableCell();
                        TR.Cells.Add(TC);

                        HtmlGenericControl I = new HtmlGenericControl("div");
                        TC.Controls.Add(I);
                        I.Attributes.Add("class", Column.ToLower() + "-by-audit");
                        I.Style.Add(HtmlTextWriterStyle.Display, "none");

                        TC = new HtmlTableCell();
                        TR.Cells.Add(TC);
                        I = new HtmlGenericControl("div");
                        TC.Controls.Add(I);
                        I.Attributes.Add("class", Column.ToLower() + "-audit");
                        I.Style.Add(HtmlTextWriterStyle.Display, "none");

                        if (Column == "Updated" && Audit == AuditModes.Detail)
                        {
                            TC = new HtmlTableCell();
                            TR.Cells.Add(TC);
                            System.Web.UI.WebControls.Image Img = new System.Web.UI.WebControls.Image();
                            Img.ImageUrl = GetImageUrl("zoom.png");
                            Img.CssClass = "audit-history";
                            Img.ToolTip = "View audit history";
                            Img.Style.Add(HtmlTextWriterStyle.Display, "none");
                            TC.Controls.Add(Img);
                        }
                        break;
                    case "header-cell":
                        C.Text = Column;
                        break;
                }
            }


        }

        ///////////////////////////////////////////////
        private TableHeaderRow BuildFilterRow()
        ///////////////////////////////////////////////
        {
            TableHeaderRow R = new TableHeaderRow();
            R.TableSection = TableRowSection.TableHeader;
            R.CssClass = "filter-row";

            foreach (GridColumn column in Columns)
            {
                if (!column.Display)
                    continue; ;

                TableHeaderCell C = new TableHeaderCell();
                R.Controls.Add(C);
                C.EnableViewState = false;
                C.CssClass = "filter-cell";

                if (column.Width != "")
                    C.Width = new Unit(column.Width);

                if (!column.Filter)
                {
                    C.Text = "&nbsp;";
                    continue;
                }

                if (column.FilterMode == GridColumn.FilterColumnSelectMode.List)
                    C.Controls.Add(BuildFilterColumnList(column));
                else
                    C.Controls.Add(BuildFilterColumnInput(column));
            }

            AddNestedSelectCell(R);
            AddAuditCells(R);

            return R;
        }

        ///////////////////////////////////////////////
        private DropDownList BuildFilterColumnList(GridColumn column)
        ///////////////////////////////////////////////
        {
            DropDownList L = new DropDownList();
            L.EnableViewState = false;
            L.CssClass = "filter-column-select drop-down";
            L.Attributes.Add("columnName", column.ColumnName.ToLower());

            if (column.Width != "")
                L.Width = new Unit(column.Width);

            string OrderPart = "1";

            if (Database.Database == DatabaseType.SqlServerCE)
                OrderPart = StripColumnRename(column.ColumnExpression);

            QueryCommandConfig Query = BuildSQL("distinct " + StripColumnRename(column.ColumnExpression) + " ", BuildFilterPart(true), OrderPart, QueryBuildModes.FilterListFilter);

            try
            {
                Database.ExecuteQuery(Query);
            }
            catch (Exception ex)
            {
                ThrowException(ex.Message, Database.CommandErrorInfo());
            }

            while (Database.Reader.Read())
            {
                if (Database.Reader[0] == System.DBNull.Value)
                {
                    L.Items.Add(new System.Web.UI.WebControls.ListItem(String.Empty, NullValueToken));
                    continue;
                }

                if (column.DataType == "Boolean")
                {
                    bool boolValue = Convert.ToBoolean(Database.Reader[0]);
                    string DisplayValue = "";

                    switch (column.BooleanDisplayMode)
                    {
                        case GridColumn.BooleanDisplayModeValues.TrueFalse:
                            DisplayValue = (boolValue ? Translate("True") : Translate("False"));
                            break;
                        case GridColumn.BooleanDisplayModeValues.Checkbox:
                        case GridColumn.BooleanDisplayModeValues.YesNo:
                            DisplayValue = (boolValue ? Translate("Yes") : Translate("No"));
                            break;
                    }

                    L.Items.Add(new System.Web.UI.WebControls.ListItem(DisplayValue, boolValue.ToString()));
                }
                else
                {
                    System.Web.UI.WebControls.ListItem Item;

                    string DisplayValue = FormatValue(Database.Reader[0], column);
                    if (!column.Lookup.Equals(string.Empty) && this.LookupTables.ContainsKey(column.ColumnKey))
                    {
                        string value = Convert.ToString(LookupValue(this.LookupTables[column.ColumnKey], Database.Reader[0]));
                        Item = new System.Web.UI.WebControls.ListItem(value, DisplayValue);
                    }
                    else
                        Item = new System.Web.UI.WebControls.ListItem(DisplayValue);

                    if (Database.Reader[0] is DateTime)
                        Item.Attributes.Add("title", Convert.ToDateTime(Database.Reader[0]).ToLongDateString());
                    else
                        Item.Attributes.Add("title", Item.Text);
                    L.Items.Add(Item);
                }
            }

            if (!column.Lookup.Equals(string.Empty))
                SortDropDownListByText(L);

            L.Items.Insert(0, new System.Web.UI.WebControls.ListItem(Translate("All"), String.Empty));

            string k = column.ColumnKey;
            if (this.ColumnFilterParams.ContainsKey(k))
                L.SelectedValue = Convert.ToString(this.ColumnFilterParams[k]);

            return L;
        }

        ///////////////////////////////////////////////
        public void SortDropDownListByText(DropDownList ddl)
        ///////////////////////////////////////////////
        {
            ArrayList AL = new ArrayList();

            foreach (System.Web.UI.WebControls.ListItem li in ddl.Items)
            {
                AL.Add(new KeyValuePair(li.Value, li.Text));
            }
            AL.Sort();
            ddl.Items.Clear();

            System.Web.UI.WebControls.ListItem nullItem = null;

            foreach (KeyValuePair keyValuePair in AL)
            {
                System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(keyValuePair.Value, keyValuePair.Key);

                if (keyValuePair.Key == NullValueToken)
                    nullItem = item;
                else
                    ddl.Items.Add(item);
            }

            if (nullItem != null)
            {
                ddl.Items.Insert(0, nullItem);
            }
        }

        ///////////////////////////////////////////////
        private TextBox BuildFilterColumnInput(GridColumn column)
        ///////////////////////////////////////////////
        {
            TextBox TB = new TextBox();
            TB.EnableViewState = false;
            TB.CssClass = "filter-column-select input";
            TB.Attributes.Add("dataType", column.DataType);
            TB.Attributes.Add("columnIndex", column.ColumnIndex.ToString());
            TB.Attributes.Add("columnKey", column.ColumnKey);
            TB.Attributes.Add("columnName", column.ColumnName.ToLower());

            if (column.Width != "")
                TB.Width = new Unit(column.Width);

            string k = column.ColumnKey;
            if (this.ColumnFilterParams.ContainsKey(k))
                TB.Text = Convert.ToString(this.ColumnFilterParams[k]);

            return TB;
        }

        ///////////////////////////////////////////////
        private void AddGroupHeaderRows(System.Web.UI.WebControls.Table T)
        ///////////////////////////////////////////////
        {
            foreach (GridColumn GC in this.Columns)
            {
                if (!GC.GroupHeader || !GC.PrintGroupHeader)
                    continue;

                AddGroupHeaderRow(T, GC);
            }
        }

        ///////////////////////////////////////////////
        private void AddGroupHeaderRow(System.Web.UI.WebControls.Table T, GridColumn GC)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            R.CssClass = "group-header-row";
            R.ID = "groupHeaderRow";

            T.Rows.Add(R);

            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.CssClass = "group-header-cell " + GC.ColumnName.ToLower();
            C.ID = "groupHeaderCell" + GC.ColumnIndex;

            foreach (TableRow TR in T.Rows)
            {
                if (TR.CssClass == "header-row")
                {
                    C.ColumnSpan = TR.Cells.Count;
                    break;
                }
            }

            Pair P = GetDBValues(GC);

            C.Text = FormattedValue(P.First, GC);
            GC.PrintGroupHeader = false;
        }

        ///////////////////////////////////////////////
        private void AddTotalRows(System.Web.UI.WebControls.Table T, bool FinalTotals)
        ///////////////////////////////////////////////
        {
            if (this.AggregateColumns().Count == 0)
                return;

            if (this.ProcedureName != "")
            {
                if (FinalTotals)
                    T.Controls.Add(BuildAggregateRow(null));
                return;
            }

            if (!FinalTotals)
                foreach (GridColumn GC in this.Columns)
                    if (GC.TotalBreak && (GC.Display || GC.GroupHeader))
                        if (this.TotalColumnCurrentValues[GC.ColumnName] == null)
                            this.TotalColumnCurrentValues[GC.ColumnName] = Database.ReaderValue(GC.ColumnName);

            bool TotalBreakFound = false;
            Stack AggRowStack = new Stack();

            foreach (GridColumn GC in this.Columns)
            {
                if (!GC.TotalBreak || (!GC.Display && !GC.GroupHeader))
                    continue;

                object DbValue = new object();

                if (!FinalTotals)
                    DbValue = Database.ReaderValue(GC.ColumnIndex);

                if (this.TotalColumnCurrentValues[GC.ColumnName].ToString() != DbValue.ToString() || TotalBreakFound || FinalTotals)
                {
                    Hashtable H = new Hashtable();
                    H["row"] = BuildAggregateRow(GC);
                    H["new_value"] = DbValue;
                    H["column"] = GC;
                    AggRowStack.Push(H);
                    TotalBreakFound = true;
                }
            }

            while (AggRowStack.Count > 0)
            {
                Hashtable H = (Hashtable)AggRowStack.Pop();
                GridColumn GC = (GridColumn)H["column"];
                T.Controls.Add((TableRow)H["row"]);
                this.TotalColumnCurrentValues[GC.ColumnName] = H["new_value"];
            }

            if (FinalTotals)
                T.Controls.Add(BuildAggregateRow(null));
        }


        ///////////////////////////////////////////////
        private TableRow BuildDataRow(int rowIndex)
        ///////////////////////////////////////////////
        {
            TableRow Row = new TableRow();
            Row.TableSection = TableRowSection.TableBody;

            //       dataRow.ID = AssignID("dataRow" + rowIndex);

            Row.CssClass = (rowIndex % 2 == 0 ? "even" : "odd");

            //           if (this.BuildMode == BuildModes.Display)
            Row.CssClass = "data-row " + Row.CssClass;

            Row.Attributes.Add("dataRowIndex", rowIndex.ToString());

            Dictionary<string, object> PK = new Dictionary<string, object>();

            foreach (GridColumn C in Columns)
            {
                if (!C.PrimaryKey || this.GroupBy)
                    continue;

                if (!PK.ContainsKey(C.ColumnName))
                    PK.Add(C.ColumnName, Database.Reader[C.ColumnIndex]);

                if (!String.IsNullOrEmpty(Row.ID))
                    Row.ID += "~";
                Row.ID += Database.Reader[C.ColumnIndex].ToString();
            }

            this.PrimaryKeyList.Add(PK);

            foreach (GridColumn C in Columns)
                BuildDataCell(C, Row);

            AddNestedSelectCell(Row);
            AddAuditCells(Row);

            return Row;
        }


        ///////////////////////////////////////////////
        private string PrimaryKeyString()
        ///////////////////////////////////////////////
        {
            List<string> A = new List<string>();

            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].PrimaryKey)
                    A.Add("pk" + A.Count.ToString() + "=" + Database.Reader[i].ToString());

                //if (Columns[i].UploadFileNameColumn != String.Empty)
                //    A.Add("fn" + A.Count.ToString() + "=" + Database.Reader[Columns[i].UploadFileNameColumn].ToString());
            }

            return string.Join("&", A.ToArray());
        }


        ///////////////////////////////////////////////
        private void BuildDataCell(GridColumn column, TableRow row)
        ///////////////////////////////////////////////
        {
            if (this.GroupBy && !column.Display && !GroupByHiddenColumns)
                return;

            if (!column.GridData)
                return;

            Pair P = GetDBValues(column);

            if (column.Display == false)
            {
                switch (this.BuildMode)
                {
                    case BuildModes.DataSource:
                        break;
                    default:
                        if (this.BuildMode == BuildModes.Display || this.OptimizeExportForLargeDataSet == false)
                        {
                            row.Attributes.Add("hc_" + column.ColumnName.ToLower(), JsonValue(P.First).ToString());
                        }
                        break;
                }
                return;
            }

            TableCell C = new TableCell();
            C.EnableViewState = false;
            C.CssClass = "data-cell";

            if (this.OptimizeExportForLargeDataSet == false)
            {
                switch (P.First.GetType().Name)
                {
                    case "Byte[]":
                        break;
                    default:
                        if (P.First == null)
                            C.Attributes.Add("value", "");
                        else if (P.First.ToString().Length < 256)
                            C.Attributes.Add("value", JsonValue(P.Second).ToString());
                        break;
                }

                if (column.DataType == "String")
                {
                    // Tell Excel this should be treated as text
                    switch (this.BuildMode)
                    {
                        case BuildModes.Excel:
                        case BuildModes.Copy:
                            C.Style.Add("mso-number-format", @"\@");
                            break;
                    }
                }
            }

            if (column.DataType == "Boolean" || column.IsBoolean)
                C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

            if (P.First.GetType().Name == "Byte[]" || column.UploadRootFolder != "")
                P.Second = PrimaryKeyString();

            if (this.BuildMode == BuildModes.Display && !column.UpdateReadOnly)
                if ((column.InlineEdit || (this.PageUpdate && column.Edit)) && column.EditControlType != EditField.ControlType.Html)
                {
                    C.CssClass += " edit-cell";

                    EditField EF = new EditField(this, column, row);
                    EF.Style.Add(HtmlTextWriterStyle.Margin, "auto");

                    EF.GridValue = GetEditFieldData(column.ColumnIndex, (Dictionary<string, object>)this.PrimaryKeyList[this.PrimaryKeyList.Count - 1]);
                    if (EF.GridValue["displayValue"] is string)
                        if (EF.GridValue["displayValue"].ToString().Length > column.MaxTextLength)
                            column.MaxTextLength = EF.GridValue["displayValue"].ToString().Length;
                    EF.Build();
                    C.Controls.Add(EF);
                    row.Controls.Add(C);
                    return;
                }

            if (column.Width != "")
                C.Width = new Unit(column.Width);

            if (P.First == null)
            {
                C.Text = "&nbsp;";
                row.Controls.Add(C);
                return;
            }

            if (P.First.GetType().Name == "Byte[]" || column.UploadRootFolder != "")
            {
                AddBinaryContent(C, P, column);
            }
            else if (P.First.GetType().Name == "Boolean" || column.IsBoolean)
            {
                AddBooleanContent(C, P, column);
            }
            else
            {
                C.Text = FormattedValue(P.First, column);
                C.Style.Add(HtmlTextWriterStyle.TextAlign, GetAlignment(P.First.GetType().Name).ToString().ToLower());
            }

            foreach (string Style in column.Style.Split(';'))
                if (Style.Split(':').Length == 2)
                {
                    if (Style.Split(':')[0].ToLower() == "text-align")
                        C.Style.Remove(HtmlTextWriterStyle.TextAlign);
                    C.Style.Add(Style.Split(':')[0], Style.Split(':')[1]);
                }

            if (column.ClearDuplicateValue)
                if (!column.PrintColumnValue)
                    C.Text = String.Empty;
                else
                    column.PrintColumnValue = false;

            row.Controls.Add(C);
        }

        ////////////////////////////////////////////////////////////////////////
        internal string FormattedValue(object Value, GridColumn Column)
        ////////////////////////////////////////////////////////////////////////
        {
            string text = FormatValue(Value, Column);

            switch (Column.Format.ToLower())
            {
                case "richtext":
                    text = RichText(text);
                    break;
                case "htmlencode":
                    text = System.Web.HttpUtility.HtmlEncode(text);
                    break;
            }

            if (text.Equals(""))
                text = "&nbsp;";

            return text;
        }

        ////////////////////////////////////////////////////////////////////////
        internal HorizontalAlign GetAlignment(string TypeName)
        ////////////////////////////////////////////////////////////////////////
        {
            switch (TypeName)
            {
                case "Byte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Decimal":
                case "Single":
                case "Double":
                    return HorizontalAlign.Right;
                default:
                    return HorizontalAlign.Left;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        internal string RichText(string S)
        ////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Text = new StringBuilder(System.Web.HttpUtility.HtmlEncode(S));

            Regex RE = new Regex("((www\\.|(http|https)://)[^ \\f\\n\\r\\t\\v\\>]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match M;
            int Offset = 0;
            for (M = RE.Match(Text.ToString()); M.Success; M = M.NextMatch())
            {
                string Pattern = M.Groups[0].ToString();
                string Url = Pattern;

                if (!Url.ToLower().StartsWith("http"))
                    Url = "http://" + Url;

                string Link = "<a target=_blank href='" + Url + "'>" + Pattern + "</a>";
                Text = Text.Replace(Pattern, Link, M.Index + Offset, Pattern.Length);
                Offset += Link.Length - Pattern.Length;
            }

            RE = new Regex(@"([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Offset = 0;
            for (M = RE.Match(Text.ToString()); M.Success; M = M.NextMatch())
            {
                string Pattern = M.Groups[0].ToString();
                string Link = "<a href=\"mailto:" + Pattern + "\">" + Pattern + "</a>";
                Text = Text.Replace(Pattern, Link, M.Index + Offset, Pattern.Length);
                Offset += Link.Length - Pattern.Length;
            }

            return Text.ToString().Replace(Environment.NewLine, "</br>").Replace("\n", "</br>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
        }

        ///////////////////////////////////////////////
        private void AddBinaryContent(TableCell C, Pair P, GridColumn Col)
        ///////////////////////////////////////////////
        {
            if (P.First.GetType() != typeof(DBNull) && !(P.First.GetType() == typeof(byte[]) && ((byte[])P.First).Length == 0))
            {
                Byte[] Data = null;
                string FileName = String.Empty;

                if (P.First is Byte[])
                {
                    Data = (Byte[])P.First;
                    if (Col.UploadFileNameColumn != String.Empty)
                        try { FileName = Database.Reader[Col.UploadFileNameColumn].ToString(); }
                        catch (Exception) { };
                }
                else
                {
                    Data = GetFileData(Col, P.First.ToString());
                    FileName = P.First.ToString();
                }


                System.Web.UI.WebControls.Image TI;

                if (DataUriSupported() && BuildMode != BuildModes.Pdf)
                {
                    TI = new DataUriImage(this, Data, Col, FileName, true);
                }
                else
                {
                    Dictionary<string, object> PK = (Dictionary<string, object>)this.PrimaryKeyList[this.PrimaryKeyList.Count - 1];
                    Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)this.PrimaryKeyList[this.PrimaryKeyList.Count - 1];
                    TI = new ThumbnailImage(this, Col, PrimaryKey, Data, FileName);
                }

                TI.ID = "thumbnail" + Col.ColumnIndex;
                TI.Attributes.Add("columnName", Col.ColumnName);
                TI.CssClass = "thumbnail";
                C.Controls.Add(TI);
            }
            else
                C.Controls.Add(new LiteralControl("&nbsp;"));

            C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
            C.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
        }

        ///////////////////////////////////////////////
        private void AddBooleanContent(TableCell dataCell, Pair cellValues, GridColumn column)
        ///////////////////////////////////////////////
        {
            if (cellValues.First == System.DBNull.Value)
                return;

            bool value = Convert.ToBoolean(cellValues.First);

            switch (column.BooleanDisplayMode)
            {
                case GridColumn.BooleanDisplayModeValues.Checkbox:
                    if (this.SaveType == "copy")
                    {
                        dataCell.Text = (value ? "&#9745;" : "&#9744;");
                    }
                    else
                    {
                        HtmlInputCheckBox CB = new HtmlInputCheckBox();
                        CB.Checked = value;
                        CB.Style.Add(HtmlTextWriterStyle.Margin, "0px");
                        CB.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                        CB.Style.Add(HtmlTextWriterStyle.Height, "14px");
                        CB.Attributes.Add("onclick", "return false;");
                        dataCell.Controls.Add(CB);
                    }
                    break;
                case GridColumn.BooleanDisplayModeValues.TrueFalse:
                    dataCell.Text = (value ? Translate("True") : Translate("False"));
                    break;
                case GridColumn.BooleanDisplayModeValues.YesNo:
                    dataCell.Text = (value ? Translate("Yes") : Translate("No"));
                    break;

            }

        }

        ///////////////////////////////////////////////
        private void AddAggregateRow(System.Web.UI.WebControls.Table gridTable)
        ///////////////////////////////////////////////
        {
            if (this.AggregateColumns().Count == 0)
                return;
        }

        ///////////////////////////////////////////////
        private QueryCommandConfig GetAggregateValueQuery(GridColumn TotalBreakColumn)
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Query = BuildSQL(BuildAggregateSelectPart(), BuildFilterPart(), "", QueryBuildModes.Totals);

            if (TotalBreakColumn != null)
            {
                List<string> Sql = new List<string>();

                foreach (GridColumn C in this.Columns)
                {
                    if (C.ColumnIndex > TotalBreakColumn.ColumnIndex)
                        break;

                    if (!C.TotalBreak || (!C.Display && !C.GroupHeader))
                        continue;

                    string ParamName = "AggFilter" + C.ColumnIndex.ToString();

                    Sql.Add(StripColumnRename(C.ColumnExpression) + " = " + Database.ParameterName(ParamName));
                    Query.Params[ParamName] = TotalColumnCurrentValues[C.ColumnName];
                }

                if (Query.Sql.ToLower().IndexOf(" where ") > -1)
                    Query.Sql += " and ";
                else
                    Query.Sql += " where ";

                Query.Sql += String.Join(" and ", Sql.ToArray());
            }
            return Query;
        }

        ///////////////////////////////////////////////
        private void AssignAggregateValues()
        ///////////////////////////////////////////////
        {
            if (this.ProcedureName == "")
                foreach (Hashtable TotalBreakRowInfo in TotalBreakRowCollection)
                    AssignAggregateValues(TotalBreakRowInfo);
        }

        ///////////////////////////////////////////////
        private void AssignAggregateValues(Hashtable TotalBreakRowInfo)
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Query = (QueryCommandConfig)TotalBreakRowInfo["query"];
            TableRow Row = (TableRow)TotalBreakRowInfo["row"];

            try
            {
                Database.ExecuteQuery(Query);
            }
            catch (Exception ex)
            {
                ThrowException(ex.Message, Database.CommandErrorInfo());
            }

            if (!Database.Reader.Read())
                return;

            for (int I = 0; I < Database.Reader.FieldCount; I++)
            {
                if (Database.Reader.IsDBNull(I))
                    continue;

                GridColumn GC = (GridColumn)this.AggregateColumns()[I];
                GC.AggregateValue = Database.Reader[I];
            }

            TableCell LC = (TableCell)FindControl("aggregateLabel", Row);

            if (LC != null)
            {
                Label L = new Label();
                L.CssClass = "aggregate-label";
                L.Text = (string)TotalBreakRowInfo["label"];
                if (L.Text == String.Empty)
                    L.Text = "&nbsp;";
                LC.Controls.Add(L);

                if (TotalBreakRowInfo["label_value"] != null)
                {
                    L = new Label();
                    L.Text = (string)TotalBreakRowInfo["label_value"];
                    L.CssClass = "aggregate-label-value";
                    LC.Controls.Add(L);
                }
            }
            foreach (GridColumn GC in Columns)
            {
                if (!GC.Display)
                    continue;

                if (GC.Aggregate == GridColumn.AggregateValues.None)
                    continue;

                TableCell C = (TableCell)FindControl("aggregateCell" + GC.ColumnIndex.ToString(), Row);

                C.Text = FormatValue(GC.AggregateValue, GC);

                if (C.Text.Equals(""))
                    C.Text = "&nbsp;";

            }
        }

        ///////////////////////////////////////////////
        private string BuildAggregateSelectPart()
        ///////////////////////////////////////////////
        {
            List<string> selectParts = new List<string>();

            foreach (GridColumn GC in this.AggregateColumns())
                selectParts.Add(GC.Aggregate.ToString() + "(" + AggregateExpression(GC) + ")");

            return string.Join(", ", selectParts.ToArray());
        }

        ///////////////////////////////////////////////
        private Control BuildAggregateRow(GridColumn TotalBreakColumn)
        ///////////////////////////////////////////////
        {
            Hashtable TotalBreakRowInfo = new Hashtable();

            if (this.ProcedureName == "")
                TotalBreakRowInfo["query"] = GetAggregateValueQuery(TotalBreakColumn);

            TableFooterRow R = new TableFooterRow();
            R.CssClass = "aggregate-row";
            R.ID = "aggregateRow";

            int ColSpan = 0;

            foreach (GridColumn GC in Columns)
            {
                if (!GC.Display)
                    continue;

                if (GC.Aggregate == GridColumn.AggregateValues.None)
                {
                    ColSpan++;
                    continue;
                }

                if (ColSpan > 0)
                {
                    R.Controls.Add(AggregateFillerCell(ColSpan));
                    ColSpan = 0;
                }

                TableCell C = new TableCell();
                R.Controls.Add(C);
                C.CssClass = "aggregate-cell";
                C.ID = "aggregateCell" + GC.ColumnIndex;
                C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

                C.Text = "&nbsp;";
                if (ProcedureName != "")
                    C.Text = FormatValue(GC.AggregateValue, GC);
            }

            if (ColSpan > 0)
                R.Controls.Add(AggregateFillerCell(ColSpan));

            foreach (TableCell C in R.Cells)
            {
                if (C.CssClass == "aggregate-filler-cell")
                {
                    C.ID = "aggregateLabel";
                    C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                    break;
                }
            }

            if (this.ProcedureName == "")
            {
                TotalBreakRowInfo["row"] = R;

                if (TotalBreakColumn != null)
                {
                    object V = this.TotalColumnCurrentValues[TotalBreakColumn.ColumnName];
                    if (TotalBreakColumn.Format != "")
                        V = FormatValue(V, TotalBreakColumn);

                    if (TotalBreakColumn.Lookup != "")
                        V = LookupValue(this.LookupTables[TotalBreakColumn.ColumnKey], V);

                    TotalBreakRowInfo["label"] = TotalBreakColumn.Label;
                    TotalBreakRowInfo["label_value"] = V.ToString();
                }
                else
                    TotalBreakRowInfo["label"] = this.FinalTotalLabel;
                TotalBreakRowInfo["column"] = TotalBreakColumn;

                TotalBreakRowCollection.Add(TotalBreakRowInfo);
            }

            AddNestedSelectCell(R);
            AddAuditCells(R);

            SetColumnPrintFlags(TotalBreakColumn);

            return R;
        }

        ///////////////////////////////////////////////
        internal void SetColumnPrintFlags(GridColumn ChangedColumn)
        ///////////////////////////////////////////////
        {
            if (ChangedColumn == null)
                return;

            foreach (GridColumn GC in this.Columns)
                if (GC.ColumnIndex >= ChangedColumn.ColumnIndex)
                {
                    if (GC.ClearDuplicateValue)
                        GC.PrintColumnValue = true;
                    if (GC.GroupHeader)
                        GC.PrintGroupHeader = true;
                }
        }

        ///////////////////////////////////////////////
        internal void ConfigureStoredProcedure()
        ///////////////////////////////////////////////
        {
            this.InsertRow = false;
            this.UpdateRow = false;
            this.DeleteRow = false;
            this.View = false;

            QueryCommandConfig QC = BuildSQL();
            ListDictionary Params = new ListDictionary();

            try
            {
                Params = Database.DeriveParameters(QC.Sql);

                foreach (string Key in Params.Keys)
                {
                    QC.Params[Key] = Params[Key];
                    ((IDbDataParameter)QC.Params[Key]).Value = System.DBNull.Value;
                }
            }
            catch (Exception) { }

            foreach (string ParamName in this.ProcedureParameters.Keys)
                Database.SetParamValue(QC.Params, ParamName, ProcedureParameters[ParamName]);

            Database.ExecuteQuery(QC);
            DataTable schema = Database.Reader.GetSchemaTable();

            if (Columns.Count == 0)
                foreach (DataRow row in schema.Rows)
                    Columns.Add(GenerateColumn(row));

            foreach (DbColumn Col in Columns)
            {
                bool found = false;

                foreach (DataRow row in schema.Rows)
                {
                    string ColumnName = (Col.ColumnName != "") ? Col.ColumnName : Col.ColumnExpression;
                    if (Convert.ToString(row["ColumnName"]).ToLower() == ColumnName.ToLower())
                    {
                        found = true;

                        Col.BaseTableName = Convert.ToString(row["BaseTableName"]);
                        Col.ColumnName = Convert.ToString(row["ColumnName"]);
                        Col.ColumnSize = Convert.ToInt32(row["ColumnSize"]);

                        Col.DataType = ((Type)row["DataType"]).Name;

                        if (Col.Label == "")
                            Col.Label = GenerateLabel(Col.ColumnName);
                        if (Col.ColumnKey == "")
                            Col.ColumnKey = Col.ColumnIndex.ToString();

                        if (Database.Database == DatabaseType.Oracle)
                        {
                            if (row["NumericScale"] != System.DBNull.Value)
                            {
                                if (Convert.ToInt16(row["NumericScale"]) == 0)
                                    Col.DataType = "Int64";
                                else if (Convert.ToInt16(row["NumericScale"]) < 11 && Col.Format == null)
                                    Col.Format = "F" + Convert.ToInt16(row["NumericScale"]).ToString();
                            }
                        }

                        break;
                    }
                }

                this.AssignColumnProperties();

                if (!found)
                {
                    ThrowException("Error in Column definitions<br /><br />" + Col.ColumnName + " was not found in the stored procedure");
                    return;
                }
            }
        }


        ///////////////////////////////////////////////
        internal QueryCommandConfig ProcedureCommandConfig()
        ///////////////////////////////////////////////
        {
            QueryCommandConfig QC = new QueryCommandConfig(this.ProcedureName);

            try
            {
                QC.Params = Database.DeriveParameters(QC.Sql);
            }
            catch (Exception) { }

            foreach (string ParamName in this.ProcedureParameters.Keys)
                Database.SetParamValue(QC.Params, ParamName, ProcedureParameters[ParamName]);

            return QC;
        }



        ///////////////////////////////////////////////
        private void AddInlineEditToolbar(System.Web.UI.WebControls.Table T)
        ///////////////////////////////////////////////
        {
            if (this.InlineEditColumns().Count == 0 || this.BuildMode != BuildModes.Display)
                return;

            TableRow TR;

            if (T.Rows.Count == 0)
            {
                if (InlineEditToolbarLocation == InlineEditToolbarOptions.Bottom)
                    return;

                TR = new TableHeaderRow();
                TR.TableSection = TableRowSection.TableHeader;
            }
            else
            {
                if (InlineEditToolbarLocation == InlineEditToolbarOptions.Top)
                    return;

                TR = new TableRow();
            }

            TR.CssClass = "inline-edit-toolbar-row";
            T.Controls.Add(TR);
            TableCell TC = new TableCell();

            TC.ColumnSpan = this.DisplayColumns().Count;
            if (MultiRowSelect)
                TC.ColumnSpan++;
            if (this.NestedGrid)
                TC.ColumnSpan++;

            if (this.Audit != AuditModes.None)
                TC.ColumnSpan += 2;

            TR.Controls.Add(TC);

            TC.Controls.Add(AddInlineEditControls());
        }

        ///////////////////////////////////////////////
        internal System.Web.UI.WebControls.Table AddInlineEditControls()
        ///////////////////////////////////////////////
        {
            System.Web.UI.WebControls.Table T1 = new System.Web.UI.WebControls.Table();
            T1.CssClass = "inline-edit-toolbar";

            TableRow TR = new TableRow();
            T1.Controls.Add(TR);
            T1.Width = new Unit("100%");

            if (InlineEditToolbarButtonLocation == InlineEditToolbarButtonAlignment.Right)
            {
                AddInlineEditMessagePanel(TR);
                AddInlineEditButtons(TR);
            }
            else
            {
                AddInlineEditButtons(TR);
                AddInlineEditMessagePanel(TR);
            }

            return T1;
        }

        ///////////////////////////////////////////////
        internal void AddInlineEditMessagePanel(TableRow TR)
        ///////////////////////////////////////////////
        {
            TableCell TC = new TableCell();
            TC.Width = new Unit("100%");

            TR.Controls.Add(TC);

            Panel P = new Panel();
            TC.Controls.Add(P);
            P.ID = "messageBox";
            P.CssClass = "dbnetgrid-message-box";

            if (this.GetTheme() == UI.Themes.Bootstrap)
                P.CssClass += " alert alert-info";

            P.Controls.Add(new LiteralControl("&nbsp;"));
        }

        ///////////////////////////////////////////////
        internal void AddInlineEditButtons(TableRow TR)
        ///////////////////////////////////////////////
        {
            if (SpellCheckEnabled())
                AddToolButton(TR, "spellCheck", "spellcheck", "CheckSpelling");

            BuildButton(TR, "apply", "apply", "Apply", "ApplyChangesToTheCurrentRecord");
            BuildButton(TR, "cancel", "undo", "Cancel", "Cancel");
        }

        ///////////////////////////////////////////////
        internal TableCell AggregateFillerCell(int ColSpan)
        ///////////////////////////////////////////////
        {
            TableCell FC = new TableCell();
            FC.EnableViewState = false;
            FC.CssClass = "aggregate-filler-cell";
            FC.Text = "&nbsp;";
            FC.ColumnSpan = ColSpan;
            return FC;
        }

        ///////////////////////////////////////////////
        private ArrayList AggregateColumns()
        ///////////////////////////////////////////////
        {
            ArrayList AggregateColumns = new ArrayList();
            foreach (GridColumn GC in this.Columns)
                if (GC.Aggregate != GridColumn.AggregateValues.None)
                    AggregateColumns.Add(GC);
            return AggregateColumns;
        }


        ///////////////////////////////////////////////
        private ArrayList InlineEditColumns()
        ///////////////////////////////////////////////
        {
            ArrayList Cols = new ArrayList();
            foreach (GridColumn GC in this.Columns)
                if (GC.InlineEdit || (this.PageUpdate && GC.Edit))
                    Cols.Add(GC);
            return Cols;
        }

        ///////////////////////////////////////////////
        private ArrayList DisplayColumns()
        ///////////////////////////////////////////////
        {
            ArrayList Cols = new ArrayList();
            foreach (GridColumn GC in this.Columns)
                if (GC.Display)
                    Cols.Add(GC);
            return Cols;
        }

        ///////////////////////////////////////////////
        internal ArrayList ViewColumns()
        ///////////////////////////////////////////////
        {
            ArrayList Cols = new ArrayList();
            foreach (GridColumn GC in this.Columns)
                if (GC.View)
                    Cols.Add(GC);
            return Cols;
        }

        ///////////////////////////////////////////////
        private void ValidateColumnFilterParameters()
        ///////////////////////////////////////////////
        {
            Resp["result"] = true;
            foreach (string key in this.ColumnFilterParams.Keys)
            {
                GridColumn column = (GridColumn)this.ColumnFromKey(key);
                string Msg = ValidateColumnFilterParameter(column, this.ColumnFilterParams[key].ToString());
                if (Msg != "")
                {
                    Resp["result"] = false;
                    Resp["columnIndex"] = column.ColumnIndex;
                    Resp["message"] = Msg;
                }
            }
        }

        ///////////////////////////////////////////////
        private string ValidateColumnFilterParameter(GridColumn column, string value)
        ///////////////////////////////////////////////
        {
            if (String.IsNullOrEmpty(value))
                if (!column.Required)
                    return "";

            if (value == NullValueToken)
                return String.Empty;

            Type dataType = GetColumType(column.DataType);

            if (column.Lookup != "" && column.FilterMode == GridColumn.FilterColumnSelectMode.Input)
            {
                dataType = GetColumType(column.LookupDataType);
            }

            return ValidateDataTypeValue(dataType, value, column.Format);
        }

        ///////////////////////////////////////////////
        private void ExportChart()
        ///////////////////////////////////////////////
        {
            //   this.Context.Response.ContentType = "text/html";
            //   this.Context.Response.Write(Query.Sql);
            //   return;
#if (!x64)
            BuildChart();
            StreamChart();
#endif
        }

        ///////////////////////////////////////////////
        private void StreamChart()
        ///////////////////////////////////////////////
        {
            string Key = this.Id + "_chart";

            if (Req.ContainsKey("chart_id"))
                Key = Req["chart_id"].ToString();

            this.Context.Response.ContentType = "image/png";
            this.Context.Response.BinaryWrite(this.Context.Session[Key] as Byte[]);
            this.Context.Response.End();
        }

#if (!x64)
        ///////////////////////////////////////////////
        private void BuildChart()
        ///////////////////////////////////////////////
        {

            Chart C = new Chart();
            ChartArea CA = new ChartArea();
            CA.Name = "MainChartArea";
            C.ChartAreas.Add(CA);

            foreach (string Key in ChartConfig.Keys)
            {
                switch (Key.ToLower())
                {
                    case "series":
                    case "legends":
                    case "titles":
                    case "legend":
                    case "title":
                        this.AddToChartCollection(C, Key);
                        break;
                    case "area3dstyle":
                    case "axisy":
                    case "axisy2":
                    case "axisx":
                    case "axisx2":
                        this.SetChartProperties(this.GetProperty(CA, Key), ChartConfig[Key]);
                        break;
                    case "borderskin":
                        this.SetChartProperties(this.GetProperty(C, Key), ChartConfig[Key]);
                        break;
                    case "chartarea":
                        this.SetChartProperties(CA, ChartConfig[Key]);
                        break;
                    default:
                        this.SetProperty(C, Key, ChartConfig[Key]);
                        break;
                }
            }

            QueryCommandConfig Query = BuildSQL();

            DataTable DT = Database.GetDataTable(Query);
            GetLookupTables();

            DataTable DT2 = DT.Clone();

            foreach (GridColumn GC in Columns)
                if (GC.Lookup != "" || GC.Format != "")
                    if (DT2.Columns.Contains(GC.ColumnName))
                        DT2.Columns[GC.ColumnName].DataType = typeof(String);

            string LabelStyleFormat = null;

            foreach (Series S in C.Series)
            {
                GridColumn GC = this.Columns[S.YValueMembers] as GridColumn;
                if (GC == null)
                    continue;

                if (LabelStyleFormat != null)
                    if (GC.Format == "" || GC.Format != LabelStyleFormat)
                    {
                        LabelStyleFormat = "";
                        break;
                    }
                LabelStyleFormat = GC.Format;
                S.LabelFormat = LabelStyleFormat;
            }

            if (!String.IsNullOrEmpty(LabelStyleFormat))
                C.ChartAreas[0].AxisY.LabelStyle.Format = LabelStyleFormat;

            foreach (DataRow R in DT.Rows)
            {
                DT2.ImportRow(R);
                DataRow R2 = DT2.Rows[DT2.Rows.Count - 1];
                foreach (GridColumn GC in Columns)
                {
                    if (!R2.Table.Columns.Contains(GC.ColumnName))
                        continue;

                    if (GC.Lookup != "")
                    {
                        if (this.LookupTables.ContainsKey(GC.ColumnKey))
                            R2[GC.ColumnName] = LookupValue(this.LookupTables[GC.ColumnKey], R2[GC.ColumnName]);
                        else
                            R2[GC.ColumnName] = LookupValue(GC, R2[GC.ColumnName], false);
                    }

                    if (GC.Format != "")
                    {
                        R2[GC.ColumnName] = FormatValue(R[GC.ColumnName], GC);
                    }
                }
            }

            if (C.Series.Count == 0)
            {
                C.DataBindTable(DT2.DefaultView);
            }
            else
            {
                C.DataSource = DT2;
                C.DataBind();
            }

            MemoryStream MS = new MemoryStream();
            C.SaveImage(MS, ChartImageFormat.Png);


            try
            {
                this.Context.Session[Id + "_chart_" + ChartConfig["chartPanel"].ToString()] = MS.ToArray();
            }
            catch { }
            finally
            {
                MS.Flush();
                MS.Close();
            }
        }


        ///////////////////////////////////////////////
        private void SetChartProperties(Object O, Object Properties)
        ///////////////////////////////////////////////
        {
            if (Properties is Dictionary<string, object>)
            {
                Dictionary<string, object> Dic = Properties as Dictionary<string, object>;
                foreach (string K in Dic.Keys)
                    this.SetProperty(O, K, Dic[K]);
            }
        }


        ///////////////////////////////////////////////
        private void SetSeriesAttributes(Series S, Object Properties)
        ///////////////////////////////////////////////
        {
            if (Properties is Dictionary<string, object>)
            {
                Dictionary<string, object> Dic = Properties as Dictionary<string, object>;
                foreach (string K in Dic.Keys)
                    S[K] = Dic[K].ToString();
            }
        }

        ///////////////////////////////////////////////
        private void AddToChartCollection(Chart C, string Key)
        ///////////////////////////////////////////////
        {
            object[] ObjectArray = new object[1];

            switch (Key.ToLower())
            {
                case "title":
                case "legend":
                    ObjectArray[0] = ChartConfig[Key];
                    break;
                default:
                    if (ChartConfig[Key] is object[])
                        ObjectArray = ChartConfig[Key] as object[];
                    else
                        ObjectArray[0] = ChartConfig[Key];
                    break;
            }

            foreach (object S in ObjectArray)
            {
                Dictionary<string, object> Dic;

                if (S is Dictionary<string, object>)
                    Dic = S as Dictionary<string, object>;
                else
                    continue;

                if (Dic.Count == 0)
                    continue;

                object Item = null;
                switch (Key.ToLower())
                {
                    case "series":
                        Item = new Series();
                        (Item as Series).ChartArea = "MainChartArea";
                        break;
                    case "legends":
                    case "legend":
                        Item = new Legend();
                        break;
                    case "titles":
                    case "title":
                        Item = new Title();
                        break;
                }

                foreach (string K in Dic.Keys)
                {
                    switch (K.ToLower())
                    {
                        case "customattributes":
                            if (Item is Series)
                                this.SetSeriesAttributes(Item as Series, Dic[K]);
                            break;
                        default:
                            this.SetProperty(Item, K, Dic[K]);
                            break;
                    }
                }

                switch (Key.ToLower())
                {
                    case "series":
                        C.Series.Add((Item as Series));
                        break;
                    case "legends":
                    case "legend":
                        C.Legends.Add((Item as Legend));
                        break;
                    case "titles":
                    case "title":
                        C.Titles.Add((Item as Title));
                        break;
                }
            }
        }
#endif

        ///////////////////////////////////////////////
        private bool ExportGridLite()
        ///////////////////////////////////////////////
        {
            switch (this.SaveType)
            {
                case "excel":
                case "html":
                case "word":
                case "pdf":
                    break;
                default:
                    return false;
            }

            this.BuildMode = (BuildModes)Enum.Parse(typeof(BuildModes), this.SaveType, true);

            System.Web.UI.WebControls.Table T = new System.Web.UI.WebControls.Table();
            T.ID = AssignID("datatable");
            T.Caption = this.Caption;
            T.CssClass = "dbnetgrid";
            T.Attributes.Add("buildMode", this.BuildMode.ToString().ToLower());

            GetLookupTables();

            this.SetContentType();

            WriteExportHeader();
            this.Context.Response.Write(RenderControlToString(T).Replace("</table>", String.Empty));

            TableHeaderRow HeaderRow = BuildHeaderRow();

            this.Context.Response.Write(RenderControlToString(HeaderRow));

            QueryCommandConfig Query = BuildSQL();

            try
            {
                Database.ExecuteQuery(Query);
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message, Database.CommandErrorInfo());
            }

            int rowCounter = 0;

            while (Database.Reader.Read())
            {
                rowCounter++;
                TableRow GridRow = BuildDataRow(rowCounter);
                this.Context.Response.Write(RenderControlToString(GridRow));
                this.Context.Response.Flush();
            }

            this.Context.Response.Write("</table>");
            WriteExportFooter();
            this.Context.Response.End();
            this.CloseConnection();

            return true;
        }

        ///////////////////////////////////////////////
        private void ExportGrid()
        ///////////////////////////////////////////////
        {
            /*
            if (this.SaveType == "pdf")
            {
                this.SetContentType();
                this.Context.Response.Write(BuildPdfExportGrid());
                this.Context.Response.End();
                this.CloseConnection();
            }
            */

            object ReportDoc = new object();

            if (Req.ContainsKey("html"))
                ReportDoc = WrapHtmlInDocument(new StringBuilder(HttpUtility.HtmlDecode(Req["html"].ToString())));
            else
            {
                if (this.OptimizeExportForLargeDataSet)
                    if (ExportGridLite())
                        return;
                ReportDoc = BuildExportGrid();
            }

            this.SetContentType();

            if (ReportDoc is System.Web.UI.WebControls.Table)
            {
                WriteExportHeader();

                System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(this.Context.Response.Output);
                (ReportDoc as System.Web.UI.WebControls.Table).RenderControl(hw);

                WriteExportFooter();
            }
            else
                this.Context.Response.Write(ReportDoc);

            this.Context.Response.End();
            this.CloseConnection();
        }

        ///////////////////////////////////////////////
        private void WriteExportHeader()
        ///////////////////////////////////////////////
        {
            this.Context.Response.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            if (this.SaveType != "xls")
                this.Context.Response.Write("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            else
                this.Context.Response.Write("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
            this.Context.Response.Write("<head>");
            this.Context.Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/Html; charset=utf-8\"></meta>");

            if (this.BuildMode != BuildModes.Excel)
                this.Context.Response.Write("<style>" + GridCSS() + "</style>");

            if (this.SaveType == "xls")
            {
                this.Context.Response.Write("<!--[if gte mso 9]><xml>");
                this.Context.Response.Write("<x:ExcelWorkbook>");
                this.Context.Response.Write("<x:ExcelWorksheets>");
                this.Context.Response.Write("<x:ExcelWorksheet>");
                this.Context.Response.Write("<x:Name>" + this.ExportFileName.Split('.')[0] + "/x:Name>");
                this.Context.Response.Write("<x:WorksheetOptions>");
                this.Context.Response.Write("<x:Print>");
                this.Context.Response.Write("<x:ValidPrinterInfo/>");
                this.Context.Response.Write("</x:Print>");
                this.Context.Response.Write("</x:WorksheetOptions>");
                this.Context.Response.Write("</x:ExcelWorksheet>");
                this.Context.Response.Write("</x:ExcelWorksheets>");
                this.Context.Response.Write("</x:ExcelWorkbook>");
                this.Context.Response.Write("</xml>");
                this.Context.Response.Write("<![endif]--> ");
            }

            this.Context.Response.Write("</head>");
            this.Context.Response.Write("<body>");

        }

        ///////////////////////////////////////////////
        private void WriteExportFooter()
        ///////////////////////////////////////////////
        {
            this.Context.Response.Write("</body>");
            this.Context.Response.Write("</html>");
        }

        ///////////////////////////////////////////////
        private void SetContentType()
        ///////////////////////////////////////////////
        {
            string Extension = String.Empty;
            string ContentType = String.Empty;
            switch (this.SaveType)
            {
                case "csv":
                    ContentType = "text/csv";
                    Extension = "csv";
                    break;
                case "slk":
                    ContentType = "application/excel";
                    Extension = "slk";
                    break;
                case "excel":
                    ContentType = "application/vnd.ms-excel";
                    Extension = "xls";
                    break;
                case "html":
                    ContentType = "text/html";
                    Extension = "html";
                    break;
                case "word":
                    ContentType = "application/vnd.ms-word";
                    Extension = "doc";
                    break;
                case "xml":
                    ContentType = "text/xml";
                    Extension = "xml";
                    break;
                case "pdf":
                    ContentType = "application/pdf";
                    Extension = "pdf";
                    break;
            }

            this.Context.Response.Clear();
            this.Context.Response.ContentType = ContentType;
            this.Context.Response.Charset = Encoding.UTF8.WebName;
            this.Context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            this.Context.Response.AddHeader("content-disposition", "inline; filename=" + ExportFileName.Split('.')[0] + "." + Extension);
            this.Context.Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
        }


        ///////////////////////////////////////////////
        private void SaveGrid()
        ///////////////////////////////////////////////
        {
            object ReportDoc = BuildExportGrid();

            var extension = $".{this.SaveType.ToLower()}";
            if ( ExportFileName.ToLower().EndsWith(extension) == false)
            {
                ExportFileName += extension;
            }
            var savePath = Path.Combine(this.Context.Request.MapPath(this.ExportFolder), this.ExportFileName);

            try
            {
                File.WriteAllText(savePath, ReportDoc.ToString());
                Resp["message"] = $"File [{this.ExportFileName}] successfully exported to the specified folder on the server";
            }
            catch(Exception ex)
            {
                Resp["message"] = $"File was not successfully exported - {ex.Message}";
            }

            this.CloseConnection();
        }

        ///////////////////////////////////////////////
        private object BuildExportGrid()
        ///////////////////////////////////////////////
        {
            if (this.SaveType == "datasource")
                if (!String.IsNullOrEmpty(this.MailMergeDocument.ToString()))
                {
                    Resp["mailMergeDocument"] = this.MailMergeDocument;

                    bool FileExists = File.Exists(this.Context.Request.MapPath(this.MailMergeDocument.ToString()));
                    Resp["mergeDocumentExists"] = FileExists;
                    if (FileExists)
                        Resp["mailMergeDocument"] = ResolveServerUrl(this.MailMergeDocument.ToString(), false);
                }

            switch (this.SaveType)
            {
                case "csv":
                    return BuildCsvExportGrid();
                case "slk":
                    return BuildSlkExportGrid();
                case "xml":
                    return BuildXmlExportGrid();
                default:
                    this.BuildMode = (BuildModes)Enum.Parse(typeof(BuildModes), this.SaveType, true);
                    if (this.CustomisedExport() || this.BuildMode == BuildModes.DataSource)
                        return BuildHtmlExportGrid();
                    else
                        return BuildGridTable();
            }
        }

        ///////////////////////////////////////////////
        private bool CustomisedExport()
        ///////////////////////////////////////////////
        {
            if (Req.ContainsKey("outputTemplate"))
                if (Req["outputTemplate"].ToString() != "")
                    return true;

            if (this.OutputPageSize > 0)
                return true;

            return false;
        }

        private static string ResolveUrl(string originalUrl)
        {
            if (!string.IsNullOrEmpty(originalUrl) && '~' == originalUrl[0])
            {
                int index = originalUrl.IndexOf('?');
                string queryString = (-1 == index) ? null : originalUrl.Substring(index);
                if (-1 != index) originalUrl = originalUrl.Substring(0, index);
                originalUrl = VirtualPathUtility.ToAbsolute(originalUrl) + queryString;
            }

            return originalUrl;
        }


        private static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            Uri result = HttpContext.Current.Request.Url;
            if (!string.IsNullOrEmpty(serverUrl))
            {
                serverUrl = ResolveUrl(serverUrl);
                result = new Uri(result, serverUrl);
            }
            if (forceHttps && !string.Equals(result, Uri.UriSchemeHttps))
            {
                UriBuilder builder = new UriBuilder(result);
                builder.Scheme = Uri.UriSchemeHttps;
                builder.Port = 443;
                result = builder.Uri;
            }

            return result.ToString();
        }

/*
        ///////////////////////////////////////////////
        private Document BuildPdfExportGrid()
        ///////////////////////////////////////////////
        {
            this.BuildMode = BuildModes.Pdf;
            iTextSharp.text.Rectangle PageSize = iTextSharp.text.PageSize.A4;

            if (Convert.ToBoolean(Req["portrait"]) == false)
                PageSize = PageSize.Rotate();

            Document pdfDoc = new Document(PageSize);

            PdfWriter.GetInstance(pdfDoc, this.Context.Response.OutputStream);
            pdfDoc.Open();

            string Html = "";

            if (Req.ContainsKey("html"))
                Html = HttpUtility.HtmlDecode(Req["html"].ToString());
            else
                Html = BuildGrid();

            Html = Html.Replace("%20", " ");


           // var regex = new Regex("<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);

        //    foreach (Match match in regex.Matches(Html))
          //  {
          //      Html.Replace(match.Groups[1].Value, match.Groups[1].Value.Replace("%20", " "));
         //   }
           

            StyleSheet styles = new StyleSheet();

            styles.LoadTagStyle("td", "size", Req["fontSize"].ToString() + "pt");
            styles.LoadTagStyle("td", "face", Req["fontFamily"].ToString().Split(' ')[0]);

            ArrayList Elements = HTMLWorker.ParseToList(new StringReader(Html), styles);

            List<float> CellWidths = new List<float>();

            if (Req.ContainsKey("cellWidths"))
                foreach (object CellWidth in (object[])Req["cellWidths"])
                    CellWidths.Add(Convert.ToSingle(CellWidth));

            int TableCount = 0;

            for (int i = 0; i < Elements.Count; i++)
            {
                IElement ielement = (IElement)Elements[i];
                if (ielement is PdfPTable)
                {

                    PdfPTable T = (ielement as PdfPTable);

                    StylePdfTable(T);
                    if (CellWidths.Count > 0)
                        try
                        {
                            T.SetWidths(CellWidths.ToArray());
                        }
                        catch (Exception)
                        {
                        }

                    if (this.OutputPageSize > 0)
                        if (T.GetRow(0).GetCells().Length > 3 || T.Rows.Count > 3)
                            TableCount++;

                    if (TableCount > 1)
                        pdfDoc.NewPage();
                }
                pdfDoc.Add(ielement);
            }
            pdfDoc.Close();
            return pdfDoc;
        }


        ///////////////////////////////////////////////
        private void StylePdfTable(PdfPTable T)
        ///////////////////////////////////////////////
        {
            for (var r = 0; r < T.Size; r++)
            {
                PdfPRow R = T.GetRow(r);
                foreach (PdfPCell C in R.GetCells())
                {
                    if (C == null)
                        continue;
                    if (r == 0)
                        C.BackgroundColor = new iTextSharp.text.Color(220, 220, 220);
                    else if (r % 2 == 0)
                        C.BackgroundColor = new iTextSharp.text.Color(245, 245, 245);

                    C.BorderWidth = 1;
                    C.BorderColor = iTextSharp.text.Color.LIGHT_GRAY;
                    C.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER | iTextSharp.text.Rectangle.TOP_BORDER;

                    foreach (IElement E in C.Chunks)
                        if (E is PdfPTable)
                            StylePdfTable(E as PdfPTable);

                }
            }
        }


        private string BuildPdfExportGrid()
        {
            return String.Empty;
        }
*/

        ///////////////////////////////////////////////
        private StringBuilder BuildSlkExportGrid()
        ///////////////////////////////////////////////
        {
            GetLookupTables();
            QueryCommandConfig sql = BuildSQL();
            Database.ExecuteQuery(sql);
            StringBuilder slk = new StringBuilder();

            string headers = "";

            headers += "ID;P" + System.Environment.NewLine;

            int[] columnWidths = new int[Database.Reader.FieldCount];
            int i = 0;

            foreach (GridColumn GC in Columns)
            {
                if (!GC.Display)
                    continue;

                headers += "C;Y1;X" + (i + 1).ToString() + ";K" + '"' + GC.Label + '"' + System.Environment.NewLine;
                columnWidths[i] = GC.Label.Length;
                i++;
            }

            slk.Append(headers);

            int recordCount = 0;

            while (Database.Reader.Read())
            {
                recordCount++;
                System.Text.StringBuilder record = new System.Text.StringBuilder();
                ArrayList Values = new ArrayList();
                i = 0;
                foreach (GridColumn column in Columns)
                {
                    if (!column.Display)
                        continue;

                    string Val = GetDBValues(column).First.ToString();
                    Val = Regex.Replace(Val, Environment.NewLine, "");
                    Val = Regex.Replace(Val, "\"", "\"\"");

                    string recordItem = "C;Y" + (recordCount + 1).ToString() + ";X" + (i + 1).ToString() + ";K\"" + Val + '"' + System.Environment.NewLine;
                    record.Append(recordItem);
                    if (Val.Length > columnWidths[i])
                        columnWidths[i] = Val.Length;
                    i++;
                }

                slk.Append(record);
            }

            i = 0;
            foreach (GridColumn GC in Columns)
            {
                if (!GC.Display)
                    continue;

                string ColNum = (i + 1).ToString();
                slk.Append(System.Environment.NewLine + "F;W" + ColNum + " " + ColNum + " " + columnWidths[i].ToString());
                i++;
            }

            slk.Append(System.Environment.NewLine + "E");

            return slk;
        }

        ///////////////////////////////////////////////
        internal DbNetButton AddToolbarButton(string ID, string Text, string Img, string Title)
        ///////////////////////////////////////////////
        {
            //string ImageUrl = this.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + Img + ".png");
            DbNetButton B = new DbNetButton(Img, Translate(Text), Translate(Title), this.Context.Request, this.PageRef);

            if (this.GetTheme() == UI.Themes.Bootstrap)
                B.Attributes.Add("class", ID + "-button btn");
            else
                B.Attributes.Add("class", ID + "-button");

            B.ID = ID + "Btn";
            return B;
        }

        ///////////////////////////////////////////////
        private void GetDataArray()
        ///////////////////////////////////////////////
        {
            object[] Cols = new object[0];
            if (Req.ContainsKey("dataColumns"))
                Cols = Req["dataColumns"] as object[];

            List<string> DataColumns = new List<string>();

            foreach (object Column in Cols)
                DataColumns.Add(Column.ToString().ToLower());

            ArrayList Data = new ArrayList();

            GetLookupTables();
            Database.ExecuteQuery(BuildSQL());

            while (Database.Reader.Read())
            {
                ArrayList Values = new ArrayList();

                if (DataColumns.Count > 0)
                {
                    foreach (string ColName in DataColumns)
                    {
                        GridColumn GC = this.Columns[ColName] as GridColumn;
                        if (GC == null)
                            Values.Add("Column [" + ColName + "] not found");
                        else
                            Values.Add(GetDBValues(GC).First);
                    }
                }
                else
                {
                    foreach (GridColumn GC in this.Columns)
                    {
                        Values.Add(GetDBValues(GC).First);
                    }
                }

                Data.Add(Values.ToArray());
            }

            Resp["data"] = Data;
        }

        ///////////////////////////////////////////////
        private StringBuilder BuildCsvExportGrid()
        ///////////////////////////////////////////////
        {
            GetLookupTables();
            QueryCommandConfig sql = BuildSQL();
            Database.ExecuteQuery(sql);
            StringBuilder csv = new StringBuilder();

            List<string> Headings = new List<string>();
            foreach (GridColumn column in Columns)
            {
                if (!column.Display)
                    continue;

                Headings.Add(column.Label);
            }
            csv.Append(String.Join(",", Headings.ToArray()));
            csv.Append('\n');

            var firstRecord = (PageSize * (this.CurrentPage - 1)) + 1;
            var lastRecord = (firstRecord + (PageSize - 1));
            var recordCounter = 0;

            if (!this.OutputCurrentPage)
            {
                firstRecord = 1;
                lastRecord = Int32.MaxValue;
            }

            while (Database.Reader.Read())
            {
                recordCounter++;
                if (recordCounter < firstRecord || recordCounter > lastRecord)
                    continue;

                ArrayList Values = new ArrayList();
                foreach (GridColumn column in Columns)
                {
                    if (!column.Display)
                        continue;

                    string Val = GetDBValues(column).First.ToString();
                    Val = Regex.Replace(Val, Environment.NewLine, "");
                    Val = Regex.Replace(Val, "\"", "\"\"");

                    if (Val.IndexOf(",") >= 0 || Val.IndexOf("\"") >= 0)
                        Values.Add("\"" + Val + "\"");
                    else
                        Values.Add(Val);
                }

                csv.Append(String.Join(",", (string[])Values.ToArray(typeof(string))));
                csv.Append('\n');
            }

            return csv;
        }

        ///////////////////////////////////////////////
        public StringBuilder BuildHtmlExportGrid()
        ///////////////////////////////////////////////
        {
            this.BuildMode = (BuildModes)Enum.Parse(typeof(BuildModes), this.SaveType, true);
            string GridTable = BuildGrid();

            StringBuilder Html = new StringBuilder();

            if (Req.ContainsKey("outputTemplate"))
            {
                if (Req["outputTemplate"].ToString() != "")
                {
                    string FilePath = Req["outputTemplate"].ToString();
                    if (FilePath.StartsWith("<"))
                        Html.Append(FilePath);
                    else
                    {
                        try
                        {
                            string Text = LoadUrl(FilePath);

                            if (Text == "")
                            {
                                FilePath = this.Context.Request.MapPath(FilePath.Replace("~", this.Context.Request.ApplicationPath));
                                if (File.Exists(FilePath))
                                    Text = System.IO.File.ReadAllText(FilePath);
                            }

                            if (this.SaveType == "pdf")
                            {
                                string tag = "style";
                                Text = Regex.Replace(Text, "<" + tag + "[^>]*>(?<contents>.*?)</" + tag + ">", "", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                            }
                            Html.Append(Text);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            if (Html.Length == 0)
            {
                Html.Append("{grid}");
                Html = WrapHtmlInDocument(Html);
            }

            ListDictionary OutputParameters = new ListDictionary();
            OutputParameters["short_date"] = DateTime.Now.ToShortDateString();
            OutputParameters["short_time"] = DateTime.Now.ToShortTimeString();
            OutputParameters["long_date"] = DateTime.Now.ToLongDateString();
            OutputParameters["long_time"] = DateTime.Now.ToLongTimeString();

            if (Req.ContainsKey("outputParameters"))
            {
                Dictionary<string, object> P = (Dictionary<string, object>)Req["outputParameters"];
                foreach (string Key in P.Keys)
                    OutputParameters[Key] = P[Key].ToString();
            }

            foreach (string Key in OutputParameters.Keys)
                Html = Html.Replace("{" + Key + "}", OutputParameters[Key].ToString());

            Html = Html.Replace("{grid}", GridTable);

            return Html;
        }

        ///////////////////////////////////////////////
        public string LoadUrl(string Url)
        ///////////////////////////////////////////////
        {
            string Text = String.Empty;
            Uri uri = new Uri(ResolveServerUrl(Url, false));

            HttpWebRequest Req = (HttpWebRequest)HttpWebRequest.Create(uri);
            Req.Method = WebRequestMethods.Http.Get;
            try
            {
                HttpWebResponse Resp = (HttpWebResponse)Req.GetResponse();

                if (Resp.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader SR = new StreamReader(Resp.GetResponseStream());
                    Text = SR.ReadToEnd();
                }
                Resp.Close();
            }
            catch (Exception)
            {
            }
            return Text;
        }


        ///////////////////////////////////////////////
        public string ConvertRelativeUrlToAbsoluteUrl(string Url)
        ///////////////////////////////////////////////
        {
            HttpRequest Req = this.Context.Request;

            if (Url.ToLower().StartsWith("http"))
                return Url;

            if (Req.IsSecureConnection)
                return string.Format("https://{0}{1}", Req.Url.Host, Page.ResolveUrl(Url));
            else
                return string.Format("http://{0}{1}", Req.Url.Host, Page.ResolveUrl(Url));
        }


        ///////////////////////////////////////////////
        public static string ResolveUrl2(string relativeUrl)
        ///////////////////////////////////////////////
        {
            if (relativeUrl == null) throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
                return relativeUrl;

            int idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                int idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(HttpRuntime.AppDomainAppVirtualPath);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/') sbUrl.Append('/');

            // found question mark already? query string, do not touch!
            bool foundQM = false;
            bool foundSlash; // the latest char was a slash?
            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else foundSlash = false;
            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?') foundQM = true;
                    else
                    {
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash) continue;
                            else
                            {
                                sbUrl.Append('/');
                                foundSlash = true;
                                continue;
                            }
                        }
                        else if (foundSlash) foundSlash = false;
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }

        ///////////////////////////////////////////////
        internal StringBuilder WrapHtmlInDocument(StringBuilder Html)
        ///////////////////////////////////////////////
        {
            StringBuilder HtmlDoc = new StringBuilder();

            HtmlDoc.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            HtmlDoc.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            HtmlDoc.Append("<head>");
            HtmlDoc.Append("<meta http-equiv=\"Content-Type\" content=\"text/Html; charset=utf-8\"></meta>");
            if (this.SaveType != "pdf" && BuildMode != BuildModes.DataSource)
                HtmlDoc.Append("<style>" + GridCSS() + "</style>");
            HtmlDoc.Append("</head>");
            HtmlDoc.Append("<body>");
            HtmlDoc.Append(Html);
            HtmlDoc.Append("</body>");
            HtmlDoc.Append("</html>");

            return HtmlDoc;
        }

        ///////////////////////////////////////////////
        protected virtual void BuildOptimisedPageSQL(QueryCommandConfig Query, int firstRecord, int lastRecord)
        ///////////////////////////////////////////////
        {
            if (Database.DatabaseVersion < 9 || this.GroupBy || this.ProcedureName != "")
                return;

            string FilterPart = BuildFilterPart();
            string OrderPart = BuildOrderPart();

            if (OrderPart == "")
                OrderPart = DefaultOrderBy(true);
            else
                OrderPart = ConvertOrderPartFromOrdinalsToNames(OrderPart);

            if (String.IsNullOrEmpty(FilterPart))
                FilterPart = "1=1";

            string OverPart = String.Empty;

            if (this.NoSort == false)
                if (OrderPart != "")
                    OverPart += " OVER( order by " + OrderPart + ")";

            string Sql = "select " + BuildSelectPart(QueryBuildModes.Normal, true) + " from " +
                "( select ROW_NUMBER()" + OverPart + " as RowNumber," + BuildSelectPart(QueryBuildModes.Normal) +
                " from " + FromPart + " where " + FilterPart + ") as GridData " +
                "where RowNumber between @StartRow and @EndRow";

            ListDictionary P = BuildParams(QueryBuildModes.Normal);
            Query.Sql = ReplaceWithCorrectParams(Sql, P);
            Query.Params.Clear();

            Query.Params = P;
            Query.Params["StartRow"] = firstRecord;
            Query.Params["EndRow"] = lastRecord;

        }


        ///////////////////////////////////////////////
        private void BuildColumnFilterSql()
        ///////////////////////////////////////////////
        {
            string Sql = string.Join(" and ", BuildColumnFilter().ToArray());

            ListDictionary Params = new ListDictionary();

            AddColumnFilterParams(Params);
            Resp["params"] = Params;
            Resp["sql"] = ReplaceWithCorrectParams(Sql, Params);
        }

        ///////////////////////////////////////////////
        internal List<string> BuildColumnFilter()
        ///////////////////////////////////////////////
        {
            List<string> FP = new List<string>();
            if (this.ColumnFilterSql.Count == 0)
                return FP;

            for (int j = 0; j < this.ColumnFilterSql.Count; j++)
            {
                string SearchPart = this.ColumnFilterSql[j].ToString();
                for (int i = 0; i < Columns.Count; i++)
                {
                    string PlaceHolder = "{col" + Columns[i].ColumnKey + "}";

                    if (!SearchPart.Contains(PlaceHolder + " {op}"))
                        continue;

                    GridColumn Col = (GridColumn)Columns[i];

                    if (Col.Lookup != "" && Col.FilterMode == GridColumn.FilterColumnSelectMode.Input)
                    {
                        SearchPart = SearchPart.Replace(PlaceHolder, "");
                        SearchPart = PlaceHolder + " in (" + SearchLookupSql(Col, SearchPart) + ")";
                    }

                    string Op = "=";

                    if (this.ColumnFilterParams[Col.ColumnKey] != null)
                        if (this.ColumnFilterParams[Col.ColumnKey].ToString() == NullValueToken)
                            SearchPart = PlaceHolder + " is null";

                    if (Col.FilterMode == GridColumn.FilterColumnSelectMode.Input)
                    {
                        string DataType = Col.DataType;
                        if (Col.Lookup != "")
                            DataType = Col.LookupDataType;

                        if (DataType == "String")
                            Op = "like";
                    }

                    if (Col.DataType == "Boolean")
                        if (Convert.ToBoolean(this.ColumnFilterParams[Col.ColumnKey]))
                            Op = "<>";

                    SearchPart = SearchPart.Replace("{op}", Op).Replace(PlaceHolder, StripColumnRename(Col.ColumnExpression));
                }
                this.ColumnFilterSql[j] = SearchPart;
            }

            FP.Add("(" + String.Join(" and ", (string[])this.ColumnFilterSql.ToArray(typeof(string))) + ")");

            return FP;
        }

        ///////////////////////////////////////////////
        internal void AddColumnFilterParams(ListDictionary P)
        ///////////////////////////////////////////////
        {
            foreach (string Key in this.ColumnFilterParams.Keys)
            {
                if (ColumnFilterParams[Key].ToString() == NullValueToken)
                    continue;

                GridColumn C = (GridColumn)ColumnFromKey(Key);

                string ParamName = "colfilterCol" + Key + "Param0";
                //object ParamValue = this.ColumnFilterParams[Key];

                string DT = C.DataType;
                if (C.FilterMode == GridColumn.FilterColumnSelectMode.Input)
                    if (C.Lookup != String.Empty)
                        DT = C.LookupDataType;

                object ParamValue = ConvertToDbParam(ColumnFilterParams[Key], DT);

                if (C.FilterMode == GridColumn.FilterColumnSelectMode.Input)
                {
                    switch (DT)
                    {
                        case "String":
                            if (!ParamValue.ToString().Contains("%"))
                                ParamValue = "%" + ParamValue + "%";
                            break;
                    }
                }
                else
                {
                    switch (DT)
                    {
                        case "Boolean":
                            ParamValue = 0;
                            break;
                    }
                }

                P.Add(ParamName, ParamValue);
            }
        }

        ///////////////////////////////////////////////
        internal void ImageData()
        ///////////////////////////////////////////////
        {
            if (String.IsNullOrEmpty(HttpContext.Current.Request["key"]))
                return;

            string Key = HttpContext.Current.Request["key"].ToString();
            bool Download = false;
            bool Inline = false;

            if (!String.IsNullOrEmpty(HttpContext.Current.Request["download"]))
                Download = (HttpContext.Current.Request["download"].ToString().ToLower() == "true");

            if (!String.IsNullOrEmpty(HttpContext.Current.Request["inline"]))
                Inline = (HttpContext.Current.Request["inline"].ToString().ToLower() == "true");

            if (!(Session[Key] is ImageData))
                return;

            ImageData ID = HttpContext.Current.Session[Key] as ImageData;

            HttpContext.Current.Response.ContentType = ID.ContentType;

            Context.Response.AddHeader("content-disposition", (((!ID.IsImage || Download) && !Inline) ? "attachment" : "inline") + "; filename=\"" + ID.FileName + "\"");
            HttpContext.Current.Response.BinaryWrite(ID.Data);
            HttpContext.Current.Response.End();
            //HttpContext.Current.Session.Remove(Key);
        }


        ///////////////////////////////////////////////
        internal string BuildPdfSettingsDialog()
        ///////////////////////////////////////////////
        {
            PdfSettingsDialog D = new PdfSettingsDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildConfigDialog()
        ///////////////////////////////////////////////
        {
            ConfigDialog D = new ConfigDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildColumnPickerDialog()
        ///////////////////////////////////////////////
        {
            ColumnPickerDialog D = new ColumnPickerDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildChartConfigDialog()
        ///////////////////////////////////////////////
        {
            return RenderControlToString(new ChartConfigDialog(this).Build());
        }

        ///////////////////////////////////////////////
        internal string BuildChartDialog()
        ///////////////////////////////////////////////
        {
            return RenderControlToString(new ChartDialog(this).Build());
        }

        ///////////////////////////////////////////////
        public string GridCSS()
        ///////////////////////////////////////////////
        {
            if (this.GetTheme() == UI.Themes.Bootstrap)
                return GetCSS(new string[] { "DbNetSuite", "Themes.bootstrap", "Themes.bootstrap.mod" });
            else
                return GetCSS(new string[] { "DbNetSuite", "DbNetGrid" });
        }

        ///////////////////////////////////////////////
        public string ViewCSS()
        ///////////////////////////////////////////////
        {
            return GetCSS(new string[] { "DbNetSuite", "ViewDialog" });
        }

        ///////////////////////////////////////////////
        private StringBuilder BuildXmlExportGrid()
        ///////////////////////////////////////////////
        {

            GetLookupTables();
            QueryCommandConfig sql = BuildSQL();
            Database.ExecuteQuery(sql);
            XmlDocument exportXml = new XmlDocument();
            XmlElement rootElement = exportXml.CreateElement("root");
            exportXml.AppendChild(rootElement);

            var firstRecord = (PageSize * (this.CurrentPage - 1)) + 1;
            var lastRecord = (firstRecord + (PageSize - 1));
            var recordCounter = 0;

            if (!this.OutputCurrentPage)
            {
                firstRecord = 1;
                lastRecord = Int32.MaxValue;
            }

            while (Database.Reader.Read())
            {
                recordCounter++;
                if (recordCounter < firstRecord || recordCounter > lastRecord)
                    continue;

                XmlElement dataRow = exportXml.CreateElement("row");
                rootElement.AppendChild(dataRow);

                foreach (GridColumn column in Columns)
                {
                    object Val = GetDBValues(column).First.ToString();

                    string elementName = column.ColumnName;

                    try
                    {
                        XmlConvert.VerifyName(elementName);
                    }
                    catch (Exception)
                    {
                        elementName = String.Format("column{0}", column.ColumnIndex);
                    }

                    if (!column.Display)
                    {
                        dataRow.SetAttribute(elementName, Val.ToString());
                        continue;
                    }

                    XmlElement dataCell = exportXml.CreateElement(elementName);
                    dataRow.AppendChild(dataCell);
                    dataCell.InnerText = Val.ToString();

                }
            }

            return new StringBuilder(exportXml.OuterXml);
        }

        ///////////////////////////////////////////////
        public void ValidateBatchUpdate()
        ///////////////////////////////////////////////
        {
            object[] UpdateParams = (object[])Req["parameters"];
            object[] PrimaryKeys = (object[])Req["primaryKeys"];
            object[] RowIndexes = (object[])Req["rowIndex"];

            object[] NewParametersList = new object[UpdateParams.Length];
            object[] CurrentRecordList = new object[UpdateParams.Length];

            Resp["ok"] = true;

            for (int i = 0; i < PrimaryKeys.Length; i++)
            {
                Dictionary<string, object> NewParameters = new Dictionary<string, object>();
                this.CurrentRecord = new Dictionary<string, object>();

                if (!ValidateEdit((Dictionary<string, object>)UpdateParams[i], (Dictionary<string, object>)PrimaryKeys[i], NewParameters))
                {
                    Resp["rowIndex"] = RowIndexes[i];
                    return;
                }

                NewParametersList[i] = NewParameters;
                CurrentRecordList[i] = this.CurrentRecord;
            }

            Resp["parameters"] = NewParametersList;
            Resp["currentRecord"] = CurrentRecordList;
            Resp["rowIndex"] = Req["rowIndex"];
        }


        ///////////////////////////////////////////////
        public void BatchUpdate()
        ///////////////////////////////////////////////
        {
            if (IsReadOnly())
            {
                Resp["ok"] = false;
                Resp["message"] = "Database modification has been disabled";
                return;
            }

            object[] UpdateParams = (object[])Req["parameters"];
            object[] PrimaryKeys = (object[])Req["primaryKeys"];
            object[] RowIndexes = (object[])Req["rowIndex"];

            string TableName = this.EditTableName();

            Resp["ok"] = true;
            Dictionary<string, object> Parameters;

            for (int i = 0; i < PrimaryKeys.Length; i++)
            {
                Parameters = (Dictionary<string, object>)UpdateParams[i];
                if (!ValidateParameters(Parameters))
                {
                    Resp["rowIndex"] = RowIndexes[i];
                    return;
                }
            }

            for (int i = 0; i < PrimaryKeys.Length; i++)
            {
                Parameters = (Dictionary<string, object>)UpdateParams[i];

                Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)PrimaryKeys[i];

                if (!ApplyRecordUpdate(Parameters, TableName, PrimaryKey))
                {
                    Resp["rowIndex"] = RowIndexes[i];
                    Resp["ok"] = false;
                    return;
                }
            }

            Resp["message"] = Translate("RecordUpdated");
        }
    }

    /////////////////////////////////////////////// 
    public class GridColumn : DbColumn
    ///////////////////////////////////////////////
    {
        /////////////////////////////////////////////// 
        public enum AggregateValues
        /////////////////////////////////////////////// 
        {
            None,
            Sum,
            Avg,
            Min,
            Max,
            Count
        }

        /////////////////////////////////////////////// 
        public enum BooleanDisplayModeValues
        /////////////////////////////////////////////// 
        {
            Checkbox,
            TrueFalse,
            YesNo
        }

        /////////////////////////////////////////////// 
        public enum FilterColumnSelectMode
        /////////////////////////////////////////////// 
        {
            List,
            Input
        }

        internal object AggregateValue = 0;
        internal bool PrintColumnValue = true;
        internal bool PrintGroupHeader = true;

        private AggregateValues _Aggregate = AggregateValues.None;
        [
        CategoryAttribute("Database"),
        DefaultValue(AggregateValues.None),
        Description("Specifies the type of aggregate to be displayed at the bottom of the column.")
        ]
        public AggregateValues Aggregate
        {
            get { return _Aggregate; }
            set { _Aggregate = value; }
        }

        private BooleanDisplayModeValues _BooleanDisplayMode = BooleanDisplayModeValues.Checkbox;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(BooleanDisplayModeValues.Checkbox),
        Description("Describes how boolean values are displayed in the grid.")
        ]
        public BooleanDisplayModeValues BooleanDisplayMode
        {
            get { return _BooleanDisplayMode; }
            set { _BooleanDisplayMode = value; }
        }

        private bool _ClearDuplicateValue = false;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(false),
        Description("Clears duplicate values in summary total reports to improve clarity.")
        ]
        public bool ClearDuplicateValue
        {
            get { return _ClearDuplicateValue; }
            set { _ClearDuplicateValue = value; }
        }

        private bool _Edit = true;
        [
        CategoryAttribute("Edit"),
        DefaultValue(true),
        Description("Determines if the column will be displayed on the edit form.")
        ]
        public bool Edit
        {
            get { return _Edit; }
            set { _Edit = value; }
        }

        private int _EditColumnOrder = 0;
        [
        CategoryAttribute("Edit"),
        DefaultValue(0),
        Description("Overrides the default order in which columns are presented in the edit dialog")
        ]
        public int EditColumnOrder
        {
            get { return _EditColumnOrder; }
            set { _EditColumnOrder = value; }
        }

        private string _EditFormat = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Applies a .NET formatting string to the column value.")
        ]
        public string EditFormat
        {
            get { return _EditFormat; }
            set { _EditFormat = value; }
        }

        private string _EditLookup = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Lookup for edit dialog.")
        ]
        public string EditLookup
        {
            get { return _EditLookup; }
            set { _EditLookup = value; }
        }

        private string _EditStyle = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Provide a CSS style that will be applied to the control in the edit dialog.")
        ]
        public string EditStyle
        {
            get { return _EditStyle; }
            set { _EditStyle = value; }
        }

        private bool _Filter = false;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(false),
        Description("Adds a filter cell to the column.")
        ]
        public bool Filter
        {
            get { return _Filter; }
            set { _Filter = value; }
        }

        private FilterColumnSelectMode _FilterMode = FilterColumnSelectMode.List;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(FilterColumnSelectMode.List),
        Description("Specifies the column filter selection mode.")
        ]
        public FilterColumnSelectMode FilterMode
        {
            get { return _FilterMode; }
            set { _FilterMode = value; }
        }


        private bool _GroupHeader = false;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(false),
        Description("Indicates that the column should be dislpayed as a grouping header line")
        ]
        public bool GroupHeader
        {
            get { return _GroupHeader; }
            set { _GroupHeader = value; }
        }

        private bool _GridData = true;
        [
        CategoryAttribute("Display"),
        DefaultValue(true),
        Description("Suppresses the selection of of a column value for the grid.")
        ]
        public bool GridData
        {
            get { return _GridData; }
            set { _GridData = value; }
        }

        private bool _InlineEdit = false;
        [
        CategoryAttribute("Edit"),
        DefaultValue(false),
        Description("Specifies that the column value can be edited in the grid.")
        ]
        public bool InlineEdit
        {
            get { return _InlineEdit; }
            set { _InlineEdit = value; }
        }

        private int _EditMaxThumbnailHeight = 30;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(30),
        Description("If the field contains binary image data a thumbnail will be displayed.  All images will be scaled so that they are no taller than this value (px) in the edit dialog.")
        ]
        public int EditMaxThumbnailHeight
        {
            get { return _EditMaxThumbnailHeight; }
            set { _EditMaxThumbnailHeight = value; }
        }

        private string _Width = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Defines the width of the grid column.")
        ]
        public string Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private bool _OrderByDescending = false;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(false),
        Description("Indicates order by for column when used for total break or group heading ")
        ]
        public bool OrderByDescending
        {
            get { return _OrderByDescending; }
            set { _OrderByDescending = value; }
        }

        private bool? _Output = null;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(false),
        Description("Indicates if the column should appear when the column is output")
        ]
        public bool? Output
        {
            get { return _Output; }
            set { _Output = value; }
        }

        private bool _TotalBreak = false;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(false),
        Description("Indicates that totals should be displayed on a change of value in this column")
        ]
        public bool TotalBreak
        {
            get { return _TotalBreak; }
            set { _TotalBreak = value; }
        }

        private bool _View = true;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(true),
        Description("Adds the column to the view dialog.")
        ]
        public bool View
        {
            get { return _View; }
            set { _View = value; }
        }


        private bool _ColumnPicker = true;
        [
        CategoryAttribute("Display"),
        DefaultValue(true),
        Description("Determines if the column will be displayed in the column picker.")
        ]
        public bool ColumnPicker
        {
            get { return _ColumnPicker; }
            set { _ColumnPicker = value; }
        }

        ///////////////////////////////////////////////
        public GridColumn()
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public GridColumn(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            this.ColumnExpression = ColumnExpression;
            //this.Label = Shared.GenerateLabel(ColumnExpression);
        }
    }


    ///////////////////////////////////////////////
    public class GridColumnCollection : DbColumnCollection
    ///////////////////////////////////////////////
    {
        public GridColumn this[int index]
        {
            get
            {
                return (GridColumn)this.List[index];
            }
            set
            {
                GridColumn column = (GridColumn)value;
                this.List[index] = column;
            }
        }

        ///////////////////////////////////////////////
        public void Add(GridColumn column)
        ///////////////////////////////////////////////
        {
            base.Add(column);
            //        this.List.Add(column);
        }

        ///////////////////////////////////////////////
        public int IndexOf(GridColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }

    public struct KeyValuePair : IComparable
    {
        private string theKey;
        private string theValue;

        public string Key
        {
            get
            {
                return theKey;
            }
        }

        public string Value
        {
            get
            {
                return theValue;
            }
        }

        public KeyValuePair(string key, string value)
        {
            theKey = key;
            theValue = value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            else
            {
                KeyValuePair check = (KeyValuePair)obj;
                return Value.CompareTo(check.Value);
            }
        }
    }
}

