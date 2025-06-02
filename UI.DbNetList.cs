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
[assembly: TagPrefix("DbNetLink.DbNetList", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{

    [
    Designer(typeof(DbNetListControlDesigner)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxData("<{0}:DbNetList runat=\"server\" ConnectionString=\"\" Sql=\"\"></{0}:DbNetList>")
    ]

    ////////////////////////////////////////////////////////////////////////////
    public class DbNetList : DbNetSuite.UI.DatabaseControl
    ////////////////////////////////////////////////////////////////////////////
    {
        public enum ClientEvents
        {
            onBeforeItemsLoaded,
            onInitialized,
            onItemsLoaded,
            onLinkSelected,
            onRowSelected,
            onRowTransform
        };


        private bool _HeaderRow = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Controls the display of a header row")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool HeaderRow
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _HeaderRow; }
            set { _HeaderRow = value; }
        }

        /*
        private string _Height = "";
        [
        Category("Display"),
        Description("Height of list")
        ]
        ////////////////////////////////////////
        public String Height
        ////////////////////////////////////////
        {
            get { return _Height; }
            set { _Height = value; }
        }
        */
        private bool _RowSelection = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Controls the display of the selected row")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool RowSelection
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _RowSelection; }
            set { _RowSelection = value; }
        }

        private string _Sql = "";
        [
        Category("Display"),
        Description("Sql used to populate the combo")
        ]
        ////////////////////////////////////////
        public String Sql
        ////////////////////////////////////////
        {
            get { return _Sql; }
            set { _Sql = value; }
        }


        private string _TreeImageUrl = "";
        [
        Category("Display"),
        Description("Url for custome tree node image")
        ]
        ////////////////////////////////////////
        public String TreeImageUrl
        ////////////////////////////////////////
        {
            get { return _TreeImageUrl; }
            set { _TreeImageUrl = value; }
        }
        
        /*
        private string _Width = "";
        [
        Category("Display"),
        Description("Width of list")
        ]
        ////////////////////////////////////////
        public String Width
        ////////////////////////////////////////
        {
            get { return _Width; }
            set { _Width = value; }
        }
        */

        ArrayList _ListClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ListClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList ListClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_ListClientEvents == null)
                {
                    _ListClientEvents = new ArrayList();
                }
                return _ListClientEvents;
            }
        }

        private ListColumnCollection _ListColumns = new ListColumnCollection();
        [
        Category("Display"),
        Description("List Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ListColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]


        ////////////////////////////////////////////////////////////////////////////
        public ListColumnCollection ListColumns
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_ListColumns == null)
                {
                    _ListColumns = new ListColumnCollection();
                }
                return _ListColumns;
            }
        }

        private ArrayList _Parameters = new ArrayList();
        [
        Category("Data"),
        Description("Parameter values for the Sql property"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ParameterCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList Parameters
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_Parameters == null)
                    _Parameters = new ArrayList();
                return _Parameters;
            }
        }

        private DbNetList _NestedList;
        [
        Category("Behavior"),
        Description("Nested List"),
        DesignerSerializationVisibility(
        DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        Browsable(false)
        ]

        ////////////////////////////////////////
        public DbNetList NestedList
        ////////////////////////////////////////
        {
            get { return _NestedList; }
            set { _NestedList = value; }
        }


        ////////////////////////////////////////////////////////////////////////////
        public DbNetList()
            : base(HtmlTextWriterTag.Div)
        ////////////////////////////////////////////////////////////////////////////
        {
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
            this.RenderClientScript();
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void RenderClientScript()
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Script = GenerateClientScript(this, false);
            Script.Insert(0, "<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + "jQuery(document).ready( function() {");

            CheckForLastControl(Script);

            Script.Append("})" + Environment.NewLine);
            Script.Append("</script>" + Environment.NewLine);

            if (NestedConfigScript.Length > 0)
                RegisterStartupScript(this.ClientID + "NestedConfigScript", "<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + NestedConfigScript.ToString() + "</script>" + Environment.NewLine);

            RegisterStartupScript(this.ClientID + "Script", Script.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////
        internal override void RenderScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            MVC.Cache(MVC.CacheKeyNames.Script).Append(this.GenerateClientScript(this, false));
            base.RenderInvocationScript(ParentControl);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected StringBuilder GenerateClientScript(DbNetList List, bool Nested)
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder ScriptText = new StringBuilder();

            if (Nested)
            {
                ScriptText.Append("///////////////////////////////////////////" + Environment.NewLine);
                ScriptText.Append("function configure" + List.ID + "List(list)" + Environment.NewLine);
                ScriptText.Append("///////////////////////////////////////////" + Environment.NewLine);
                ScriptText.Append("{" + Environment.NewLine);

                ScriptText.Append("\twith (list)" + Environment.NewLine);
                ScriptText.Append("\t{" + Environment.NewLine);
            }
            else
            {
                ScriptText.Append("var o = new " + ComponentName + "(\"" + List.ClientID + "\");" + Environment.NewLine);
                ScriptText.Append("window.DbNetLink.components[\"" + this.ID + "\"] = o;" + Environment.NewLine);
                ScriptText.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            }

            PropertyInfo[] PI = typeof(DbNetList).GetProperties();
            DbNetList DefaultControl = new DbNetList();

            ArrayList ScriptLines = new ArrayList();

            foreach (PropertyInfo I in PI)
            {
                object PropertyValue;

                try
                {
                    if (I.GetValue(List, null).ToString() == I.GetValue(DefaultControl, null).ToString())
                        continue;

                    PropertyValue = I.GetValue(List, null);
                }
                catch (Exception)
                {
                    continue;
                }

                switch (I.Name)
                {
                    case "ConnectionString":
                    case "Sql":
                        PropertyValue = DbNetLink.Util.Encrypt(PropertyValue.ToString());
                        break;
                    default:
                        if (I.PropertyType.IsEnum)
                            PropertyValue = PropertyValue.ToString();

                        break;
                }
                ScriptLines.Add(WriteClientPropertyAssignment(I.Name, PropertyValue));
            }

            ScriptLines.AddRange(WriteParams(List.Parameters, "parameters"));
            ScriptLines.AddRange(WriteColumnProperties(List));
            ScriptLines.AddRange(WriteListClientEvents(List));

            if (List.NestedList != null)
                ScriptLines.Add("addNestedList(configure" + List.NestedList.ClientID + "List);");

            if (List.NestedList != null)
            {
                if (List.NestedList.ConnectionString == "")
                    List.NestedList.ConnectionString = List.ConnectionString;

                StringBuilder NestedScript = GenerateClientScript(List.NestedList, true);
                NestedConfigScript.Append(NestedScript);
                MVC.Cache(MVC.CacheKeyNames.FunctionsScript).Append(NestedScript);
            }

            foreach (string Line in ScriptLines)
                ScriptText.Append((Nested ? "\t\t" : "o.") + Line + Environment.NewLine);

            if (Nested)
            {
                ScriptText.Append("\t}" + Environment.NewLine);
                ScriptText.Append("}" + Environment.NewLine);
            }

            return ScriptText;
        }


        ////////////////////////////////////////////////////////////////////////////
        internal static ArrayList WriteListClientEvents(DbNetList List)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (List.ListClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in List.ListClientEvents)
            {
                if (!(O is ListClientEvent))
                    continue;

                ListClientEvent E = (ListClientEvent)O;

                Source.Add("bind(\"" + E.EventName.ToString() + "\",\"" + E.Handler + "\");");
            }

            return Source;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class ListClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetList.ClientEvents _EventName;
        private string _Handler = "";

        public DbNetList.ClientEvents EventName
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
    public class ListClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public ListClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ListClientEvent);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class ListColumn : DbNetLink.DbNetSuite.ListColumn
    ////////////////////////////////////////////////////////////////////////////
    {
        ///////////////////////////////////////////////
        public ListColumn(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            this.ColumnExpression = ColumnExpression;
        }

        ///////////////////////////////////////////////
        public ListColumn()
        ///////////////////////////////////////////////
        {
        }
    }


    [Serializable()]
    ///////////////////////////////////////////////
    public class ListColumnCollection : DbColumnCollection
    ///////////////////////////////////////////////
    {
        public ListColumn this[int index]
        {
            get
            {
                return (ListColumn)this.List[index];
            }
            set
            {
                ListColumn column = (ListColumn)value;
                this.List[index] = column;
            }
        }

        ///////////////////////////////////////////////
        public void Add(ListColumn column)
        ///////////////////////////////////////////////
        {
            base.Add(column);
        }

        ///////////////////////////////////////////////
        public ListColumn Add(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            ListColumn C = new ListColumn(ColumnExpression);
            base.Add(C);
            return C;
        }

        ///////////////////////////////////////////////
        public int IndexOf(ListColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class ListColumnCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public ListColumnCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ListColumn);
        }
    }

}