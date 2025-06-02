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
[assembly: TagPrefix("DbNetLink.DbNetCombo", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
    [
    Designer(typeof(DbNetComboControlDesigner)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxData("<{0}:DbNetCombo runat=\"server\" ConnectionString=\"\" Sql=\"\"></{0}:DbNetCombo>")
    ]

    ////////////////////////////////////////////////////////////////////////////
    public class DbNetCombo : DbNetSuite.UI.DatabaseControl
    ////////////////////////////////////////////////////////////////////////////
    {
        public enum ClientEvents
        {
            onInitialized,
            onBeforeItemsLoaded,
            onItemsLoaded,
            onChange,
            onItemsCleared
        };

               private bool _AddEmptyOption = false;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Adds an empty option to the top of the items")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool AddEmptyOption
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _AddEmptyOption; }
            set { _AddEmptyOption = value; }
        }


        private string _EmptyOptionText = "";
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("Text for the empty option")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string EmptyOptionText
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _EmptyOptionText; }
            set { _EmptyOptionText = value; }
        }

        private string _Sql = "";
        [
        Category("Data"),
        Description("Sql used to populate the combo")
        ]
        ////////////////////////////////////////
        public String Sql
        ////////////////////////////////////////
        {
            get { return _Sql; }
            set { _Sql = value; }
        }


        ArrayList _ComboClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(ComboClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList ComboClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_ComboClientEvents == null)
                {
                    _ComboClientEvents = new ArrayList();
                }
                return _ComboClientEvents;
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

        ////////////////////////////////////////////////////////////////////////////
        public DbNetCombo()
            : base(HtmlTextWriterTag.Select)
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

            StringBuilder Script = new StringBuilder();
            Script.Append("<script language=\"JavaScript\"" + NonceAttribute + ">" + Environment.NewLine + "jQuery(document).ready( function() {");
            Script.Append(this.GenerateClientScript(false));

            CheckForLastControl(Script);

            Script.Append("})" + Environment.NewLine);

            Script.Append("</script>" + Environment.NewLine);

            RegisterStartupScript(this.ClientID + "Script", Script.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////
        internal override void RenderScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            MVC.Cache(MVC.CacheKeyNames.Script).Append(GenerateClientScript(true));
            base.RenderInvocationScript(ParentControl);
        }


        ////////////////////////////////////////////////////////////////////////////
        private StringBuilder GenerateClientScript(bool MVCMode)
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Script = new StringBuilder();

            Script.Append("var o = new " + ComponentName + "(\"" + this.ClientID + "\");" + Environment.NewLine);

            PropertyInfo[] PI = typeof(DbNetCombo).GetProperties();
            DbNetCombo DefaultControl = new DbNetCombo();

            ArrayList ScriptLines = new ArrayList();

            foreach (PropertyInfo I in PI)
            {
                object PropertyValue;

                try
                {
                    if (I.GetValue(this, null).ToString() == I.GetValue(DefaultControl, null).ToString())
                        continue;

                    PropertyValue = I.GetValue(this, null);
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

            ScriptLines.AddRange(WriteParams(this.Parameters, "parameters"));
            ScriptLines.AddRange(WriteComboClientEvents(this));

            if (!MVCMode)
                DeferredScript.Append(WriteLinkedControls(this));

            foreach (string Line in ScriptLines)
                Script.Append("o." + Line + Environment.NewLine);

            Script.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            Script.Append("window.DbNetLink.components[\"" + this.ID + "\"] = o;" + Environment.NewLine);
            return Script;
        }


        ////////////////////////////////////////////////////////////////////////////
        internal static ArrayList WriteComboClientEvents(DbNetCombo Combo)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Combo.ComboClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in Combo.ComboClientEvents)
            {
                if (!(O is ComboClientEvent))
                    continue;

                ComboClientEvent E = (ComboClientEvent)O;

                Source.Add("bind(\"" + E.EventName.ToString() + "\",\"" + E.Handler + "\");");
            }

            return Source;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class ComboClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetCombo.ClientEvents _EventName;
        private string _Handler = "";

        public DbNetCombo.ClientEvents EventName
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
    public class ComboClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public ComboClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ComboClientEvent);
        }
    }

}