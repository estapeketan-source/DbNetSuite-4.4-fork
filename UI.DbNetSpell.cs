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
[assembly: TagPrefix("DbNetLink.DbNetSpell", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
    [
    Designer(typeof(DbNetSpellControlDesigner)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxData("<{0}:DbNetSpell runat=\"server\" ConnectionString=\"\" DictionaryTableName=\"\"></{0}:DbNetSpell>")
    ]

    ////////////////////////////////////////////////////////////////////////////
    public class DbNetSpell : DbNetSuite.UI.DatabaseControl
    ////////////////////////////////////////////////////////////////////////////
    {
        public enum ClientEvents
        {
            onInitialized,
            onElementSpellCheckCompleted,
            onSpellCheckCompleted
        };

        private bool _CreateButton = true;
        [
        CategoryAttribute("Configuration"),
        DefaultValue(false),
        Description("Create a spell check initiation button")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public bool CreateButton
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _CreateButton; }
            set { _CreateButton = value; }
        }


        private string _DictionaryTableName = "";
        [
        CategoryAttribute("Configuration"),
        DefaultValue(""),
        Description("The name of the database table containing the dictionary")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public string DictionaryTableName
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _DictionaryTableName; }
            set { _DictionaryTableName = value; }
        }


        private ArrayList _SpellCheckElements = new ArrayList();
        [
        Category("Configuration"),
        Description("Elements to be spell checked"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(SpellCheckElementCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList SpellCheckElements
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_SpellCheckElements == null)
                    _SpellCheckElements = new ArrayList();
                return _SpellCheckElements;
            }
        }


        ArrayList _SpellClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(SpellClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public ArrayList SpellClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_SpellClientEvents == null)
                {
                    _SpellClientEvents = new ArrayList();
                }
                return _SpellClientEvents;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        public DbNetSpell()
            : base(HtmlTextWriterTag.Div)
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void RenderContents(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
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

            Script.Append(this.GenerateClientScript());

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
        internal StringBuilder GenerateClientScript()
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Script = new StringBuilder();
            Script.Append("var o = new " + ComponentName + "(\"" + this.ClientID + "\");" + Environment.NewLine);

            ArrayList ScriptLines = new ArrayList();

            this.DbNetSpellClientScript(ScriptLines, this);

            foreach (string Line in ScriptLines)
                Script.Append("o." + Line + Environment.NewLine);

            Script.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            Script.Append("window.DbNetLink.components[\"" + this.ID + "\"] = o;" + Environment.NewLine);
 
            return Script;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal static ArrayList WriteSpellClientEvents(DbNetSpell Spell)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Spell.SpellClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in Spell.SpellClientEvents)
            {
                if (!(O is SpellClientEvent))
                    continue;

                SpellClientEvent E = (SpellClientEvent)O;

                Source.Add("bind(\"" + E.EventName.ToString() + "\",\"" + E.Handler + "\");");
            }

            return Source;
        }


        ////////////////////////////////////////////////////////////////////////////
        internal static ArrayList WriteSpellCheckElements(DbNetSpell Spell)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Spell.SpellCheckElements == null)
                return Source;

            List<string> Params = new List<string>();

            foreach (Object O in Spell.SpellCheckElements)
            {
                if (!(O is SpellCheckElement))
                    continue;

                SpellCheckElement P = (SpellCheckElement)O;

                Source.Add("registerElement(\"" + P.Selector + "\");");
           }

            return Source;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    public class SpellCheckElement
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _Selector = "";

        public string Selector
        {
            get { return _Selector; }
            set { _Selector = value; }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class SpellClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetSpell.ClientEvents _EventName;
        private string _Handler = "";

        public DbNetSpell.ClientEvents EventName
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
    public class SpellClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public SpellClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(SpellClientEvent);
        }
    }

   
    ////////////////////////////////////////////////////////////////////////////
    public class SpellCheckElementCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public SpellCheckElementCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(SpellCheckElement);
        }
    }
}