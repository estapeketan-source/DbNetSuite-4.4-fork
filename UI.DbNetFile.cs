using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Caching;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Security;
using System.Security.Permissions;
using System.Web.UI.Design; 
using System.Data; 
using System.Drawing; 
using System.Drawing.Imaging; 
using System.Data.SqlClient; 
using System.Data.OleDb; 
using System.Globalization; 
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Drawing.Design;
[assembly: TagPrefix ("DbNetLink.DbNetFile" , "DNL") ]

public enum IncludeExclude {Include, Exclude};
public enum FileDisplayMode {Standard, FolderTree};
public enum ColumnEnum {Name,Size,Type,DateCreated,DateLastModified,DateLastAccessed};
public enum WindowType {Window,Regular,Download,Dialog,None,Edit,Modal};

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
	[
    ParseChildren(true),
    PersistChildren(false),
    Designer(typeof(DbNetFileControlDesigner)),
    ToolboxData("<{0}:DbNetFile runat=\"server\"></{0}:DbNetFile>"),
    ]

	////////////////////////////////////////////////////////////////////////////
	public class DbNetFile : DbNetSuite.UI.Component
	////////////////////////////////////////////////////////////////////////////
	{
        public enum ClientEvents
        {
            onBeforeDocumentDisplayed,
            onBeforeFileDeleted,
            onBeforeFileSelected,
            onBeforeInitialized,
            onCellTransform,
            onFileUploaded,
            onFolderSelected,
            onInitialized,
            onPageLoaded,
            onRowSelected
        };

        private bool _AllowFolderDeletion = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Allow deletion of non-empty folders")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool AllowFolderDeletion
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _AllowFolderDeletion; }
            set { _AllowFolderDeletion = value; }
        }

        private bool _AutoSelectFolder = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Automatically selects the folder in the first row")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool AutoSelectFolder
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _AutoSelectFolder; }
            set { _AutoSelectFolder = value; }
        }

        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.SearchModes.FileSystem),
        Description("Specifies the mode used to browse the file system. ")
        ]
        public DbNetLink.DbNetSuite.DbNetFile.SearchModes _BrowseMode = DbNetLink.DbNetSuite.DbNetFile.SearchModes.FileSystem;

        private bool _CreateFolder = false;
        [
        CategoryAttribute("Display"),
        DefaultValue(false),
        Description("Creates the RootFolder if it does not exist")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool CreateFolder
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _CreateFolder; }
            set { _CreateFolder = value; }
        }

        private ArrayList _CustomMimeTypes = new ArrayList();
        [
        Category("Display"),
        Description("File Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(CustomMimeTypeCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList CustomMimeTypes
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_CustomMimeTypes == null)
                {
                    _CustomMimeTypes = new ArrayList();
                }
                return _CustomMimeTypes;
            }
        }

        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Adds delete button to the toolbar")
        ]

        private bool _DeleteRow = false;
        ////////////////////////////////////////////////////////////////////////////
        public bool DeleteRow
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _DeleteRow; }
            set { _DeleteRow = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.SelectionModes _SelectionMode = DbNetLink.DbNetSuite.DbNetFile.SelectionModes.FoldersAndFiles;
        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.SelectionModes.FoldersAndFiles),
        Description("File/folder selection mode")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.SelectionModes SelectionMode
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _SelectionMode; }
            set { _SelectionMode = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.DisplayStyles _DisplayStyle = DbNetLink.DbNetSuite.DbNetFile.DisplayStyles.Grid;
        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.DisplayStyles.Grid),
        Description("File/folder display style")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.DisplayStyles DisplayStyle
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _DisplayStyle; }
            set { _DisplayStyle = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.FileSelectionActions _FileSelectionAction = DbNetLink.DbNetSuite.DbNetFile.FileSelectionActions.Preview;
        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.FileSelectionActions.Preview),
        Description("Default file selection action")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.FileSelectionActions FileSelectionAction
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _FileSelectionAction; }
            set { _FileSelectionAction = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation _FolderPathLocation = DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation.Top;
        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation.Top),
        Description("Folder path location")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation FolderPathLocation
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _FolderPathLocation; }
            set { _FolderPathLocation = value; }
        }


        private string _FileFilter = String.Empty;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Filter applied to file selection")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string FileFilter
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _FileFilter; }
            set { _FileFilter = value; }
        }

        private string _FolderFilter = String.Empty;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Filter applied to folder selection")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string FolderFilter
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _FolderFilter; }
            set { _FolderFilter = value; }
        }

        private bool _HeaderRow = true;
        [
        CategoryAttribute("Display"),
        DefaultValue(true),
        Description("Display column headings")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool HeaderRow 
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _HeaderRow; }
            set { _HeaderRow = value; }
        }

        private Unit _Height = Unit.Empty;
        [
        CategoryAttribute("Display"),
        Description("Control height")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public Unit Height
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _Height; }
            set { _Height = value; }
        }

        private string _IndexingServiceCatalog = "system";
        [
        CategoryAttribute("Display"),
        Description("Indexing service catalog name")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string IndexingServiceCatalog
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _IndexingServiceCatalog; }
            set { _IndexingServiceCatalog = value; }
        }

        private int _PageSize = 20;
        [
        CategoryAttribute("Display"),
        DefaultValue(20),
        Description("Number files/folders per page")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int PageSize
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _PageSize; }
            set { _PageSize = value; }
        }

        private int _MaxSearchMatches = 100;
        [
        CategoryAttribute("Display"),
        DefaultValue(100),
        Description("Maximum number of matches found when searching the file system")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int MaxSearchMatches
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _MaxSearchMatches; }
            set { _MaxSearchMatches = value; }
        }

        private bool _NewFolder = false;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(false),
        Description("Add create folder button to the toolbar")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool NewFolder
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _NewFolder; }
            set { _NewFolder = value; }
        }

        private string _OrderBy = "Name";
        [
        CategoryAttribute("Configuration"),
        DefaultValue("Name"),
        Description("Column type by which to order the data")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string OrderBy
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _OrderBy; }
            set { _OrderBy = value; }
        }

        private Unit _PreviewDialogHeight = new Unit("300px");
        [
        CategoryAttribute("Configuration"),
        Description("Height of file preview dialog")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public Unit PreviewDialogHeight
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _PreviewDialogHeight; }
            set { _PreviewDialogHeight = value; }
        }

        private Unit _PreviewDialogWidth = new Unit("400px");
        [
        CategoryAttribute("Configuration"),
        Description("Width of file preview dialog")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public Unit PreviewDialogWidth
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _PreviewDialogWidth; }
            set { _PreviewDialogWidth = value; }
        }

        private string _RootFolder = "";
        [
        CategoryAttribute("Configuration"),
        Description("Root folder path")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public String RootFolder
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _RootFolder; }
            set { _RootFolder = value; }
        }

        private string _RootFolderAlias = "";
        [
        CategoryAttribute("Configuration"),
        Description("Root folder alias")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public String RootFolderAlias
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _RootFolderAlias; }
            set { _RootFolderAlias = value; }
        }

        private bool _Search = true;
        [
        CategoryAttribute("Toolbar"),
        DefaultValue(true),
        Description("Add search button to the toolbar")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool Search
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _Search; }
            set { _Search = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.SearchModes _SearchMode = DbNetLink.DbNetSuite.DbNetFile.SearchModes.FileSystem;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.SearchModes.FileSystem),
        Description("Mode used to search the file system")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.SearchModes SearchMode
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _SearchMode; }
            set { _SearchMode = value; }
        }

        private string _SelectableFileTypes = "";
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Mode used to search the file system")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string SelectableFileTypes
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _SelectableFileTypes; }
            set { _SelectableFileTypes = value; }
        }

        private int _ThumbnailPercent = 0;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail size as a percentage")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int ThumbnailPercent
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ThumbnailPercent; }
            set { _ThumbnailPercent = value; }
        }

        private int _ThumbnailHeight = 0;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail height in pixels")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int ThumbnailHeight
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ThumbnailHeight; }
            set { _ThumbnailHeight = value; }
        }

        private int _ThumbnailWidth = 0;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(0),
        Description("Thumbnail width in pixels")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int ThumbnailWidth
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ThumbnailWidth; }
            set { _ThumbnailWidth = value; }
        }

        private DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation _ToolbarLocation = DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation.Top;
        [
        CategoryAttribute("Display"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation.Top),
        Description("Toolbar location")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.ToolbarLocation ToolbarLocation
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ToolbarLocation; }
            set { _ToolbarLocation = value; }
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
        Description("Add upload button to the toolbar")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool Upload
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _Upload; }
            set { _Upload = value; }
        }

        private bool _UploadOverwrite = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Allow file to overwritten when uploading")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool UploadOverwrite
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _UploadOverwrite; }
            set { _UploadOverwrite = value; }
        }

        private string _UploadFileTypes = "";
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("File types that can be uploaded")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string UploadFileTypes
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _UploadFileTypes; }
            set { _UploadFileTypes = value; }
        }

        private int _UploadMaxFileSizeKb = 1024;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(1024),
        Description("Maximum size of upload file in KB")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public int UploadMaxFileSizeKb
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _UploadMaxFileSizeKb; }
            set { _UploadMaxFileSizeKb = value; }
        }

        private string _VisibleFileTypes = "";
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Displayed file types")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string VisibleFileTypes
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _VisibleFileTypes; }
            set { _VisibleFileTypes = value; }
        }


        private Unit _Width = Unit.Empty;
        [
        CategoryAttribute("Configuration"),
        Description("Width of control")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public Unit Width
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private string _WindowsSearchConnectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows';";
        [
        CategoryAttribute("Configuration"),
        DefaultValue("Provider=Search.CollatorDSO;Extended Properties='Application=Windows';"),
        Description("Width of control")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string WindowsSearchConnectionString
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _WindowsSearchConnectionString; }
            set { _WindowsSearchConnectionString = value; }
        }




        ArrayList _FileClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(FileClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList FileClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_FileClientEvents == null)
                {
                    _FileClientEvents = new ArrayList();
                }
                return _FileClientEvents;
            }
        }


        private FileColumnCollection _FileColumns = new FileColumnCollection();
        [
        Category("Display"),
        Description("File Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        Editor(typeof(FileColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]


        ////////////////////////////////////////////////////////////////////////////
        public FileColumnCollection FileColumns
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_FileColumns == null)
                {
                    _FileColumns = new FileColumnCollection();
                }
                return _FileColumns;
            }
        }





		////////////////////////////////////////////////////////////////////////////
		public DbNetFile() : base(HtmlTextWriterTag.Div) 
		////////////////////////////////////////////////////////////////////////////
		{
		}

		////////////////////////////////////////////////////////////////////////////
		protected override void RenderContents(HtmlTextWriter Writer) 
		////////////////////////////////////////////////////////////////////////////
		{
			Writer.Write( "" );
			base.RenderContents(Writer);
		}


		////////////////////////////////////////////////////////////////////////////
		protected override void AddAttributesToRender(HtmlTextWriter Writer) 
		////////////////////////////////////////////////////////////////////////////
		{
			base.AddAttributesToRender(Writer);
		}

		////////////////////////////////////////////////////////////////////////////
		protected override void OnPreRender( EventArgs e )
		////////////////////////////////////////////////////////////////////////////
		{
		    base.OnPreRender(e);

            StringBuilder Script = new StringBuilder();
            Script.Append("<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + "jQuery(document).ready( function() {");
  
            Script.Append(this.GenerateClientScript());
            DeferredScript.Append(WriteLinkedControls(this));

            CheckForLastControl(Script);

            Script.Append("})" + Environment.NewLine);

            Script.Append("</script>" + Environment.NewLine);

            RegisterStartupScript(this.ClientID + "Script", Script.ToString());
		}

        ////////////////////////////////////////////////////////////////////////////
        internal override void RenderScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            MVC.Cache(MVC.CacheKeyNames.Script).Append(this.GenerateClientScript());
            base.RenderInvocationScript(ParentControl);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected StringBuilder GenerateClientScript()
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList ScriptLines = new ArrayList();
            StringBuilder Script = new StringBuilder();

            PropertyInfo[] PI = typeof(DbNetFile).GetProperties();
            DbNetFile DefaultFile = new DbNetFile();

            foreach (PropertyInfo I in PI)
            {
                object PropertyValue;

                try
                {
                    if (I.GetValue(this, null).ToString() == I.GetValue(DefaultFile, null).ToString())
                        continue;

                    PropertyValue = I.GetValue(this, null);
                }
                catch (Exception)
                {
                    continue;
                }

                switch (I.Name)
                {
                    //Encrypted strings
                    case "RootFolder":
                        if (PropertyValue.ToString() != "")
                        {
                            PropertyValue = PropertyValue.ToString().Replace("\\", "\\\\");
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

            ScriptLines.AddRange(WriteColumnProperties(this));
            ScriptLines.AddRange(WriteFileClientEvents(this));
            ScriptLines.AddRange(WriteCustomMimeTypes());

            Script.Append("var o = new " + ComponentName + "(\"" + this.ClientID + "\");" + Environment.NewLine);

            foreach (string Line in ScriptLines)
                Script.Append("o." + Line + Environment.NewLine);

            Script.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            Script.Append("window.DbNetLink.components[\"" + this.ID + "\"] = o;" + Environment.NewLine);

            return Script;
        }


        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteFileClientEvents(DbNetFile File)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (File.FileClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in File.FileClientEvents)
            {
                if (!(O is FileClientEvent))
                    continue;

                FileClientEvent E = (FileClientEvent)O;

                Source.Add("bind(\"" + E.EventName.ToString() + "\",\"" + E.Handler + "\");");
            }

            return Source;
        }


	
		////////////////////////////////////////////////////////////////////////////
		protected String StringValueFromPropertyInfo(String PropertyName, Type T, object FromObject)
		////////////////////////////////////////////////////////////////////////////
		{
			String ReturnString = String.Empty;
			PropertyInfo PI = T.GetProperty(PropertyName);
			object Val = PI.GetValue(FromObject, null);
			ReturnString = Val.ToString();
			return (ReturnString);
		}
		
		////////////////////////////////////////////////////////////////////////////
		public string BuildList() 
		////////////////////////////////////////////////////////////////////////////
		{
			return "";
		}

		////////////////////////////////////////////////////////////////////////////
		public string MakeAlias( string S ) 
		////////////////////////////////////////////////////////////////////////////
		{
			if ( S != "/" )
			{
				S = Regex.Replace( S, @"\/$", "");
				return S.Split('/')[ S.Split('/').Length-1];
			}
			else
				return S;
		}

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteCustomMimeTypes()
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (this.CustomMimeTypes == null)
                return Source;

            List<string> Params = new List<string>();

            foreach (Object O in this.CustomMimeTypes)
            {
                if (!(O is MimeType))
                    continue;

                MimeType P = (MimeType)O;

                Params.Add(P.Extension + " : \"" + P.ContentType + "\"");
            }

            if (Params.Count > 0)
                Source.Add("customMimeTypes = {" + String.Join(",", Params.ToArray()) + "};");

            return Source;
        }

        ///////////////////////////////////////////////
        internal string GetRootFolderAlias()
        ///////////////////////////////////////////////
        {
            if (this.RootFolderAlias != "")
                return this.RootFolderAlias;

            string RootFolder = this.RootFolder;
            return RootFolder.Split('\\')[RootFolder.Split('\\').Length - 1];
        }


        ///////////////////////////////////////////////
        internal void AddDefaultColumns()
        ///////////////////////////////////////////////
        {
            if (FileColumns.Count == 0)
            {
                if (this.DisplayStyle == DbNetSuite.DbNetFile.DisplayStyles.Grid)
                    FileColumns.Add(new DbNetSuite.FileColumn(DbNetSuite.DbNetFile.ColumnTypes.Icon));
                FileColumns.Add(new DbNetSuite.FileColumn(DbNetSuite.DbNetFile.ColumnTypes.Name, "File Name"));
                (FileColumns[FileColumns.Count - 1] as DbNetSuite.FileColumn).Selectable = true;
            }
        }

	}

    ////////////////////////////////////////////////////////////////////////////
    public class MimeType
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _Extension = "";
        private string _ContentType = "";

        public string Extension
        {
            get { return _Extension; }
            set { _Extension = value; }
        }
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class FileClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetFile.ClientEvents _EventName;
        private string _Handler = "";

        public DbNetFile.ClientEvents EventName
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

    ////////////////////////////////////////////////////////////////////////////
    public class FileClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public FileClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FileClientEvent);
        }
    }


    [Serializable()]
    ///////////////////////////////////////////////
    public class FileColumnCollection : DbNetLink.DbNetSuite.FileColumnCollection
    ///////////////////////////////////////////////
    {
        public FileColumn this[int index]
        {
            get
            {
                return (FileColumn)this.List[index];
            }
            set
            {
                FileColumn column = (FileColumn)value;
                this.List[index] = column;
            }
        }

        ///////////////////////////////////////////////
        public void Add(FileColumn column)
        ///////////////////////////////////////////////
        {
            base.Add(column);
        }

        ///////////////////////////////////////////////
        public FileColumn Add(DbNetSuite.DbNetFile.ColumnTypes ColumnType)
        ///////////////////////////////////////////////
        {
            FileColumn C = new FileColumn(ColumnType);
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
    public class FileColumn : DbNetLink.DbNetSuite.FileColumn
    ////////////////////////////////////////////////////////////////////////////
    {
        ///////////////////////////////////////////////
        public FileColumn(DbNetSuite.DbNetFile.ColumnTypes ColumnType)
        ///////////////////////////////////////////////
        {
            this.ColumnType = ColumnType;
        }

        ///////////////////////////////////////////////
        public FileColumn(DbNetSuite.DbNetFile.ColumnTypes ColumnType, string Label)
        ///////////////////////////////////////////////
        {
            this.ColumnType = ColumnType;
            this.Label = Label;
        }

        ///////////////////////////////////////////////
        public FileColumn()
        ///////////////////////////////////////////////
        {
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class FileColumnCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public FileColumnCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FileColumn);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class CustomMimeTypeCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public CustomMimeTypeCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(MimeType);
        }
    }
}