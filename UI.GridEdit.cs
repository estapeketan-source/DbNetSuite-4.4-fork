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

[assembly: TagPrefix("DbNetLink.DbNetSuite", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
    ////////////////////////////////////////////////////////////////////////////
    public abstract partial class GridEdit : DbNetLink.DbNetSuite.UI.DatabaseControl
    ////////////////////////////////////////////////////////////////////////////
    {
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

        private string _AuditDateFormat = "d";
        [
        Category("Database"),
        Description("Controls the formatting of the audit log date"),
        DefaultValue("d")
        ]
        public string AuditDateFormat
        {
            get { return _AuditDateFormat; }
            set { _AuditDateFormat = value; }
        }

        private string _AuditUser = "";
        [
        Category("Database"),
        Description("The id of the the current user for auditing purposes"),
        DefaultValue("")
        ]
        public string AuditUser
        {
            get { return _AuditUser; }
            set { _AuditUser = value; }
        }


        private bool _AdvancedSearch = true;
        [
        CategoryAttribute("Searching"),
        DefaultValue(false),
        Description("Enables advanced searching")
        ]
        ////////////////////////////////////////
        public bool AdvancedSearch
        ////////////////////////////////////////
        {
            get { return _AdvancedSearch; }
            set { _AdvancedSearch = value; }
        }

        private bool _AutoSave = false;
        [
        CategoryAttribute("Editing"),
        DefaultValue(false),
        Description("Attempts to save any changes automatically when navigating away from the record(s)")
        ]
        ////////////////////////////////////////
        public bool AutoSave
        ////////////////////////////////////////
        {
            get { return _AutoSave; }
            set { _AutoSave = value; }
        }

        private string _CustomProfileProperties = "";
        [
        CategoryAttribute("Profiles"),
        DefaultValue(""),
        Description("Comma separated list of additional custom window variables or input elements to be saved with a profile.")
        ]
        ////////////////////////////////////////
        public string CustomProfileProperties
        ////////////////////////////////////////
        {
            get { return _CustomProfileProperties; }
            set { _CustomProfileProperties = value; }
        }

        private DbNetSpell _DbNetSpell;
        [
        Category("Behavior"),
        Description("Spell checker"),
        DesignerSerializationVisibility(
        DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]

        ////////////////////////////////////////
        public DbNetSpell DbNetSpell
        ////////////////////////////////////////
        {
            get { return _DbNetSpell; }
            set { _DbNetSpell = value; }
        }

        private string _EmptyOptionText = String.Empty;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Show delete button")
        ]
        ////////////////////////////////////////
        public bool DeleteRow
        ////////////////////////////////////////
        {
            get { return _DeleteRow; }
            set { _DeleteRow = value; }
        }

        private bool _DeleteRow = true;
        [
        CategoryAttribute("Editing"),
        DefaultValue(""),
        Description("Sets the text for the empty option in a drop-downlist control for a required field.")
        ]
        public string EmptyOptionText
        {
            get { return _EmptyOptionText; }
            set { _EmptyOptionText = value; }
        }

        private ArrayList _SearchFilterParams = new ArrayList();
        [
        Category("Filtering"),
        Description("Parameter values for the SearchFilterSql property"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ParameterCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList SearchFilterParams
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_SearchFilterParams == null)
                    _SearchFilterParams = new ArrayList();
                return _SearchFilterParams;
            }
        }

        private string _SearchFilterSql = "";
        [
        CategoryAttribute("Filtering"),
        DefaultValue(""),
        Description("Applies a filter to the grid dataset")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string SearchFilterSql
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _SearchFilterSql; }
            set { _SearchFilterSql = value; }
        }

        private ArrayList _FixedFilterParams = new ArrayList();
        [
        Category("Filtering"),
        Description("Parameter values for the FixedFilterSql property"),
        Editor(typeof(ParameterCollectionEditor), typeof(UITypeEditor)),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList FixedFilterParams
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_FixedFilterParams == null)
                    _FixedFilterParams = new ArrayList();
                return _FixedFilterParams;
            }
        }

        private string _FixedFilterSql = "";
        [
        CategoryAttribute("Filtering"),
        DefaultValue(""),
        Description("Applies a fixed filter to the grid dataset")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string FixedFilterSql
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _FixedFilterSql; }
            set { _FixedFilterSql = value; }
        }

        private String _FromPart = String.Empty;
        [
        Category("Data"),
        Description("Name of table or view")
        ]
        ////////////////////////////////////////
        public String FromPart
        ////////////////////////////////////////
        {
            get { return _FromPart; }
            set { _FromPart = value; }
        }

        private bool _InsertRow = true;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Display insert button")
        ]
        ////////////////////////////////////////
        public bool InsertRow
        ////////////////////////////////////////
        {
            get { return _InsertRow; }
            set { _InsertRow = value; }
        }


        private bool _IgnorePrimaryKeys = false;
        [
        Category("Database"),
        DefaultValue(true),
        Description("Control automatic selection of primary key information.")
        ]
        ////////////////////////////////////////
        public bool IgnorePrimaryKeys
        ////////////////////////////////////////
        {
            get { return _IgnorePrimaryKeys; }
            set { _IgnorePrimaryKeys = value; }
        }

        private GridEditControl.MultiValueLookupSelectStyles _MultiValueLookupSelectStyle = GridEditControl.MultiValueLookupSelectStyles.Select;
        [
        Category("Search"),
        DefaultValue(GridEditControl.MultiValueLookupSelectStyles.Select),
        Description("Controls the style of selection for lookups where multiple values can be selected.")
        ]
        ////////////////////////////////////////
        public GridEditControl.MultiValueLookupSelectStyles MultiValueLookupSelectStyle
        ////////////////////////////////////////
        {
            get { return _MultiValueLookupSelectStyle; }
            set { _MultiValueLookupSelectStyle = value; }
        }

        private bool _Navigation = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Adds page navigation buttons to the toolbar")
        ]
        ////////////////////////////////////////
        public bool Navigation
        ////////////////////////////////////////
        {
            get { return _Navigation; }
            set { _Navigation = value; }
        }

        private string _OrderBy = "";
        [
        CategoryAttribute("Sorting"),
        DefaultValue(""),
        Description("Fixes the top level of sorting in the grid")
        ]
        ////////////////////////////////////////
        public string OrderBy
        ////////////////////////////////////////
        {
            get { return _OrderBy; }
            set { _OrderBy = value; }
        }


        private bool _PageInfo = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Displays page information in the toolbar")
        ]
        ////////////////////////////////////////
        public bool PageInfo
        ////////////////////////////////////////
        {
            get { return _PageInfo; }
            set { _PageInfo = value; }
        }

        private string _ProfileKey = "";
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(""),
        Description("The key with which user profiles are associated. Defaults to page name/component id.")
        ]
        ////////////////////////////////////////
        public string ProfileKey
        ////////////////////////////////////////
        {
            get { return _ProfileKey; }
            set { _ProfileKey = value; }
        }

        private string _ProfileUser = "";
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(""),
        Description("The user id with which user profiles are associated. Set automatically on authenticated pages.")
        ]
        ////////////////////////////////////////
        public string ProfileUser
        ////////////////////////////////////////
        {
            get { return _ProfileUser; }
            set { _ProfileUser = value; }
        }


        private bool _QuickSearch = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds a text search box to the toolbar")
        ]
        ////////////////////////////////////////
        public bool QuickSearch
        ////////////////////////////////////////
        {
            get { return _QuickSearch; }
            set { _QuickSearch = value; }
        }

        private bool _RowInfo = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Displays number of rows selected in the toolbar")
        ]
        ////////////////////////////////////////
        public bool RowInfo
        ////////////////////////////////////////
        {
            get { return _RowInfo; }
            set { _RowInfo = value; }
        }

        private bool _Search = true;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Display search button")
        ]
        ////////////////////////////////////////
        public bool Search
        ////////////////////////////////////////
        {
            get { return _Search; }
            set { _Search = value; }
        }

        private string _SearchDialogHeight = "";
        [
        Category("Search"),
        DefaultValue(""),
        Description("Set the height of the search dialog")
        ]
        ////////////////////////////////////////
        public string SearchDialogHeight
        ////////////////////////////////////////
        {
            get { return _SearchDialogHeight; }
            set { _SearchDialogHeight = value; }
        }

        private GridEditControl.SearchDialogModes _SearchDialogMode = GridEditControl.SearchDialogModes.Standard;
        [
        Category("Search"),
        DefaultValue(GridEditControl.SearchDialogModes.Standard),
        Description("")
        ]
        public GridEditControl.SearchDialogModes SearchDialogMode
        {
            get { return _SearchDialogMode; }
            set { _SearchDialogMode = value; }
        }

        private int _SearchLayoutColumns = 1;
        [
        CategoryAttribute("Search"),
        DefaultValue(1),
        Description("Specifies the number of columns over which the search criteria are distributed")
        ]
        public int SearchLayoutColumns
        {
            get { return _SearchLayoutColumns; }
            set { _SearchLayoutColumns = value; }
        }

        private bool _SearchValuesOnly = false;
        [
        CategoryAttribute("Search"),
        DefaultValue(false),
        Description("Specifies if only the search values are selectable in the search dialog/panel")
        ]
        public bool SearchValuesOnly
        {
            get { return _SearchValuesOnly; }
            set { _SearchValuesOnly = value; }
        }

        private string _SearchPanelId = "";
        [
        CategoryAttribute("Searching"),
        DefaultValue(""),
        Description("Specifies the ID or Class Name of an HTML element to contain the search criteria instead of the default search dialog")
        ]
        ////////////////////////////////////////
        public string SearchPanelId
        ////////////////////////////////////////
        {
            get { return _SearchPanelId; }
            set { _SearchPanelId = value; }
        }

        private bool _SimpleSearch = true;
        [
        CategoryAttribute("Searching"),
        DefaultValue(false),
        Description("Enables simple searching")
        ]
        ////////////////////////////////////////
        public bool SimpleSearch
        ////////////////////////////////////////
        {
            get { return _SimpleSearch; }
            set { _SimpleSearch = value; }
        }

        private bool _Sort = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the column sort dialog button to the toolbar.")
        ]
        ////////////////////////////////////////
        public bool Sort
        ////////////////////////////////////////
        {
            get { return _Sort; }
            set { _Sort = value; }
        }

        private bool _SpellCheck = false;
        [
        Category("Toolbar"),
        DefaultValue(true),
        Description("Enable spell checking")
        ]
        ////////////////////////////////////////
        public bool SpellCheck
        ////////////////////////////////////////
        {
            get { return _SpellCheck; }
            set { _SpellCheck = value; }
        }

        private Shared.ToolButtonStyles _ToolbarButtonStyle = Shared.ToolButtonStyles.Image;
        [
        Category("Toolbar"),
        Description("Sets the style of the toolbar button"),
        DefaultValue(Shared.ToolButtonStyles.Image)
        ]
        ////////////////////////////////////////
        public Shared.ToolButtonStyles ToolbarButtonStyle
        ////////////////////////////////////////
        {
            get { return _ToolbarButtonStyle; }
            set { _ToolbarButtonStyle = value; }
        }

        private bool _Upload = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the data upload button to the toolbar.")
        ]
        ////////////////////////////////////////
        public bool Upload
        ////////////////////////////////////////
        {
            get { return _Upload; }
            set { _Upload = value; }
        }

        private string _UploadDataFolder = String.Empty;
        [
        CategoryAttribute("Upload Data"),
        DefaultValue(false),
        Description("Specifies the folder into which the upload data is saved.")
        ]
        ////////////////////////////////////////
        public string UploadDataFolder
        ////////////////////////////////////////
        {
            get { return _UploadDataFolder; }
            set { _UploadDataFolder = value; }
        }

        private string _UploadDataTable = String.Empty;
        [
        CategoryAttribute("Upload Data"),
        DefaultValue(false),
        Description("Specifies the table/sheet name in the upload data source.")
        ]
        ////////////////////////////////////////
        public string UploadDataTable
        ////////////////////////////////////////
        {
            get { return _UploadDataTable; }
            set { _UploadDataTable = value; }
        }

        private string _UploadExtFilter = String.Empty;
        [
        CategoryAttribute("Upload Data"),
        DefaultValue(false),
        Description("Specifies the allowed file extension(s) for uploaded data files.")
        ]
        ////////////////////////////////////////
        public string UploadExtFilter
        ////////////////////////////////////////
        {
            get { return _UploadExtFilter; }
            set { _UploadExtFilter = value; }
        }

        private bool _UserProfile = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the user profile dialog button to the toolbar.")
        ]
        ////////////////////////////////////////
        public bool UserProfile
        ////////////////////////////////////////
        {
            get { return _UserProfile; }
            set { _UserProfile = value; }
        }

        private bool _UserProfileSelect = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds the user profile selection list to the toolbar.")
        ]
        ////////////////////////////////////////
        public bool UserProfileSelect
        ////////////////////////////////////////
        {
            get { return _UserProfileSelect; }
            set { _UserProfileSelect = value; }
        }

        ////////////////////////////////////////////////////////////////////////////
        public GridEdit() : base(HtmlTextWriterTag.Div)
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnLoad(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnLoad(e);

            foreach (Object O in this.LinkedControls)
            {
                if (!(O is LinkedControl))
                    continue;

                LinkedControl LC = (LinkedControl)O;

                DbNetLink.DbNetSuite.UI.GridEdit C;

                if (LC.LinkedControlRef != null)
                    C = (DbNetLink.DbNetSuite.UI.GridEdit)LC.LinkedControlRef;
                else
                    C = (DbNetLink.DbNetSuite.UI.GridEdit)DbNetFindControl(LC.LinkedControlID);

                if (C == null)
                    continue;

                if (C.FromPart.ToLower() == this.FromPart.ToLower())
                    LC.OneToOne = true;

                if (LC.OneToOne)
                {
			        C.DeleteRow = false;
                    if (this is DbNetGrid)
    			        (this as DbNetGrid).UpdateRow = false;
			        this.InsertRow = false;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void RenderContents(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            Writer.Write("");
            base.RenderContents(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void AddAttributesToRender(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.AddAttributesToRender(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnPreRender(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnPreRender(e);
            // Call the base implementation
            base.CreateChildControls();
        }

    }
}