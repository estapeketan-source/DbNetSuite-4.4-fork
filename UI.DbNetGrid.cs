using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security;
using System.Security.Permissions;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Net;

[assembly: TagPrefix("DbNetLink.DbNetSuite.UI", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
    [
    Designer(typeof(DbNetGridControlDesigner)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxData("<{0}:DbNetGrid runat=\"server\" ConnectionString=\"\" FromPart=\"\"></{0}:DbNetGrid>")
    ]

    ////////////////////////////////////////////////////////////////////////////
    public class DbNetGrid : DbNetLink.DbNetSuite.UI.GridEdit
    ////////////////////////////////////////////////////////////////////////////
    {
        public enum ClientEvents
        {
            onBeforeBulkInsertLookup,
            onBeforeBulkInsert,
            onBeforeEditInitialized,
            onBeforeFilePreview,
            onBeforeFileUploadValidate,
            onBeforeFileUploaded,
            onBeforeInitialized,
            onBeforeMailMerge,
            onBeforeOutput,
            onBeforePageLoaded,
            onBeforeRecordBatchUpdated,
            onBeforeRecordDeleted,
            onBeforeRecordValidated,
            onBeforeTinyMceInit,
            onBeforeUserProfileRestored,
            onBeforeUserProfileSaved,
            onCellTransform,
            onColumnMappingConfigured,
            onColumnsSelected,
            onColumnSort,
            onColumnValidate,
            onDataUploaded,
            onDependentListLoaded,
            onEditDialogInitialized,
            onFieldValueChanged,
            onFileUploaded,
            onFilterColumnChange,
            onOutput,
            onHeaderCellTransform,
            onInitialized,
            onInsertInitialize,
            onPageLoaded,
            onPageLoadError,
            onParentFilterAssigned,
            onRecordBatchUpdated,
            onRecordDeleteError,
            onRecordDeleted,
            onRecordInsertError,
            onRecordInserted,
            onRecordSelected,
            onRecordUpdateError,
            onRecordUpdated,
            onRowSelected,
            onRowTransform,
            onRecordValidate,
            onSearchDialogApply,
            onSearchPanelInitialized,
            onToolbarConfigured,
            onUniqueConstraintViolated,
            onValidationFailed,
            onViewDialogInitialized,
            onViewRecordSelected,
            onViewOutput
        };


        private bool _AutoRowSelect = true;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Automatically selects the first row in the grid when a page is loaded (defaults to true).")
        ]
        ////////////////////////////////////////
        public bool AutoRowSelect
        ////////////////////////////////////////
        {
            get { return _AutoRowSelect; }
            set { _AutoRowSelect = value; }
        }

        private string _Caption = "";
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Adds a Caption element to the top of the grid table.")
        ]
        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }

        private bool _Chart = false;
        [
        Category("Toolbar"),
        DefaultValue(false),
        Description("Adds access to the chart configuration dialog to the toolbar")
        ]
        ////////////////////////////////////////
        public bool Chart
        ////////////////////////////////////////
        {
            get { return _Chart; }
            set { _Chart = value; }
        }

        private bool _ChartSerialize = false;
        [
        Category("Output"),
        DefaultValue(false),
        Description("Adds a serialization button to the chart dialog")
        ]
        ////////////////////////////////////////
        public bool ChartSerialize
        ////////////////////////////////////////
        {
            get { return _ChartSerialize; }
            set { _ChartSerialize = value; }
        }

        private string _ChartConfig = "";
        [
        CategoryAttribute("Output"),
        DefaultValue(""),
        Description("Specifies a JSON encoded chart configuration.")
        ]
        public string ChartConfig
        {
            get { return _ChartConfig; }
            set { _ChartConfig = value; }
        }

        private bool _Config = false;
        [
        Category("Toolbar"),
        DefaultValue(false),
        Description("Adds access to the runtime configuration dialog to the toolbar")
        ]
        ////////////////////////////////////////
        public bool Config
        ////////////////////////////////////////
        {
            get { return _Config; }
            set { _Config = value; }
        }

        private string _ColumnFilterSql = "";
        [
        CategoryAttribute("Filtering"),
        DefaultValue(""),
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string ColumnFilterSql
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ColumnFilterSql; }
            set { _ColumnFilterSql = value; }
        }

        private bool _Copy = true;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Add copy to clipboard button to toolbar (IE only)")
        ]
        ////////////////////////////////////////
        public bool Copy
        ////////////////////////////////////////
        {
            get { return _Copy; }
            set { _Copy = value; }
        }

        private bool _ColumnPicker = false;
        [
        Category("Toolbar"),
        DefaultValue(false),
        Description("Add the column selection dialog button to the toolbar")
        ]
        ////////////////////////////////////////
        public bool ColumnPicker
        ////////////////////////////////////////
        {
            get { return _ColumnPicker; }
            set { _ColumnPicker = value; }
        }

        private bool _DragAndDrop = true;
        [
        Category("Toolbar"),
        Description("Enables/disables re-arranging columns by drag and drop")
        ]
        ////////////////////////////////////////
        public bool DragAndDrop
        ////////////////////////////////////////
        {
            get { return _DragAndDrop; }
            set { _DragAndDrop = value; }
        }

        private string _EditDialogHeight = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue("0px"),
        Description("Sets the height of the Edit Dialog.")
        ]
        ////////////////////////////////////////
        public string EditDialogHeight
        ////////////////////////////////////////
        {
            get { return _EditDialogHeight; }
            set { _EditDialogHeight = value; }
        }

        private int _EditLayoutColumns = 1;
        [
        CategoryAttribute("Edit"),
        DefaultValue(1),
        Description("Defines the number of columns over which the generated edit dialog layout is distributed.")
        ]
        ////////////////////////////////////////
        public int EditLayoutColumns
        ////////////////////////////////////////
        {
            get { return _EditLayoutColumns; }
            set { _EditLayoutColumns = value; }
        }

        private string _FinalTotalLabel = "";
        [
        CategoryAttribute("Totals"),
        DefaultValue("Final Totals"),
        Description("Label applied to the final totals row for column aggregation")
        ]

        private string _ExportFileName = String.Empty;
        [
        CategoryAttribute("Output"),
        DefaultValue(""),
        Description("Specifies the download file name for the exported grid")
        ]
        public string ExportFileName
        {
            get { return _ExportFileName; }
            set { _ExportFileName = value; }
        }

        private string _ExportFolder = String.Empty;
        [
        CategoryAttribute("Output"),
        DefaultValue(""),
        Description("Specifies the name of the web folder into which the exported grid is saved")
        ]
        public string ExportFolder
        {
            get { return _ExportFolder; }
            set { _ExportFolder = value; }
        }

        ////////////////////////////////////////
        public string FinalTotalLabel
        ////////////////////////////////////////
        {
            get { return _FinalTotalLabel; }
            set { _FinalTotalLabel = value; }
        }

        private DbNetSuite.DbNetGrid.FilterColumnModeValues _FilterColumnMode = DbNetSuite.DbNetGrid.FilterColumnModeValues.Simple;
        [
        CategoryAttribute("Filtering"),
        DefaultValue(DbNetSuite.DbNetGrid.FilterColumnModeValues.Simple),
        Description("Determines the way in which filter column selections are combined")
        ]
        public DbNetSuite.DbNetGrid.FilterColumnModeValues FilterColumnMode
        {
            get { return _FilterColumnMode; }
            set { _FilterColumnMode = value; }
        }

        private string _FixedOrderBy = "";
        [
        CategoryAttribute("Sorting"),
        DefaultValue(""),
        Description("Fixes the top level of sorting in the grid")
        ]
        public string FixedOrderBy
        {
            get { return _FixedOrderBy; }
            set { _FixedOrderBy = value; }
        }

        private int _FrozenColumns = 0;
        [
        CategoryAttribute("Display"),
        DefaultValue(0),
        Description("Freezes the first n columns against which the remaining columns can be scrolled horizontally")
        ]
        public int FrozenColumns
        {
            get { return _FrozenColumns; }
            set { _FrozenColumns = value; }
        }


        private string _Having = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Specifies the 'group by' having clause")
        ]
        public string Having
        {
            get { return _Having; }
            set { _Having = value; }
        }


        private bool _OutputCurrentPage = false;
        [
        Category("Output"),
        DefaultValue(false),
        Description("Restricts the output to the current page only")
        ]
        ////////////////////////////////////////
        public bool OutputCurrentPage
        ////////////////////////////////////////
        {
            get { return _OutputCurrentPage; }
            set { _OutputCurrentPage = value; }
        }

        private int _OutputPageSize = 0;
        [
        Category("Output"),
        DefaultValue(0),
        Description("When set to a value greater than 0 will split the output into pages each with the size specified by the property.")
        ]
        ////////////////////////////////////////
        public int OutputPageSize
        ////////////////////////////////////////
        {
            get { return _OutputPageSize; }
            set { _OutputPageSize = value; }
        }

        private bool _CustomSave = false;
        [
        Category("Output"),
        DefaultValue(false),
        Description("Send the output to the client control for customization before output")
        ]
        ////////////////////////////////////////
        public bool CustomSave
        ////////////////////////////////////////
        {
            get { return _CustomSave; }
            set { _CustomSave = value; }
        }

        private bool _GroupBy = false;
        [
        Category("Data"),
        DefaultValue(false),
        Description("Group data by non-aggregate columns")
        ]
        ////////////////////////////////////////
        public bool GroupBy
        ////////////////////////////////////////
        {
            get { return _GroupBy; }
            set { _GroupBy = value; }
        }

        DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarOptions _InlineEditToolbarLocation = DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarOptions.Bottom;
        [
        CategoryAttribute("Edit"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarOptions.Bottom),
        Description("Configures the position of the inline edit toolbars.")
        ]
        ////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarOptions InlineEditToolbarLocation
        ////////////////////////////////////////
        {
            get { return _InlineEditToolbarLocation; }
            set { _InlineEditToolbarLocation = value; }
        }

        DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarButtonAlignment _InlineEditToolbarButtonLocation = DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarButtonAlignment.Right;
        [
        CategoryAttribute("Edit"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarButtonAlignment.Right),
        Description("Configures the position of the inline edit toolbar buttons.")
        ]
        ////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetGrid.InlineEditToolbarButtonAlignment InlineEditToolbarButtonLocation
        ////////////////////////////////////////
        {
            get { return _InlineEditToolbarButtonLocation; }
            set { _InlineEditToolbarButtonLocation = value; }
        }

        private bool _MailMerge = false;
        [
        Category("Toolbar"),
        DefaultValue(false),
        Description("Enabled the Mail-merge output option (IE only)")
        ]
        ////////////////////////////////////////
        public bool MailMerge
        ////////////////////////////////////////
        {
            get { return _MailMerge; }
            set { _MailMerge = value; }
        }

        private ArrayList _MailMergeDocuments = new ArrayList();
        [
        Category("Output"),
        Description("Mail Merge Documents"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(MailMergeDocumentCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList MailMergeDocuments
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_MailMergeDocuments == null)
                    _MailMergeDocuments = new ArrayList();
                return _MailMergeDocuments;
            }
        }

        private bool _MultiRowSelect = false;
        [
        Category("Configuration"),
        Description("Display checkboxes to allow multirow")
        ]
        ////////////////////////////////////////
        public bool MultiRowSelect
        ////////////////////////////////////////
        {
            get { return _MultiRowSelect; }
            set { _MultiRowSelect = value; }
        }

        private bool _NoSort = false;
        [
        Category("Sorting"),
        Description("Suppresses any sorting of the data.")
        ]
        ////////////////////////////////////////
        public bool NoSort
        ////////////////////////////////////////
        {
            get { return _NoSort; }
            set { _NoSort = value; }
        }

        private DbNetSuite.DbNetGrid.MultiRowSelectLocations _MultiRowSelectLocation = DbNetSuite.DbNetGrid.MultiRowSelectLocations.Right;
        [
        Category("Configuration"),
        Description("Defines location of multi-row selection checkboxes")
        ]
        ////////////////////////////////////////
        public DbNetSuite.DbNetGrid.MultiRowSelectLocations MultiRowSelectLocation
        ////////////////////////////////////////
        {
            get { return _MultiRowSelectLocation; }
            set { _MultiRowSelectLocation = value; }
        }

        private bool _OptimizeForLargeDataSet = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Optimizes the data selection algorithm for very large datasets")
        ]
        ////////////////////////////////////////
        public bool OptimizeForLargeDataSet
        ////////////////////////////////////////
        {
            get { return _OptimizeForLargeDataSet; }
            set { _OptimizeForLargeDataSet = value; }
        }

        private bool _OptimizeExportForLargeDataSet = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Optimizes the grid export for very large datasets")
        ]
        ////////////////////////////////////////
        public bool OptimizeExportForLargeDataSet
        ////////////////////////////////////////
        {
            get { return _OptimizeExportForLargeDataSet; }
            set { _OptimizeExportForLargeDataSet = value; }
        }

        private bool _OutputPageSelect = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds current page output option to the toolbar")
        ]
        public bool OutputPageSelect
        {
            get { return _OutputPageSelect; }
            set { _OutputPageSelect = value; }
        }

        private int _PageSize = 20;
        [
        CategoryAttribute("Layout"),
        DefaultValue(20),
        Description("Defines the number of rows that will be displayed by the grid on each page.")
        ]
        public int PageSize
        {
            get { return _PageSize; }
            set { _PageSize = value; }
        }

        private bool _Print = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Print button to the toolbar.")
        ]
        public bool Print
        {
            get { return _Print; }
            set { _Print = value; }
        }

        private String _ProcedureName = String.Empty;
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Name of stored procedure")
        ]
        public String ProcedureName
        {
            get { return _ProcedureName; }
            set { _ProcedureName = value; }
        }

        private ArrayList _ProcedureParameters = new ArrayList();
        [
        Category("Data"),
        Description("Procedure Parameters"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ParameterCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList ProcedureParameters
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_ProcedureParameters == null)
                    _ProcedureParameters = new ArrayList();
                return _ProcedureParameters;
            }
        }


        private bool _Save = true;
        [
        Category("Toolbar"),
        DefaultValue(false),
        Description("Adds save/export options to the toolbar")
        ]
        ////////////////////////////////////////
        public bool Save
        ////////////////////////////////////////
        {
            get { return _Save; }
            set { _Save = value; }
        }

        private String _SaveOptions = "HTML,Word,Excel,XML,CSV,PDF";
        [
        CategoryAttribute("Toolbar"),
        DefaultValue("HTML,Word,Excel,XML,CSV,PDF"),
        Description("Selectable save options")
        ]
        public String SaveOptions
        {
            get { return _SaveOptions; }
            set { _SaveOptions = value; }
        }

        private string _SelectModifier = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Modfies the select statement e.g. top 100 or distinct")
        ]
        public String SelectModifier
        {
            get { return _SelectModifier; }
            set { _SelectModifier = value; }
        }

        private DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions _ToolbarLocation = DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions.Top;
        [
        Category("Toolbar"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions.Top),
        Description("Controls the location of the toolbar")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions ToolbarLocation
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ToolbarLocation; }
            set { _ToolbarLocation = value; }
        }

        private DbNetSuite.DbNetGrid.UpdateModes _UpdateMode = DbNetSuite.DbNetGrid.UpdateModes.Row;
        [
        CategoryAttribute("Edit"),
        DefaultValue(DbNetSuite.DbNetGrid.UpdateModes.Row),
        Description("Determines the mode of record editing for the grid")
        ]
        public DbNetSuite.DbNetGrid.UpdateModes UpdateMode
        {
            get { return _UpdateMode; }
            set { _UpdateMode = value; }
        }

        private bool _UpdateRow = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Update button to the toolbar.")
        ]
        public bool UpdateRow
        {
            get { return _UpdateRow; }
            set { _UpdateRow = value; }
        }

        private bool _View = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the View button to the toolbar.")
        ]
        public bool View
        {
            get { return _View; }
            set { _View = value; }
        }

        private string _ViewDialogHeight = "";
        [
        CategoryAttribute("View"),
        DefaultValue(""),
        Description("Height of the View Dialog.")
        ]
        public string ViewDialogHeight
        {
            get { return _ViewDialogHeight; }
            set { _ViewDialogHeight = value; }
        }

        private string _ViewDialogWidth = "";
        [
        CategoryAttribute("View"),
        DefaultValue(""),
        Description("Height of the View Dialog.")
        ]
        public string ViewDialogWidth
        {
            get { return _ViewDialogWidth; }
            set { _ViewDialogWidth = value; }
        }

        private int _ViewLayoutColumns = 1;
        [
        CategoryAttribute("View"),
        DefaultValue(1),
        Description("Number of columns in the View Dialog.")
        ]
        public int ViewLayoutColumns
        {
            get { return _ViewLayoutColumns; }
            set { _ViewLayoutColumns = value; }
        }

        private bool _ViewPrint = true;
        [
        CategoryAttribute("View"),
        DefaultValue(true),
        Description("Adds a Print button to the view dialog.")
        ]
        public bool ViewPrint
        {
            get { return _ViewPrint; }
            set { _ViewPrint = value; }
        }

        private Xml _ViewTemplate = null;
        [
        Category("View"),
 //       Editor(typeof(DbNetEditLayoutEditor), typeof(UITypeEditor)),
        Description("HTML layout for view dialog"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////
        public Xml ViewTemplate
        ////////////////////////////////////////
        {
            get
            {
                if (_ViewTemplate == null)
                    _ViewTemplate = new Xml();
                return _ViewTemplate;
            }
            set { _ViewTemplate = value; }
        }


        private string _ViewTemplatePath = "";
        [
        Category("Content"),
        Description("Virtual path to HTML template file for view gialog")
        ]
        ////////////////////////////////////////
        public string ViewTemplatePath
        ////////////////////////////////////////
        {
            get { return _ViewTemplatePath; }
            set { _ViewTemplatePath = value; }
        }

        private DbNetGrid _NestedGrid;
        [
        Category("Behavior"),
        Description("Nested Grid"),
        DesignerSerializationVisibility(
        DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]

        ////////////////////////////////////////
        public DbNetGrid NestedGrid
        ////////////////////////////////////////
        {
            get { return _NestedGrid; }
            set { _NestedGrid = value; }
        }

        ArrayList _GridClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(GridClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList GridClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_GridClientEvents == null)
                {
                    _GridClientEvents = new ArrayList();
                }
                return _GridClientEvents;
            }
        }

        private GridColumnCollection _GridColumns = new GridColumnCollection();
        [
        Category("Display"),
        Description("Grid Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        Editor(typeof(GridColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public GridColumnCollection GridColumns
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_GridColumns == null)
                    _GridColumns = new GridColumnCollection();
                return _GridColumns;
            }
        }

        #region Legacy Properties

        [Obsolete("This method will be removed from future versions")]
        public string SelectPart = "";
        [Obsolete("This method will be removed from future versions")]
        public string Headings = "";
        [Obsolete("This method will be removed from future versions")]
        public string EditFields = "";
        [Obsolete("This method will be removed from future versions")]
        public string EditLabels = "";
        [Obsolete("This method will be removed from future versions")]
        public string SearchFields = "";
        [Obsolete("This method will be removed from future versions")]
        public string SearchLabels = "";
        [Obsolete("This method will be removed from future versions")]
        public string PrimaryKeyColumn = "";
        [Obsolete("This method will be removed from future versions")]
        public bool EditRow = true;
        [Obsolete("This method will be removed from future versions")]
        public string OrderColumn = "";
        [Obsolete("This method will be removed from future versions")]
        public string OrderSequence = "";
        [Obsolete("This method will be removed from future versions")]
        public string ForeignKeyColumn = "";
        [Obsolete("This method will be removed from future versions")]
        public string Procedure = "";

        [Obsolete("This method will be removed from future versions")]
        public string SearchFilterPart = "";
        [Obsolete("This method will be removed from future versions")]
        public string FilterPart = "";
        [Obsolete("This method will be removed from future versions")]
        public string DataOnlyColumns = "";
        [Obsolete("This method will be removed from future versions")]
        public string FilterColumns = "";
        [Obsolete("This method will be removed from future versions")]
        public string TotalColumns = "";
        [Obsolete("This method will be removed from future versions")]
        public string TotalBreakColumns = "";
        [Obsolete("This method will be removed from future versions")]
        public string TotalFinalLabel = "";
        [Obsolete("This method will be removed from future versions")]
        public string WrapHeadings = "";
        [Obsolete("This method will be removed from future versions")]
        public string SaveType = "";
        [Obsolete("This method will be removed from future versions")]
        public string PrintType = "";
        [Obsolete("This method will be removed from future versions")]
        public string PrintPageSize = "";

        [Obsolete("This method will be removed from future versions")]
        public string RowValidation = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnPageLoaded = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnEditApply = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnRowSelected = "";
        [Obsolete("This method will be removed from future versions")]
        public string EditRowInitialisation = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnGridPrint = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnBeforeGridSave = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnBeforeGridPrint = "";
        [Obsolete("This method will be removed from future versions")]
        public string OnGridSave = "";

        private ArrayList _GridColumnProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList GridColumnProperties
        {
            get{if (_GridColumnProperties == null){_GridColumnProperties = new ArrayList();}return _GridColumnProperties;}
        }
        
        private ArrayList _EditColumnProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList EditColumnProperties
        {
            get { if (_EditColumnProperties == null) { _EditColumnProperties = new ArrayList(); } return _EditColumnProperties; }
        }
        
        private ArrayList _ViewColumnProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList ViewColumnProperties
        {
            get { if (_ViewColumnProperties == null) { _ViewColumnProperties = new ArrayList(); } return _ViewColumnProperties; }
        }
     
        private ArrayList _SearchColumnProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList SearchColumnProperties
        {
            get { if (_SearchColumnProperties == null) { _SearchColumnProperties = new ArrayList(); } return _SearchColumnProperties; }
        }
  
        private ArrayList _GridColumnLookups = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList GridColumnLookups
        {
            get { if (_GridColumnLookups == null) { _GridColumnLookups = new ArrayList(); } return _GridColumnLookups; }
        }

        private ArrayList _DetailGrids;
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList DetailGrids
        ////////////////////////////////////////////////////////////////////////////
        {
            get{if (_DetailGrids == null){_DetailGrids = new ArrayList();}return _DetailGrids;}
        }

        private ArrayList _TotalBreaks = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList TotalBreaks
        {
            get { if (_TotalBreaks == null) { _TotalBreaks = new ArrayList(); } return _TotalBreaks; }
        }

        ////////////////////////////////////////////////////////////////////////////
        // End of legacy Properties
        ////////////////////////////////////////////////////////////////////////////
        #endregion


        ////////////////////////////////////////////////////////////////////////////
        public DbNetGrid()
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void RenderContents(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            Writer.Write(RenderViewTemplate());
            base.RenderContents(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void AddAttributesToRender(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.AddAttributesToRender(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnLoad(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnLoad(e);
            this.ConvertLegacyCode();
        }

        #region Legacy code conversion

        ////////////////////////////////////////////////////////////////////////////
        protected void ConvertLegacyCode()
        ////////////////////////////////////////////////////////////////////////////
        {
            ConfigLegacyColumns(this.SelectPart, this.Headings, "");
            ConfigLegacyColumns(this.EditFields, this.EditLabels, "Edit");
            ConfigLegacyColumns(this.SearchFields, this.SearchLabels, "Search");

            if (this.Procedure != "")
                this.ProcedureName = this.Procedure;

            if (this.SearchFilterPart != "")
                this.SearchFilterSql = this.SearchFilterPart;
            if (this.FilterPart != "")
                if (this.SearchFilterSql == "")
                    this.SearchFilterSql = this.FilterPart;
                else
                    this.SearchFilterSql += " and " + this.FilterPart;

            if (this.TotalFinalLabel != "")
                this.FinalTotalLabel = this.TotalFinalLabel;

            if (this.PrintPageSize != "")
                this.OutputPageSize = Convert.ToInt32(this.PrintPageSize);

            if (this.PrintType.ToLower() == "enhanced")
                this.CustomSave = true;

            if (this.SaveType.ToLower() == "enhanced")
                this.CustomSave = true;

            ConfigLegacyColumns(this.DataOnlyColumns, "", "Display");
            ConfigLegacyColumns(this.FilterColumns, "", "Filter");
            ConfigLegacyColumns(this.TotalColumns, "", "Aggregate");

            if (!this.EditRow)
            {
                this.UpdateRow = false;
                this.InsertRow = false;
                this.DeleteRow = false;
            }

            if (this.PrimaryKeyColumn != "")
                foreach (string PK in this.PrimaryKeyColumn.Split(','))
                    SetColumnProperty(PK.Trim(), "PrimaryKey", true);

            if (this.ForeignKeyColumn != "")
                SetColumnProperty(this.ForeignKeyColumn, "ForeignKey", true);

            if (this.OrderColumn != "")
            {
                this.OrderBy = this.OrderColumn;
                if (this.OrderSequence != "")
                    this.OrderBy += " " + this.OrderSequence;
            }

            this.ConvertColumnProperties(this.GridColumnProperties);
            this.ConvertColumnProperties(this.EditColumnProperties);
            this.ConvertColumnProperties(this.SearchColumnProperties);

            foreach (object O in this.GridColumnLookups)
            {
                if (!(O is GridColumnLookup))
                    continue;
                GridColumnLookup L = O as GridColumnLookup;
                string Sql = "select " + L.ForeignKeyColumn + "," + L.ForeignDescriptionColumn + " from " + L.ForeignTable;
                SetColumnProperty(L.ColumnName, "Lookup", Sql);
            }

            foreach (object O in this.TotalBreaks)
            {
                if ((O is TotalBreak))
                    SetColumnProperty((O as TotalBreak).ColumnName, "TotalBreak", true);
            }

            foreach (object O in this.DetailGrids)
            {
                if (!(O is DetailGrid))
                    continue;
                DetailGrid DG = O as DetailGrid;
                LinkedControl LC = new LinkedControl();
                LC.LinkedControlID = DG.GridID;

                this.LinkedControls.Add(LC);

                object Ctrl = DbNetFindControl(DG.GridID);

                if (Ctrl is DbNetGrid)
                {
                    DbNetGrid Grid = (Ctrl as DbNetGrid);
                    if (Grid.GridColumns.Count > 0)
                        Grid.SetColumnProperty(DG.ForeignKeyColumn, "ForeignKey", true);
                    else
                        Grid.ForeignKeyColumn = DG.ForeignKeyColumn;
                }
            }

            if (this.RowValidation != "")
                AddLegacyEventHandler(ClientEvents.onRecordValidate, this.RowValidation);
            if (this.OnPageLoaded != "")
                AddLegacyEventHandler(ClientEvents.onPageLoaded, this.OnPageLoaded);
            if (this.OnEditApply != "")
            {
                AddLegacyEventHandler(ClientEvents.onRecordInserted, this.OnEditApply);
                AddLegacyEventHandler(ClientEvents.onRecordUpdated, this.OnEditApply);
            }
            if (this.OnRowSelected != "")
                AddLegacyEventHandler(ClientEvents.onRowSelected, this.OnRowSelected);
            if (this.EditRowInitialisation != "")
                AddLegacyEventHandler(ClientEvents.onRecordSelected, this.EditRowInitialisation);
            if (this.OnGridPrint != "")
                AddLegacyEventHandler(ClientEvents.onOutput, this.OnGridPrint);
            if (this.OnGridSave != "")
                AddLegacyEventHandler(ClientEvents.onOutput, this.OnGridSave);
            if (this.OnBeforeGridPrint != "")
                AddLegacyEventHandler(ClientEvents.onBeforeOutput, this.OnBeforeGridPrint);
            if (this.OnBeforeGridSave != "")
                AddLegacyEventHandler(ClientEvents.onBeforeOutput, this.OnBeforeGridSave);
            
            if (this.NestedGrid != null)
                this.NestedGrid.ConvertLegacyCode();

        }

        ////////////////////////////////////////////////////////////////////////////
        public GridColumn FindColumn(string ColumnName)
        ////////////////////////////////////////////////////////////////////////////
        {
            return FindColumn(ColumnName, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        public GridColumn FindColumn(string ColumnName, bool AddColumn)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.GridColumns == null)
                return null;

            GridColumn GC;

            foreach (Object O in this.GridColumns)
            {
                if (O is GridColumn)
                {
                    GC = (GridColumn)O;

                    if (GC.ColumnExpression.ToLower() == ColumnName.ToLower())
                        return GC;
                }
            }

            if (AddColumn)
            {
                GC = new GridColumn(ColumnName);
                GC.Display = false;
                GC.Edit = false;
                GC.Search = false;
                GridColumns.Add(GC);

                return GC;
            }
            else
            {
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void AddLegacyEventHandler(ClientEvents EventType, string Handler)
        ////////////////////////////////////////////////////////////////////////////
        {
            GridClientEvent CE = new GridClientEvent();
            CE.EventName = EventType;
            CE.Handler = Handler;
            this.GridClientEvents.Add(CE);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void ConfigLegacyColumns(string _Columns, string _Labels, string ColumnType)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (_Columns == "")
                return;

            if (ColumnType != "")
                ResetProperty(ColumnType);

            string[] Columns = _Columns.Split(',');
            string[] Labels = _Labels.Split(',');

            for (int I = 0; I < Columns.Length; I++)
            {
                GridColumn GC = this.FindColumn(Columns[I].Trim(),true);
                if (I < Labels.Length)
                    GC.Label = Labels[I].Trim();

                switch (ColumnType)
                {
                    case "Edit":
                    case "Search":
                    case "Display":
                        SetColumnProperty(Columns[I].Trim(), ColumnType, true);
                        break;
                    case "Aggregate":
                        SetColumnProperty(Columns[I].Trim(), ColumnType, "Sum");
                        break;
                    default:
                        SetColumnProperty(Columns[I].Trim(), "Display", true);
                        SetColumnProperty(Columns[I].Trim(), "Edit", true);
                        SetColumnProperty(Columns[I].Trim(), "Search", true);
                        break;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetColumnProperty(string ColumnExpression, string PropertyName, object PropertyValue)
        ////////////////////////////////////////////////////////////////////////////
        {
            GridColumn GC = this.FindColumn(ColumnExpression,true);
            SetPropertyValue(GC, PropertyName, PropertyValue);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void ResetProperty(string Property)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (GridColumn GC in this.GridColumns)
                SetPropertyValue(GC, Property, false);
        }


        ////////////////////////////////////////////////////////////////////////////
        protected void ConvertColumnProperties(ArrayList List)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (object O in List)
            {
                if (!(O is ColumnProperty))
                    continue;
                ColumnProperty CP = O as ColumnProperty;

                switch (CP.Property.ToLower())
                {
                    case "transform":
                        AddLegacyEventHandler(ClientEvents.onCellTransform, CP.Value);
                        break;
                    case "onchange":
                        AddLegacyEventHandler(ClientEvents.onFieldValueChanged, CP.Value);
                        break;
                    case "editlookup":
                        SetColumnProperty(CP.ColumnName, "lookup", CP.Value);
                        SetColumnProperty(CP.ColumnName, "EditControlType", "TextBoxLookup");
                        break;
                    case "lookuptext":
                        SetColumnProperty(CP.ColumnName, "EditControlType", "TextBoxLookup");
                        break;
                    case "html":
                        SetColumnProperty(CP.ColumnName, "EditControlType", "Html");
                        break;
                    default:
                        SetColumnProperty(CP.ColumnName, CP.Property, CP.Value);
                        break;
                }
            }
        }

        #endregion 

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnPreRender(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnPreRender(e);

            // Call the base implementation
            base.CreateChildControls();
            this.RenderClientScript();
        }

        ////////////////////////////////////////////////////////////////////////////
        internal override void RenderScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            MVC.Cache(MVC.CacheKeyNames.Script).Append( GenerateClientScript(this, false, true) );
            base.RenderInvocationScript(ParentControl);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void RenderClientScript()
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Script = GenerateClientScript(this, false, false);
            Script.Insert(0, "<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + "jQuery(document).ready( function() {");

            CheckForLastControl(Script);

            Script.Append("})" + Environment.NewLine);
            Script.Append("</script>" + Environment.NewLine);

            if (NestedConfigScript.Length > 0)
                RegisterStartupScript(this.ClientID + "NestedConfigScript", "<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + NestedConfigScript.ToString() + "</script>" + Environment.NewLine);

            RegisterStartupScript(this.ClientID + "Script", Script.ToString().Replace(Environment.NewLine,String.Empty));
        }

        ////////////////////////////////////////////////////////////////////////////
        protected StringBuilder GenerateClientScript(DbNetGrid Grid, bool Nested, bool MVCMode)
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder ScriptText = new StringBuilder();

            if (Nested)
            {
                ScriptText.Append("///////////////////////////////////////////" + Environment.NewLine);
                ScriptText.Append("function configure" + Grid.ID + "Grid(grid)" + Environment.NewLine);
                ScriptText.Append("///////////////////////////////////////////" + Environment.NewLine);
                ScriptText.Append("{" + Environment.NewLine);

                ScriptText.Append("\twith (grid)" + Environment.NewLine);
                ScriptText.Append("\t{" + Environment.NewLine);
            }
            else
                ScriptText.Append("var o = new " + ComponentName + "(\"" + Grid.ClientID + "\");" + Environment.NewLine);

            ArrayList ScriptLines = new ArrayList();

            PropertyInfo[] PI = typeof(DbNetGrid).GetProperties();
            DbNetGrid DefaultGrid = new DbNetGrid();

            foreach (PropertyInfo I in PI)
            {
                if (I.DeclaringType == typeof(Control))
                    continue;

                if (I.PropertyType == typeof(DbNetSpell) || I.PropertyType == typeof(DbNetGrid))
                    continue;

                object PropertyValue;

                try
                {
                    if (I.GetValue(DefaultGrid, null) == null)
                    {
                        if (I.GetValue(Grid, null) == null)
                            continue;
                    }
                    else
                    {
                        if (I.GetValue(Grid, null).ToString() == I.GetValue(DefaultGrid, null).ToString())
                            continue;
                    }
                    PropertyValue = I.GetValue(Grid, null);
                }
                catch (Exception)
                {
                    continue;
                }

                switch (I.Name)
                {
                    //Encrypted strings
                    case "ConnectionString":
                    case "FromPart":
                        if (PropertyValue.ToString() != "")
                        {
                            PropertyValue = DbNetLink.Util.Encrypt(PropertyValue.ToString());
                            ScriptLines.Add(WriteClientPropertyAssignment(I.Name, PropertyValue));
                        }
                        break;
                    default:

                        if (I.PropertyType.IsEnum)
                            PropertyValue = PropertyValue.ToString();

                        ScriptLines.Add(WriteClientPropertyAssignment(I.Name, PropertyValue));
                        break;

               }
            }
            ScriptLines.AddRange(WriteColumnProperties(Grid));
            ScriptLines.AddRange(WriteParams(Grid.FixedFilterParams, "fixedFilterParams"));
            ScriptLines.AddRange(WriteParams(Grid.SearchFilterParams, "searchFilterParams"));
            ScriptLines.AddRange(WriteParams(Grid.ProcedureParameters, "procedureParameters"));
            ScriptLines.AddRange(WriteMailMergeDocuments());
            ScriptLines.AddRange(WriteGridClientEvents(Grid));

            if (!MVCMode)
                if (Nested)
                    ScriptLines.Add(WriteLinkedControls(Grid, true));
                else
                    DeferredScript.Append(WriteLinkedControls(Grid));

            if (Grid.NestedGrid != null)
                ScriptLines.Add("addNestedGrid(configure" + Grid.NestedGrid.ClientID + "Grid);");

 //           if (!Nested && !NoLoad)
 //               ScriptLines.Add("initialize()");

            if (Grid.NestedGrid != null)
            {
                if (Grid.NestedGrid.ConnectionString == "")
                {
                    Grid.NestedGrid.ConnectionString = Grid.ConnectionString;
                    Grid.NestedGrid.CommandTimeout = Grid.CommandTimeout;
                    Grid.NestedGrid.CaseInsensitiveSearch = Grid.CaseInsensitiveSearch;
                }

                StringBuilder NestedScript = GenerateClientScript(Grid.NestedGrid, true, false);
                NestedConfigScript.Append(NestedScript);
                MVC.Cache(MVC.CacheKeyNames.FunctionsScript).Append(NestedScript);
            }

            if (Grid.DbNetSpell != null)
                DbNetSpellClientScript(ScriptLines, Grid.DbNetSpell);

            foreach (string Line in ScriptLines)
                ScriptText.Append((Nested ? "\t\t" : "o.") + Line + Environment.NewLine);

            if (Nested)
            {
                ScriptText.Append("\t}" + Environment.NewLine);
                ScriptText.Append("}" + Environment.NewLine);
            }
            else
            {
                ScriptText.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
                ScriptText.Append("window.DbNetLink.components[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            }

            return ScriptText;
        }


        ////////////////////////////////////////////////////////////////////////////
        public StringBuilder RenderViewTemplate()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (!this.DesignMode && (ViewTemplateContent() != ""))
            {
                XmlDocument Doc = new XmlDocument();

                try
                {
                    if (ViewTemplatePath == "")
                        Doc.LoadXml(ViewTemplateContent());
                }
                catch (Exception Ex)
                {
                    return new StringBuilder("<div style='padding:10pt;border:1pt solid;background-color:gold'><B>Error Loading View Template:</B> " + Ex.Message + "</div>");
                }

                return new StringBuilder(Doc.OuterXml);

            }
            else
                return new StringBuilder(ViewTemplateContent());
        }

        ////////////////////////////////////////////////////////////////////////////
        public string ViewTemplateContent()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (ViewTemplate.Document != null)
                return "<div class=\"view-dialog-template\">" + ViewTemplate.Document.OuterXml + "</div>";
            else
                return "";
        }

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteMailMergeDocuments()
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();
            List<string> Params = new List<string>();

            foreach (Object O in this.MailMergeDocuments)
            {
                if (!(O is MailMergeDocument))
                    continue;

                MailMergeDocument P = (MailMergeDocument)O;

                Params.Add("\"" + P.Title + "\"" + " : \"" + P.Path + "\"");
            }

            if (Params.Count > 0)
                Source.Add("mailMergeDocuments = {" + String.Join(",", Params.ToArray()) + "};");

            return Source;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteGridClientEvents(DbNetGrid Grid)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Grid.GridClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in Grid.GridClientEvents)
            {
                if (!(O is GridClientEvent))
                    continue;

                GridClientEvent GCE = (GridClientEvent)O;

                Source.Add("bind(\"" + GCE.EventName.ToString() + "\",\"" + GCE.Handler + "\");");
            }

            return Source;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    public class GridClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetGrid.ClientEvents _EventName;
        private string _Handler = "";

        public GridClientEvent()
        {
        }

        public GridClientEvent(DbNetGrid.ClientEvents EventName, string Handler)
        {
            this._EventName = EventName;
            this._Handler = Handler;
        }

        public DbNetGrid.ClientEvents EventName
        {
            get { return _EventName; }
            set { _EventName = value; }
        }
        public String Handler
        {
            get { return _Handler; }
            set { _Handler = value; }
        }
    }

    [Serializable()]
    ////////////////////////////////////////////////////////////////////////////
    public class GridColumn : DbNetLink.DbNetSuite.GridColumn
    ////////////////////////////////////////////////////////////////////////////
    {
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

    [Serializable()]
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
        }

        ///////////////////////////////////////////////
        public GridColumn Add(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            GridColumn C = new GridColumn(ColumnExpression);
            base.Add(C);
            return C;
        }

        ///////////////////////////////////////////////
        public GridColumn Add(string ColumnExpression, string Label)
        ///////////////////////////////////////////////
        {
            GridColumn C = new GridColumn(ColumnExpression);
            C.Label = Label;
            base.Add(C);
            return C;
        }

        ///////////////////////////////////////////////
        public int IndexOf(GridColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class GridColumnCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public GridColumnCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(GridColumn);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class ParameterCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public ParameterCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(Parameter);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class GridClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public GridClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(GridClientEvent);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class LinkedControlCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public LinkedControlCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(LinkedControl);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class MailMergeDocumentCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public MailMergeDocumentCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(MailMergeDocument);
        }
    }

     ////////////////////////////////////////////////////////////////////////////
    public class MailMergeDocument
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _Title = "";
        private string _Path = "";

        public String Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public String Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        public MailMergeDocument()
        {
        }

        public MailMergeDocument(string Title, string Path)
        {
            this._Title = Title;
            this._Path = Path;
        }

    }
 
    ////////////////////////////////////////////////////////////////////////////
    public class Parameter
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _Name = "";
        private object _Value = "";

        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public String Value
        {
            get { return _Value.ToString(); }
            set { _Value = value; }
        }

        public Parameter()
        {
        }

        public Parameter(string Name, object Value)
        {
            this._Name = Name;
            this._Value = Value;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    public class ProcedureParameter : Parameter
    ////////////////////////////////////////////////////////////////////////////
    {
    }

    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class ColumnProperty
    ////////////////////////////////////////////////////////////////////////////
    {
        public string ColumnName = "";
        public string Property = "";
        public string Value = "";
        public string ApplyToRow = "";
        public string Expression = "";
    }
    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class GridColumnLookup
    ////////////////////////////////////////////////////////////////////////////
    {
        public string ColumnName = "";
        public string ForeignKeyColumn = "";
        public string ForeignDescriptionColumn = "";
        public string ForeignTable = "";
    }
    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class DetailGrid
    ////////////////////////////////////////////////////////////////////////////
    {
        public string GridID = "";
        public string ForeignKeyColumn = "";
    }
    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class TotalBreak
    ////////////////////////////////////////////////////////////////////////////
    {
        public string ColumnName = "";
        public string Label = "";
        public string Format = "";
        public bool Descending = false;
    }
  }