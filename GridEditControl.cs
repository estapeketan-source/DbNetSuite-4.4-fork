using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Script.Serialization;
using System.IO;
using System.Configuration;
using System.Data;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Globalization;
using System.Xml;

using DbNetLink.Data;
using System.Linq;

namespace DbNetLink.DbNetSuite
{
    public class GridEditControl : Shared
    {
        public enum ToolbarOptions
        {
            Top,
            Bottom,
            Hidden
        }

        public enum ColumnPropertyNames
        {
            GroupHeader,
            ClearDuplicateValue
        }

        protected enum QueryBuildModes
        {
            Configuration,
            PrimaryKeysOnly,
            FilterListFilter,
            Totals,
            Count,
            Normal,
            Edit,
            View
        }

        public enum SearchDialogModes
        {
            Advanced,
            Simple,
            Standard
        }

        public enum AuditModes
        {
            None,
            Summary,
            Detail
        }

        public enum MultiValueLookupSelectStyles
        {
            Select,
            Checkbox
        }

        internal enum KeyTypes
        {
            PrimaryKey,
            ForeignKey
        }
        public enum EditModes
        {
            Insert,
            Update
        }

        public enum HashTypes
        {
            None,
            SHA,
            SHA1,
            MD5,
            SHA256,
            SHA384,
            SHA512
        }

        [
        Category("Database"),
        Description("Stores and displays log of updates to the record"),
        DefaultValue(AuditModes.None)
        ]
        public AuditModes Audit = AuditModes.None;

        [
        Category("Database"),
        Description("Controls the formatting of the audit log date"),
        DefaultValue("d")
        ]
        public string AuditDateFormat = "d";

        [
        Category("Database"),
        Description("The id of the the current user for auditing purposes"),
        DefaultValue("")
        ]
        public string AuditUser = "";

        [
        Category("Toolbar"),
        Description("Makes the advanced search dialog available from the toolbar"),
        DefaultValue(true)
        ]
        public bool AdvancedSearch = false;

        public string AdvancedSearchFilterJoin = "and";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Delete button to the toolbar.")
        ]
        public bool DeleteRow = true;

        [
        CategoryAttribute("Editing"),
        DefaultValue(""),
        Description("Sets the text for the empty option in a drop-downlist control for a required field.")
        ]
        public string EmptyOptionText = String.Empty;

