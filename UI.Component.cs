using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;

public enum LookupSelectionType { Standard, Checkbox };
public enum PageRow { Page, Row };
public enum RightLeft { Right, Left };
public enum AndOr { And, Or };
public enum SearchDialogType { Simple, Standard, Advanced };
public enum OutputQuality { Standard, Enhanced };
public enum FilterColumnTypes { Simple, Composite };
public enum AscDesc { Asc, Desc };

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
    public enum Themes
    {
        Unassigned,
        Classic,
        Light,
        Dark,
        Bootstrap
    }

    ////////////////////////////////////////////////////////////////////////////
    public static class MVC
    ////////////////////////////////////////////////////////////////////////////
    {
        public enum CacheKeyNames
        {
            DbNetSuiteLibrariesRendered,
            Script,
            DeferredScript,
            FunctionsScript,
            Html
        }
        ////////////////////////////////////////////////////////////////////////////
        public static string Libraries()
        ////////////////////////////////////////////////////////////////////////////
        {
            return Libraries(Themes.Light);
        }

        ///////////////////////////////////////////////
        internal static string AssignHandler(string Handler)
        ///////////////////////////////////////////////
        {
            return HttpContext.Current.Request.ApplicationPath + ((HttpContext.Current.Request.ApplicationPath == "/") ? "" : "/") + Handler;
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string Libraries(Themes Theme, string NonceAttribute = "", string Nonce = "")
        ////////////////////////////////////////////////////////////////////////////
        {
            string ThemeString = (Theme == Themes.Unassigned) ? String.Empty : "?theme=" + Theme.ToString();
            StringBuilder Html = new StringBuilder();
            Html.Append("<script language=\"JavaScript\"" + NonceAttribute + " src=\"" + AssignHandler("DbNetSuite.js.ashx") + "\"></script>" + System.Environment.NewLine);
            Html.Append(Shared.WriteUserToken(Nonce) + System.Environment.NewLine);
            Html.Append("<link rel=\"stylesheet\" type=\"text/css\"" + NonceAttribute + " href=\"" + AssignHandler("DbNetSuite.css.ashx") + ThemeString + "\"/>" + System.Environment.NewLine);

            HttpContext.Current.Items[CacheKeyNames.DbNetSuiteLibrariesRendered] = true;
            return Html.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////
        public static StringBuilder Cache(CacheKeyNames CacheName)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (HttpContext.Current.Items[CacheName] == null)
                HttpContext.Current.Items[CacheName] = new StringBuilder();

            return HttpContext.Current.Items[CacheName] as StringBuilder;
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string Render(Component component)
        ////////////////////////////////////////////////////////////////////////////
        {
            return Script(component);
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string Script(Component component)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (HttpContext.Current.Items[CacheKeyNames.DbNetSuiteLibrariesRendered] == null)
                Cache(CacheKeyNames.Script).Append(Libraries(component.Theme, component.NonceAttribute, component.Nonce));

            Cache(CacheKeyNames.Script).Append("<script language=\"JavaScript\"" + component.NonceAttribute + ">" + Environment.NewLine + "jQuery(document).ready( function() {");

            ScriptLinkedControls(component);
            ScriptControl(component, null);

            Cache(CacheKeyNames.Script).Append(Cache(CacheKeyNames.DeferredScript));
            Cache(CacheKeyNames.Script).Append("})" + Environment.NewLine);

            if (Cache(CacheKeyNames.FunctionsScript).Length > 0)
                Cache(CacheKeyNames.Script).Append(Cache(CacheKeyNames.FunctionsScript));

            Cache(CacheKeyNames.Script).Append("</script>" + Environment.NewLine);

            Cache(CacheKeyNames.Script).Insert(0, Cache(CacheKeyNames.Html));


            string Script = Cache(CacheKeyNames.Script).ToString().ToString();

            HttpContext.Current.Items[CacheKeyNames.Script] = null;
            HttpContext.Current.Items[CacheKeyNames.DeferredScript] = null;
            HttpContext.Current.Items[CacheKeyNames.FunctionsScript] = null;
            HttpContext.Current.Items[CacheKeyNames.Html] = null;

            return Script;
        }

        ////////////////////////////////////////////////////////////////////////////
        private static void ScriptControl(Component Component, Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            Component.RenderScript(ParentControl);
            //DbNetLink.Util.EncryptionEnabled = false;
            /*
            if (Component is DbNetGrid)
                (Component as DbNetGrid).RenderScript(ParentControl);
            else if (Component is DbNetEdit)
                (Component as DbNetEdit).RenderScript(ParentControl);
            else if (Component is DbNetFile)
                (Component as DbNetFile).RenderScript(ParentControl);
           */
        }


        ////////////////////////////////////////////////////////////////////////////
        private static void ScriptLinkedControls(Component component)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (component.LinkedControls == null)
                return;

            foreach (object LC in component.LinkedControls)
            {
                DatabaseControl Control;
                bool OneToOne = false;
                if (LC is LinkedControl)
                {
                    Control = (LC as LinkedControl).LinkedControlRef as GridEdit;
                    OneToOne = (LC as LinkedControl).OneToOne;

                    if (Control != null)
                        if (OneToOne)
                            Control.RelationshipToParent = GridEdit.ParentRelationships.OneToOne;
                }
                else
                    Control = (DatabaseControl)LC;

                if (Control == null)
                    continue;

                ScriptLinkedControls(Control);
                ScriptControl(Control, component);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class Component : System.Web.UI.WebControls.WebControl, INamingContainer
    ////////////////////////////////////////////////////////////////////////////
    {
        internal enum ParentRelationships
        {
            Unspecified,
            OneToOne,
            ManyToOne
        }

        internal ParentRelationships RelationshipToParent = ParentRelationships.Unspecified;
        protected string ComponentName;

        protected string[] ScriptFiles = { };

        private Themes _Theme = Themes.Unassigned;
        private string _UserLanguage = String.Empty;
        private bool _NoLoad = false;
        private bool _DeferredLoad = false;
        private bool _ClientSideEncryption = true;
        private string _Nonce = String.Empty;

        internal StringBuilder DeferredScript = new StringBuilder();
        internal bool AllParsed = true;
        internal bool NoRender = false;
        internal bool Parsed = false;
        internal bool IsChildControl = false;
        internal ArrayList AllControls = new ArrayList();
        internal string _DesignTimeErrorMessage = "";
        internal StringBuilder NestedConfigScript = new StringBuilder();
        internal int _MessageTimeout = 3;


        [
        Category("Configuration"),
        Description("Defer running of client-side script until DOM fully loaded")
        ]
        ////////////////////////////////////////
        public bool DeferredLoad
        ////////////////////////////////////////
        {
            get { return _DeferredLoad; }
            set { _DeferredLoad = value; }
        }

        [
        Category("Configuration"),
        Description("Load content automatically on page load")
        ]
        ////////////////////////////////////////
        public bool NoLoad
        ////////////////////////////////////////
        {
            get { return _NoLoad; }
            set { _NoLoad = value; }
        }

        [
        Category("Configuration"),
        Description("Encrypt database information in client-side code")
        ]
        ////////////////////////////////////////
        public bool ClientSideEncryption
        ////////////////////////////////////////
        {
            get { return _ClientSideEncryption; }
            set { _ClientSideEncryption = value; }
        }

        [
        Category("Configuration"),
        Description("Style theme for components")
        ]
        ////////////////////////////////////////
        public Themes Theme
        ////////////////////////////////////////
        {
            get { return _Theme; }
            set { _Theme = value; }
        }

        [
        Category("Configuration"),
        Description("Sets the culture in place of the default browser language")
        ]
        ////////////////////////////////////////
        public string UserLanguage
        ////////////////////////////////////////
        {
            get { return _UserLanguage; }
            set { _UserLanguage = value; }
        }

        [
        Category("Configuration"),
        Description("Sets the number of seconds that an error message is displayed for")
        ]
        ////////////////////////////////////////
        public int MessageTimeout
        ////////////////////////////////////////
        {
            get { return _MessageTimeout; }
            set { _MessageTimeout = value; }
        }

        [
        Category("Configuration"),
        Description("Sets the Nonce script/style tag value for CSP support")
        ]
        ////////////////////////////////////////
        public string Nonce
        ////////////////////////////////////////
        {
            get { return _Nonce; }
            set { _Nonce = value; }
        }


        internal string NonceAttribute {
            get {
                if (String.IsNullOrEmpty(Nonce))
                {
                    return String.Empty;
                }
                else
                {
                    return $" nonce=\"{this.Nonce}\"";
                }
            }
        }

        private ArrayList _LinkedControls = new ArrayList();
        [
        Category("Display"),
        Description("Linked controls"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(LinkedControlCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList LinkedControls
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_LinkedControls == null)
                    _LinkedControls = new ArrayList();
                return _LinkedControls;
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        public Component(HtmlTextWriterTag Tag)
            : base(Tag)
        ////////////////////////////////////////////////////////////////////////////
        {
            ComponentName = this.GetType().Name;

            if (ComponentName == "DbNetFile")
                ClientSideEncryption = false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected string ComponentPath()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string P = "/" + ConfigValue("VirtualDir");
            if (P == "")
                P = HttpContext.Current.Request.ApplicationPath;

            P += "/" + ConfigValue("SubFolder") + "/";

            return P.Replace("//", "/");
        }

        ////////////////////////////////////////////////////////////////////////////
        public string ConfigValue(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            Key = ComponentName + Key;
            string V = ConfigurationManager.AppSettings[Key];
            if (V == null)
                V = "";

            return V;
        }


        ///////////////////////////////////////////////
        public Control DbNetFindControl(string id)
        ///////////////////////////////////////////////
        {
            return FindControlUp(id, this);
        }

        ///////////////////////////////////////////////
        public Control FindControlUp(string id, Control startControl)
        ///////////////////////////////////////////////
        {
            Control container = startControl;
            Control findControl = container.FindControl(id);
            while (findControl == null)
            {
                if (container.NamingContainer == null)
                    break;

                container = container.NamingContainer;
                findControl = container.FindControl(id);
            }

            if (findControl == null)
                return FindControlDown(container, id);
            else
                return findControl;
        }

        ///////////////////////////////////////////////
        public Control FindControlDown(Control root, string id)
        ///////////////////////////////////////////////
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls)
            {
                Control findControl = FindControlDown(control, id);
                if (findControl != null)
                    return findControl;
            }

            return null;
        }


        ////////////////////////////////////////////////////////////////////////////
        protected override void OnPreRender(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnPreRender(e);

            DbNetLink.Util.EncryptionEnabled = this.ClientSideEncryption;

            if (!IsClientScriptBlockRegistered("DbNetSuiteJS"))
            {
                string Url = "DbNetSuite.js.ashx";
                if (!String.IsNullOrEmpty(this.UserLanguage))
                    Url += "?userlanguage=" + this.UserLanguage;

                RegisterStartupScript("DbNetSuiteJS", "<script language=\"JavaScript\" src=\"" + Url + "\"" + NonceAttribute + "></script>" + System.Environment.NewLine);
            }
            if (!IsClientScriptBlockRegistered("DbNetSuiteJS"))
                RegisterStartupScript("CSRFToken", Shared.WriteUserToken(Nonce) + System.Environment.NewLine);
            if (!IsClientScriptBlockRegistered("DbNetSuiteCSS"))
                RegisterStartupScript("DbNetSuiteCSS", "<link rel=\"stylesheet\" type=\"text/css\"" + NonceAttribute + " href=\"DbNetSuite.css.ashx?theme=" + this.Theme.ToString() + "\"/>" + System.Environment.NewLine);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void AddAttributesToRender(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            Writer.AddAttribute("ServerID", this.ID);
            base.AddAttributesToRender(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void Render(HtmlTextWriter writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.NoRender)
                return;
            base.Render(writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        public string WriteClientPropertyAndValue(string PropertyName, object PropertyValue, bool AddQuotes)
        ////////////////////////////////////////////////////////////////////////////
        {
            return WriteClientPropertyAndValue(PropertyName, PropertyValue, AddQuotes, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        public string WriteClientPropertyAndValue(string PropertyName, object PropertyValue, bool AddQuotes, bool Nested)
        ////////////////////////////////////////////////////////////////////////////
        {
            String QuoteString = (AddQuotes) ? "\"" : String.Empty;
            string Prefix = (Nested) ? "\t\t" : "o.";
            return (String.Format(Prefix + "{1} = {0}{2}{0};{3}", QuoteString, ClientPropertyName(PropertyName, false), (PropertyValue.GetType().Name == "Boolean") ? PropertyValue.ToString().ToLower() : PropertyValue.ToString(), Environment.NewLine));
        }

        ////////////////////////////////////////////////////////////////////////////
        public string WriteClientPropertyAssignment(string PropertyName, object PropertyValue)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (PropertyValue is Unit)
                PropertyValue = PropertyValue.ToString();

            String QuoteString = (PropertyValue is String) ? "\"" : String.Empty;

            if (PropertyValue is Boolean)
                PropertyValue = PropertyValue.ToString().ToLower();

            switch (PropertyName)
            {
                case "CustomProfileProperties":
                    PropertyValue = "['" + String.Join("','", PropertyValue.ToString().Split(',')) + "']";
                    QuoteString = "";
                    break;
                case "ChartConfig":
                    QuoteString = "";
                    break;
            }

            PropertyName = ClientPropertyName(PropertyName, false);

            return PropertyName + " = " + QuoteString + PropertyValue.ToString() + QuoteString + ";";
        }

        ////////////////////////////////////////////////////////////////////////////
        public string ClientPropertyName(string PropertyName)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ClientPropertyName(PropertyName, true);
        }

        ////////////////////////////////////////////////////////////////////////////
        public string ClientPropertyName(string PropertyName, bool AddQuotes)
        ////////////////////////////////////////////////////////////////////////////
        {
            String Quote = (AddQuotes) ? "\"" : String.Empty;
            return (Quote + PropertyName.Substring(0, 1).ToLower() + PropertyName.Substring(1) + Quote);
        }

        ////////////////////////////////////////////////////////////////////////////
        public bool IsClientScriptBlockRegistered(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            return Page.ClientScript.IsClientScriptBlockRegistered(Key);
        }

        ////////////////////////////////////////////////////////////////////////////
        public void RegisterStartupScript(string Key, string Script)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Key == (this.ClientID + "Script"))
                Script = AddStartUpMethod(Script);

            Page.ClientScript.RegisterStartupScript(Page.GetType(), Key, Script);
        }

        ////////////////////////////////////////////////////////////////////////////
        public string AddStartUpMethod(string Script)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.DeferredLoad)
            {
                Script = Script.Insert(Script.IndexOf(">") + 1, "jQuery(document).ready( function(){");
                Script = Script.Insert(Script.IndexOf("</script>"), "});");
            }

            return Script;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteColumnProperties(Component Ctrl)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();
            ColumnCollection Columns = null;

            if (Ctrl is DbNetGrid)
                Columns = (Ctrl as DbNetGrid).GridColumns;
            else if (Ctrl is DbNetEdit)
                Columns = (Ctrl as DbNetEdit).EditColumns;
            else if (Ctrl is DbNetFile)
                Columns = (Ctrl as DbNetFile).FileColumns;
            else if (Ctrl is DbNetList)
                Columns = (Ctrl as DbNetList).ListColumns;

            if (Columns == null)
                return Source;

            if (Columns.Count == 0)
                return Source;

            List<string> ColumnExpressions = new List<string>();
            List<string> Labels = new List<string>();

            foreach (Object O in Columns)
            {
                if (!(O is Column))
                    continue;

                Column C = (Column)O;

                if (C is DbColumn)
                {
                    if ((C as DbColumn).ColumnExpression == "")
                        continue;

                    ColumnExpressions.Add(DbNetLink.Util.Encrypt((C as DbColumn).ColumnExpression));
                    Labels.Add(C.Label);
                }

                if (C is ListColumn)
                {
                    if ((C as ListColumn).ColumnExpression == "")
                        continue;

                    ColumnExpressions.Add(DbNetLink.Util.Encrypt((C as ListColumn).ColumnExpression));
                }

                if (C is FileColumn)
                {
                    FileColumn FC = (C as FileColumn);
                    switch (FC.ColumnType)
                    {
                        case DbNetLink.DbNetSuite.DbNetFile.ColumnTypes.WindowsSearch:
                            FC.ColumnExpression = FC.WindowsSearchColumnType.ToString();
                            break;
                        case DbNetLink.DbNetSuite.DbNetFile.ColumnTypes.IndexingService:
                            FC.ColumnExpression = FC.IndexingServiceColumnType.ToString();
                            break;
                        default:
                            FC.ColumnExpression = FC.ColumnType.ToString();
                            break;
                    }
                    ColumnExpressions.Add(FC.ColumnExpression);
                }

            }

            if (ColumnExpressions.Count == 0)
                return Source;

            if (Ctrl is GridEdit)
            {
                Source.Add("setColumnExpressions(\"" + String.Join("\",\"", ColumnExpressions.ToArray()) + "\");");
                Source.Add("setColumnLabels(\"" + String.Join("\",\"", Labels.ToArray()) + "\");");
            }

            if (Ctrl is DbNetFile)
                Source.Add("setColumnTypes(\"" + String.Join("\",\"", ColumnExpressions.ToArray()) + "\");");

            PropertyInfo[] PI = null;
            Column DefaultColumn = null;

            switch (Ctrl.GetType().Name)
            {
                case "DbNetGrid":
                    PI = typeof(GridColumn).GetProperties();
                    DefaultColumn = new GridColumn();
                    break;
                case "DbNetEdit":
                    PI = typeof(EditColumn).GetProperties();
                    DefaultColumn = new EditColumn();
                    break;
                case "DbNetFile":
                    PI = typeof(FileColumn).GetProperties();
                    DefaultColumn = new FileColumn();
                    break;
                case "DbNetList":
                    PI = typeof(ListColumn).GetProperties();
                    DefaultColumn = new ListColumn();
                    break;
            }

            foreach (Object O in Columns)
            {
                if (Ctrl is DbNetGrid)
                    if (!(O is GridColumn))
                        continue;

                if (Ctrl is DbNetEdit)
                    if (!(O is EditColumn))
                        continue;

                if (Ctrl is DbNetFile)
                    if (!(O is FileColumn))
                        continue;

                if (Ctrl is DbNetList)
                    if (!(O is ListColumn))
                        continue;

                Column C = (Column)O;

                if (C is DbColumn)
                    if ((C as DbColumn).ColumnExpression == "")
                        continue;

                foreach (PropertyInfo I in PI)
                {
                    object PropertyValue;

                    try
                    {
                        if (I.GetValue(C, null).ToString() == I.GetValue(DefaultColumn, null).ToString())
                            continue;
                        PropertyValue = I.GetValue(C, null);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    switch (I.Name)
                    {
                        case "ColumnExpression":
                            if (C is DbColumn || C is ListColumn)
                                continue;
                            break;
                        case "Label":
                            if (C is DbColumn)
                                continue;
                            break;
                        case "Lookup":
                            if (!(PropertyValue.ToString().StartsWith("[")))
                                PropertyValue = DbNetLink.Util.Encrypt(PropertyValue.ToString());
                            break;
                        case "WindowsSearchColumnType":
                        case "IndexingServiceColumnType":
                        case "ColumnType":
                            continue;
                            break;
                    }

                    string Quote = "";
                    if (PropertyValue is String || I.PropertyType.IsEnum)
                        Quote = "\"";

                    if (PropertyValue is bool)
                        PropertyValue = PropertyValue.ToString().ToLower();

                    string PropertyName = char.ToLower(I.Name[0]) + I.Name.Substring(1);

                    if (C is DbColumn)
                        Source.Add("setColumnProperty(\"" + DbNetLink.Util.Encrypt((C as DbColumn).ColumnExpression) + "\",\"" + PropertyName + "\"," + Quote + PropertyValue.ToString() + Quote + ");");
                    else if (C is FileColumn)
                        Source.Add("setColumnProperty(\"" + (C as FileColumn).ColumnExpression + "\",\"" + PropertyName + "\"," + Quote + PropertyValue.ToString() + Quote + ");");
                    else if (C is ListColumn)
                        Source.Add("setColumnProperty(\"" + DbNetLink.Util.Encrypt((C as ListColumn).ColumnExpression) + "\",\"" + PropertyName + "\"," + Quote + PropertyValue.ToString() + Quote + ");");
                }
            }

            return Source;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string WriteLinkedControls(Component Ctrl)
        ////////////////////////////////////////////////////////////////////////////
        {
            return WriteLinkedControls(Ctrl, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string WriteLinkedControls(Component Ctrl, bool Nested)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Ctrl.LinkedControls == null)
                return "";

            foreach (Object O in Ctrl.LinkedControls)
            {
                string LinkedControlID = String.Empty;
                bool OneToOne = false;
                DbNetLink.DbNetSuite.UI.Component C;

                if (O is string)
                {
                    LinkedControlID = O.ToString();
                    C = (DbNetLink.DbNetSuite.UI.Component)DbNetFindControl(LinkedControlID);
                }
                else if ((O is LinkedControl))
                {
                    LinkedControl LC = O as LinkedControl;
                    OneToOne = LC.OneToOne;

                    if (LC.LinkedControlRef != null)
                        C = LC.LinkedControlRef;
                    else
                    {
                        LinkedControlID = LC.LinkedControlID;
                        C = (DbNetLink.DbNetSuite.UI.Component)DbNetFindControl(LinkedControlID);
                    }
                }
                else if ((O is Component))
                {
                    C = (Component)O;
                    LinkedControlID = C.ID;
                }
                else
                    continue;

                if (C == null)
                    throw new Exception("Linked control [" + LinkedControlID + "] not found.");

                C.NoLoad = true;

                if (C == null)
                    continue;

                string S = "";

                if (Nested)
                    S += "addLinkedControl(DbNetLink.components['" + C.ClientID + "']";
                else
                    S += "DbNetLink.components['" + Ctrl.ClientID + "'].addLinkedControl(DbNetLink.components['" + C.ClientID + "']";

                if (this is GridEdit)
                    S += "," + OneToOne.ToString().ToLower();
                S += ");";

                Source.Add(S);
            }

            StringBuilder Script = new StringBuilder();

            foreach (string Line in Source)
                Script.Append(Line + Environment.NewLine);

            return Script.ToString();
        }



        ////////////////////////////////////////////////////////////////////////////
        internal virtual void RenderScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void RenderInvocationScript(Component ParentControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            string OneToOne = "false";
            string Script = String.Empty;

            if (ParentControl is GridEdit)
                OneToOne = (this.RelationshipToParent == ParentRelationships.OneToOne).ToString().ToLower();

            if (ParentControl == null)
            {
                if (this.NoLoad == false)
                    Script = "DbNetLink.components[\"" + this.ID + "\"].initialize();";
            }
            else
                Script = "DbNetLink.components['" + ParentControl.ID + "'].addLinkedControl(DbNetLink.components['" + this.ID + "']," + OneToOne + ");";

            MVC.Cache(MVC.CacheKeyNames.DeferredScript).Append(Script + Environment.NewLine);
        }


        ////////////////////////////////////////////////////////////////////////////
        public void CheckForParsed(Control Parent)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (Control C in Parent.Controls)
            {
                if (C.HasControls())
                    CheckForParsed(C);
                else
                    if (C is Component)
                    if (!((Component)C).Parsed)
                        this.AllParsed = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        public void CheckForLastControl(StringBuilder Script)
        ////////////////////////////////////////////////////////////////////////////
        {
            this.Parsed = true;
            this.AllParsed = true;

            CheckForParsed(Page);

            if (AllParsed)
            {
                FindAllControls(Page);

                foreach (Component Ctrl in this.AllControls)
                    Script.Append(Ctrl.DeferredScript);

                foreach (Component Ctrl in this.AllControls)
                    if (!Ctrl.NoLoad)
                        Script.Append("DbNetLink.components[\"" + Ctrl.ClientID + "\"].initialize();" + Environment.NewLine);
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        public void FindAllControls(Control Parent)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (Control C in Parent.Controls)
            {
                if (C.HasControls())
                    FindAllControls(C);
                else
                    if (C is Component)
                    this.AllControls.Add(C);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteParams(ArrayList ParamCollection, string ParamCollectionName)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (ParamCollection == null)
                return Source;

            List<string> Params = new List<string>();

            foreach (Object O in ParamCollection)
            {
                if (!(O is Parameter))
                    continue;

                Parameter P = (Parameter)O;

                Params.Add(P.Name + " : \"" + P.Value.ToString() + "\"");
            }

            if (Params.Count > 0)
                Source.Add(ParamCollectionName + " = {" + String.Join(",", Params.ToArray()) + "};");

            return Source;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void SetPropertyValue(object Obj, string PropertyName, object PropertyValue)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            BindingFlags BF = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

            PropertyInfo P = Obj.GetType().GetProperty(PropertyName, BF);

            if (P == null)
                return;

            if (!P.CanWrite)
                return;

            try
            {
                if (P.PropertyType.IsEnum)
                    P.SetValue(Obj, Enum.Parse(P.PropertyType, PropertyValue.ToString(), true), null);
                else
                    P.SetValue(Obj, PropertyValue, null);
            }
            catch (Exception)
            {
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void DbNetSpellClientScript(ArrayList ScriptLines, DbNetSpell Spell)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Lines = new ArrayList();

            PropertyInfo[] PI = typeof(DbNetSpell).GetProperties();
            DbNetSpell DefaultFile = new DbNetSpell();

            foreach (PropertyInfo I in PI)
            {
                object PropertyValue;

                try
                {
                    if (I.GetValue(Spell, null).ToString() == I.GetValue(DefaultFile, null).ToString())
                        continue;

                    PropertyValue = I.GetValue(Spell, null);
                }
                catch (Exception)
                {
                    continue;
                }

                switch (I.Name)
                {
                    default:
                        if (I.PropertyType.IsEnum)
                            PropertyValue = PropertyValue.ToString();

                        Lines.Add(WriteClientPropertyAssignment(I.Name, PropertyValue));
                        break;

                }
            }

            if (this is DbNetGrid || this is DbNetEdit)
                for (int I = 0; I < Lines.Count; I++)
                    Lines[I] = "dbNetSpell." + Lines[I].ToString();

            Lines.AddRange(DbNetSpell.WriteSpellClientEvents(Spell));
            Lines.AddRange(DbNetSpell.WriteSpellCheckElements(Spell));

            ScriptLines.AddRange(Lines);
        }


    }

    ////////////////////////////////////////////////////////////////////////////
    public class SearchFilter
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _Label = "";
        private string _Filter = "";

        public String Label
        {
            get { return _Label; }
            set { _Label = value; }
        }

        public String Filter
        {
            get { return _Filter; }
            set { _Filter = value; }
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class SearchFilterCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public SearchFilterCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(SearchFilter);
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class SearchDialogContentItem
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _ColumnName = "";
        private string _Operator = "";
        private string _Value = "";
        private string _Value2 = "";

        public String ColumnName
        {
            get { return _ColumnName; }
            set { _ColumnName = value; }
        }

        public String Operator
        {
            get { return _Operator; }
            set { _Operator = value; }
        }

        public String Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public String Value2
        {
            get { return _Value2; }
            set { _Value2 = value; }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class SearchDialogContentCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public SearchDialogContentCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(SearchDialogContentItem);
        }

    }


    ////////////////////////////////////////////////////////////////////////////
    public class LinkedControl
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _LinkedControlID = String.Empty;
        private bool _OneToOne = false;
        private Component _LinkedControlRef = null;

        public LinkedControl()
        {
        }

        public LinkedControl(string LinkedControlID)
        {
            this._LinkedControlID = LinkedControlID;
        }

        public LinkedControl(string LinkedControlID, bool OneToOne)
        {
            this._LinkedControlID = LinkedControlID;
            this._OneToOne = OneToOne;
        }

        public LinkedControl(Component LinkedControlRef)
        {
            this._LinkedControlRef = LinkedControlRef;
        }

        public LinkedControl(Component LinkedControlRef, bool OneToOne)
        {
            this._LinkedControlRef = LinkedControlRef;
            this._OneToOne = OneToOne;
        }

        [TypeConverter(typeof(DbNetSuiteControlConverter))]
        public String LinkedControlID
        {
            get { return _LinkedControlID; }
            set { _LinkedControlID = value; }
        }

        public Component LinkedControlRef
        {
            get { return _LinkedControlRef; }
            set { _LinkedControlRef = value; }
        }

        public bool OneToOne
        {
            get { return _OneToOne; }
            set { _OneToOne = value; }
        }
    }

}