        [
        CategoryAttribute("Database"),
        Description("A set of parameters used in conjunction with the FixedFilter property")
        ]
        public Dictionary<string, object> FixedFilterParams = new Dictionary<string, object>();

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Applies a filter to the data that cannot be modified or removed by the user.")
        ]
        public string FixedFilterSql = "";

        [
        CategoryAttribute("Database"),
        Description("Parameter values used in conjunction with the FilterSql property")
        ]
        public Dictionary<string, object> FilterParams = new Dictionary<string, object>();

        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Applies a filter to the data that can be overridden by the user.")
        ]
        public string FilterSql = "";

        [
        CategoryAttribute("Database"),
        Description("The name of the database table or view")
        ]
        public string FromPart = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Insert Record button to the toolbar.")
        ]
        public bool InsertRow = true;

        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Control automatic selection of primary key information.")
        ]
        public bool IgnorePrimaryKeys = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the navigation buttons to the toolbar.")
        ]
        public bool Navigation = true;

        [
        CategoryAttribute("Database"),
        Description("Defines order by which the data is sorted.")
        ]
        public string OrderBy = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Page Information ('Page X of Y') to the toolbar.")
        ]
        public bool PageInfo = true;

        internal string ProfileKey = "";
        internal string ProfileUser = "";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds a search box to the toolbar which can be used to enter a search token which is applied to all the searchable columns")
        ]
        public bool QuickSearch = false;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds the Row Information ('X Rows') to the toolbar.")
        ]
        public bool RowInfo = true;

        [
        Category("Toolbar"),
        Description("Makes the search dialog available from the toolbar"),
        DefaultValue(true)
        ]
        public bool Search = true;

        [
        CategoryAttribute("Search"),
        DefaultValue("Standard"),
        Description("Default search dialog mode")
        ]
        public SearchDialogModes SearchDialogMode = SearchDialogModes.Standard;

        [
        CategoryAttribute("Search"),
        DefaultValue(1),
        Description("Specifies the number of columns over which the search criteria are distributed")
        ]
        public int SearchLayoutColumns = 1;

        [
        CategoryAttribute("Search"),
        DefaultValue(false),
        Description("Specifies if only the search values are selectable in the search dialog")
        ]
        public bool SearchValuesOnly = false;

        [
        CategoryAttribute("Search"),
        DefaultValue(""),
        Description("Specifies the ID or Class Name of an HTML element to contain the search criteria instead of the default search dialog")
        ]
        public string SearchPanelId = "";

        [
        Category("Toolbar"),
        Description("Makes the simple search dialog available from the toolbar"),
        DefaultValue(true)
        ]
        public bool SimpleSearch = false;

        [
        CategoryAttribute("Search"),
        DefaultValue("0px"),
        Description("Fixes the maximum height of the Search Dialog. If the number of search fields causes the height to be exceeded then the search panel will scroll.")
        ]
        public string SearchDialogHeight = "";

        public string SearchFilterJoin = "and";

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the column sort dialog button to the toolbar.")
        ]
        public bool Sort = false;

        [
        Category("Toolbar"),
        Description("Makes the spell check button available on the toolbar"),
        DefaultValue(false)
        ]
        public bool SpellCheck = false;

        [
        Category("Toolbar"),
        Description("Makes the standard search dialog available from the toolbar"),
        DefaultValue(true)
        ]
        public bool StandardSearch = true;

        [
        Category("Toolbar"),
        Description("Sets the style of the toolbar button"),
        DefaultValue(ToolButtonStyles.Image)
        ]
        public ToolButtonStyles ToolbarButtonStyle = ToolButtonStyles.Image;

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the Upload button to the toolbar.")
        ]
        public bool Upload = false;

        [
        CategoryAttribute("Upload"),
        DefaultValue(""),
        Description("Specifies the table/sheet name in the upload data source.")
        ]
        public string UploadDataTable = String.Empty;

        [
        CategoryAttribute("Upload"),
        DefaultValue(""),
        Description("Specifies the folder into which uploaded data is saved.")
        ]
        public string UploadDataFolder = String.Empty;

        [
        CategoryAttribute("Upload"),
        DefaultValue(""),
        Description("Specifies the allowed file extensions for uploaded data.")
        ]
        public string UploadExtFilter = String.Empty;

        [
        Category("Toolbar"),
        Description("Makes the user profile dialog available from the toolbar"),
        DefaultValue(false)
        ]
        public bool UserProfile = false;

        [
        Category("Toolbar"),
        Description("Makes user profile selection available from the toolbar"),
        DefaultValue(false)
        ]
        public bool UserProfileSelect = false;

        internal Dictionary<string, object> ColumnProperties = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        internal DbColumnCollection Columns;
        internal int CurrentPage = 0;
        internal Dictionary<string, object> CurrentRecord = new Dictionary<string, object>();
        internal Dictionary<string, DataTable> LookupTables = new Dictionary<string, DataTable>();
        internal Dictionary<string, object> ParentFilterParams = new Dictionary<string, object>();
        internal ArrayList ParentFilterSql = new ArrayList();
        internal List<object> PrimaryKeyList = new List<object>();
        internal Dictionary<string, object> SearchFilterParams = new Dictionary<string, object>();
        internal ArrayList SearchFilterSql = new ArrayList();
        internal Object[] SearchFilter = new Object[0];
        internal string UserProfileTableName = "dbnetsuite_profile_table";
        internal string AuditTableName = "dbnetsuite_audit_table";
        internal string UserProfileDefaultColumnName = "default_profile";
        internal Winista.Mime.MimeTypes MimeInfo = null;
        internal bool Browse = false;
        internal int AuditValueSize = -1;
        internal Assembly MsAntiXss;
        internal XmlDocument XmlDataDoc = new XmlDocument();

        internal bool SearchColumnOrderAssigned
        {
            get
            {
                foreach (DbColumn Col in Columns)
                {
                    if (Col.SearchColumnOrder > 0)
                        return true;
                }

                return false;
            }
        }

        internal bool EditColumnOrderAssigned
        {
            get
            {
                if (this is DbNetGrid)
                {
                    foreach (GridColumn Col in Columns)
                    {
                        if (Col.EditColumnOrder > 0)
                            return true;
                    }

                    return false;
                }
                if (this is DbNetEdit)
                {
                    foreach (EditColumn Col in Columns)
                    {
                        if (Col.ColumnOrder > 0)
                            return true;
                    }

                    return false;
                }
                else
                {
                    return false;
                }
            }
        }


        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            if (!Req.ContainsKey("method"))
                return;

            switch (Req["method"].ToString())
            {
                case "bulk-insert":
                case "lookup-options":
                case "insert-record":
                case "update-record":
                case "delete-record":
                case "validate-edit":
                case "get-suggested-items":
                case "get-lookup-text":
                case "get-options":
                case "save-user-profile":
                case "load-user-profiles":
                case "delete-user-profile":
                case "select-user-profile":
                case "document-size":
                case "search-dialog-filter":
                case "update-search-filter-text":
                case "peek-lookup":
                case "audit-history":
                case "column-mapping":
                case "load-data-upload":
                case "get-data-uri":
                case "search-dialog":
                case "user-profile-dialog":
                    this.OpenConnection();
                    break;
                case "thumbnail":
                case "stream":
                    if (this.ConnectionString != "")
                        this.OpenConnection();
                    break;
            }

            switch (Req["method"].ToString())
            {
                case "search-dialog":
                    Resp["html"] = BuildSearchDialog();
                    break;
                case "user-profile-dialog":
                    Resp["html"] = BuildUserProfileDialog();
                    break;
                case "lookup-dialog":
                    Resp["html"] = BuildLookupDialog();
                    break;
                case "text-editor":
                    Resp["html"] = BuildTextEditor();
                    break;
                case "html-editor":
                    Resp["html"] = BuildHtmlEditor();
                    break;
                case "simple-search-dialog":
                    Resp["html"] = BuildSimpleSearchDialog();
                    break;
                case "audit-dialog":
                    Resp["html"] = BuildAuditDialog();
                    break;
                case "audit-history":
                    AuditHistory();
                    break;
                case "advanced-search-dialog":
                    Resp["html"] = BuildAdvancedSearchDialog();
                    break;
                case "validate-search-params":
                    ValidateSearchParams();
                    break;
                case "lookup-options":
                    LookupOptions();
                    break;
                case "insert-record":
                    InsertRecord();
                    break;
                case "bulk-insert":
                    BulkInsert();
                    break;
                case "update-record":
                    UpdateRecord();
                    break;
                case "delete-record":
                    DeleteRecord();
                    break;
                case "thumbnail":
                case "stream":
                case "document-size":
                    StreamData();
                    break;
                case "validate-edit":
                    ValidateEdit();
                    break;
                case "get-suggested-items":
                    GetSuggestedItems();
                    break;
                case "peek-lookup":
                    PeekLookup();
                    break;
                case "get-lookup-text":
                    GetLookupText();
                    break;
                case "get-options":
                    GetOptions();
                    break;
                case "execute-query":
                    ExecuteQuery();
                    break;
                case "preview-dialog":
                    Resp["html"] = BuildFilePreviewDialog();
                    break;
                case "save-user-profile":
                    SaveUserProfile();
                    break;
                case "load-user-profiles":
                    Resp["items"] = LoadUserProfiles();
                    break;
                case "delete-user-profile":
                    DeleteUserProfile();
                    break;
                case "select-user-profile":
                    SelectUserProfile();
                    break;
                case "column-sort-dialog":
                    Resp["html"] = BuildColumnSortDialog();
                    break;
                case "search-dialog-filter":
                    BuildSearchDialogFilterSql();
                    break;
                case "update-search-filter-text":
                    UpdateSearchFilterText();
                    break;
                case "data-upload-dialog":
                    Resp["html"] = BuildDataUploadDialog();
                    break;
                case "load-data-upload":
                    LoadDataUpload();
                    break;
                case "column-mapping":
                    DataUploadColumnMapping();
                    break;
                case "get-data-uri":
                    GetDataUri();
                    break;
            }
            this.CloseConnection();
        }


        ///////////////////////////////////////////////
        internal void BuildToolbar()
        ///////////////////////////////////////////////
        {
            Table Toolbar = new Table();
            Toolbar.CellPadding = 0;
            Toolbar.CellSpacing = 0;

            Toolbar.ID = AssignID("toolbar");
            Toolbar.CssClass = "toolbar";

            Toolbar.Rows.Add(new TableRow());
            TableRow TR = Toolbar.Rows[0];

            if (this.Search && this.SearchPanelId == "")
                AddToolButton(TR, "search", "find", "DisplayTheSearchWindow");
            if (this.QuickSearch)
                AddQuickSearch(TR);

            if (this is DbNetGrid)
            {
                if ((this as DbNetGrid).View)
                    AddToolButton(TR, "view", "view", "ViewTheSelectedRecord");

                if ((this as DbNetGrid).UpdateRow)
                    AddToolButton(TR, "updateRow", "update", "UpdateTheSelectedRecord");

                AddInsertDelete(TR);
            }

            BuildPaging(TR);

            if (this is DbNetGrid)
                (this as DbNetGrid).BuildOutputOptions(TR);

            if (this is DbNetEdit)
            {
                AddInsertDelete(TR);

                if (SpellCheckEnabled())
                    AddToolButton(TR, "spellCheck", "spellcheck", "CheckSpelling");

                AddToolButton(TR, "apply", "apply", "ApplyChangesToTheCurrentRecord");
                AddToolButton(TR, "cancel", "undo", "Cancel");
            }

            if (this is DbNetGrid)
            {
                if ((this as DbNetGrid).ColumnPicker)
                    AddToolButton(TR, "columnPicker", "ColumnPicker", "OpenColumnSelectionDialog");
            }

            if (this.Sort)
                AddToolButton(TR, "sort", "Sort", "OpenColumnSortDialog");

            if (this.UserProfile)
                AddToolButton(TR, "userProfile", "UserProfile", "OpenUserProfileDialog");

            if (this.UserProfileSelect)
            {
                TableCell C = new TableCell();
                TR.Cells.Add(C);
                DropDownList D = new DropDownList();
                C.Controls.Add(D);
                D.ID = this.AssignID("userProfileSelect");
                D.CssClass = "user-profile-select";
                D.Items.Add(new ListItem("", ""));

                List<object> Items = LoadUserProfiles();

                foreach (Dictionary<string, object> Item in Items)
                {
                    ListItem I = new ListItem();
                    I.Value = Item["val"].ToString();
                    I.Text = Item["text"].ToString();
                    if (Item.ContainsKey(this.UserProfileDefaultColumnName))
                        I.Attributes.Add(this.UserProfileDefaultColumnName, Item[this.UserProfileDefaultColumnName].ToString());
                    D.Items.Add(I);
                }
            }

            if (this is DbNetGrid)
                if ((this as DbNetGrid).Config)
                    AddToolButton(TR, "config", "Options", "OpenConfigDialog");

            if (this.UserProfile || this.UserProfileSelect)
                GetDefaultUserProfileId();

            Resp["toolbar"] = RenderControlToString(Toolbar);
        }


        ///////////////////////////////////////////////
        internal bool SpellCheckEnabled()
        ///////////////////////////////////////////////
        {
            if (this.SpellCheck)
                return true;

            foreach (DbColumn Col in this.Columns)
                if (Col.SpellCheck)
                    return true;

            return false;
        }

        ///////////////////////////////////////////////
        internal void AddInsertDelete(TableRow TR)
        ///////////////////////////////////////////////
        {
            if (this.Upload)
                AddToolButton(TR, "upload", "upload", "UploadData");
            if (this.InsertRow)
                AddToolButton(TR, "insertRow", "insert", "AddANewRecord");
            if (this.DeleteRow)
                AddToolButton(TR, "deleteRow", "delete", "DeleteTheSelectedRecord");
        }

        ///////////////////////////////////////////////
        protected virtual void BuildPaging(TableRow parentControl)
        ///////////////////////////////////////////////
        {
            TableCell pagingCell = new TableCell();

            parentControl.Controls.Add(pagingCell);
            pagingCell.Style.Add(HtmlTextWriterStyle.PaddingRight, "6px");

            ToolbarOptions TL;

            if (this is DbNetGrid)
                TL = (this as DbNetGrid).ToolbarLocation;
            else
                TL = (this as DbNetEdit).ToolbarLocation;

            if (this.Navigation && TL != ToolbarOptions.Hidden)
            {
                AddPageInformation(pagingCell);
            }
            else
            {
                if (this is DbNetGrid)
                    (this as DbNetGrid).PageSize = Int32.MaxValue;
            }

            if (this is DbNetGrid && this.RowInfo)
            {
                TableCell C = new TableCell();
                C.Wrap = false;
                C.VerticalAlign = VerticalAlign.Middle;

                parentControl.Controls.Add(C);

                Table T = new Table();
                C.Controls.Add(T);
                T.CellPadding = 0;
                T.CellSpacing = 0;

                TableRow R = new TableRow();
                T.Controls.Add(R);

                C = new TableCell();
                R.Controls.Add(C);
                TextBox TB = new TextBox();
                C.Controls.Add(TB);
                TB.ID = this.AssignID("totalRows");
                TB.Width = Unit.Pixel(30);
                TB.ReadOnly = true;
                TB.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

                C = new TableCell();
                C.Text = "&nbsp;" + Translate("Rows") + "&nbsp;";
                R.Controls.Add(C);

                foreach (TableCell cell in R.Cells)
                {
                    cell.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                    cell.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                }
            }


        }

        ///////////////////////////////////////////////
        internal void AddQuickSearch(TableRow R)
        ///////////////////////////////////////////////
        {
            TableCell TC = new TableCell();
            R.Controls.Add(TC);

            TextBox QS = new TextBox();
            TC.Controls.Add(QS);
            TC.Style.Add(HtmlTextWriterStyle.PaddingRight, "6px");

            QS.ID = this.AssignID("quickSearch");
            QS.CssClass = "quick-search";
            QS.Width = 100;
        }


        ///////////////////////////////////////////////
        protected internal void ConfigureColumns()
        ///////////////////////////////////////////////
        {
            if (Columns.Count > 0)
                if (Columns[0].ColumnName != "")
                    return;

            if (this is DbNetGrid)
            {
                DbNetGrid Grid = (this as DbNetGrid);
                if (!String.IsNullOrEmpty(Grid.ProcedureName))
                {
                    Grid.ConfigureStoredProcedure();
                    return;
                }
            }

            string Sql = "";

            Dictionary<string, bool> BaseTables = new Dictionary<string, bool>();

            bool AddPrimaryKeys = true;

            if (!UniversalEmptyFilter())
                AddPrimaryKeys = this.AddPrimaryKeys(BaseTables, Database.GetSchemaTable("select * from " + FromPart + " where 1=2"));

            Sql = "select " + BuildSelectPart(QueryBuildModes.Configuration) + " from " + FromPart + " where 1=2";

            DataTable DT = Database.GetSchemaTable(Sql);

            if (UniversalEmptyFilter())
                AddPrimaryKeys = this.AddPrimaryKeys(BaseTables, DT);

            string EditableBaseTable = "";

            int rowCounter = 0;

            foreach (DataRow row in DT.Rows)
            {
                if (rowCounter >= Columns.Count)
                    break;

                DbColumn C = Columns[rowCounter];

                if (C.BaseTableName == "")
                    C.BaseTableName = Convert.ToString(row["BaseTableName"]);

                GetBaseSchemaName(C, row);

                if (C.ColumnExpression.Contains("("))
                    if (this is DbNetGrid)
                        ((GridColumn)C).Edit = false;

                C.ColumnName = Convert.ToString(row["ColumnName"]);
                C.ColumnSize = Convert.ToInt32(row["ColumnSize"]);

                if (C.ColumnKey == "")
                    C.ColumnKey = C.ColumnIndex.ToString();

                if (C.Label == "")
                    C.Label = GenerateLabel(C.ColumnName);

                if (C.IsBoolean)
                    C.DataType = "Boolean";
                else
                    C.DataType = ((Type)row["DataType"]).Name;

                if (row.Table.Columns.Contains("ProviderType"))
                    C.DbDataType = row["ProviderType"].ToString();

                switch (C.DataType)
                {
                    case "Byte[]":
                        if (this is DbNetGrid)
                            ((GridColumn)C).Filter = false;
                        C.Search = false;
                        C.SimpleSearch = false;
                        break;
                    case "Guid":
                        if (!C.Required)
                            if (String.IsNullOrEmpty(C.Lookup))
                                C.ReadOnly = true;
                        break;
                    default:
                        if (!Convert.ToBoolean(row["AllowDBNull"]) && String.IsNullOrEmpty(row["DefaultValue"].ToString()))
                            C.Required = true;
                        break;
                }

                if (!C.PrimaryKey)
                    AssignColumnProperty(C, "primaryKey");

                if (C.PrimaryKey)
                    EditableBaseTable = C.BaseTableName;

                if (Convert.ToBoolean(row["IsKey"]))
                {
                    if (EditableBaseTable == "")
                        EditableBaseTable = C.BaseTableName;

                    if (EditableBaseTable == C.BaseTableName && AddPrimaryKeys)
                        C.PrimaryKey = true;
                }

                if (Database.Database != DatabaseType.Oracle)
                    C.SequenceName = "";

                if (row.Table.Columns.Contains("IsAutoIncrement"))
                    if (!C.AutoIncrement)
                        C.AutoIncrement = Convert.ToBoolean(row["IsAutoIncrement"]);

                if (C.AutoIncrement || C.SequenceName != "")
                {
                    if (EditableBaseTable == "")
                        EditableBaseTable = C.BaseTableName;

                    if (EditableBaseTable == C.BaseTableName && AddPrimaryKeys)
                        C.PrimaryKey = true;
                    C.AutoIncrement = true;
                    C.ReadOnly = true;
                    C.Required = false;
                }
                else if (C.PrimaryKey)
                    C.UpdateReadOnly = true;

                if (C.BulkInsert)
                    if (C.Lookup == "")
                        C.BulkInsert = false;

                if (C.Unique)
                    C.Required = true;

                if (C.Format == "")
                {
                    switch (C.DataType)
                    {
                        case "DateTime":
                            C.Format = "d";
                            break;
                        case "TimeSpan":
                            C.Format = "t";
                            break;
                    }
                }

                C.UploadRootFolder = C.UploadRootFolder.Replace("~", this.Context.Request.ApplicationPath);

                if (C.IsBoolean)
                    C.EditControlType = EditField.ControlType.CheckBox;

                if (C.ReadOnly)
                {
                    C.InsertReadOnly = true;
                    C.UpdateReadOnly = true;
                }

                rowCounter++;
            }

            AssignColumnProperties();

            if (EditableBaseTable != "")
                BaseTables[EditableBaseTable.ToLower()] = true;

            int StandardSearchColumns = 0;
            int SimpleSearchColumns = 0;
            int ColumnOrder = 0;

            foreach (DbColumn Col in Columns)
            {
                ColumnOrder++;

                if (Col.IsBoolean)
                    Col.EditControlType = EditField.ControlType.CheckBox;

                if (Col.BaseTableName == "" || Col.ForeignKey)
                {
                    Col.InsertReadOnly = true;
                    Col.UpdateReadOnly = true;
                }
                else if (EditableBaseTable != Col.BaseTableName)
                    if (this is DbNetGrid)
                        ((GridColumn)Col).Edit = false;

                if (this.SearchColumnOrderAssigned)
                    if (Col.SearchColumnOrder == 0)
                        Col.SearchColumnOrder = ColumnOrder * 100;

                if (this is DbNetGrid)
                {
                    GridColumn GC = (GridColumn)Col;
                    if (GC.ForeignKey && !GC.Edit)
                    {
                        GC.Edit = true;
                        GC.EditDisplay = false;
                    }

                    if (!GC.GridData)
                        GC.Display = false;

                    if (this.EditColumnOrderAssigned)
                        if (GC.EditColumnOrder == 0)
                            GC.EditColumnOrder = ColumnOrder * 100;
                }

                if (Col.Search)
                    StandardSearchColumns++;

                if (Col.DataType != "String" && !Col.Lookup.ToLower().StartsWith("select"))
                    Col.SimpleSearch = false;

                if (Col.SimpleSearch)
                    SimpleSearchColumns++;

                if (this is DbNetEdit)
                {
                    EditColumn EC = (EditColumn)Col;

                    if (EC.Browse)
                        this.Browse = true;

                    if (EC.ColumnOrder == 0)
                        EC.ColumnOrder = ColumnOrder * 100;
                }

                if (Col.Encryption != HashTypes.None)
                    Col.EditControlType = EditField.ControlType.Password;
            }

            /*
                    if (this.NamingContainer.GetType() == typeof(DbNetGrid.EditDialog))
                        for (int i = Columns.Count - 1; i >= 0; i--)
                            if (EditableBaseTable != Columns[i].BaseTableName)
                                Columns.RemoveAt(i);    
            */

            if (StandardSearchColumns == 0)
                this.StandardSearch = false;

            if (SimpleSearchColumns == 0)
            {
                this.SimpleSearch = false;
                this.QuickSearch = false;
            }

            if (!this.SimpleSearch)
                this.SearchDialogMode = SearchDialogModes.Standard;

            if (!this.StandardSearch)
                this.SearchDialogMode = SearchDialogModes.Simple;

            if (!this.SimpleSearch && !this.StandardSearch)
                this.Search = false;

            if (!this.PrimaryKeySupplied())
            {
                this.DeleteRow = false;
                if (this is DbNetGrid)
                {
                    (this as DbNetGrid).UpdateRow = false;
                    (this as DbNetGrid).InsertRow = false;
                    (this as DbNetGrid).View = false;
                }
            }

            foreach (DbColumn Col in Columns)
            {
                if (Col.Lookup.Equals(string.Empty))
                    continue;

                if (Col.Lookup.StartsWith("["))
                {
                    Col.LookupTable = "";
                    Col.LookupTextField = "text";
                    Col.LookupValueField = "value";
                    continue;
                }

                Sql = Col.Lookup;
                ListDictionary Params = Database.ParseParameters(Sql);

                if (Params.Count > 0)
                    Sql = Regex.Replace(Sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                try
                {
                    DT = Database.GetSchemaTable(Sql);
                }
                catch (Exception ex)
                {
                    ThrowException("Error in column <b>Lookup</b> property<br /><br />" + ex.Message, Database.CommandErrorInfo());
                    return;
                }

                int TextRowIndex = (DT.Rows.Count == 1) ? 0 : 1;

                try
                {
                    Col.LookupDataType = ((Type)DT.Rows[TextRowIndex]["DataType"]).Name;
                    if (DT.Rows[TextRowIndex]["BaseTableName"] != System.DBNull.Value)
                        Col.LookupTable = (string)DT.Rows[TextRowIndex]["BaseTableName"];
                    else
                        Col.LookupTable = "";
                    Col.LookupTextField = (string)DT.Rows[TextRowIndex]["ColumnName"];
                    Col.LookupTextExpression = Col.LookupTextField;

                    if (TextRowIndex > 0)
                    {
                        string[] Cols = GetSelectColumns(Col.Lookup);
                        if (Cols.Length > 1)
                            Col.LookupTextExpression = Cols[1];
                    }

                    Col.LookupValueField = (string)DT.Rows[0]["ColumnName"];
                }
                catch (Exception ex)
                {
                    ThrowException("Error [<b>" + ex.Message + "</b>] in column <b>Lookup</b> property<br /><br />The SQL is not in the expected format.  Expected format is  \"SELECT IdField, TextDescriptionField FROM Table\"");
                    return;
                }
            }


            if (this is DbNetGrid)
            {
                foreach (GridColumn Col in Columns)
                {
                    if (Col.PrimaryKey)
                        if (!Col.Edit)
                        {
                            Col.Edit = true;
                            Col.EditDisplay = false;
                        }

                    if (Col.Output == null)
                        Col.Output = Col.Display;

                    if ((this as DbNetGrid).ProcedureName != "")
                        Col.TotalBreak = false;

                    if (Col.GroupHeader)
                        Col.Display = false;
                }

            }
        }

        ////////////////////////////////////////////////////////////////
        private bool AddPrimaryKeys(Dictionary<string, bool> BaseTables, DataTable DT)
        ////////////////////////////////////////////////////////////////
        {
            bool AddPrimaryKeys = true;

            foreach (DataRow row in DT.Rows)
            {
                string T = Convert.ToString(row["BaseTableName"]);
                if (!BaseTables.ContainsKey(T))
                    BaseTables.Add(T, false);
            }

            if (BaseTables.Count > 1 || FromPart.ToLower().Contains(" join ") || IgnorePrimaryKeys)
                AddPrimaryKeys = false;

            if (this is DbNetGrid)
                if (Regex.IsMatch((this as DbNetGrid).SelectModifier, "(distinct|unique)", RegexOptions.IgnoreCase))
                    AddPrimaryKeys = false;

            if (Columns.Count == 0)
            {
                foreach (DataRow row in DT.Rows)
                {
                    if (row.Table.Columns.Contains("IsHidden"))
                        if (Convert.ToBoolean(row["IsHidden"]))
                            continue;
                    Columns.Add(GenerateColumn(row));
                }
            }
            else
            {
                if (AddPrimaryKeys)
                {
                    foreach (DataRow R in DT.Rows)
                    {
                        if (Convert.ToBoolean(R["IsKey"]) && !ColumnIncluded(R))
                        {
                            DbColumn C = GenerateColumn(R);
                            if (C is GridColumn)
                            {
                                ((GridColumn)C).Display = false;
                                ((GridColumn)C).Edit = true;
                            }
                            C.EditDisplay = false;
                            C.Search = false;
                            Columns.Add(C);
                        }
                    }
                }
            }

            return AddPrimaryKeys;
        }


        ///////////////////////////////////////////////
        internal bool IsXmlDataType(DbColumn C)
        ///////////////////////////////////////////////
        {
            switch (Database.Database)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServerCE:
                    switch (C.DbDataType.ToLower())
                    {
                        case "xml":
                        case "25":
                            return true;
                    }
                    break;
            }

            return false;
        }


        ///////////////////////////////////////////////
        internal void GetBaseSchemaName(DbColumn C, DataRow Row)
        ///////////////////////////////////////////////
        {
            if (C.BaseSchemaName == "")
                if (Row.Table.Columns.Contains("BaseSchemaName"))
                    if (Row["BaseSchemaName"] != System.DBNull.Value)
                        C.BaseSchemaName = Convert.ToString(Row["BaseSchemaName"]);
        }

        ///////////////////////////////////////////////
        internal void AssignColumnProperty(DbColumn Col, string PropertyName)
        ///////////////////////////////////////////////
        {
            if (this.ColumnProperties.ContainsKey(Col.ColumnName))
            {
                Dictionary<string, object> Properties = (Dictionary<string, object>)this.ColumnProperties[Col.ColumnName];
                if (Properties.ContainsKey(PropertyName))
                    SetProperty(Col, PropertyName, Properties[PropertyName]);
            }
        }

        ///////////////////////////////////////////////
        internal void AssignColumnProperties()
        ///////////////////////////////////////////////
        {
            foreach (string ColumnName in this.ColumnProperties.Keys)
            {
                if (ColumnName != "*")
                    continue;

                Dictionary<string, object> Properties = (Dictionary<string, object>)this.ColumnProperties[ColumnName];
                foreach (DbColumn Col in this.Columns)
                    foreach (string Property in Properties.Keys)
                        SetProperty(Col, Property, Properties[Property]);
            }

            foreach (string ColumnName in this.ColumnProperties.Keys)
            {
                Dictionary<string, object> Properties = (Dictionary<string, object>)this.ColumnProperties[ColumnName];

                DbColumn Col = this.Columns[ColumnName];

                if (Col == null)
                {
                    // Col = this.Columns[DbNetLink.Util.Decrypt(ColumnName)];
                    // if (Col == null)
                    continue;
                }

                foreach (string Property in Properties.Keys)
                    SetProperty(Col, Property, Properties[Property]);
            }
        }

        ///////////////////////////////////////////////
        protected bool ColumnIncluded(DataRow R)
        ///////////////////////////////////////////////
        {
            string ColumnName = R["ColumnName"].ToString().ToLower();
            foreach (DbColumn C in Columns)
            {
                string[] ColumnParts = C.ColumnExpression.Split('.');
                string CE = ColumnParts[ColumnParts.Length - 1].ToLower();
                if (Database.UnqualifiedDbObjectName(CE) == ColumnName)
                    return true;
            }
            return false;
        }

        ///////////////////////////////////////////////
        internal ArrayList SearchColumns()
        ///////////////////////////////////////////////
        {
            ArrayList SearchColumns = new ArrayList();
            foreach (DbColumn C in this.Columns)
                if (C.Search)
                    SearchColumns.Add(C);
            return SearchColumns;
        }

        ///////////////////////////////////////////////
        protected List<string> TotalBreakColumns()
        ///////////////////////////////////////////////
        {
            DbNetGrid Grid = this as DbNetGrid;
            List<string> TotalBreakColumns = new List<string>();

            int Idx = 0;

            foreach (GridColumn GC in Grid.Columns)
            {
                if (!GC.Display)
                    continue;

                Idx++;

                if (GC.TotalBreak)
                    TotalBreakColumns.Add(Idx.ToString());
            }

            return TotalBreakColumns;
        }

        ///////////////////////////////////////////////
        protected List<string> ColumnList(string PropertyName)
        ///////////////////////////////////////////////
        {
            return ColumnList(PropertyName, false, false);
        }

        ///////////////////////////////////////////////
        protected List<string> ColumnList(string PropertyName, bool FullyQualified, bool IncludeSequence)
        ///////////////////////////////////////////////
        {
            DbNetGrid Grid = this as DbNetGrid;
            List<string> Columns = new List<string>();

            foreach (GridColumn GC in Grid.Columns)
                if (Convert.ToBoolean(Database.GetPropertyValue(GC, PropertyName)))
                    if (FullyQualified)
                    {
                        string Expr = StripColumnRename(GC.ColumnExpression);
                        if (IncludeSequence)
                            Expr = Expr + (GC.OrderByDescending ? " desc" : String.Empty);
                        Columns.Add(Expr);
                    }
                    else
                        Columns.Add(GC.ColumnName);

            return Columns;
        }

        ///////////////////////////////////////////////
        internal DbColumn GenerateColumn(DataRow row)
        ///////////////////////////////////////////////
        {
            DbColumn C;

            if (this is DbNetGrid)
                C = new GridColumn();
            else
                C = new EditColumn();

            C.ColumnExpression = Database.QualifiedDbObjectName(Convert.ToString(row["ColumnName"]));
            C.ColumnName = Convert.ToString(row["ColumnName"]);
            C.BaseTableName = Convert.ToString(row["BaseTableName"]);

            GetBaseSchemaName(C, row);

            string TableName = GetBaseTableName(C.BaseTableName);

            if (TableName != "")
                C.ColumnExpression = Database.QualifiedDbObjectName(TableName) + "." + C.ColumnExpression;

            C.AddedByUser = false;

            return C;
        }

        ///////////////////////////////////////////////
        internal string BuildSearchDialog()
        ///////////////////////////////////////////////
        {
            SearchDialog SD = new SearchDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildSearchPanel()
        ///////////////////////////////////////////////
        {
            SearchDialog SD = new SearchDialog(this);
            SD.Dialog = false;
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildUserProfileDialog()
        ///////////////////////////////////////////////
        {
            UserProfileDialog SD = new UserProfileDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildSimpleSearchDialog()
        ///////////////////////////////////////////////
        {
            SimpleSearchDialog SD = new SimpleSearchDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildAuditDialog()
        ///////////////////////////////////////////////
        {
            AuditDialog D = new AuditDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildDataUploadDialog()
        ///////////////////////////////////////////////
        {
            DataUploadDialog D = new DataUploadDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildAdvancedSearchDialog()
        ///////////////////////////////////////////////
        {
            AdvancedSearchDialog SD = new AdvancedSearchDialog(this);
            return RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildLookupDialog()
        ///////////////////////////////////////////////
        {
            LookupDialog LD = new LookupDialog(this);
            return RenderControlToString(LD.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildTextEditor()
        ///////////////////////////////////////////////
        {
            TextEditor D = new TextEditor(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildHtmlEditor()
        ///////////////////////////////////////////////
        {
            HtmlEditor D = new HtmlEditor(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildColumnSortDialog()
        ///////////////////////////////////////////////
        {
            ColumnSortDialog D = new ColumnSortDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string QualifiedColumnName(DbColumn C)
        ///////////////////////////////////////////////
        {
            string ColumnName = C.ColumnName;
            string TableName = GetBaseTableName(C.BaseTableName);
            if (TableName != "")
                ColumnName = Database.QualifiedDbObjectName(TableName) + "." + C.ColumnName;

            return ColumnName;
        }

        ///////////////////////////////////////////////
        internal string GetBaseTableName(object TableName)
        ///////////////////////////////////////////////
        {
            string BaseTableName = "";

            if (Regex.IsMatch(this.FromPart, @"\b" + TableName.ToString() + @"\b", RegexOptions.IgnoreCase))
                BaseTableName = TableName.ToString();

            return BaseTableName;
        }

        ///////////////////////////////////////////////
        protected QueryCommandConfig BuildSQL()
        ///////////////////////////////////////////////
        {
            return BuildSQL(QueryBuildModes.Normal);
        }

        ///////////////////////////////////////////////
        protected QueryCommandConfig BuildSQL(QueryBuildModes BuildMode)
        ///////////////////////////////////////////////
        {
            if (this is DbNetGrid)
                if ((this as DbNetGrid).ProcedureName != "")
                    return (this as DbNetGrid).ProcedureCommandConfig();

            return BuildSQL(BuildSelectPart(BuildMode), BuildFilterPart(), BuildOrderPart(BuildMode), BuildMode);
        }

        ///////////////////////////////////////////////
        protected virtual QueryCommandConfig BuildSQL(string SelectPart, string FilterPart, string OrderPart)
        ///////////////////////////////////////////////
        {
            return BuildSQL(SelectPart, FilterPart, OrderPart, QueryBuildModes.Normal);
        }

        ///////////////////////////////////////////////
        protected virtual QueryCommandConfig BuildSQL(string SelectPart, string FilterPart, string OrderPart, QueryBuildModes BuildMode)
        ///////////////////////////////////////////////
        {
            string sql = "select " + SelectPart + " from " + FromPart;
            if (!String.IsNullOrEmpty(FilterPart))
                sql += " where " + FilterPart;

            if (this is DbNetGrid && BuildMode != QueryBuildModes.Totals)
            {
                DbNetGrid Grid = (this as DbNetGrid);

                if (Grid.GroupBy)
                {
                    List<string> GroupByColumns = new List<string>();

                    foreach (GridColumn C in Grid.Columns)
                    {
                        if (C.Aggregate == GridColumn.AggregateValues.None && (C.Display || Grid.GroupByHiddenColumns))
                            GroupByColumns.Add(StripColumnRename(C.ColumnExpression));
                    }

                    if (GroupByColumns.Count > 0)
                    {
                        sql += " group by  " + String.Join(",", GroupByColumns.ToArray());

                        if (!String.IsNullOrEmpty(Grid.Having))
                            sql += " having  " + Grid.Having;

                        if (Database.Database == DatabaseType.SqlServerCE)
                            OrderPart = String.Join(",", GroupByColumns.ToArray());
                    }
                }
            }

            if (BuildMode != QueryBuildModes.Count)
            {
                if (OrderPart == "")
                    OrderPart = DefaultOrderBy();
                else
                    OrderPart = ReconfigureOrderPart(OrderPart);

                if (this is DbNetEdit || (this is DbNetGrid && (this as DbNetGrid).NoSort == false))
                    if (BuildMode != QueryBuildModes.Totals)
                        if (OrderPart != "")
                            sql += " order by " + OrderPart;
            }

            ListDictionary P = BuildParams(BuildMode);
            sql = ReplaceWithCorrectParams(sql, P);

            QueryCommandConfig Query = new QueryCommandConfig(sql);
            Query.Params = P;
            return Query;
        }

        ///////////////////////////////////////////////
        protected string ReconfigureOrderPart(string OrderPart)
        ///////////////////////////////////////////////
        {
            if (Database.Database != DatabaseType.SqlServerCE)
                return OrderPart;

            return ConvertOrderPartFromOrdinalsToNames(OrderPart);
        }

        ///////////////////////////////////////////////
        protected string ConvertOrderPartFromOrdinalsToNames(string OrderPart)
        ///////////////////////////////////////////////
        {
            string[] OrderParts = OrderPart.Split(',');
            List<string> OrderPartsList = new List<string>();

            foreach (string OP in OrderParts)
            {
                try
                {
                    int Idx = Convert.ToInt32(OP.Trim().Split(' ')[0]);
                    string ColumnName = this.Columns[Idx - 1].ColumnExpression;
                    ColumnName = StripColumnRename(ColumnName);

                    OrderPartsList.Add(OP.Replace(Idx.ToString(), ColumnName));
                }
                catch (Exception)
                {
                    OrderPartsList.Add(OP);
                }
            }

            return String.Join(",", OrderPartsList.ToArray());
        }

        ///////////////////////////////////////////////
        protected string StripColumnRename(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            string[] ColumnParts = ColumnExpression.Split(')');
            ColumnParts[ColumnParts.Length - 1] = Regex.Replace(ColumnParts[ColumnParts.Length - 1], " as .*", "", RegexOptions.IgnoreCase);
            ColumnParts[0] = Regex.Replace(ColumnParts[0], "unique |distinct ", "", RegexOptions.IgnoreCase);

            return String.Join(")", ColumnParts);
        }

        ///////////////////////////////////////////////
        protected virtual ListDictionary BuildParams()
        ///////////////////////////////////////////////
        {
            return BuildParams(QueryBuildModes.Normal);
        }

        ///////////////////////////////////////////////
        protected virtual ListDictionary BuildParams(QueryBuildModes BuildMode)
        ///////////////////////////////////////////////
        {
            ListDictionary P = new ListDictionary();

            foreach (string key in this.FixedFilterParams.Keys)
            {
                P.Add(key, this.FixedFilterParams[key]);
            }

            foreach (string key in this.ParentFilterParams.Keys)
            {
                DbColumn C = ColumnFromParamName(key);
                if (C != null)
                    P.Add(key, ConvertToDbParam(this.ParentFilterParams[key], C));
                else
                    P.Add(key, ConvertToDbParam(this.ParentFilterParams[key]));
            }

            if (this is DbNetGrid)
            {
                DbNetGrid Grid = (this as DbNetGrid);

                if (BuildMode != QueryBuildModes.FilterListFilter || Grid.FilterColumnMode == DbNetGrid.FilterColumnModeValues.Composite)
                {
                    Grid.AddColumnFilterParams(P);
                }
            }

            AddSearchDialogParams(P);

            return P;
        }


        ///////////////////////////////////////////////
        internal void AddSearchDialogParams(ListDictionary P)
        ///////////////////////////////////////////////
        {
            this.AddSearchFilterParams(this.SearchFilterParams, P);
            foreach (Object SF in this.SearchFilter)
            {
                Dictionary<string, object> SearchFilter = (Dictionary<string, object>)SF;
                if (SearchFilter.ContainsKey("params"))
                    this.AddSearchFilterParams((Dictionary<string, object>)SearchFilter["params"], P);
            }
        }

        ///////////////////////////////////////////////
        internal DbColumn ColumnFromKey(string Key)
        ///////////////////////////////////////////////
        {
            foreach (DbColumn Col in this.Columns)
                if (Col.ColumnKey == Key)
                    return Col;

            foreach (DbColumn Col in this.Columns)
                if (Col.ColumnName.Split('.')[Col.ColumnName.Split('.').Length - 1].ToLower() == Key.ToLower())
                    return Col;

            return null;
        }

        ///////////////////////////////////////////////
        internal DbColumn ColumnFromParamName(string Key)
        ///////////////////////////////////////////////
        {
            string ColumnKey = "";
            Regex RE = new Regex(@"Col(\d{1,3})Param");
            Match M = RE.Match(Key);

            if (M.Success)
                ColumnKey = M.Groups[1].Value;
            else
                ColumnKey = Key;
            return ColumnFromKey(ColumnKey);
        }

        ///////////////////////////////////////////////
        internal void AddSearchFilterParams(Dictionary<string, object> SearchFilterParams, ListDictionary P)
        ///////////////////////////////////////////////
        {
            foreach (string key in SearchFilterParams.Keys)
            {
                switch (key.ToLower())
                {
                    case "simplesearchtoken":
                        string token = SearchFilterParams[key].ToString();
                        if (!token.Contains("%"))
                            token = "%" + token + "%";

                        if (Database.ParameterName("simplesearchtoken") == "?")
                        {
                            foreach (DbColumn Col in this.Columns)
                                if (Col.SimpleSearch)
                                    P.Add("simplesearchtoken" + Col.ColumnIndex.ToString(), token);
                        }
                        else
                            P.Add("simplesearchtoken", token);
                        break; ;
                    default:
                        DbColumn C = ColumnFromParamName(key);

                        string T = SearchFilterParams[key].GetType().Name;

                        if (C != null)
                        {
                            T = C.DataType;

                            if (C.Lookup != "")
                                if (C.LookupSearchMode == DbColumn.LookupSearchModeValues.SearchText)
                                    T = C.LookupDataType;
                        }

                        object ParamValue = ConvertToDbParam(SearchFilterParams[key], T);

                        // For SQL Server "Time" data type we need to convert the DateTime to a TimeSpan
                        if (T == "TimeSpan")
                            ParamValue = Convert.ToDateTime(ParamValue).TimeOfDay;

                        P.Add(key, ParamValue);
                        break;
                }
            }
        }

        ///////////////////////////////////////////////
        protected string ReplaceWithCorrectParams(string Sql, ListDictionary P)
        ///////////////////////////////////////////////
        {
            //loops through the parameters and replaces the sql with correctly identified,
            //db specific parameters.
            Regex ParamPattern = new Regex(@"Col\d{1,3}Param\d{1,3}");

            foreach (string key in P.Keys)
            {
                if (ParamPattern.IsMatch(key))
                    Sql = Regex.Replace(Sql, @"([\(, ])" + key + @"\b", "$1" + Database.ParameterName(key));
            }

            return Sql;
        }

        ///////////////////////////////////////////////
        protected void AddFilterParameters(ListDictionary parameters)
        ///////////////////////////////////////////////
        {
            foreach (string key in this.FilterParams.Keys)
            {
                object value = this.FilterParams[key];

                if (value == null)
                    value = DBNull.Value;

                if (value is string)
                    if (value.ToString() == "")
                        value = DBNull.Value;

                parameters.Add(key, this.FilterParams[key]);
            }
        }

        ///////////////////////////////////////////////
        protected virtual string BuildSelectPart(QueryBuildModes BuildMode)
        ///////////////////////////////////////////////
        {
            return BuildSelectPart(BuildMode, false);
        }

        ///////////////////////////////////////////////
        protected virtual string BuildSelectPart(QueryBuildModes BuildMode, bool UnqualifiedColumnNames)
        ///////////////////////////////////////////////
        {
            DbNetGrid Grid = null;

            if (this is DbNetGrid)
                Grid = (DbNetGrid)this;

            if (BuildMode == QueryBuildModes.Count)
                return "count(*)";

            List<string> SelectParts = new List<string>();

            foreach (DbColumn column in Columns)
            {
                if (BuildMode == QueryBuildModes.PrimaryKeysOnly)
                    if (!column.PrimaryKey)
                        continue;

                if (BuildMode == QueryBuildModes.Normal)
                    if (column is GridColumn)
                        if (!(column as GridColumn).GridData)
                            continue;

                string CE = column.ColumnExpression;

                if (UnqualifiedColumnNames)
                    CE = CE.Split('.')[CE.Split('.').Length - 1];

                if (BuildMode != QueryBuildModes.Configuration)
                    if (this is DbNetGrid)
                        if (Grid.GroupBy)
                        {
                            GridColumn GC = column as GridColumn;

                            if (!GC.Display && !Grid.GroupByHiddenColumns)
                                continue;
                            if (GC.Aggregate != GridColumn.AggregateValues.None)
                                CE = GC.Aggregate.ToString() + "(" + AggregateExpression(column) + ") as " + GC.ColumnName;
                        }

                if (IsXmlDataType(column))
                    CE = XmlQueryExpression(column) + " as " + column.ColumnName;

                SelectParts.Add(CE);
            }

            if (BuildMode == QueryBuildModes.PrimaryKeysOnly)
                if (SelectParts.Count == 0)
                    ThrowException("Unable to find any primary key columns.");

            string ColumnList = string.Join(", ", SelectParts.ToArray());

            if (this is DbNetGrid)
                if (Grid.SelectModifier != "")
                    ColumnList = Grid.SelectModifier + " " + ColumnList;

            return ColumnList;
        }

        ///////////////////////////////////////////////
        internal string AggregateExpression(DbColumn C)
        ///////////////////////////////////////////////
        {
            string Expr = C.ColumnExpression;
            return Regex.Replace(Expr, @" as \w*$", "", RegexOptions.IgnoreCase);
        }

        ///////////////////////////////////////////////
        internal bool PrimaryKeySupplied()
        ///////////////////////////////////////////////
        {
            foreach (DbColumn col in Columns)
                if (col.PrimaryKey)
                    return true;

            return false;
        }

        ///////////////////////////////////////////////
        protected virtual string BuildFilterPart()
        ///////////////////////////////////////////////
        {
            return BuildFilterPart(false);
        }

        ///////////////////////////////////////////////
        protected virtual string BuildFilterPart(bool FilterListFilter)
        ///////////////////////////////////////////////
        {
            List<string> FP = new List<string>();

            if (this.FixedFilterSql != "")
                FP.Add("(" + DbNetLink.Util.Decrypt(this.FixedFilterSql) + ")");

            if (this.FilterSql != "")
                FP.Add("(" + this.FilterSql + ")");

            if (this.ParentFilterSql.Count > 0)
            {
                string S = String.Join(" and ", (string[])this.ParentFilterSql.ToArray(typeof(string)));
                S = S.Replace("{@}", Database.ParameterTemplate.Replace("{0}", string.Empty));
                FP.Add("(" + S + ")");
            }

            FP.AddRange(BuildSearchDialogFilter());

            if (this is DbNetGrid)
            {
                DbNetGrid Grid = (DbNetGrid)this;

                if (!FilterListFilter || Grid.FilterColumnMode == DbNetGrid.FilterColumnModeValues.Composite)
                {
                    FP.AddRange(Grid.BuildColumnFilter());
                }
            }

            string filterPart = string.Join(" and ", FP.ToArray());

            for (int i = 0; i < Columns.Count; i++)
            {
                string ColumnExpression = RefineSearchExpression(Columns[i]);
                filterPart = filterPart.Replace("{col" + Columns[i].ColumnKey + "}", ColumnExpression);
            }

            return filterPart;
        }

        protected bool UniversalEmptyFilter()
        {
            if (this.FixedFilterSql != "")
                if (DbNetLink.Util.Decrypt(this.FixedFilterSql) == "2=1")
                    return true;

            if (this.SearchFilterSql.Count > 0)
                if (DbNetLink.Util.Decrypt(this.SearchFilterSql[0].ToString()) == "2=1")
                    return true;

            return false;
        }

        ///////////////////////////////////////////////
        private void BuildSearchDialogFilterSql()
        ///////////////////////////////////////////////
        {
            string Sql = string.Join(" and ", BuildSearchDialogFilter().ToArray());

            for (int i = 0; i < Columns.Count; i++)
            {
                string ColumnExpression = RefineSearchExpression(Columns[i]);
                Sql = Sql.Replace("{col" + Columns[i].ColumnKey + "}", ColumnExpression);
            }

            ListDictionary Params = new ListDictionary();

            AddSearchDialogParams(Params);
            Resp["params"] = Params;
            Resp["sql"] = ReplaceWithCorrectParams(Sql, Params);
        }

        ///////////////////////////////////////////////
        private List<string> BuildSearchDialogFilter()
        ///////////////////////////////////////////////
        {
            List<string> FP = new List<string>();

            if (this.SearchFilterSql.Count > 0)
                FP.Add(BuildSearchFilterPart(this.SearchFilterSql, this.SearchFilterJoin));

            List<string> SearchFilterSqlList = new List<string>();

            foreach (Object SF in this.SearchFilter)
            {
                Dictionary<string, object> SearchFilter = (Dictionary<string, object>)SF;
                ArrayList AL;

                if (SearchFilter["sql"] is string)
                {
                    AL = new ArrayList();
                    AL.Add((string)SearchFilter["sql"]);
                }
                else
                    AL = new ArrayList((Object[])SearchFilter["sql"]);

                if (AL.Count > 0)
                {
                    string Join = "and";
                    if (SearchFilter.ContainsKey("join"))
                        Join = (string)SearchFilter["join"];
                    SearchFilterSqlList.Add(BuildSearchFilterPart(AL, Join));
                }
            }

            if (SearchFilterSqlList.Count > 0)
                FP.Add("(" + String.Join(" " + this.AdvancedSearchFilterJoin + " ", SearchFilterSqlList.ToArray()) + ")");

            return FP;
        }


        ///////////////////////////////////////////////
        private string BuildSearchFilterPart(ArrayList SearchFilterSql, string Join)
        ///////////////////////////////////////////////
        {
            if (SearchFilterSql.Count == 0)
                return "";

            if (SearchFilterSql[0].ToString() == "{simplesearchsql}")
            {
                return ("(" + BuildSimpleSearchFilter() + ")");
            }
            else
            {
                for (int j = 0; j < SearchFilterSql.Count; j++)
                {
                    string SearchPart = SearchFilterSql[j].ToString();
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        string PlaceHolder = "{col" + Columns[i].ColumnKey + "}";

                        if (!SearchPart.Contains(PlaceHolder))
                            continue;

                        DbColumn Col = Columns[i];

                        if (Col.Lookup != "" && Col.LookupSearchMode == DbColumn.LookupSearchModeValues.SearchText)
                        {
                            SearchPart = SearchPart.Replace(PlaceHolder, "");
                            SearchPart = PlaceHolder + " in (" + SearchLookupSql(Col, SearchPart) + ")";
                        }
                    }
                    SearchFilterSql[j] = SearchPart;
                }

                return ("(" + String.Join(" " + Join + " ", (string[])SearchFilterSql.ToArray(typeof(string))) + ")");
            }
        }

        ///////////////////////////////////////////////
        private string BuildSimpleSearchFilter()
        ///////////////////////////////////////////////
        {
            List<string> SearchFilter = new List<string>();
            foreach (DbColumn Col in this.Columns)
            {
                if (!Col.SimpleSearch)
                    continue;

                if (Col.Lookup.ToLower().StartsWith("select") && Col.LookupValueField != Col.LookupTextField)
                {
                    SearchFilter.Add(Col.ColumnExpression + " in (" + SearchLookupSql(Col, "") + ")");
                    continue;
                }

                SearchFilter.Add(StripColumnRename(Col.ColumnExpression) + " like " + Database.ParameterName("simplesearchtoken"));
            }

            return String.Join(" or ", SearchFilter.ToArray());
        }

        ///////////////////////////////////////////////
        private string RefineSearchExpression(DbColumn Col)
        ///////////////////////////////////////////////
        {
            string ColumnExpression = StripColumnRename(Col.ColumnExpression);

            if (IsXmlDataType(Col))
            {
                ColumnExpression = XmlQueryExpression(Col);
            }

            if (Col.DataType != typeof(DateTime).Name)
                return ColumnExpression;

            if (Col.Format.ToUpper() == "G")
                return ColumnExpression;

            var paramName = "searchCol" + Col.ColumnKey + "Param";

            for (int I = 0; I < 2; I++)
            {
                string Key = paramName + I.ToString();
                if (this.SearchFilterParams.ContainsKey(Key))
                    if (!String.IsNullOrEmpty((string)this.SearchFilterParams[Key]))
                    {
                        DateTime D = (DateTime)ConvertToDbParam(this.SearchFilterParams[Key], Col);
                        if ((D.Hour + D.Minute + D.Second) > 0)
                            return ColumnExpression;
                    }
            }

            switch (Database.Database)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServerCE:
                    if (Col.DbDataType != "31") // "Date"
                        ColumnExpression = "convert(datetime, floor(convert(float," + ColumnExpression + ")))";
                    break;
                case DatabaseType.Oracle:
                    ColumnExpression = "trunc(" + ColumnExpression + ")";
                    break;
                case DatabaseType.PostgreSql:
                    ColumnExpression = "date_trunc('day'," + ColumnExpression + ")";
                    break;
                case DatabaseType.VisualFoxPro:
                    ColumnExpression = "ctod(dtoc(" + ColumnExpression + "))";
                    break;
                case DatabaseType.Firebird:
                    ColumnExpression = "cast(" + ColumnExpression + " AS DATE)";
                    break;
                case DatabaseType.Sybase:
                    ColumnExpression = "convert(datetime, convert(binary(4), " + ColumnExpression + "))";
                    break;
            }

            return ColumnExpression;
        }

        ///////////////////////////////////////////////
        protected string XmlQueryExpression(DbColumn Col)
        ///////////////////////////////////////////////
        {
            string ColumnExpression = String.Empty;

            if (Col.XmlElementName == String.Empty)
                ColumnExpression = "cast(" + Col.ColumnName + " as nvarchar(max))";
            else if (Col.XmlAttributeName == String.Empty)
                ColumnExpression = "cast(" + Col.ColumnName + ".query('data(/locale" + Col.XmlElementName + ")') as nvarchar(max))";
            else
                ColumnExpression = "cast(" + Col.ColumnName + ".query('data(/" + Col.XmlElementName + "/@" + Col.XmlAttributeName + ")') as nvarchar(max))";

            return ColumnExpression;
        }

        ///////////////////////////////////////////////
        protected string BuildOrderPart()
        ///////////////////////////////////////////////
        {
            return BuildOrderPart(QueryBuildModes.Normal);
        }

        ///////////////////////////////////////////////
        protected string BuildOrderPart(QueryBuildModes BuildMode)
        ///////////////////////////////////////////////
        {
            if (this is DbNetEdit)
                return this.OrderBy;

            if (BuildMode == QueryBuildModes.Count)
                return String.Empty;

            List<string> OrderPart = new List<string>();

            if (this is DbNetGrid)
            {
                DbNetGrid Grid = this as DbNetGrid;
                if (Grid.TotalBreakColumns().Count > 0)
                    Grid.FixedOrderBy = string.Join(",", Grid.TotalBreakColumns().ToArray());
                else if (Grid.ColumnList(ColumnPropertyNames.GroupHeader.ToString()).Count > 0)
                    Grid.FixedOrderBy = string.Join(",", Grid.ColumnList(ColumnPropertyNames.GroupHeader.ToString(), true, true).ToArray());
                else if (Grid.ColumnList(ColumnPropertyNames.ClearDuplicateValue.ToString()).Count > 0)
                    Grid.FixedOrderBy = string.Join(",", Grid.ColumnList(ColumnPropertyNames.ClearDuplicateValue.ToString(), true, true).ToArray());

                if (Grid.FixedOrderBy != "")
                    OrderPart.Add(Grid.FixedOrderBy);
            }

            if (this.OrderBy != "")
                OrderPart.Add(this.OrderBy);

            return string.Join(",", OrderPart.ToArray());
        }

        ///////////////////////////////////////////////
        private string OrderByExpression(DbColumn Col)
        ///////////////////////////////////////////////
        {
            int Ordinal = 0;
            foreach (DbColumn GC in Columns)
            {
                if (GC is GridColumn)
                    if (!(GC as GridColumn).Display)
                        continue;

                Ordinal++;

                if (GC.ColumnExpression == Col.ColumnExpression)
                    return Ordinal.ToString();
            }

            return Col.ColumnExpression;
        }

        ///////////////////////////////////////////////
        protected void GetLookupTables()
        ///////////////////////////////////////////////
        {
            LookupTables.Clear();

            Database.CloseConnectionOnError = false;

            foreach (DbColumn column in Columns)
            {
                if (column.Lookup.Equals(string.Empty))
                    continue;

                if (column.Lookup.StartsWith("["))
                {
                    LookupTables.Add(column.ColumnKey, ArrayToDataTable(column.Lookup));
                }
                else
                {
                    string sql = column.Lookup;

                    ListDictionary Params = Database.ParseParameters(sql);

                    if (Params.Count > 0)
                        sql = Regex.Replace(sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                    sql = AddLookupOrder(sql);
                    try
                    {
                        LookupTables.Add(column.ColumnKey, Database.GetDataTable(sql));
                    }
                    catch (Exception ex)
                    {
                        ThrowException(ex.Message, sql);
                    }
                }
            }
            Database.CloseConnectionOnError = true;
        }

        ///////////////////////////////////////////////
        protected DataTable ArrayToDataTable(string JsonArray)
        ///////////////////////////////////////////////
        {
            return ArrayToDataTable((object[])JSON.DeserializeObject(JsonArray));
        }

        ///////////////////////////////////////////////
        protected DataTable ArrayToDataTable(object[] LookupObject)
        ///////////////////////////////////////////////
        {
            DataTable DT = new DataTable();

            try
            {
                DT.Columns.Add("value", typeof(string));
                DT.Columns.Add("text", typeof(string));

                foreach (object R in LookupObject)
                {
                    object[] Item;
                    if (R is string)
                        Item = new object[] { R.ToString() };
                    else
                        Item = (object[])R;

                    DataRow DR = DT.NewRow();
                    DR[0] = Item[0].ToString();
                    if (Item.Length > 1)
                        DR[1] = Item[1].ToString();
                    else
                        DR[1] = DR[0];

                    DT.Rows.Add(DR);
                }
            }
            catch (Exception) { }

            return DT;
        }

        ///////////////////////////////////////////////
        internal string AddLookupOrder(string Sql)
        ///////////////////////////////////////////////
        {
            if (Sql.IndexOf("order by", StringComparison.CurrentCultureIgnoreCase) > -1)
                return Sql;

            if (Sql.IndexOf("group by", StringComparison.CurrentCultureIgnoreCase) > -1)
                return Sql;

            string[] Columns = new string[0];

            Match M = Regex.Match(Sql, "select (.*?) from ", RegexOptions.IgnoreCase);

            if (M.Success)
                Columns = Regex.Replace(M.Groups[1].Value, @",(?=[^\']*\'([^\']*\'[^\']*\')*$)", "~").Split(',');

            if (Columns.Length == 0)
                return Sql;

            if (Database.Database == DatabaseType.SqlServerCE)
                Sql += " order by " + StripColumnRename(((Columns.Length == 1) ? Columns[0] : Columns[1]));
            else
                Sql += " order by " + ((Columns.Length == 1) ? "1" : "2");
            return Sql;
        }

        ///////////////////////////////////////////////
        internal Pair GetDBValues(DbColumn column)
        ///////////////////////////////////////////////
        {
            return GetDBValues(column, false);
        }

        ///////////////////////////////////////////////
        internal Pair GetDBValues(DbColumn column, bool EditFieldData)
        ///////////////////////////////////////////////
        {
            Pair P = new Pair();

            P.First = Database.Reader[column.ColumnName];
            P.Second = Database.Reader[column.ColumnName];

            if (column.Lookup != "")
                if (column.Lookup.StartsWith("[") || !EditFieldData)
                {
                    if (this.LookupTables.ContainsKey(column.ColumnKey))
                        P.First = LookupValue(this.LookupTables[column.ColumnKey], P.Second);
                }
                else
                    P.First = LookupValue(column, P.Second, EditFieldData);

            if (P.First == null)
                P.First = System.DBNull.Value;

            if (P.Second == null)
                P.Second = System.DBNull.Value;

            if (IsXmlDataType(column))
            {
                P = GetTextFromXml(column, P);
            }

            return P;
        }


        ///////////////////////////////////////////////
        protected object BuildXmlFromText(DbColumn C, object Value)
        ///////////////////////////////////////////////
        {
            if (C.XmlElementName == String.Empty)
                if (Value == null)
                    return System.DBNull.Value;
                else
                    return Value;

            if (Value == null)
                Value = String.Empty;

            XmlDataDoc.LoadXml("<root/>");

            XmlElement el = (XmlElement)XmlDataDoc.DocumentElement.AppendChild(XmlDataDoc.CreateElement(C.XmlElementName));

            if (C.XmlAttributeName == String.Empty)
                el.InnerText = Value.ToString();
            else
                el.SetAttribute(C.XmlAttributeName, Value.ToString());

            return XmlDataDoc.DocumentElement.InnerXml;
        }

        ///////////////////////////////////////////////
        protected Pair GetTextFromXml(DbColumn C, Pair P)
        ///////////////////////////////////////////////
        {
            if (P.First == System.DBNull.Value)
                return P;

            if (C.XmlElementName == String.Empty)
            {
                P.First = System.Web.HttpUtility.HtmlEncode(P.First.ToString());
                P.Second = P.First;
            }
            return P;

            /*
            try
            {
                XmlDataDoc.LoadXml("<root>" + P.First.ToString() + "</root>");

                if (XmlDataDoc.DocumentElement.HasChildNodes)
                    if (XmlDataDoc.DocumentElement.ChildNodes.Count == 1)
                    {
                        XmlNode node = XmlDataDoc.DocumentElement.SelectSingleNode("//" + C.XmlElementName);

                        if (!node.HasChildNodes)
                        {
                            if (C.XmlAttributeName == String.Empty)
                                P.First = node.InnerText;
                            else
                                P.First = node.Attributes[C.XmlAttributeName].Value;

                        }
                    }
            }
            catch (Exception) { }

            if (P.First == P.Second)
                P.First = System.Web.HttpUtility.HtmlEncode(P.First.ToString());

            P.Second = P.First;

            return P;
            */
        }

        ///////////////////////////////////////////////
        protected object LookupValue(DbColumn C, object Value)
        ///////////////////////////////////////////////
        {
            return LookupValue(Value, C.Lookup);
        }

        ///////////////////////////////////////////////
        protected object LookupValue(DbColumn C, object Value, bool EditFieldData)
        ///////////////////////////////////////////////
        {
            if (C.Lookup == "")
                return "";

            if (EditFieldData)
            {
                switch (C.EditControlType)
                {
                    case EditField.ControlType.DropDownList:
                    case EditField.ControlType.MultiValueTextBoxLookup:
                        return Value;
                }
            }

            return LookupValue(Value, C.Lookup);
        }


        ///////////////////////////////////////////////
        protected object LookupValue(DataTable dataTable, object value)
        ///////////////////////////////////////////////
        {
            if (value == null)
                return System.DBNull.Value;

            if (dataTable.Columns.Count > 1)
                foreach (DataRow row in dataTable.Rows)
                    if (row[0].ToString().Equals(value.ToString()))
                        return row[1];

            return value;
        }

        ///////////////////////////////////////////////
        protected object FindLookupValue(DataTable dataTable, object value)
        ///////////////////////////////////////////////
        {
            if (value == null || value == System.DBNull.Value)
                return null;

            foreach (DataRow row in dataTable.Rows)
                foreach (DataColumn col in dataTable.Columns)
                    if (row[col].ToString().Equals(value.ToString()))
                        return row[0];

            return null;
        }

        ///////////////////////////////////////////////
        protected object ConvertToDbParam(object value)
        ///////////////////////////////////////////////
        {
            return ConvertToDbParam(value, String.Empty, System.Threading.Thread.CurrentThread.CurrentCulture, string.Empty);
        }

        ///////////////////////////////////////////////
        protected object ConvertToDbParam(object value, string dataType)
        ///////////////////////////////////////////////
        {
            return ConvertToDbParam(value, dataType, System.Threading.Thread.CurrentThread.CurrentCulture, string.Empty);
        }

        ///////////////////////////////////////////////
        protected object ConvertToDbParam(object value, DbColumn column)
        ///////////////////////////////////////////////
        {
            CultureInfo c = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (column.Culture != "")
                c = new CultureInfo(column.Culture);

            if (column.IsBoolean && column.EditControlType == EditField.ControlType.CheckBox)
                value = Convert.ToBoolean(value) ? 1 : 0;

            return ConvertToDbParam(value, column.DataType, c, column.Format);
        }

        ///////////////////////////////////////////////
        protected object ConvertToDbParam(object value, string dataType, CultureInfo C, string Format)
        ///////////////////////////////////////////////
        {
            if (value == null)
            {
                if (dataType == "Byte[]")
                    return new byte[0];
                else
                    return DBNull.Value;
            }

            if (dataType == String.Empty)
                dataType = value.GetType().Name;

            if (value is string)
            {
                string valueString = (string)value;
                if (valueString.Equals("") || valueString.Equals(string.Empty))
                    return DBNull.Value;
            }

            try
            {
                switch (dataType)
                {
                    case "Boolean":
                        if (value.ToString() == String.Empty)
                            return DBNull.Value;
                        else
                            return ParseBoolean(value.ToString());
                    case "TimeSpan":
                        return Convert.ChangeType(value, GetColumType("DateTime"));
                    case "Byte[]":
                        return value;
                    case "Guid":
                        return new Guid(value.ToString());
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                    case "Single":
                    case "Double":
                        if (value is string)
                            value = value.ToString().Replace(C.NumberFormat.CurrencySymbol, "");
                        return Convert.ChangeType(value, GetColumType(dataType));
                    default:
                        return Convert.ChangeType(value, GetColumType(dataType));
                }
            }
            catch (Exception E)
            {
                ThrowException(E.Message, "ConvertToDbParam: Value: " + value.ToString() + " DataType:" + dataType);
                return DBNull.Value;
            }
        }

        ///////////////////////////////////////////////
        internal Int32 ParseBoolean(string BoolString)
        ///////////////////////////////////////////////
        {
            switch (BoolString.ToLower())
            {
                case "true":
                case "1":
                    return 1;
                default:
                    return 0;
            }
        }

        ///////////////////////////////////////////////
        protected string SearchLookupSql(DbColumn Col, string SubFilter)
        ///////////////////////////////////////////////
        {
            Match M = Regex.Match(Col.Lookup, @"select (.*?) from", RegexOptions.IgnoreCase);
            string ColumnList = M.Groups[1].ToString();

            string ValueColumn = Database.QualifiedDbObjectName(Col.LookupValueField);

            if (ValueColumn == String.Empty)
                ValueColumn = GetSelectColumns(Col.Lookup)[0];

            string Sql = Col.Lookup.Replace(ColumnList, ValueColumn);
            Sql = Regex.Replace(Sql, " order by .*", "", RegexOptions.IgnoreCase);

            if (SubFilter == "")
                SubFilter = "like " + Database.ParameterName("simplesearchtoken");

            string Filter = "(" + StripColumnRename(Col.LookupTextExpression) + ")" + SubFilter;

            if (Regex.IsMatch(Sql, " where ", RegexOptions.IgnoreCase))
                Sql = Regex.Replace(Sql, " where ", " where " + Filter + " and ", RegexOptions.IgnoreCase);
            else
                Sql += " where " + Filter;

            return Sql;
        }

        ///////////////////////////////////////////////
        private void UpdateSearchFilterText()
        ///////////////////////////////////////////////
        {
            Resp["ok"] = true;

            Dictionary<string, object> SearchCriteriaStore = (Dictionary<string, object>)Req["searchCriteriaStore"];

            foreach (string Key in SearchCriteriaStore.Keys)
            {
                if (!(SearchCriteriaStore[Key] is Dictionary<string, object>))
                    continue;

                Dictionary<string, object> RowCriteria = SearchCriteriaStore[Key] as Dictionary<string, object>;

                if (!RowCriteria.ContainsKey("columnKey"))
                    continue;

                DbColumn C = this.ColumnFromKey(RowCriteria["columnKey"].ToString());
                if (C.Lookup == String.Empty && C.SearchLookup == String.Empty)
                    continue;

                if (C.LookupSearchMode != DbColumn.LookupSearchModeValues.SearchValue)
                    continue;

                RowCriteria["value1"] = UpdateFilterWithLookupText(C, RowCriteria["value1"].ToString());
                RowCriteria["value2"] = UpdateFilterWithLookupText(C, RowCriteria["value2"].ToString());
            }

            Resp["searchCriteriaStore"] = SearchCriteriaStore;
        }

        ///////////////////////////////////////////////
        private string UpdateFilterWithLookupText(DbColumn C, string Value)
        ///////////////////////////////////////////////
        {
            string[] ValueArray = Value.Split(',');
            List<string> TextArray = new List<string>();

            foreach (string V in ValueArray)
                TextArray.Add(LookupValue(V, (C.SearchLookup == String.Empty) ? C.Lookup : C.SearchLookup).ToString());

            return String.Join(",", TextArray.ToArray());
        }

        ///////////////////////////////////////////////
        private void ValidateSearchParams()
        ///////////////////////////////////////////////
        {
            foreach (string key in this.SearchFilterParams.Keys)
            {
                DbColumn column = this.ColumnFromParamName(key);

                if (column == null)
                    throw new Exception("ValidateSearchParams: Column [" + key + "] not found");

                if (!column.DataType.ToString().Equals("System.Byte[]"))
                {
                    string Msg = ValidateSearchParam(column, this.SearchFilterParams[key].ToString());
                    if (!Msg.Equals(""))
                    {
                        Resp["result"] = false;
                        Resp["columnKey"] = column.ColumnKey;
                        Resp["columnIndex"] = column.ColumnIndex.ToString();
                        Resp["message"] = Msg;
                        Resp["key"] = key;
                        return;
                    }
                }
            }

            Resp["result"] = true;
        }

        ///////////////////////////////////////////////
        private string ValidateSearchParam(DbColumn column, string value)
        ///////////////////////////////////////////////
        {
            string DT = column.DataType;

            if (column.Lookup != "")
                if (column.LookupSearchMode == DbColumn.LookupSearchModeValues.SearchText)
                    DT = column.LookupDataType;

            return ValidateDataTypeValue(DT, value, column.Format);
        }

        ///////////////////////////////////////////////
        internal void LookupOptions()
        ///////////////////////////////////////////////
        {
            //         DbColumn Col = this.Columns[Convert.ToInt32(Req["columnIndex"])];
            DbColumn Col = this.ColumnFromKey(Req["columnKey"].ToString());
            Dictionary<string, object> Params = (Dictionary<string, object>)Req["params"];
            ListBox LB = new ListBox();
            LB.CssClass = "lookup-options";
            LB.Height = Unit.Pixel(200);
            LB.Width = Unit.Pixel(275);

            string Lookup = Col.Lookup;

            if (Req["dialogType"].ToString() == "Search")
                if (Col.SearchLookup != String.Empty)
                    Lookup = Col.SearchLookup;

            if (!Lookup.StartsWith("["))
            {
                QueryCommandConfig Query = new QueryCommandConfig();
                Query.Sql = Lookup;

                if (Req["dialogType"].ToString() == "BulkInsert")
                {
                    if (Col is GridColumn)
                        if ((Col as GridColumn).EditLookup != "")
                            Query.Sql = (Col as GridColumn).EditLookup;
                }

                if (Col.EditControlType == EditField.ControlType.TextBoxSearchLookup)
                    ConfigureSearchLookupSql(Req["searchToken"].ToString(), Query);
                else
                    Query.Sql = AddLookupOrder(Query.Sql);

                if (Req["dialogType"].ToString() == "BulkInsert")
                {
                    ConfigureBulkInsertLookup(Query, Params);
                }
                else
                    foreach (string Key in Params.Keys)
                        Query.Params[Key] = Params[Key];

                Database.ExecuteQuery(Query);

                int TextIdx = TextOrdinal();

                while (Database.Reader.Read())
                {
                    ListItem LI = new ListItem(
                        Database.Reader[TextIdx].ToString(),
                        Database.Reader[0].ToString()
                        );
                    LB.Items.Add(LI);
                }

                Database.Reader.Close();
            }
            else
            {
                PopulateListBoxFromArray(Lookup, LB);
            }

            if (Req["selectionMode"].ToString() == "multiple")
                LB.SelectionMode = ListSelectionMode.Multiple;
            else
                LB.SelectionMode = ListSelectionMode.Single;

            int MaxCharacters = 40;
            foreach (ListItem LI in LB.Items)
                if (LI.Text.Length > MaxCharacters)
                    MaxCharacters = LI.Text.Length;

            LB.Width = Unit.Pixel(MaxCharacters * 8);

            Resp["html"] = RenderControlToString(LB);
            Resp["maxCharacters"] = MaxCharacters;
        }

        ///////////////////////////////////////////////
        internal void PopulateListBoxFromArray(string Lookup, ListBox LB)
        ///////////////////////////////////////////////
        {
            DataTable DT = ArrayToDataTable(Lookup);

            foreach (DataRow row in DT.Rows)
            {
                ListItem LI = new ListItem(Convert.ToString(row[1]), Convert.ToString(row[0]));
                LB.Items.Add(LI);
            }
        }


        ///////////////////////////////////////////////
        internal string BulkInsertColumnName()
        ///////////////////////////////////////////////
        {
            foreach (DbColumn C in this.Columns)
                if (C.BulkInsert)
                    return C.ColumnName;

            return "";
        }

        ///////////////////////////////////////////////
        internal void ConfigureBulkInsertLookup(QueryCommandConfig Query, Dictionary<string, object> Params)
        ///////////////////////////////////////////////
        {
            if (Params.Count == 0)
                return;

            string[] Cols = GetSelectColumns(Query.Sql);

            List<string> SubSelectFilter = new List<string>();
            List<string> SelectFilter = new List<string>();

            foreach (string Key in Params.Keys)
            {
                DbColumn C = ColumnFromParamName(Key);

                if (C != null)
                {
                    SubSelectFilter.Add(Database.QualifiedDbObjectName(C.ColumnName) + " = " + Database.ParameterName(C.ColumnName));
                    Query.Params[C.ColumnName] = ConvertToDbParam(Params[Key], C);
                }
                else
                {
                    SelectFilter.Add(Key + " = " + Database.ParameterName(Key));
                    Query.Params[Key] = Params[Key];
                }
            }

            string Filter = " " + ((Query.Sql.ToLower().IndexOf(" where ") > -1) ? "and" : "where");
            Filter += " " + Cols[0] + " not in ( select " + Database.QualifiedDbObjectName(BulkInsertColumnName()) + " from " + this.FromPart + " where " + Database.QualifiedDbObjectName(BulkInsertColumnName()) + " is not null";

            if (SubSelectFilter.Count > 0)
                Filter += " and " + String.Join(" and ", SubSelectFilter.ToArray());

            Filter += ")";

            if (SelectFilter.Count > 0)
                Filter += " and " + String.Join(" and ", SelectFilter.ToArray());

            if (Query.Sql.ToLower().IndexOf(" order by ") > -1)
                Query.Sql = Regex.Replace(Query.Sql, " order by ", Filter + " order by ", RegexOptions.IgnoreCase);
            else
                Query.Sql += Filter;
        }

        ///////////////////////////////////////////////
        protected void ConfigureSearchLookupSql(string SearchToken, QueryCommandConfig Query)
        ///////////////////////////////////////////////
        {
            string[] Columns = new string[0];

            Match M = Regex.Match(Query.Sql, "select (.*?) from ", RegexOptions.IgnoreCase);

            if (M.Success)
                Columns = Regex.Replace(M.Groups[1].Value, @",(?=[^\']*\'([^\']*\'[^\']*\')*$)", "~").Split(',');

            if (Columns.Length == 0)
                return;

            List<string> Filter = new List<string>();

            if (SearchToken != "")
            {
                int StartCol = 1;

                if (Columns.Length == 1)
                    StartCol = 0;

                for (int i = StartCol; i < Columns.Length; i++)
                {
                    string Token = "token" + i.ToString();
                    string Column = Columns[i];
                    Column = Regex.Replace(Column, "(distinct|unique) ", "", RegexOptions.IgnoreCase);
                    Filter.Add(Column + " like " + Database.ParameterName(Token));

                    if (SearchToken.Contains("%"))
                        Query.Params[Token] = SearchToken;
                    else
                        Query.Params[Token] = "%" + SearchToken + "%";
                }
            }

            if (Filter.Count == 0)
                Filter.Add("1=2");

            string FilterString = "";

            if (Query.Sql.ToLower().IndexOf(" where ") == -1)
                FilterString = " where (" + String.Join(" or ", Filter.ToArray()) + ")";
            else
                FilterString = " and (" + String.Join(" or ", Filter.ToArray()) + ")";

            if (Query.Sql.ToLower().IndexOf(" order by ") > -1)
                Query.Sql = Regex.Replace(Query.Sql, " order by ", FilterString + "$0", RegexOptions.IgnoreCase);
            else if (Database.Database == DatabaseType.SqlServerCE)
                Query.Sql += FilterString + " order by " + Columns[Columns.Length - 1];
            else
                Query.Sql += FilterString + " order by " + Columns.Length.ToString();

        }

        ///////////////////////////////////////////////
        protected ListDictionary GetEditFieldData(int columnIndex, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            DbColumn Col = Columns[columnIndex];

            object value = String.Empty;
            object displayValue = String.Empty;

            ListDictionary D = new ListDictionary();

            Pair P = GetDBValues(Col, true);

            switch (Col.EditControlType)
            {
                case EditField.ControlType.Upload:
                    if (P.First != System.DBNull.Value)
                    {
                        Byte[] Data;
                        string FileName = String.Empty;

                        if (P.First is Byte[])
                            Data = (Byte[])P.First;
                        else
                        {
                            FileName = P.First.ToString();
                            Data = GetFileData(Col, FileName);
                        }

                        Image TI;

                        if (DataUriSupported())
                            TI = new DataUriImage(this, Data, Col, FileName, true);
                        else
                            TI = new ThumbnailImage(this, Col, PrimaryKey, Data, "");

                        displayValue = TI.ImageUrl;

                        if (P.First is Byte[])
                        {
                            if (TI is ThumbnailImage)
                                value = (TI as ThumbnailImage).Key;
                            else
                                value = Guid.NewGuid().ToString();
                        }
                        else
                            value = P.First.ToString();
                    }
                    break;
                default:
                    if (!(P.First is Byte[]))
                    {
                        value = P.Second;
                        if (Col.DataType.Equals("Boolean"))
                            displayValue = P.First;
                        else
                            displayValue = FormatValue(P.First, Col, false);
                    }
                    break;
            }

            /*
            if (Col.Encryption != HashTypes.None)
            {
                displayValue = String.Empty;
                value = String.Empty;
            }
            */

            D["displayValue"] = displayValue;
            D["value"] = value;

            ListControl LC = GetDependentLookupOptions(Col, null, false, true);

            D["options"] = "";

            if (LC is ListControl)
                if (this is DbNetEdit)
                    D["options"] = RenderControlToString(LC);
                else
                    D["options"] = LC;

            return D;
        }

        ///////////////////////////////////////////////
        internal Byte[] GetFileData(DbColumn Col, string FileName)
        ///////////////////////////////////////////////
        {
            Byte[] Data = new Byte[0];
            string Path = Col.UploadRootFolder + "/" + FileName;

            try
            {
                Path = this.Context.Request.MapPath(Path);
            }
            catch (Exception)
            {
                return Data;
            }

            if (!File.Exists(Path))
            {
                return Data;
            }
            else
            {
                FileStream S = new FileStream(Path, FileMode.Open, FileAccess.Read);
                BinaryReader R = new BinaryReader(S);
                Data = R.ReadBytes(Convert.ToInt32(S.Length));
                S.Close();
            }

            return Data;
        }

        ///////////////////////////////////////////////
        public void GetOptions()
        ///////////////////////////////////////////////
        {
            ListDictionary R = new ListDictionary();

            object Value = Req["value"];
            int ColumnIndex = Convert.ToInt32(Req["targetColumnIndex"]);

            DbColumn C = this.Columns[ColumnIndex];
            ListControl LC = GetDependentLookupOptions(C, Value, Convert.ToBoolean(Req["dialog"]), false);

            if (LC is ListControl)
                Resp["html"] = RenderControlToString(LC);
            else
                Resp["html"] = "";

        }

        ///////////////////////////////////////////////
        internal ListControl GetDependentLookupOptions(DbColumn Col, object Value, bool Dialog, bool Dependent)
        ///////////////////////////////////////////////
        {
            string Lookup = Col.Lookup;

            if (Col is GridColumn)
                if ((Col as GridColumn).EditLookup != "")
                    Lookup = (Col as GridColumn).EditLookup;

            if (Lookup == "")
                return null;

            if (Lookup.StartsWith("["))
                return null;

            ListDictionary Params = Database.ParseParameters(Lookup);

            if (this is DbNetEdit)
                if (Dependent)
                    if (Params.Count == 0)
                        return null;

            QueryCommandConfig QC = new QueryCommandConfig(Lookup);

            foreach (string ParamName in Params.Keys)
                if (Value == null)
                    QC.Params[ParamName] = Database.Reader[ParamName];
                else
                    QC.Params[ParamName] = Value;

            DbNetData Db = GetDbNetDataInstance();

            try
            {
                Db.ExecuteQuery(QC);
            }
            catch (Exception Ex)
            {
                ThrowException("GetDependentLookupOptions ==> " + Ex.Message + Db.CommandErrorInfo());
                Db.Close();
            }

            ListControl LC;

            if (!Dialog)
            {
                LC = new DropDownList();
                LC.Style.Add(HtmlTextWriterStyle.Display, "none");
                LC.Items.Add(new ListItem(this.EmptyOptionText, String.Empty));
            }
            else
            {
                LC = new ListBox();
                LC.Height = Unit.Pixel(200);
                LC.Width = Unit.Pixel(275);
            }

            int TextIndex = (Db.Reader.FieldCount == 1) ? 0 : 1;
            while (Db.Reader.Read())
            {
                ListItem LI = new ListItem();
                LI.Value = Db.Reader[0].ToString();
                LI.Text = Db.Reader[TextIndex].ToString();

                for (int I = TextIndex + 1; I < Db.Reader.FieldCount; I++)
                    LI.Attributes.Add(Db.Reader.GetName(I), Db.Reader[I].ToString());

                LC.Items.Add(LI);
            }

            Db.Close();
            return LC;
        }

        ///////////////////////////////////////////////
        protected void PeekLookup()
        ///////////////////////////////////////////////
        {
            RunSuggestLookup("");
            int Items = 0;
            int TextIdx = TextOrdinal();

            while (Database.Reader.Read())
            {
                Items++;
                if (Items == 1)
                {
                    Resp["value"] = Database.ReaderValue(0).ToString();
                    Resp["label"] = Database.ReaderValue(TextIdx).ToString();
                }
            }

            Resp["items"] = Items;
        }

        ///////////////////////////////////////////////
        protected void RunSuggestLookup(string Token)
        ///////////////////////////////////////////////
        {
            string SearchToken = Token + "%";
            DbColumn C = this.Columns[Convert.ToInt32(Req["columnIndex"])];

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = C.Lookup;

            Dictionary<string, object> Params = (Dictionary<string, object>)Req["params"];

            foreach (string Key in Params.Keys)
                Query.Params[Key] = Params[Key];

            ConfigureSearchLookupSql(SearchToken, Query);
            Database.ExecuteQuery(Query);
        }

        ///////////////////////////////////////////////
        protected int TextOrdinal()
        ///////////////////////////////////////////////
        {
            return (Database.Reader.FieldCount == 1 ? 0 : 1);
        }

        ///////////////////////////////////////////////
        protected void GetSuggestedItems()
        ///////////////////////////////////////////////
        {
            RunSuggestLookup(Req["token"].ToString());

            Table t = new Table();
            t.CssClass = "suggest-lookup-table";
            t.CellSpacing = 0;
            t.Style["width"] = "100%";

            List<object> Items = new List<object>();

            int TextIdx = TextOrdinal();

            while (Database.Reader.Read())
            {
                TableRow tr = new TableRow();
                tr.CssClass = "suggest-lookup-item";
                tr.Attributes.Add("value", Database.ReaderValue(0).ToString());
                t.Controls.Add(tr);
                TableCell tc = new TableCell();
                tc.Text = Database.ReaderValue(TextIdx).ToString();
                tr.Controls.Add(tc);

                ListDictionary I = new ListDictionary();
                I["value"] = Database.ReaderValue(0).ToString();
                I["label"] = Database.ReaderValue(TextIdx).ToString();
                Items.Add(I);
            }

            ListDictionary R = new ListDictionary();
            Resp["html"] = RenderControlToString(t);
            Resp["items"] = Items;

        }

        ///////////////////////////////////////////////
        internal string EditTableName()
        ///////////////////////////////////////////////
        {
            if (this.FromPart.Split(' ').Length == 1)
                return this.FromPart;

            foreach (DbColumn Col in this.Columns)
                if (Col.PrimaryKey)
                    return Col.BaseTableName;

            foreach (DbColumn Col in this.Columns)
                return Col.BaseTableName;

            return "";
        }

        ///////////////////////////////////////////////
        internal void ValidateEdit()
        ///////////////////////////////////////////////
        {
            Resp["ok"] = true;

            Dictionary<string, object> Parameters = (Dictionary<string, object>)Req["params"];
            Dictionary<string, object> PrimaryKey = new Dictionary<string, object>();
            Dictionary<string, object> NewParameters = new Dictionary<string, object>();

            if (Req.ContainsKey("primaryKey"))
                PrimaryKey = (Dictionary<string, object>)Req["primaryKey"];

            if (!ValidateEdit(Parameters, PrimaryKey, NewParameters))
                return;

            Resp["parameters"] = NewParameters;
            Resp["currentRecord"] = this.CurrentRecord;
        }


        ///////////////////////////////////////////////
        internal bool ValidateEdit(Dictionary<string, object> Parameters, Dictionary<string, object> PrimaryKey, Dictionary<string, object> NewParameters)
        ///////////////////////////////////////////////
        {
            if (!ValidateParameters(Parameters))
                return false;

            string TableName = EditTableName();

            foreach (string key in Parameters.Keys)
            {
                DbColumn col = this.Columns[key];
                if (col.BaseTableName != TableName && this.FromPart != TableName)
                    continue;

                object Val;

                if (col.EditControlType == EditField.ControlType.Upload)
                    Val = Parameters[key];
                else if (col.Encryption != HashTypes.None)
                    Val = Parameters[key];
                else
                    Val = AssignDbParam(col, Parameters[key]);

                if (Val is DateTime)
                    Val = DateTime.SpecifyKind(Convert.ToDateTime(Val), DateTimeKind.Utc);

                NewParameters.Add(key, Val);
            }

            if (PrimaryKey.Count > 0)
                this.CurrentRecord = GetRecord(TableName, PrimaryKey);

            return true;
        }

        ///////////////////////////////////////////////
        internal Dictionary<string, object> GetRecord(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            if (PrimaryKey.Count == 0)
                return new Dictionary<string, object>();

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select * from " + Database.QualifiedDbObjectName(TableName);
            AddPrimaryKeyFilter(Query, PrimaryKey);

            return JsonData(Query);
        }

        ///////////////////////////////////////////////
        internal void DeleteUploadedFiles(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select * from " + TableName;
            AddPrimaryKeyFilter(Query, PrimaryKey);

            Database.ExecuteQuery(Query);

            while (Database.Reader.Read())
            {
                foreach (DbColumn Col in this.Columns)
                {
                    if (Col.UploadRootFolder == String.Empty)
                        continue;

                    if (Database.ReaderString(Col.ColumnName) == String.Empty)
                        continue;

                    RemoveFileFromDisk(Col, Database.ReaderString(Col.ColumnName));
                }
            }

        }


        ///////////////////////////////////////////////
        private DbColumn GetColumnByName(string ColumnName)
        ///////////////////////////////////////////////
        {
            foreach (DbColumn C in this.Columns)
                if (C.ColumnName.ToLower() == ColumnName.ToLower() || C.ColumnExpression.ToLower() == ColumnName.ToLower())
                    return C;

            return null;
        }

        ///////////////////////////////////////////////
        internal ListDictionary GetCurrentValues(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            ListDictionary CurrentValues = new ListDictionary(new CaseInsensitiveComparer());
            if (PrimaryKey.Count == 0)
                return CurrentValues;

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select * from " + Database.QualifiedDbObjectName(TableName);
            AddPrimaryKeyFilter(Query, PrimaryKey);

            if (!Database.ExecuteSingletonQuery(Query))
                return CurrentValues;

            for (int i = 0; i < Database.Reader.FieldCount; i++)
            {
                if (!Database.Reader.IsDBNull(i))
                    if (Database.Reader.GetValue(i).GetType().Name == "Byte[]")
                        continue;

                string ColumnName = Database.Reader.GetName(i).ToLower();

                DbColumn C = GetColumnByName(ColumnName);

                CurrentValues.Add(ColumnName, FormatValue(Database.Reader.GetValue(i), C));
            }
            return CurrentValues;
        }

        ///////////////////////////////////////////////
        public void BulkInsert()
        ///////////////////////////////////////////////
        {
            string Ids = Req["ids"].ToString();
            Dictionary<string, object> Params = Req["params"] as Dictionary<string, object>;

            CommandConfig InsertCommand = new CommandConfig();
            InsertCommand.Sql = Database.QualifiedDbObjectName(EditTableName());

            foreach (string Key in Params.Keys)
            {
                DbColumn C = ColumnFromParamName(Key);
                if (C != null)
                    InsertCommand.Params[C.ColumnName] = Params[Key];
                else
                    InsertCommand.Params[Key] = Params[Key];
            }

            foreach (string Id in Ids.Split(','))
            {
                InsertCommand.Params[this.BulkInsertColumnName()] = Id;
                Database.ExecuteInsert(InsertCommand);
            }
        }

        ///////////////////////////////////////////////
        internal void InsertRecord()
        ///////////////////////////////////////////////
        {
            if (IsReadOnly())
            {
                Resp["ok"] = false;
                Resp["message"] = "Database modification has been disabled";
                return;
            }

            Dictionary<string, object> Parameters = (Dictionary<string, object>)Req["params"];
            if (!ValidateParameters(Parameters))
                return;

            string TableName = EditTableName();

            List<string> colList = new List<string>();
            List<string> paramList = new List<string>();
            ListDictionary dbParams = new ListDictionary();

            QueryCommandConfig UniqueQuery = new QueryCommandConfig();

            Dictionary<string, object> PK = new Dictionary<string, object>();

            foreach (string key in Parameters.Keys)
            {
                DbColumn col = this.Columns[key];

                if (col == null)
                {
                    colList.Add(key);
                    paramList.Add(Database.ParameterName(key));
                    dbParams[key] = Parameters[key];
                    continue;
                }

                if (col.BaseTableName != TableName && this.FromPart != TableName)
                    continue;

                string ParamName = key;

                colList.Add(col.ColumnName);
                paramList.Add(Database.ParameterName(ParamName));

                dbParams[ParamName] = AssignDbParam(col, Parameters[key]);

                if (col.Unique)
                    UniqueQuery.Params[ParamName] = dbParams[ParamName];

                if (col.PrimaryKey)
                    PK.Add(col.ColumnName, dbParams[key]);
            }

            if (UniqueQuery.Params.Count > 0)
            {
                UniqueQuery.Sql = "select * from " + Database.QualifiedDbObjectName(TableName);
                Dictionary<string, object> ForeignKeyValues = (Dictionary<string, object>)Req["foreignKeyValues"];

                foreach (string Key in ForeignKeyValues.Keys)
                {
                    DbColumn C = ColumnFromParamName(Key);

                    if (C != null)
                        UniqueQuery.Params[C.ColumnName] = ConvertToDbParam(ForeignKeyValues[Key], C);
                }

                if (Database.ExecuteSingletonQuery(UniqueQuery))
                {
                    Resp["ok"] = false;
                    Resp["uniqueConstraintViolated"] = true;
                    Resp["message"] = "Highlighted fields must be unique";
                    return;
                }
            }

            long returnedId = -1;
            Guid UniqID = Guid.Empty;

            foreach (DbColumn Col in this.Columns)
            {
                if (Col.SequenceName != "")
                {
                    returnedId = AssignSequenceValue(Col, colList, paramList, dbParams);
                    if (Col.PrimaryKey)
                        PK.Add(Col.ColumnName, returnedId);
                }

                if (Col.DataType == "Guid" && Col.PrimaryKey && !Col.AutoIncrement)
                {
                    UniqID = AssignGuid(Col, colList, paramList, dbParams);
                    if (Col.PrimaryKey)
                        PK.Add(Col.ColumnName, UniqID);
                }
            }

            for (int i = 0; i < colList.Count; i++)
                colList[i] = Database.QualifiedDbObjectName(colList[i]);

            string sql = "insert into " + Database.QualifiedDbObjectName(TableName) + " (";
            sql += string.Join(", ", colList.ToArray());
            sql += ") values (";
            sql += string.Join(", ", paramList.ToArray());
            sql += ")";

            DbColumn AutoIncPrimaryKeyCol = null;

            foreach (DbColumn C in this.Columns)
                if (C.AutoIncrement)
                    AutoIncPrimaryKeyCol = C;

            bool previousIdentityState = Database.ReturnAutoIncrementValue;
            Database.ReturnAutoIncrementValue = (AutoIncPrimaryKeyCol != null);

            try
            {
                long genId = Database.ExecuteInsert(sql, dbParams);

                if (returnedId == -1)
                    if (genId != -1)
                    {
                        returnedId = genId;
                        if (AutoIncPrimaryKeyCol != null)
                            PK.Add(AutoIncPrimaryKeyCol.ColumnName, returnedId);
                    }

                Resp["ok"] = true;
                Resp["message"] = Translate("RecordInserted");
                Resp["autoIncrementValue"] = "";
                Dictionary<string, object> Rec = GetRecord(TableName, PK);
                if (Rec.Count == 0)
                    PK.Clear();
                Resp["primaryKey"] = PK;
                Resp["inserted"] = Rec;

                if (UniqID != Guid.Empty)
                    Resp["autoIncrementValue"] = UniqID.ToString();
                else if (returnedId > -1)
                    Resp["autoIncrementValue"] = returnedId;
            }
            catch (Exception ex)
            {
                Resp["ok"] = false;
                Resp["message"] = ex.Message + Database.CommandErrorInfo();

                return;
            }
            finally
            {
                Database.ReturnAutoIncrementValue = previousIdentityState;
            }

            LogInsert(TableName, PK);

            return;
        }


        ///////////////////////////////////////////////
        private long AssignSequenceValue(DbColumn Col, List<string> colList, List<string> paramList, ListDictionary dbParams)
        ///////////////////////////////////////////////
        {
            long returnedId = Database.GetSequenceValue(Col.SequenceName, true);
            AssignKey(returnedId, Col, colList, paramList, dbParams);
            return returnedId;
        }

        ///////////////////////////////////////////////
        private Guid AssignGuid(DbColumn Col, List<string> colList, List<string> paramList, ListDictionary dbParams)
        ///////////////////////////////////////////////
        {
            Guid G = System.Guid.NewGuid();
            AssignKey(G, Col, colList, paramList, dbParams);
            return G;
        }

        ///////////////////////////////////////////////
        private void AssignKey(object KeyValue, DbColumn Col, List<string> colList, List<string> paramList, ListDictionary dbParams)
        ///////////////////////////////////////////////
        {
            string ParamKey = Col.ColumnName;

            if (!colList.Contains(Col.ColumnName))
            {
                colList.Add(Col.ColumnName);
                paramList.Add(Database.ParameterName(ParamKey));
            }

            dbParams[ParamKey] = KeyValue;
        }

        ///////////////////////////////////////////////
        protected bool ValidateParameters(Dictionary<string, object> parameters)
        ///////////////////////////////////////////////
        {
            foreach (string key in parameters.Keys)
            {
                DbColumn C = this.Columns[key];

                if (C == null)
                    continue;

                if (C.DataType.Equals("Byte[]"))
                    continue;

                string Msg = ValidateParameter(C, parameters[key]);
                if (Msg != "")
                {
                    Resp["ok"] = false;
                    Resp["columnName"] = C.ColumnName;
                    Resp["message"] = Msg;
                    return false;
                }
            }

            return true;
        }

        ///////////////////////////////////////////////
        private string ValidateParameter(DbColumn column, object value)
        ///////////////////////////////////////////////
        {
            string StringValue = null;

            if (value != null)
                StringValue = value.ToString();

            if (String.IsNullOrEmpty(StringValue))
            {
                if (column.Required)
                    return Translate("AValueIsRequiredFor") + " <b>" + column.Label + "</b>";
                else
                    return "";
            }

            CultureInfo C = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (column.Culture != "")
                C = new CultureInfo(column.Culture);

            try
            {
                Type DataType = GetColumType(column.DataType);
                object v;

                switch (column.DataType)
                {
                    case "Boolean":
                        return "";
                        break;
                    case "Guid":
                        v = new Guid(value.ToString());
                        break;
                    case "TimeSpan":
                        v = Convert.ChangeType(value, GetColumType("DateTime"));
                        //if (value is String)
                        //    v = DateTime.ParseExact(value.ToString(), column.Format, C);

                        break;
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                    case "Single":
                    case "Double":
                        if (column.EditControlType == EditField.ControlType.CheckBox)
                            return "";
                        else if (value is string)
                            v = Convert.ChangeType(value.ToString().Replace(C.NumberFormat.CurrencySymbol, ""), DataType);
                        else
                            v = Convert.ChangeType(value, DataType);
                        break;
                    case "String":
                        switch (column.Format.ToLower())
                        {
                            case "email":
                                if (IsValidEmail(value.ToString()) == false)
                                {
                                    throw new Exception("Invalid email address");
                                };
                                break;
                        }
                        break;
                    default:
                        v = Convert.ChangeType(value, DataType);
                        break;
                }
            }
            catch (Exception)
            {
                string ExampleValue = "";
                switch (column.DataType)
                {
                    case "DateTime":
                    case "TimeSpan":
                        ExampleValue = System.DateTime.Now.ToString(column.Format) + "</b>";
                        break;
                    case "Guid":
                        ExampleValue = new Guid().ToString();
                        break;
                    case "String":
                        switch (column.Format.ToLower())
                        {
                            case "email":
                                return Translate("InvalidEmailAddress");
                                break;
                        }
                        break;
                    default:
                        ExampleValue = 123.ToString(column.Format);
                        break;
                }

                return Translate("ExampleFormat") + ": <b>" + ExampleValue + "</b> (" + value.ToString() + ")";

            }

            return "";
        }

        bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return true;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        ///////////////////////////////////////////////
        internal void UpdateRecord()
        ///////////////////////////////////////////////
        {
            if (IsReadOnly())
            {
                Resp["ok"] = false;
                Resp["message"] = "Database modification has been disabled";
                return;
            }

            Dictionary<string, object> Parameters = (Dictionary<string, object>)Req["params"];
            string TableName = EditTableName();

            if (!ValidateParameters(Parameters))
                return;

            Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)Req["primaryKey"];

            if (!ApplyRecordUpdate(Parameters, TableName, PrimaryKey))
            {
                Resp["ok"] = false;
                return;
            }

            Resp["ok"] = true;
            Resp["updated"] = GetRecord(TableName, PrimaryKey);
            Resp["message"] = Translate("RecordUpdated");
        }


        ///////////////////////////////////////////////
        internal bool ApplyRecordUpdate(Dictionary<string, object> Parameters, string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            List<string> UpdatePart = new List<string>();
            List<string> FilterPart = new List<string>();

            UpdateCommandConfig UpdateCommand = new UpdateCommandConfig();
            ListDictionary UniqueParams = new ListDictionary(new CaseInsensitiveComparer());

            foreach (string key in Parameters.Keys)
            {
                DbColumn C = this.Columns[key];

                if (C == null)
                {
                    UpdatePart.Add(Database.QualifiedDbObjectName(key) + " = " + Database.ParameterName(key));
                    UpdateCommand.Params[key] = Parameters[key];
                    continue;
                }

                if (C.BaseTableName != TableName && this.FromPart != TableName)
                    continue;

                UpdatePart.Add(Database.QualifiedDbObjectName(C.ColumnName) + " = " + Database.ParameterName(key));
                UpdateCommand.Params[key] = AssignDbParam(C, Parameters[key]);

                if (UpdateCommand.Params[key] is Byte[])
                    if ((UpdateCommand.Params[key] as Byte[]).Length == 0)
                    {
                        // Binary columns need a parameter with the type assigned 
                        // in order for it to be set to null
                        IDbDataParameter P = Database.Command.CreateParameter();
                        P.DbType = DbType.Binary;
                        P.ParameterName = key;
                        P.Value = System.DBNull.Value;
                        UpdateCommand.Params[key] = P;
                    }

                if (C.DataType == "Decimal" && C.DbDataType == "9" /*Money*/)
                    if (UpdateCommand.Params[key].ToString() == String.Empty)
                    {
                        // MS SMoney columns need a parameter with the type assigned 
                        // in order for it to be set to null
                        IDbDataParameter P = Database.Command.CreateParameter();
                        P.DbType = DbType.Decimal;
                        P.ParameterName = key;
                        P.Value = System.DBNull.Value;
                        UpdateCommand.Params[key] = P;
                    }


                if (C.Unique)
                    UniqueParams[key] = UpdateCommand.Params[key];
            }

            if (UniqueParams.Count > 0)
            {
                Dictionary<string, object> CurrentRecord = GetRecord(TableName, PrimaryKey);

                foreach (DbColumn C in this.Columns)
                    if (C.Unique)
                        if (!UniqueParams.Contains(C.ColumnName))
                            UniqueParams[C.ColumnName] = CurrentRecord[C.ColumnName].ToString();

                QueryCommandConfig UniqueQuery = new QueryCommandConfig();
                UniqueQuery.Sql = "select * from " + Database.QualifiedDbObjectName(TableName);
                AddPrimaryKeyFilter(UniqueQuery, PrimaryKey);
                UniqueQuery.Sql = UniqueQuery.Sql.Replace(" = ", " <> ").Replace(" and ", " or ");

                List<string> FilterSql = new List<string>();
                foreach (string Key in UniqueParams.Keys)
                {
                    DbColumn C = this.Columns[Key];
                    FilterSql.Add(C.ColumnName + " = " + Database.ParameterName(Key));
                    UniqueQuery.Params[Key] = ConvertToDbParam(UniqueParams[Key], C);
                }


                if (Req.ContainsKey("foreignKeyValues"))
                {
                    Dictionary<string, object> ForeignKeyValues = (Dictionary<string, object>)Req["foreignKeyValues"];

                    foreach (string Key in ForeignKeyValues.Keys)
                    {
                        DbColumn C = ColumnFromParamName(Key);

                        if (C == null)
                            continue;

                        FilterSql.Add(C.ColumnName + " = " + Database.ParameterName(C.ColumnName));
                        UniqueQuery.Params[C.ColumnName] = ConvertToDbParam(ForeignKeyValues[Key], C);
                    }
                }

                /*
                foreach (string Key in this.ParentFilterParams.Keys)
                {
                    DbColumn C = ColumnFromParamName(Key);
                    FilterSql.Add(C.ColumnName + " = " + Database.ParameterName(C.ColumnName));
                    UniqueQuery.Params[C.ColumnName] = ConvertToDbParam(this.ParentFilterParams[Key], C);
                }
                */

                UniqueQuery.Sql += " and (" + String.Join(" and ", FilterSql.ToArray()) + ")";

                if (Database.ExecuteSingletonQuery(UniqueQuery))
                {
                    Resp["ok"] = false;
                    Resp["uniqueConstraintViolated"] = true;
                    Resp["message"] = "Highlighted fields must be unique";
                    return false;
                }
            }

            UpdateCommand.Sql = "update " + Database.QualifiedDbObjectName(TableName);
            UpdateCommand.Sql += " set " + string.Join(", ", UpdatePart.ToArray());

            AddPrimaryKeyFilter(UpdateCommand, PrimaryKey);

            try
            {
                Database.ExecuteUpdate(UpdateCommand);
                LogUpdate(TableName, Parameters, PrimaryKey);
                return true;
            }
            catch (Exception ex)
            {
                Resp["message"] = ex.Message + Database.CommandErrorInfo();
                Resp["ok"] = false;
                return false;
            }
        }

        ///////////////////////////////////////////////
        internal void AuditHistory()
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)Req["primaryKey"];
            string TableName = Req["tableName"].ToString();

            CheckAuditTableExists(this.AuditTableName);
            QueryCommandConfig QueryAudit = new QueryCommandConfig();

            QueryAudit.Sql = "select * from " + this.AuditTableName + " ";
            QueryAudit.Sql += "where table_name = " + Database.ParameterName("table_name") + " ";
            QueryAudit.Sql += "and column_name = " + Database.ParameterName("column_name") + " ";
            QueryAudit.Sql += "and primary_key = " + Database.ParameterName("primary_key") + " ";
            QueryAudit.Sql += "order by updated desc";

            QueryAudit.Params["table_name"] = Database.UnqualifiedDbObjectName(DbNetLink.Util.Decrypt(TableName)).ToLower();

            DbColumn Col = null;

            if (Req.ContainsKey("columnKey"))
            {
                Col = this.ColumnFromKey(Req["columnKey"].ToString());
                QueryAudit.Params["column_name"] = Col.ColumnName.ToLower();
            }
            else
            {
                QueryAudit.Params["column_name"] = "_row_updated";
            }

            string PK = "";

            foreach (string Key in PrimaryKey.Keys)
                PK += PrimaryKey[Key].ToString();

            QueryAudit.Params["primary_key"] = PK;

            Database.ExecuteQuery(QueryAudit);

            HtmlTable T = new HtmlTable();
            T.Attributes.Add("class", "audit-history");

            while (Database.Reader.Read())
            {
                HtmlTableRow Row = new HtmlTableRow();

                if ((T.Rows.Count % 2) != 0)
                    Row.Attributes.Add("class", "odd");
                else
                    Row.Attributes.Add("class", "even");

                T.Rows.Add(Row);
                HtmlTableCell Cell = new HtmlTableCell();
                Row.Cells.Add(Cell);
                Cell.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                Cell.InnerText = Convert.ToDateTime(Database.ReaderValue("updated")).ToString("g");

                Cell = new HtmlTableCell();
                Row.Cells.Add(Cell);
                Cell.InnerText = Database.ReaderString("updated_by");

                if (Req.ContainsKey("columnKey"))
                {
                    Cell = new HtmlTableCell();
                    Row.Cells.Add(Cell);

                    Cell.InnerText = Database.ReaderString("updated_value");

                    if (Col != null)
                        if (Col.Lookup != "")
                            Cell.InnerText = LookupValue(Col, Database.ReaderValue("updated_value")).ToString();
                }
            }

            Resp["html"] = RenderControlToString(T);
        }


        ///////////////////////////////////////////////
        internal ListDictionary AuditData(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            ListDictionary AuditRows = new ListDictionary(new CaseInsensitiveComparer());

            CheckAuditTableExists(this.AuditTableName);
            QueryCommandConfig QueryAudit = new QueryCommandConfig();

            QueryAudit.Sql = "select * from " + this.AuditTableName + " ";
            QueryAudit.Sql += "where table_name = " + Database.ParameterName("table_name") + " ";
            QueryAudit.Sql += "and primary_key = " + Database.ParameterName("primary_key") + " ";
            QueryAudit.Sql += "order by updated desc";

            QueryAudit.Params["table_name"] = Database.UnqualifiedDbObjectName(TableName).ToLower();

            string PK = "";

            foreach (string Key in PrimaryKey.Keys)
                PK += PrimaryKey[Key].ToString();

            QueryAudit.Params["primary_key"] = PK;

            Database.ExecuteQuery(QueryAudit);

            while (Database.Reader.Read())
            {
                string ColumnName = Database.ReaderString("column_name");

                if (AuditRows.Contains(ColumnName))
                    continue;

                ListDictionary AuditRow = new ListDictionary();

                AuditRow["updated_by"] = Database.ReaderString("updated_by");
                AuditRow["updated"] = "";

                if (Database.ReaderValue("updated") != System.DBNull.Value)
                    AuditRow["updated"] = Convert.ToDateTime(Database.ReaderValue("updated")).ToString(AuditDateFormat);
                AuditRows[ColumnName] = AuditRow;
            }

            return AuditRows;
        }

        ///////////////////////////////////////////////
        internal void LogInsert(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            if (Audit == GridEditControl.AuditModes.None)
                return;

            CheckAuditTableExists(this.AuditTableName);
            UpdateAuditRow(TableName, "_row_created", PrimaryKey);
        }

        ///////////////////////////////////////////////
        internal void LogDelete(string TableName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            if (Audit == GridEditControl.AuditModes.None)
                return;

            CheckAuditTableExists(this.AuditTableName);
            UpdateAuditRow(TableName, "_row_deleted", PrimaryKey);
        }

        ///////////////////////////////////////////////
        internal void LogUpdate(string TableName, Dictionary<string, object> Parameters, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            ListDictionary CurrentValues = new ListDictionary();

            if (Audit == GridEditControl.AuditModes.None && AuditColumns().Count == 0)
                return;

            if (Audit == GridEditControl.AuditModes.Detail || AuditDetailColumns().Count > 0)
                CurrentValues = GetCurrentValues(TableName, PrimaryKey);

            CheckAuditTableExists(this.AuditTableName);

            if (this.Audit != AuditModes.None)
                UpdateAuditRow(TableName, "_row_updated", PrimaryKey);

            foreach (string ColumnName in Parameters.Keys)
                UpdateAuditRow(TableName, ColumnName, PrimaryKey, CurrentValues);
        }

        ///////////////////////////////////////////////
        internal void UpdateAuditRow(string TableName, string ColumnName, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            UpdateAuditRow(TableName, ColumnName, PrimaryKey, null);
        }

        ///////////////////////////////////////////////
        internal void UpdateAuditRow(string TableName, string ColumnName, Dictionary<string, object> PrimaryKey, ListDictionary CurrentValues)
        ///////////////////////////////////////////////
        {
            string UserID = (this.AuditUser == "") ? this.Context.User.Identity.Name : this.AuditUser;
            string PK = "";

            foreach (string Key in PrimaryKey.Keys)
                PK += PrimaryKey[Key].ToString();

            DbColumn Column = null;
            AuditModes AuditMode = AuditModes.Summary;
            object Now = DateTime.Now;

            if (Database.Database == DatabaseType.Access)
            {
                Now = DateTime.Now.ToOADate();
            }

            switch (ColumnName)
            {
                case "_row_updated":
                case "_row_created":
                case "_row_deleted":
                    AuditMode = this.Audit;
                    break;
                default:
                    Column = GetColumnByName(ColumnName);
                    if (Column != null)
                        AuditMode = Column.Audit;
                    break;
            }

            if (AuditMode == AuditModes.None)
                return;

            TableName = Database.UnqualifiedDbObjectName(TableName).ToLower();

            if (AuditMode == AuditModes.Summary)
            {
                UpdateCommandConfig UpdateAudit = new UpdateCommandConfig();

                UpdateAudit.Sql = "update " + AuditTableName + " set ";
                UpdateAudit.Sql += "updated_by = " + Database.ParameterName("updated_by") + ",";
                UpdateAudit.Sql += "updated = " + Database.ParameterName("updated") + " ";
                UpdateAudit.Sql += "where table_name = " + Database.ParameterName("table_name") + " ";

                UpdateAudit.Sql += "and column_name = " + Database.ParameterName("column_name") + " ";

                UpdateAudit.Sql += "and primary_key = " + Database.ParameterName("primary_key");

                UpdateAudit.Params["updated_by"] = UserID;
                UpdateAudit.Params["updated"] = Now;
                UpdateAudit.FilterParams["table_name"] = TableName.ToLower();
                UpdateAudit.FilterParams["column_name"] = ColumnName.ToLower();
                UpdateAudit.FilterParams["primary_key"] = PK;

                if (Database.ExecuteUpdate(UpdateAudit) > 0)
                    return;
            }

            CommandConfig InsertAudit = new CommandConfig();
            InsertAudit.Sql = AuditTableName;

            InsertAudit.Params["updated_by"] = UserID;
            InsertAudit.Params["updated"] = Now;
            InsertAudit.Params["table_name"] = TableName.ToLower();
            InsertAudit.Params["column_name"] = ColumnName.ToLower();
            InsertAudit.Params["primary_key"] = PK;

            if (AuditMode == AuditModes.Detail)
                if (CurrentValues != null)
                    if (CurrentValues.Contains(ColumnName))
                    {
                        GetAuditValueSize();
                        string CurrentValue = CurrentValues[ColumnName].ToString();

                        if (CurrentValue.Length > this.AuditValueSize)
                            CurrentValue = CurrentValue.Substring(0, this.AuditValueSize);
                        InsertAudit.Params["updated_value"] = CurrentValue;
                    }

            Database.ExecuteInsert(InsertAudit);
        }

        ///////////////////////////////////////////////
        internal ArrayList AuditColumns()
        ///////////////////////////////////////////////
        {
            ArrayList Cols = new ArrayList();
            foreach (DbColumn C in this.Columns)
                if (C.Audit != AuditModes.None)
                    Cols.Add(C);
            return Cols;
        }

        ///////////////////////////////////////////////
        internal ArrayList AuditDetailColumns()
        ///////////////////////////////////////////////
        {
            ArrayList Cols = new ArrayList();
            foreach (DbColumn C in this.Columns)
                if (C.Audit == AuditModes.Detail)
                    Cols.Add(C);
            return Cols;
        }

        ///////////////////////////////////////////////
        internal void GetAuditValueSize()
        ///////////////////////////////////////////////
        {
            if (this.AuditValueSize > -1)
                return;

            DataTable DT = Database.GetSchemaTable("select updated_value from " + AuditTableName);
            this.AuditValueSize = Convert.ToInt32(DT.Rows[0]["ColumnSize"]);
        }

        ///////////////////////////////////////////////
        internal TableCell AddSearchDialogLink(string LinkType)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            HyperLink H = new HyperLink();

            H.ID = LinkType.ToLower() + "SearchLink";
            H.CssClass = LinkType.ToLower() + "-search-link";
            H.Text = Translate(LinkType + "Search").Replace(" ", "&nbsp;");
            H.NavigateUrl = "#";
            C.Controls.Add(H);
            return C;
        }

        ///////////////////////////////////////////////
        internal void AddSearchOptionsRow(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);
            R.CssClass = "search-options-row";

            TableCell C = new TableCell();
            C.CssClass = "search-toolbar";
            R.Controls.Add(C);

            Table T1 = new Table();
            C.Controls.Add(T1);

            R = new TableRow();
            T1.Controls.Add(R);
            T1.Style.Add(HtmlTextWriterStyle.Width, "100%");

            C = new TableCell();
            R.Controls.Add(C);

            DropDownList D = new DropDownList();
            C.Controls.Add(D);
            D.ID = "searchFilterJoin";
            D.CssClass = "search-filter-join";
            D.Items.Add(new ListItem(Translate("MatchAllCriteria"), "and", true));
            D.Items.Add(new ListItem(Translate("MatchAtLeastOneCriteria"), "or"));
        }

        ///////////////////////////////////////////////
        internal string DefaultOrderBy()
        ///////////////////////////////////////////////
        {
            return DefaultOrderBy(false);
        }

        ///////////////////////////////////////////////
        internal string DefaultOrderBy(bool UseColumnName)
        ///////////////////////////////////////////////
        {
            int OrderColumn = 1;
            foreach (DbColumn C in this.Columns)
            {
                if (C is GridColumn)
                    if (!(C as GridColumn).Display)
                        continue;

                OrderColumn = (C.ColumnIndex + 1);
                break;
            }

            if (Database.Database == DatabaseType.SqlServerCE || UseColumnName)
                return StripColumnRename(this.Columns[OrderColumn - 1].ColumnExpression) + " asc";
            else
                return OrderColumn.ToString() + " asc";
        }


        ///////////////////////////////////////////////
        internal void GetLookupText()
        ///////////////////////////////////////////////
        {
            object Value = Req["value"];
            int ColumnIndex = Convert.ToInt32(Req["columnIndex"]);

            DbColumn C = this.Columns[ColumnIndex];

            Resp["text"] = LookupValue(C, Value);
        }

        ///////////////////////////////////////////////
        internal void AddPrimaryKeyFilter(CommandConfig Cmd, Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            AddPrimaryKeyFilter(Cmd, PrimaryKey, false);
        }

        ///////////////////////////////////////////////
        internal void AddPrimaryKeyFilter(CommandConfig Cmd, Dictionary<string, object> PrimaryKey, bool Replace)
        ///////////////////////////////////////////////
        {
            if (PrimaryKey == null)
                return;

            List<string> FilterSql = new List<string>();
            foreach (string Key in PrimaryKey.Keys)
            {
                DbColumn C = this.Columns[Key];

                if (C == null)
                    continue;

                FilterSql.Add(C.ColumnExpression + " = " + Database.ParameterName(Key));
                Cmd.Params[Key] = ConvertToDbParam(PrimaryKey[Key], C);
            }

            if (Replace)
            {
                if (Cmd.Sql.ToLower().Contains(" where "))
                    Cmd.Sql = Regex.Replace(Cmd.Sql, " where .*", "", RegexOptions.IgnoreCase);
                if (Cmd.Sql.ToLower().Contains(" order by "))
                    Cmd.Sql = Regex.Replace(Cmd.Sql, " order by .*", "", RegexOptions.IgnoreCase);
            }

            Cmd.Sql += " where (" + String.Join(" and ", FilterSql.ToArray()) + ")";
        }

        ///////////////////////////////////////////////
        internal void AddParentFilterParams(CommandConfig Cmd)
        ///////////////////////////////////////////////
        {
            foreach (string Key in this.ParentFilterParams.Keys)
            {
                DbColumn C = ColumnFromParamName(Key);

                if (C != null)
                    Cmd.Params[C.ColumnName] = ConvertToDbParam(ParentFilterParams[Key], C);
            }
        }

        ///////////////////////////////////////////////
        private object AssignDbParam(DbColumn col, object value)
        ///////////////////////////////////////////////
        {
            if (IsXmlDataType(col))
                return BuildXmlFromText(col, value);

            if (value == null)
                return System.DBNull.Value;

            switch (col.DataType)
            {
                case "Boolean":
                    if (col.IsBoolean)
                        value = Convert.ToInt16(Convert.ToBoolean(value));
                    break;
                case "Byte[]":
                    value = GetByteArray(value.ToString());
                    break;
                case "Int16":
                case "Int32":
                case "Int64":
                case "Decimal":
                case "Single":
                case "Double":
                    if (value is string)
                        value = value.ToString().Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol, "");
                    break;
                default:
                    if (col.Encryption != HashTypes.None && value.ToString() != String.Empty)
                        value = DbNetLink.Util.HashString(value.ToString(), col.Encryption.ToString());

                    if (col.UploadRootFolder != String.Empty)
                        if (String.IsNullOrEmpty((string)value))
                            RemoveFileFromDisk(col, value.ToString());
                        else
                            SaveFileToDisk(col, value.ToString());
                    break;
            }

            return ConvertToDbParam(value, col);
        }





        ///////////////////////////////////////////////
        private void SaveFileToDisk(DbColumn col, string FileName)
        ///////////////////////////////////////////////
        {
            try
            {
                Byte[] Buf = GetByteArray(FileName);

                string SaveFolder = this.UploadFolder(col);
                this.CreateDirectory(SaveFolder);
                string Path = this.Context.Request.MapPath(col.UploadRootFolder) + "/" + FileName;
                FileStream FS = new FileStream(Path, System.IO.FileMode.Create);
                FS.Write(Buf, 0, Buf.Length);
                FS.Close();
            }
            catch (Exception Ex)
            {
                ThrowException("Cannot save file to disk:" + Ex.Message);
            }
        }

        ///////////////////////////////////////////////
        private void SaveUserProfile()
        ///////////////////////////////////////////////
        {
            CheckUserProfileTableExists(this.UserProfileTableName);

            bool defaultprofile = false;
            bool defaultColumnExists = Database.ColumnExists(this.UserProfileTableName, this.UserProfileDefaultColumnName);

            if (defaultColumnExists)
                defaultprofile = Convert.ToBoolean(Req["defaultProfile"]);

            string UserID = (this.ProfileUser == String.Empty) ? this.Context.User.Identity.Name : this.ProfileUser;

            QueryCommandConfig Query = GetUserProfileQuery(true);
            long profileId = 0;

            if (Database.ExecuteSingletonQuery(Query))
            {
                UpdateCommandConfig Update = new UpdateCommandConfig();
                Update.Sql = this.UserProfileTableName;
                profileId = Convert.ToInt64(Database.ReaderValue("id"));
                Update.FilterParams["id"] = Database.ReaderValue("id");
                Update.Params["profile"] = JSON.Serialize(Req["profileProperties"]);
                if (defaultColumnExists)
                    Update.Params[this.UserProfileDefaultColumnName] = Req["defaultProfile"];

                Database.ExecuteUpdate(Update);

                if (defaultprofile)
                    ResetDefaultFlag(profileId, UserID);
            }
            else
            {
                CommandConfig Insert = new CommandConfig();
                Insert.Sql = this.UserProfileTableName;
                Insert.Params["profile_key"] = this.ProfileKey;

                if (UserID != "")
                    Insert.Params["user_id"] = UserID;

                Insert.Params["title"] = Req["profileTitle"].ToString();
                if (defaultColumnExists)
                    Insert.Params[this.UserProfileDefaultColumnName] = Req["defaultProfile"];

                Insert.Params["profile"] = JSON.Serialize(Req["profileProperties"]);
                Database.ReturnAutoIncrementValue = true;
                profileId = Database.ExecuteInsert(Insert);

                if (defaultprofile)
                    ResetDefaultFlag(profileId, UserID);

                Query = GetUserProfileQuery(false);
                Resp["items"] = this.GetComboItems(Query);
            }
        }

        ///////////////////////////////////////////////
        private void ResetDefaultFlag(long profileId, string UserID)
        ///////////////////////////////////////////////
        {
            UpdateCommandConfig Update = new UpdateCommandConfig();
            Update.Sql = "update " + this.UserProfileTableName + " set " + this.UserProfileDefaultColumnName + " = " + Database.ParameterName(this.UserProfileDefaultColumnName);
            Update.Params[this.UserProfileDefaultColumnName] = false;

            Update.Sql += " where profile_key = " + Database.ParameterName("profile_key");
            Update.Params["profile_key"] = this.ProfileKey;
            if (UserID == String.Empty)
                Update.Sql += " and user_id is null";
            else
            {
                Update.Sql += " and user_id = " + Database.ParameterName("user_id");
                Update.Params["user_id"] = UserID;
            }

            Update.Sql += " and id <> " + Database.ParameterName("id");
            Update.Params["id"] = profileId;

            Database.ExecuteUpdate(Update);
        }

        ///////////////////////////////////////////////
        private void DeleteUserProfile()
        ///////////////////////////////////////////////
        {
            CommandConfig Delete = new CommandConfig();
            Delete.Sql = this.UserProfileTableName;
            Delete.Params["id"] = Req["profileID"].ToString();
            Database.ExecuteDelete(Delete);

            QueryCommandConfig Query = GetUserProfileQuery(false);
            Resp["items"] = this.GetComboItems(Query);
        }

        ///////////////////////////////////////////////
        private void SelectUserProfile()
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Select = new QueryCommandConfig();
            Select.Sql = "select * from " + this.UserProfileTableName;
            Select.Params["id"] = Req["profileID"].ToString();
            Resp["profileProperties"] = "";
            if (Database.ExecuteSingletonQuery(Select))
                Resp["profileProperties"] = Database.ReaderValue("profile");

        }

        ///////////////////////////////////////////////
        private List<object> LoadUserProfiles()
        ///////////////////////////////////////////////
        {
            CheckUserProfileTableExists(this.UserProfileTableName);
            QueryCommandConfig Query = GetUserProfileQuery(false);
            return this.GetComboItems(Query);
        }

        ///////////////////////////////////////////////
        private void GetDefaultUserProfileId()
        ///////////////////////////////////////////////
        {
            CheckUserProfileTableExists(this.UserProfileTableName);

            if (Database.ColumnExists(this.UserProfileTableName, this.UserProfileDefaultColumnName))
            {
                QueryCommandConfig Query = GetUserProfileQuery(false);
                Database.ExecuteQuery(Query);

                while (Database.Reader.Read())
                {
                    if (Database.ReaderString(this.UserProfileDefaultColumnName) == "True")
                    {
                        Resp["defaultUserProfileId"] = Database.ReaderString("id");
                        break;
                    }
                }
            }
        }


        ///////////////////////////////////////////////
        private QueryCommandConfig GetUserProfileQuery(bool IncludeTitle)
        ///////////////////////////////////////////////
        {
            string UserID = this.ProfileUser == String.Empty ? this.Context.User.Identity.Name : this.ProfileUser;

            List<string> columns = new List<string>() { "id", "title" };

            if (Database.ColumnExists(this.UserProfileTableName, this.UserProfileDefaultColumnName))
                columns.Add(this.UserProfileDefaultColumnName);

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select " + String.Join(",", columns.ToArray()) + " from " + this.UserProfileTableName;
            Query.Sql += " where profile_key = " + Database.ParameterName("profile_key");
            Query.Params["profile_key"] = this.ProfileKey;
            if (UserID == String.Empty)
                Query.Sql += " and user_id is null";
            else
            {
                Query.Sql += " and user_id = " + Database.ParameterName("user_id");
                Query.Params["user_id"] = UserID;
            }
            if (IncludeTitle)
            {
                Query.Sql += " and title = " + Database.ParameterName("title");
                Query.Params["title"] = Req["profileTitle"].ToString();
            }

            Query.Sql += " order by title";

            return Query;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        protected void CheckUserProfileTableExists(string TableName)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (Database.TableExists(TableName))
                return;

            CreateUserProfileTable(TableName);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        protected void CheckAuditTableExists(string TableName)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (Database.TableExists(TableName))
                return;

            CreateAuditTable(TableName);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        protected void CreateAuditTable(string TableName)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Database.AddTable(TableName);
            Database.AddColumn(TableName, "table_name", "varchar(50)");
            Database.AddColumn(TableName, "column_name", "varchar(50)");
            Database.AddColumn(TableName, "primary_key", "varchar(50)");
            Database.AddColumn(TableName, "updated_by", "varchar(50)");
            Database.AddColumn(TableName, "updated", "datetime");
            Database.AddColumn(TableName, "updated_value", "varchar(255)");

            Database.AddIndex(TableName, new string[] { "table_name", "primary_key" });
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        protected void CreateUserProfileTable(string TableName)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Database.AddTable(TableName);
            Database.AddColumn(TableName, "component", "varchar(50)");
            Database.AddColumn(TableName, "profile_key", "varchar(255)");
            Database.AddColumn(TableName, "user_id", "varchar(50)");
            Database.AddColumn(TableName, "title", "varchar(255)");
            Database.AddColumn(TableName, "profile", "text");
            Database.AddColumn(TableName, UserProfileDefaultColumnName, "bit");
        }

        ///////////////////////////////////////////////
        private void RemoveFileFromDisk(DbColumn Col, string FileName)
        ///////////////////////////////////////////////
        {
            try
            {
                string FilePath = Col.UploadRootFolder + "/" + FileName;
                FilePath = this.Context.Request.MapPath(FilePath.Replace("//", "/"));
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (Exception)
            {
            }
        }

        ///////////////////////////////////////////////
        private Byte[] GetByteArray(string Key)
        ///////////////////////////////////////////////
        {
            if (Key == "")
                return new byte[0];

            byte[] Buf = null;
            if (HttpContext.Current.Session == null)
            {
                string fileName = "";
                Stream stream = new FileStream(fileName, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);
                Buf = reader.ReadBytes(Convert.ToInt32(stream.Length));
                stream.Close();
                File.Delete(fileName);
            }
            else
            {
                if (this.Context.Session[Key] == null)
                    ThrowException("Binary session data for key: " + Key.ToString() + " not found.");
                else
                {
                    ThumbnailInfo TI = (ThumbnailInfo)this.Context.Session[Key];
                    Buf = TI.Buffer;
                    this.Context.Session.Remove(Key);
                }
            }

            return Buf;
        }

        ///////////////////////////////////////////////
        protected string PrimaryKeyTableName()
        ///////////////////////////////////////////////
        {
            string TableName = "";
            foreach (DbColumn column in Columns)
            {
                if (column.PrimaryKey)
                {
                    TableName = Database.QualifiedDbObjectName(column.BaseTableName);
                    if (column.BaseSchemaName != String.Empty)
                        TableName = Database.QualifiedDbObjectName(column.BaseSchemaName) + "." + TableName;
                    break;
                }
            }

            return TableName;
        }

        ///////////////////////////////////////////////
        protected bool IsReadOnly()
        ///////////////////////////////////////////////
        {
            return ConfigValue("ReadOnly").ToLower() == "yes" || ConfigValue("ReadOnly").ToLower() == "true";
        }

        ///////////////////////////////////////////////
        protected void DeleteRecord()
        ///////////////////////////////////////////////
        {
            if (IsReadOnly())
            {
                Resp["ok"] = false;
                Resp["message"] = "Database modification has been disabled";
                return;
            }

            string TableName = PrimaryKeyTableName();
            object[] PrimaryKeyList = (object[])Req["pks"];
            ArrayList Records = new ArrayList();

            foreach (Dictionary<string, object> PrimaryKey in PrimaryKeyList)
            {
                Records.Add(GetRecord(TableName, PrimaryKey));

                DeleteUploadedFiles(TableName, PrimaryKey);

                UpdateCommandConfig DeleteCommand = new UpdateCommandConfig();
                DeleteCommand.Sql = "delete from " + TableName;
                AddPrimaryKeyFilter(DeleteCommand, PrimaryKey);

                try
                {
                    Database.ExecuteDelete(DeleteCommand);
                }
                catch (Exception ex)
                {
                    Resp["ok"] = false;
                    Resp["message"] = ex.Message + Database.CommandErrorInfo();
                    return;
                }

                LogDelete(TableName, PrimaryKey);
            }

            if (PrimaryKeyList.Length > 1)
                Resp["message"] = Translate("RecordsDeleted");
            else
                Resp["message"] = Translate("RecordDeleted");

            Resp["records"] = Records.ToArray();
            Resp["ok"] = true;
        }

        ///////////////////////////////////////////////
        protected void DataUploadColumnMapping()
        ///////////////////////////////////////////////
        {
            HtmlTable T = new HtmlTable();
            T.Attributes.Add("class", "column-map-table");

            HtmlTableRow R = new HtmlTableRow();
            T.Rows.Add(R);
            R.Attributes.Add("class", "column-map-header");

            HtmlTableCell C = new HtmlTableCell();
            R.Cells.Add(C);
            C.InnerText = "Source Column";
            C = new HtmlTableCell();
            R.Cells.Add(C);
            C.InnerText = "Target Column";

            string FileName = Req["fileName"].ToString();
            string FilePath = this.Context.Request.MapPath(this.UploadDataFolder) + "\\" + FileName;

            DataTable TargetColumns = Database.GetSchemaTable("select * from " + FromPart + " where 1=2");

            Resp["message"] = String.Empty;
            Resp["tableNotFound"] = false;

            try
            {
                using (DbNetData Db = new DbNetData(DbNetData.BuildConnectionStringFromFileName(FilePath)))
                {
                    Db.Open();

                    string TableName = Req["tableName"].ToString();

                    if (Path.GetExtension(FilePath).ToLower() == ".csv")
                    {
                        Db.ExecuteQuery($"select * from {Path.GetFileName(FilePath)}");
                        Db.Reader.Read();
                        DataTable SourceColumns = new DataTable();
                        SourceColumns.Columns.Add(new DataColumn("ColumnName"));
                        for (int i = 0; i < Db.Reader.FieldCount; i++)
                        {
                            string ColumnName = Db.Reader.GetName(i).ToLower();
                            DataRow row = SourceColumns.NewRow();
                            row["ColumnName"] = ColumnName;
                            SourceColumns.Rows.Add(row);
                        }
                        Db.Reader.Close();
                        BuildMappingTable(SourceColumns, TargetColumns, T);
                    }
                    else
                    {
                        if (Convert.ToBoolean(Req["loadTables"]))
                        {
                            DataRow[] SourceTables = Db.MetaDataCollection(MetaDataType.Tables).Select(Db.UserTableFilter(), "TABLE_SCHEMA,TABLE_NAME");

                            if (SourceTables.Length > 0)
                            {
                                HtmlGenericControl P = new HtmlGenericControl("div");
                                HtmlGenericControl TableText = new HtmlGenericControl("span");
                                TableText.InnerText = "Table ";
                                TableText.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                                P.Controls.Add(TableText);
                                HtmlSelect TableSelect = new HtmlSelect();
                                TableSelect.Attributes.Add("class", "table-select");
                                P.Controls.Add(TableSelect);

                                foreach (DataRow SourceTable in SourceTables)
                                {
                                    string SourceTableName = SourceTable["TABLE_NAME"].ToString();

                                    ListItem I = new ListItem(SourceTableName);
                                    I.Selected = (TableName.ToLower() == SourceTableName.ToLower());
                                    TableSelect.Items.Add(I);
                                }

                                if (TableName == String.Empty)
                                    TableName = SourceTables[0]["TABLE_NAME"].ToString();

                                Resp["table_select"] = RenderControlToString(P);
                            }
                        }


                        if (TableName != String.Empty)
                        {

                            if (Path.GetExtension(FilePath).ToLower() != ".csv")
                            {
                                DataTable SourceTables = Db.MetaDataCollection(MetaDataType.Tables);

                                if (SourceTables.Select("TABLE_NAME = '" + TableName.Replace("'", "''") + "'").Length == 0)
                                {
                                    Resp["tableNotFound"] = true;
                                    Resp["message"] = "Table [<b>" + TableName + "</b>] not found";
                                    TableName = SourceTables.Rows[0]["TABLE_NAME"].ToString();
                                }
                            }

                            DataTable SourceColumns;

                            try
                            {
                                SourceColumns = Db.GetSchemaTable(TableName);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Unable to read column information");
                            }

                            BuildMappingTable(SourceColumns, TargetColumns, T);

                        }
                    }
                    Db.Close();
                }
            }
            catch (Exception Ex)
            {
                Resp["message"] = Ex.Message;
                return;
            }


            Resp["column_select"] = RenderControlToString(T);
        }


        private void BuildMappingTable(DataTable SourceColumns, DataTable TargetColumns, HtmlTable T)
        {
            bool FixedColumnMapping = false;

            foreach (DataRow SourceColumn in SourceColumns.Rows)
            {
                string SourceColumnName = SourceColumn["ColumnName"].ToString();
                if (Regex.IsMatch(SourceColumnName, @"F\d{1,3}"))
                    continue;

                HtmlTableRow R = new HtmlTableRow();
                R.Attributes.Add("class", "column-map-data");
                T.Rows.Add(R);

                HtmlTableCell C = new HtmlTableCell();
                R.Cells.Add(C);
                C.InnerText = SourceColumnName;

                C = new HtmlTableCell();
                R.Cells.Add(C);

                HtmlSelect S = new HtmlSelect();
                S.Items.Add(new ListItem(Translate("Unmatched"), String.Empty));
                S.SelectedIndex = 0;

                foreach (DataRow TargetColumn in TargetColumns.Rows)
                {
                    string TargetColumnName = TargetColumn["ColumnName"].ToString();
                    S.Items.Add(TargetColumnName);

                    string UploadDataColumn = String.Empty;

                    DbColumn Col = this.GetColumnByName(TargetColumnName);

                    if (Col != null)
                        UploadDataColumn = Col.UploadDataColumn;

                    if (UploadDataColumn.StartsWith("{"))
                        continue;

                    if (StrippedColumnName(SourceColumnName) != String.Empty)
                    {
                        if (
                            StrippedColumnName(TargetColumnName) == StrippedColumnName(SourceColumnName) ||
                            StrippedColumnName(UploadDataColumn) == StrippedColumnName(SourceColumnName)
                        )
                        {
                            S.Attributes.Add("currentValue", TargetColumnName);
                            S.SelectedIndex = S.Items.Count - 1;
                        }
                    }

                    if (UploadDataColumn != String.Empty)
                        if (StrippedColumnName(UploadDataColumn) == StrippedColumnName(SourceColumnName))
                        {
                            S.Attributes.Add("uploadDataColumn", UploadDataColumn);
                            FixedColumnMapping = true;
                        }
                }

                if (S.SelectedIndex == 0)
                    S.Attributes.Add("class", "un-mapped");

                C.Controls.Add(S);
            }

            Resp["fixedColumnMapping"] = FixedColumnMapping;
        }

        ///////////////////////////////////////////////
        protected void LoadDataUpload()
        ///////////////////////////////////////////////
        {
            string FileName = Req["fileName"].ToString();
            string TableName = Req["tableName"].ToString();
            string FilePath = this.Context.Request.MapPath(this.UploadDataFolder) + "\\" + FileName;
            Dictionary<string, object> Mapping = Req["mapping"] as Dictionary<string, object>;
            DateTime Today = System.DateTime.Today;
            DateTime Now = System.DateTime.Now;
            string UniqueID = Guid.NewGuid().ToString();
            string User = (this.AuditUser == "") ? this.Context.User.Identity.Name : this.AuditUser;
            int RecordCount = 0;

            // FileName.Replace((HttpContext.Current.Session.SessionID + "_"), String.Empty);
            Resp["message"] = String.Empty;

            //DataTable TargetColumns = Database.GetSchemaTable("select * from " + FromPart + " where 1=2");

            int LineNo = 0;
            GetLookupTables();

            try
            {
                using (DbNetData Db = new DbNetData(DbNetData.BuildConnectionStringFromFileName(FilePath)))
                {
                    try
                    {
                        Db.Open();
                        Database.BeginTransaction();

                        if (Path.GetExtension(FilePath).ToLower() == ".csv")
                        {
                            Db.ExecuteQuery("select * from " + Path.GetFileName(FilePath));
                        }
                        else
                        {
                            Db.ExecuteQuery("select * from " + Db.QualifiedDbObjectName(TableName));
                        }

                        QueryCommandConfig UniqueQuery = new QueryCommandConfig("select * from " + Db.QualifiedDbObjectName(this.FromPart));
                        List<string> UniqueColumns = new List<string>();

                        foreach (string Key in Mapping.Keys)
                        {
                            string MappedColumn = Mapping[Key].ToString();
                            if (MappedColumn == String.Empty)
                                continue;

                            DbColumn Col = this.GetColumnByName(MappedColumn);

                            if (Col.Unique)
                                UniqueColumns.Add(Key);
                        }

                        while (Db.Reader.Read())
                        {
                            ListDictionary Params = new ListDictionary();
                            ListDictionary FilterParams = new ListDictionary();

                            LineNo++;
                            bool EmptyRow = true;
                            for (var i = 0; i < Db.Reader.FieldCount; i++)
                                if (Db.ReaderString(i).Trim() != String.Empty)
                                {
                                    EmptyRow = false;
                                    break;
                                }

                            if (EmptyRow)
                                continue;

                            foreach (string Key in Mapping.Keys)
                            {
                                string MappedColumn = Mapping[Key].ToString();
                                if (MappedColumn == String.Empty)
                                    continue;

                                DbColumn Col = this.GetColumnByName(MappedColumn);
                                object ColumnValue = Db.ReaderValue(Key);

                                if (Col.Required && Db.ReaderString(Key) == String.Empty)
                                    throw new Exception("Value required in column [<b>" + Key + "</b>]");
                                else if (Col.Unique)
                                    UniqueQuery.Params[MappedColumn] = ColumnValue;
                                else if (Col.Lookup != "")
                                {
                                    ColumnValue = FindLookupValue(this.LookupTables[Col.ColumnKey], ColumnValue);
                                    if (ColumnValue == null)
                                        throw new Exception("Value [<b>" + Db.ReaderString(Key) + "</b>] in column [<b>" + Key + "</b>] is not valid");
                                }

                                if (ColumnValue != System.DBNull.Value)
                                    ColumnValue = ConvertToDbParam(ColumnValue, Col);

                                if (Col.PrimaryKey)
                                    FilterParams[MappedColumn] = ColumnValue;
                                else
                                    Params[MappedColumn] = ColumnValue;
                            }

                            foreach (DbColumn Col in this.Columns)
                            {
                                object Value = null;

                                switch (Col.UploadDataColumn.ToLower())
                                {
                                    case "{today}":
                                        Value = Today;
                                        break;
                                    case "{now}":
                                        Value = Now;
                                        break;
                                    case "{user}":
                                        Value = User;
                                        break;
                                    case "{filename}":
                                        Value = FileName;
                                        break;
                                    case "{filename_no_extension}":
                                        Value = FileName.Split('.').First();
                                        break;
                                    case "{uniqueid}":
                                        Value = UniqueID;
                                        break;
                                }

                                if (Value != null)
                                    Params[Col.ColumnName] = Value;
                            }

                            if (UniqueQuery.Params.Count > 0)
                                if (Database.ExecuteSingletonQuery(UniqueQuery))
                                {
                                    throw new Exception("Value(s) in column(s) [<b>" + String.Join(",", UniqueColumns.ToArray()) + "</b>] must be unique.");
                                }

                            RecordCount++;

                            if (FilterParams.Count > 0)
                            {
                                QueryCommandConfig Query = new QueryCommandConfig("select * from " + Database.QualifiedDbObjectName(this.FromPart));
                                Query.Params = FilterParams;

                                if (Database.ExecuteSingletonQuery(Query))
                                {
                                    UpdateCommandConfig Update = new UpdateCommandConfig(Database.QualifiedDbObjectName(this.FromPart));
                                    Update.Params = Params;
                                    Update.FilterParams = FilterParams;
                                    Database.ExecuteUpdate(Update);
                                    continue;
                                }
                                else
                                {
                                    foreach (string Key in FilterParams.Keys)
                                        Params[Key] = FilterParams[Key];
                                }
                            }
                            CommandConfig Insert = new CommandConfig(Database.QualifiedDbObjectName(this.FromPart));
                            Insert.Params = Params;
                            Database.ExecuteInsert(Insert);
                        }
                    }
                    catch (Exception Ex)
                    {
                        throw new Exception((LineNo == 0) ? String.Empty : ("Error on Line No. [<b>" + LineNo.ToString() + "</b>]. ") + Ex.Message);
                    }

                    Database.Commit();
                    Db.Close();
                }
            }
            catch (Exception Ex)
            {
                Database.Close();
                Resp["message"] = Ex.Message;
            }

            Resp["record_count"] = RecordCount;
            Resp["today"] = Today;
            Resp["now"] = Now;
            Resp["user"] = User;
            Resp["filename"] = FileName;
            Resp["uniqueid"] = UniqueID;
        }

        ///////////////////////////////////////////////
        private string StrippedColumnName(string ColumnName)
        ///////////////////////////////////////////////
        {
            return Regex.Replace(ColumnName.ToLower(), "[^A-Za-z0-9]", String.Empty);
        }

        ///////////////////////////////////////////////
        protected void StreamData()
        ///////////////////////////////////////////////
        {
            ThumbnailInfo Info = null;
            string Key = "";
            byte[] Buffer = null;

            if (this.Context.Session == null)
            {
                if (Req.ContainsKey("query"))
                {
                    QueryCommandConfig Query = new QueryCommandConfig();
                    Query.Sql = DbNetLink.Util.Decrypt(Req["query"].ToString());

                    Dictionary<string, object> D = (Dictionary<string, object>)JSON.DeserializeObject(DbNetLink.Util.Decrypt(Req["pk"].ToString()));

                    foreach (string K in D.Keys)
                        Query.Params.Add(K, D[K]);

                    Database.ExecuteSingletonQuery(Query);
                    Buffer = (byte[])Database.Reader[0];
                    Buffer = StripOleHeader(Buffer);
                    StreamBinaryData(Buffer, Convert.ToInt32(Req["maxthumbnailheight"]), String.Empty, false, String.Empty);
                    return;
                }

            }
            else if (Req.ContainsKey("key"))
            {
                Key = Req["key"].ToString();
                Info = (ThumbnailInfo)this.Context.Session[Key];
            }

            if (Info == null)
                return;

            bool fileNotFound = false;

            string FileName = string.Empty;

            if (Info.Buffer != null)
            {
                Buffer = Info.Buffer;
                Buffer = StripOleHeader(Buffer);
            }
            else
            {
                if (!Database.ExecuteSingletonQuery(Info.Query))
                    ThrowException(Info.Query.Sql, Database.CommandErrorInfo());

                if (Info.DataType == "Byte[]")
                {
                    Buffer = (byte[])Database.Reader[0];
                    Buffer = StripOleHeader(Buffer);
                    FileName = Info.FileName;
                }
                else
                {
                    FileName = Convert.ToString(Database.Reader[0]);
                    string path = Info.UploadRootFolder + "/" + FileName;

                    try
                    {
                        path = this.Context.Request.MapPath(path);
                    }
                    catch (Exception)
                    {
                    }

                    if (!File.Exists(path))
                    {
                        fileNotFound = true;
                    }
                    else
                    {
                        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                        BinaryReader reader = new BinaryReader(stream);
                        Buffer = reader.ReadBytes(Convert.ToInt32(stream.Length));
                        stream.Close();
                    }
                }
            }

            switch (Req["method"].ToString())
            {
                case "thumbnail":
                case "stream":
                    if (!Info.Persist)
                        (this.Context.Session[Key] as ThumbnailInfo).Buffer = null;
                    StreamBinaryData(Buffer, Info.MaxThumbnailHeight, Info.UploadExtFilter, fileNotFound, FileName);
                    break;
                case "document-size":
                    GetDocumentSize(Buffer, fileNotFound);
                    break;
                default:
                    this.Context.Response.Clear();
                    this.Context.Response.StatusCode = 404;
                    this.Context.Response.End();
                    this.CloseConnection();
                    break;
            }
        }


        ///////////////////////////////////////////////
        public void GetDataUri()
        ///////////////////////////////////////////////
        {
            QueryCommandConfig Query = new QueryCommandConfig();

            List<string> ColumnNames = new List<string>();

            ColumnNames.Add(Req["columnName"].ToString());
            string FileName = Req["fileName"].ToString();

            DbColumn Col = this.GetColumnByName(ColumnNames[0]);

            if (Col.UploadFileNameColumn != String.Empty)
                ColumnNames.Add(Col.UploadFileNameColumn);

            Query.Sql = "select " + String.Join(",", ColumnNames.ToArray()) + " from " + FromPart;
            AddPrimaryKeyFilter(Query, (Dictionary<string, object>)Req["primaryKey"]);

            Resp["url"] = String.Empty;
            Byte[] Data;

            if (!Database.ExecuteSingletonQuery(Query))
                return;

            if (Database.Reader.GetValue(0) is Byte[])
            {
                Data = Database.Reader.GetValue(0) as Byte[];
                if (Database.Reader.FieldCount > 1)
                    FileName = Database.Reader.GetValue(1).ToString();
            }
            else
            {
                FileName = Database.Reader.GetValue(0).ToString();
                Data = this.GetFileData(Col, FileName);
            }

            ByteInfo II = new ByteInfo(Data, FileName, Col.UploadExtFilter);

            ImageData ID = new ImageData(Data, II.ContentType, FileName);
            ID.IsImage = II.IsImage;

            Resp["url"] = "data:" + II.ContentType + ";base64," + Convert.ToBase64String(Data);

            if (HttpContext.Current.Session != null)
            {
                string Key = Guid.NewGuid().ToString();
                HttpContext.Current.Session[Key] = ID;
                Resp["url"] = (HttpContext.Current.Request.ApplicationPath + "/dbnetgrid.ashx?method=image-data&key=" + Key).Replace("//", "/");
            }

            Resp["fileName"] = FileName;
            Resp["isImage"] = II.IsImage;

            if (II.Img != null)
            {
                Resp["width"] = II.Img.Width;
                Resp["height"] = II.Img.Height;
            }
        }


        ///////////////////////////////////////////////
        internal string GetExtension(string FileName)
        ///////////////////////////////////////////////
        {
            if (FileName == String.Empty)
                return String.Empty;
            else
                return Path.GetExtension(FileName).ToLower().Trim().TrimStart('.').Trim(); ;
        }


        ///////////////////////////////////////////////
        internal bool IsMatchingPrimaryKey(Dictionary<string, object> PK)
        ///////////////////////////////////////////////
        {
            bool Match = true;

            for (var i = 0; i < Database.Reader.FieldCount; i++)
            {
                string K = Database.Reader.GetName(i);
                if (PK.ContainsKey(K))
                    if (PK[K].ToString() != Database.ReaderValue(i).ToString())
                    {
                        Match = false;
                        break;
                    }
            }

            return Match;
        }

        ///////////////////////////////////////////////
        protected byte[] StripOleHeader(byte[] byteArray)
        ///////////////////////////////////////////////
        {
            if (byteArray.Length > 2 && byteArray[0] == 21 && byteArray[1] == 28)
            {
                byte[] headerRemoved = new byte[byteArray.Length - 78];
                System.Buffer.BlockCopy(byteArray, 78, headerRemoved, 0, byteArray.Length - 78);
                byteArray = headerRemoved;
            }
            return byteArray;
        }


        ///////////////////////////////////////////////
        internal void GetDocumentSize(byte[] Buffer, bool fileNotFound)
        ///////////////////////////////////////////////
        {
            if (fileNotFound)
                Buffer = GetWarningImage();

            int DocumentHeight = -1;
            int DocumentWidth = -1;

            try
            {
                MemoryStream MS = new MemoryStream(Buffer);
                System.Drawing.Image I = System.Drawing.Image.FromStream(MS);
                DocumentHeight = I.Height;
                DocumentWidth = I.Width;
            }
            catch (Exception)
            {
            }

            Resp["documentIsImage"] = (DocumentHeight > -1);
            Resp["documentHeight"] = DocumentHeight;
            Resp["documentWidth"] = DocumentWidth;
        }


        ///////////////////////////////////////////////
        internal byte[] GetWarningImage()
        ///////////////////////////////////////////////
        {
            byte[] Buffer;

            Assembly A = Assembly.GetAssembly(typeof(Shared));
            Stream S = A.GetManifestResourceStream("DbNetLink.Resources.Images.Warning.gif");
            BinaryReader R = new BinaryReader(S);
            Buffer = R.ReadBytes(Convert.ToInt32(S.Length));
            R.Close();
            return Buffer;
        }

        ///////////////////////////////////////////////
        internal void StreamBinaryData(byte[] Buffer, int MaxThumbnailHeight, string UploadExtFilter, bool fileNotFound, string FileName)
        ///////////////////////////////////////////////
        {
            if (fileNotFound)
                Buffer = GetWarningImage();

            bool Download = Req.ContainsKey("download");

            if (Download)
                if (FileName == String.Empty)
                {
                    FileName = "unknown";
                    if (Req.ContainsKey("filename"))
                        if (Req["filename"].ToString() != String.Empty)
                            FileName = Req["filename"].ToString();
                }

            ByteInfo II = new ByteInfo(Buffer, FileName, UploadExtFilter);

            this.Context.Response.Clear();

            if (II.IsImage && !Download)
            {
                int w = II.Img.Width;
                int h = II.Img.Height;

                if (Req["method"].ToString() == "thumbnail")
                    if (II.Img.Height > MaxThumbnailHeight)
                    {
                        double factor = ((double)MaxThumbnailHeight / (double)II.Img.Height);
                        w = Convert.ToInt16((w * factor));
                        h = Convert.ToInt16((h * factor));
                    }
                this.Context.Response.ContentType = "image/jpeg";
                new System.Drawing.Bitmap(II.Img, w, h).Save(this.Context.Response.OutputStream, ImageFormat.Jpeg);
            }
            else
            {
                if (Req["method"].ToString().ToLower() == "thumbnail")
                {
                    string ResourceName = "DbNetLink.Resources.Images.Ext.unknown.gif";

                    if (II.Ext != String.Empty)
                    {
                        ResourceName = "DbNetLink.Resources.Images.Ext." + II.Ext.ToLower() + ".gif";

                        if (Assembly.GetExecutingAssembly().GetManifestResourceInfo(ResourceName) == null)
                            ResourceName = "DbNetLink.Resources.Images.Ext.unknown.gif";
                    }

                    Assembly A = Assembly.GetAssembly(typeof(Shared));

                    Stream s = A.GetManifestResourceStream(ResourceName);
                    this.Context.Response.ContentType = "image/gif";
                    BinaryReader reader = new BinaryReader(s);
                    this.Context.Response.BinaryWrite(reader.ReadBytes(Convert.ToInt32(s.Length)));
                }
                else
                {
                    this.Context.Response.ContentType = II.ContentType;

                    if (Download)
                    {
                        if (!FileName.Contains("."))
                            FileName = FileName + "." + II.Ext.Replace(".", String.Empty);

                        Context.Response.AddHeader("content-disposition", "attachment; filename=\"" + FileName + "\"");
                        Context.Response.BinaryWrite(Buffer);
                    }
                    else
                    {
                        Context.Response.BinaryWrite(Buffer);
                    }
                }
            }

            this.CloseConnection();
            this.Context.Response.End();
        }

        ///////////////////////////////////////////////
        internal Winista.Mime.MimeType GetMimeInfo(byte[] byteArray)
        ///////////////////////////////////////////////
        {
            if (this.MimeInfo == null)
            {
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("DbNetLink.Resources.Xml.mime-types.xml");
                System.Xml.XmlDocument MT = new System.Xml.XmlDocument();
                StreamReader SR = new StreamReader(s);
                MT.LoadXml(SR.ReadToEnd());
                SR.Close();
                this.MimeInfo = new Winista.Mime.MimeTypes(MT);
            }
            sbyte[] fileData = Winista.Mime.SupportUtil.ToSByteArray(byteArray);
            return this.MimeInfo.GetMimeType(fileData);
        }
    }


    /////////////////////////////////////////////// 
    public class DbColumn : Column
    ///////////////////////////////////////////////
    {
        public enum LookupSearchModeValues
        {
            SearchValue,
            SearchText
        }

        internal int MaxTextLength = 10;
        internal bool AddedByUser = true;

        private GridEditControl.AuditModes _Audit = GridEditControl.AuditModes.None;
        [
        Category("Database"),
        Description("Stores and displays log of updates to the record"),
        DefaultValue(GridEditControl.AuditModes.None)
        ]
        public GridEditControl.AuditModes Audit
        {
            get { return _Audit; }
            set { _Audit = value; }
        }

        private bool _AutoIncrement = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Indicates that the underlying primary key column is auto-incrementing and thus read-only.")
        ]
        public bool AutoIncrement
        {
            get { return _AutoIncrement; }
            set { _AutoIncrement = value; }
        }

        private bool _IsBoolean = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Can be used with databases that do not support a boolean data type. Setting this to true will force the value to be displayed and edited as a boolean.")
        ]
        public bool IsBoolean
        {
            get { return _IsBoolean; }
            set { _IsBoolean = value; }
        }

        private string _BaseSchemaName = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Indicates the underlying table schema name.")
        ]
        public string BaseSchemaName
        {
            get { return _BaseSchemaName; }
            set { _BaseSchemaName = value; }
        }

        private string _BaseTableName = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Indicates the underlying table name when using a view as the data source.")
        ]
        public string BaseTableName
        {
            get { return _BaseTableName; }
            set { _BaseTableName = value; }
        }

        private bool _BulkInsert = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Enables multi-record insertion via the insert button using the Lookup property assigned against the same column")
        ]
        public bool BulkInsert
        {
            get { return _BulkInsert; }
            set { _BulkInsert = value; }
        }

        private string _ColumnExpression = "";
        [
        CategoryAttribute("Database"),
        Description("The SQL expression that represents the column e.g. column name")
        ]
        public string ColumnExpression
        {
            get { return _ColumnExpression; }
            set { _ColumnExpression = value; }
        }

        public string ColumnExpressionKey = "";
        public int ColumnSize = 0;


        private string _Culture = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Overrides the default culture when formatting a column value.")
        ]
        public string Culture
        {
            get { return _Culture; }
            set { _Culture = value; }
        }

        public string DataType = "";
        public string DbDataType = "";

        private Dictionary<string, object> _EditControlProperties = new Dictionary<string, object>();

        public Dictionary<string, object> EditControlProperties
        {
            get { return _EditControlProperties; }
            set { _EditControlProperties = value; }
        }

        private bool _Display = true;
        [
        CategoryAttribute("Appearance"),
        DefaultValue(true),
        Description("Specifies whether the column is displayed on the grid.")
        ]
        public bool Display
        {
            get { return _Display; }
            set { _Display = value; }
        }

        private EditField.ControlType _EditControlType = EditField.ControlType.Auto;
        [
        CategoryAttribute("Edit"),
        DefaultValue(EditField.ControlType.Auto),
        Description("Use this to manually choose the ControlType that you want to use to edit the database column.")
        ]
        public EditField.ControlType EditControlType
        {
            get { return _EditControlType; }
            set { _EditControlType = value; }
        }

        private bool _EditDisplay = true;
        [
        CategoryAttribute("Edit"),
        DefaultValue(true),
        Description("Determines if the column will be displayed on the edit form.")
        ]
        public bool EditDisplay
        {
            get { return _EditDisplay; }
            set { _EditDisplay = value; }
        }

        private GridEditControl.HashTypes _Encryption = GridEditControl.HashTypes.None;
        [
        CategoryAttribute("Edit"),
        DefaultValue(GridEditControl.HashTypes.None),
        Description("Specifies the algorithm used to encrypt the value before being added to the database.")
        ]
        public GridEditControl.HashTypes Encryption
        {
            get { return _Encryption; }
            set { _Encryption = value; }
        }

        private bool _UpdateReadOnly = false;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(false),
        Description("This will set a field to read only while updating (but not inserting).")
        ]
        public bool UpdateReadOnly
        {
            get { return _UpdateReadOnly; }
            set { _UpdateReadOnly = value; }
        }

        private bool _ForeignKey = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Used to identify a foreign key field in a child control. This column is matched with the PrimaryKey column in the parent control.")
        ]
        public bool ForeignKey
        {
            get { return _ForeignKey; }
            set { _ForeignKey = value; }
        }

        private string _Format = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Applies a .NET formatting string to the column value.")
        ]
        public string Format
        {
            get { return _Format; }
            set { _Format = value; }
        }

        private string _SeachFormat = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Applies a .NET formatting string to the column value.")
        ]
        public string SearchFormat
        {
            get { return _SeachFormat; }
            set { _SeachFormat = value; }
        }

        private string _InitialValue = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Supplies an initial default value for a column when adding a new record")
        ]
        public string InitialValue
        {
            get { return _InitialValue; }
            set { _InitialValue = value; }
        }

        private bool _InsertReadOnly = false;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(false),
        Description("This will set a field to read only while inserting (but not editing).")
        ]
        public bool InsertReadOnly
        {
            get { return _InsertReadOnly; }
            set { _InsertReadOnly = value; }
        }

        private string _Lookup = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Associates a coded value with a defined list of descriptive values and converts the coded value to the descriptive value.")
        ]
        public string Lookup
        {
            get { return _Lookup; }
            set { _Lookup = value; }
        }

        public string LookupDataType = "";

        private LookupSearchModeValues _LookupSearchMode = LookupSearchModeValues.SearchValue;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(LookupSearchModeValues.SearchValue),
        Description("Specifies how searches should be performed against a lookup column.")
        ]
        public LookupSearchModeValues LookupSearchMode
        {
            get { return _LookupSearchMode; }
            set { _LookupSearchMode = value; }
        }

        public string LookupTable = "";
        public string LookupTextField = "";
        public string LookupTextExpression = "";
        public string LookupValueField = "";

        private int _MaxThumbnailHeight = 30;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(30),
        Description("If the field contains binary image data a thumbnail will be displayed.  All images will be scaled so that they are no taller than this value (px).")
        ]
        public int MaxThumbnailHeight
        {
            get { return _MaxThumbnailHeight; }
            set { _MaxThumbnailHeight = value; }
        }

        private string _PlaceHolder = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Defines a watermark/placeholder value for an edit input column")
        ]
        public string PlaceHolder
        {
            get { return _PlaceHolder; }
            set { _PlaceHolder = value; }
        }

        private bool _PrimaryKey = false;
        [
        CategoryAttribute("Database"),
        DefaultValue(false),
        Description("Identifies a column as a primary key.")
        ]
        public bool PrimaryKey
        {
            get { return _PrimaryKey; }
            set { _PrimaryKey = value; }
        }

        private bool _ReadOnly = false;
        [
        CategoryAttribute("Functionality"),
        DefaultValue(false),
        Description("Setting this modifies the UpdateReadOnly and InsertReadOnly properties.")
        ]
        public bool ReadOnly
        {
            get { return _ReadOnly; }
            set { _ReadOnly = value; }
        }

        private bool _Required = false;
        [
        CategoryAttribute("Edit"),
        DefaultValue(false),
        Description("Marks the field as being required to the user, ensuring a value is entered.")
        ]
        public bool Required
        {
            get { return _Required; }
            set { _Required = value; }
        }

        private bool _SimpleSearch = true;
        [
        Category("Search"),
        Description("Indicates that the column will searched against from the simple search dialog."),
        DefaultValue(true)
        ]
        public bool SimpleSearch
        {
            get { return _SimpleSearch; }
            set { _SimpleSearch = value; }
        }

        private bool _Search = true;
        [
        Category("Search"),
        Description("Indicates that the column will appear in the standard search dialog."),
        DefaultValue(true)
        ]
        public bool Search
        {
            get { return _Search; }
            set { _Search = value; }
        }

        private string _SearchLookup = "";
        [
        CategoryAttribute("Search"),
        DefaultValue(""),
        Description("Lookup for search dialog (when different from the Lookup property).")
        ]
        public string SearchLookup
        {
            get { return _SearchLookup; }
            set { _SearchLookup = value; }
        }

        private int _SearchColumnOrder = 0;
        [
        CategoryAttribute("Search"),
        DefaultValue(0),
        Description("Overrides the default order in which columns are presented in the search dialog")
        ]
        public int SearchColumnOrder
        {
            get { return _SearchColumnOrder; }
            set { _SearchColumnOrder = value; }
        }

        private string _SequenceName = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Indicates the name of the Oracle/Firebird sequence used to generate the primary key.")
        ]
        public string SequenceName
        {
            get { return _SequenceName; }
            set { _SequenceName = value; }
        }

        private bool _SpellCheck = false;
        [
        Category("Edit"),
        Description("Indicates if the column will be spell checked."),
        DefaultValue(false)
        ]
        public bool SpellCheck
        {
            get { return _SpellCheck; }
            set { _SpellCheck = value; }
        }

        private string _Style = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Provide a CSS style that will be applied to the column or edit field.")
        ]
        public string Style
        {
            get { return _Style; }
            set { _Style = value; }
        }

        private string _ToolTip = "";
        [
        CategoryAttribute("Edit"),
        DefaultValue(""),
        Description("Tooltip for edit control that will appear when mouse is held over the control")
        ]
        public string ToolTip
        {
            get { return _ToolTip; }
            set { _ToolTip = value; }
        }

        private bool _Unique = false;
        [
        CategoryAttribute("Edit"),
        DefaultValue(false),
        Description("If set to true then the value must be unique in the table (also makes the field required).")
        ]
        public bool Unique
        {
            get { return _Unique; }
            set { _Unique = value; }
        }

        private string _UploadDataColumn = "None";
        [
        CategoryAttribute("Data Upload"),
        DefaultValue("None"),
        Description("The name of the corresponding column in the data upload table.")
        ]
        public string UploadDataColumn
        {
            get { return _UploadDataColumn; }
            set { _UploadDataColumn = value; }
        }

        private string _UploadExtFilter = String.Empty;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(""),
        Description("A comma separated list of allowed file upload extensions.")
        ]
        public string UploadExtFilter
        {
            get { return _UploadExtFilter; }
            set { _UploadExtFilter = value; }
        }

        private string _UploadFileNameColumn = String.Empty;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(""),
        Description("The name of the column that is used to store the file name (blob only)")
        ]
        public string UploadFileNameColumn
        {
            get { return _UploadFileNameColumn; }
            set { _UploadFileNameColumn = value; }
        }

        private int _UploadMaxFileSize = 0;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(0),
        Description("The max size in Kb for file uploads.")
        ]
        public int UploadMaxFileSize
        {
            get { return _UploadMaxFileSize; }
            set { _UploadMaxFileSize = value; }
        }

        private bool _UploadOverwrite = false;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(false),
        Description("Sets if the user is allowed to overwrite files. The user will still be asked to confirm an overwrite.")
        ]
        public bool UploadOverwrite
        {
            get { return _UploadOverwrite; }
            set { _UploadOverwrite = value; }
        }

        private bool _UploadRename = false;
        [
        CategoryAttribute("File Upload"),
        DefaultValue(false),
        Description("Sets if the user is allowed to rename files.")
        ]
        public bool UploadRename
        {
            get { return _UploadRename; }
            set { _UploadRename = value; }
        }

        private string _UploadRootFolder = "";
        [
        CategoryAttribute("File Upload"),
        DefaultValue(""),
        Description("Specify the root folder for uploaded files. Setting this value will allow file system uploads.")
        ]
        public string UploadRootFolder
        {
            get { return _UploadRootFolder; }
            set { _UploadRootFolder = value; }
        }

        private string _UploadSubFolder = "";
        [
        CategoryAttribute("File Upload"),
        DefaultValue(""),
        Description("Specify the sub folder for uploaded files.")
        ]
        public string UploadSubFolder
        {
            get { return _UploadSubFolder; }
            set { _UploadSubFolder = value; }
        }


        private string _XmlAttributeName = String.Empty;
        [
        CategoryAttribute("Format"),
        DefaultValue(""),
        Description("Attribute name for XML fragment element.")
        ]
        public string XmlAttributeName
        {
            get { return _XmlAttributeName; }
            set { _XmlAttributeName = value; }
        }

        private string _XmlElementName = String.Empty;
        [
        CategoryAttribute("Format"),
        DefaultValue(""),
        Description("Element name for XML fragment.")
        ]
        public string XmlElementName
        {
            get { return _XmlElementName; }
            set { _XmlElementName = value; }
        }

        public string TableName = "";
        public int ParentColumnIndex = -1;

        ///////////////////////////////////////////////
        public DbColumn()
        ///////////////////////////////////////////////
        {
        }
    }

    ///////////////////////////////////////////////
    public class DbColumnCollection : ColumnCollection
    ///////////////////////////////////////////////
    {
        public DbColumn this[int index]
        {
            get
            {
                return (DbColumn)this.List[index];
            }
            set
            {
                DbColumn column = (DbColumn)value;
                this.List[index] = column;
            }
        }

        public DbColumn this[string ColumnName]
        {
            get
            {
                foreach (DbColumn C in this)
                {
                    if (C.ColumnExpression.ToLower() == ColumnName.ToLower())
                        return (DbColumn)C;
                    else if (C.ColumnName.ToLower() == ColumnName.ToLower())
                        return (DbColumn)C;
                }
                return null;
            }
        }

        ///////////////////////////////////////////////
        public void Add(DbColumn column)
        ///////////////////////////////////////////////
        {
            column.ParentCollection = this;
            this.List.Add(column);
        }

        ///////////////////////////////////////////////
        public virtual DbColumn GetNewColumn()
        ///////////////////////////////////////////////
        {
            return null;
        }

        ///////////////////////////////////////////////
        public int IndexOf(DbColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }
}
