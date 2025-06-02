using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using System.Windows.Forms.Design.Behavior;
using System.Drawing;
using System.Drawing.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms.Design;
using System.Windows.Forms;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
///////////////////////////////////////////////
{
    #region Designers

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetSuiteControlDesigner : System.Web.UI.Design.ControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected StringBuilder Sb;
        protected Table T;
        protected DbNetSuite.UI.Component DbNetSuiteControl;

        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            DbNetSuiteControl = component as DbNetSuite.UI.Component;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void InitDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            Sb = new StringBuilder();
            T = new Table();
            T.Style.Add(HtmlTextWriterStyle.FontSize, "8pt");
            T.Style.Add(HtmlTextWriterStyle.FontFamily, "verdana");
        }

        ////////////////////////////////////////////////////////////////////////////
        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs ce)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnComponentChanged(sender, ce);
            switch (ce.Member.Name)
            {
                case "FromPart":
                    PopulateColumns(ce.NewValue.ToString());
                    this.UpdateDesignTimeHtml();
                    break;
            }

        }

        ////////////////////////////////////////////////////////////////////////////
        internal ListDictionary GetConnectionInfo()
        ////////////////////////////////////////////////////////////////////////////
        {
            return DesignUtility.ConnectionInfo(Component.Site);
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void PopulateColumns(string TableName)
        ////////////////////////////////////////////////////////////////////////////
        {
            string Msg = "";

            DbNetLink.Data.DbNetData Db = DesignUtility.DbConnection((DbNetSuiteControl as GridEdit).ConnectionString, Component.Site, ref Msg);

            if (Db == null)
                return;

            if (DbNetSuiteControl is DbNetGrid)
                (DbNetSuiteControl as DbNetGrid).GridColumns.Clear();
            else
                (DbNetSuiteControl as DbNetEdit).EditColumns.Clear();

            DataTable DT = Db.GetSchemaTable(TableName);
            Hashtable HT = new Hashtable();

            foreach (DataRow R in DT.Rows)
            {
                string ColumnName = R["ColumnName"].ToString();

                if (HT.ContainsKey(ColumnName))
                    continue;

                HT[ColumnName] = true;
                if (DbNetSuiteControl is DbNetGrid)
                    (DbNetSuiteControl as DbNetGrid).GridColumns.Add(new GridColumn(ColumnName));
                else
                    (DbNetSuiteControl as DbNetEdit).EditColumns.Add(new EditColumn(ColumnName));
            }

            Db.Close();
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string NoConnectionString()
        ////////////////////////////////////////////////////////////////////////////
        {
            string Msg = "";

            ListDictionary Connections = GetConnectionInfo();

            Msg = @"Please specify the <b>ConnectionString</b> property.";

            if (Connections.Count == 0)
            {
                Msg += @"The value must be defined in the <b>ConnectionStrings</b>
                            section of your application <b>web.config</b> file e.g</p>
<pre style='font-family:courier new'>
&lt;configuration&gt;
  ...
  &lt;connectionStrings&gt;
    ...
    &lt;add name=&quot;<i>name</i>&quot; connectionString=&quot;<i>connection string</i>&quot;/&gt;
    ...
  &lt;/connectionStrings&gt;
  ...
&lt;/configuration&gt;
</pre>";
            }

            return AddMessage(Msg);
        }


        ////////////////////////////////////////////////////////////////////////////
        protected string AddMessage( string Msg )
        ////////////////////////////////////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            T.Controls.Add(TR);
            TableCell TC = new TableCell();
            TC.BackColor = System.Drawing.Color.LightYellow;
            TR.Controls.Add(TC);

            TC.Text = Msg;
            return Shared.RenderControlToString(T);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string ConnectionError()
        ////////////////////////////////////////////////////////////////////////////
        {
            string Msg = @"Connection Error ==>" + DbNetSuiteControl._DesignTimeErrorMessage;
            DbNetSuiteControl._DesignTimeErrorMessage = "";
            return AddMessage(Msg);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string NoColumns()
        ////////////////////////////////////////////////////////////////////////////
        {
            string Msg = "To select particular columns from the table, view or procedure use the " +
                    "<b>Columns</b> collection specifying the column name in the <b>ColumnExpression</b> property";
            return AddMessage(Msg);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionListCollection ActionLists
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                DesignerActionListCollection AI = new DesignerActionListCollection();

                if ( DbNetSuiteControl is GridEdit )
                    AI.Add(new GridEditDesignerActionList(this));
                else if (DbNetSuiteControl is DbNetSpell)
                    AI.Add(new DbNetSpellDesignerActionList(this));
                else if (DbNetSuiteControl is DbNetList)
                    AI.Add(new DbNetListDesignerActionList(this));
                else if (DbNetSuiteControl is DbNetCombo)
                    AI.Add(new DbNetComboDesignerActionList(this));
                return AI;
            }
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetGridControlDesigner : DbNetSuiteControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected DbNetGrid Grid;
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            Grid = (DbNetGrid)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                InitDesignTimeHtml();

                T.Style.Add(HtmlTextWriterStyle.BorderCollapse, "collapse");
                T.Attributes.Add("BorderColor", System.Drawing.Color.Silver.ToString());
                T.Attributes.Add("Border", "1");

                TableRow TR;

                if (Grid._DesignTimeErrorMessage != "")
                    return ConnectionError(); 

                if (Grid.ConnectionString == "")
                    return NoConnectionString();

                if (Grid.FromPart == "" && Grid.ProcedureName == "")
                {
                    TR = new TableRow();
                    T.Controls.Add(TR);
                    TableCell TC = new TableCell();
                    TR.Controls.Add(TC);
                    TC.Text = @"Please specify either the <b>FromPart</b> or <b>ProcedureName</b> property";
                    TC.BackColor = System.Drawing.Color.LightYellow;

                    return Shared.RenderControlToString(T);
                }

                if (Grid.GridColumns.Count == 0)
                    return NoColumns();

                TR = new TableRow();
                T.Controls.Add(TR);
                TR.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                TR.Style.Add(HtmlTextWriterStyle.BackgroundColor, "gainsboro");

                foreach (GridColumn C in Grid.GridColumns)
                {
                    TableCell TC = new TableCell();
                    TR.Controls.Add(TC);
                    TC.Text = String.IsNullOrEmpty(C.Label) ? C.ColumnExpression : C.Label;
                }

                for (int r = 0; r < 10; r++)
                {
                    TR = new TableRow();
                    T.Controls.Add(TR);

                    foreach (GridColumn C in Grid.GridColumns)
                    {
                        TableCell TC = new TableCell();
                        TR.Controls.Add(TC);
                        TC.Text = "&nbsp;";
                    }
                }

                return Shared.RenderControlToString(T);

            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetEditControlDesigner : DbNetSuiteControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected DbNetEdit Edit;
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            Edit = (DbNetEdit)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                InitDesignTimeHtml();

                if (Edit.ConnectionString == "")
                    return NoConnectionString();

                if (Edit._DesignTimeErrorMessage != "")
                    return ConnectionError();


                TableRow TR = new TableRow();

                if (Edit.FromPart == "")
                {
                    T.Controls.Add(TR);
                    TableCell TC = new TableCell();
                    TR.Controls.Add(TC);
                    TC.Text = @"Please specify the <b>FromPart</b> property";
                    TC.BackColor = System.Drawing.Color.LightYellow;

                    return Shared.RenderControlToString(T);
                }

                if (Edit.EditColumns.Count == 0)
                    return NoColumns();

                T.Controls.Add(TR);
                TR.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                TR.Style.Add(HtmlTextWriterStyle.BackgroundColor, "gainsboro");

                foreach (DbNetSuite.EditColumn C in Edit.EditColumns)
                {
                    TR = new TableRow();
                    T.Controls.Add(TR);
                    TableCell TC = new TableCell();
                    TR.Controls.Add(TC);
                    TC.Text = String.IsNullOrEmpty(C.Label) ? C.ColumnExpression : C.Label;

                    TC = new TableCell();
                    TR.Controls.Add(TC);

                    System.Web.UI.WebControls.TextBox TB = new System.Web.UI.WebControls.TextBox();
                    TC.Controls.Add(TB);
                    TB.Style.Add(HtmlTextWriterStyle.FontSize, "8pt");
                    TB.Style.Add(HtmlTextWriterStyle.FontFamily, "verdana");
                }

                return Shared.RenderControlToString(T);

            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetFileControlDesigner : System.Web.UI.Design.ControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected StringBuilder Sb;
        protected Table T;
        protected DbNetFile File;
        
        ////////////////////////////////////////////////////////////////////////////
        protected void InitDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            Sb = new StringBuilder();
            T = new Table();
            T.Style.Add(HtmlTextWriterStyle.FontSize, "8pt");
            T.Style.Add(HtmlTextWriterStyle.FontFamily, "verdana");
        }
        
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            File = (DbNetFile)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                InitDesignTimeHtml();

                T.Style.Add(HtmlTextWriterStyle.BorderCollapse, "collapse");
                T.Attributes.Add("BorderColor", System.Drawing.Color.Silver.ToString());
                T.Attributes.Add("Border", "1");

                TableRow TR;

                if (File.RootFolder == "")
                    return AddMessage("Please specify the <b>RootFolder</b> property as a virtual or physical path");

                TR = new TableRow();
                T.Controls.Add(TR);

                if (File.DisplayStyle == DbNetSuite.DbNetFile.DisplayStyles.Tree)
                {
                    TableCell TC = new TableCell();
                    TR.Controls.Add(TC);
                    TC.Controls.Add(AddTreeView());
                }
                else
                {
                    if (File.FileColumns.Count == 0)
                        File.AddDefaultColumns();

                    TR.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                    TR.Style.Add(HtmlTextWriterStyle.BackgroundColor, "gainsboro");

                    foreach (DbNetSuite.FileColumn C in File.FileColumns)
                    {
                        TableCell TC = new TableCell();
                        TR.Controls.Add(TC);
                        TC.Text = String.IsNullOrEmpty(C.Label) ? C.ColumnType.ToString() : C.Label;
                    }

                    for (int r = 0; r < 10; r++)
                    {
                        TR = new TableRow();
                        T.Controls.Add(TR);

                        foreach (DbNetSuite.FileColumn C in File.FileColumns)
                        {
                            TableCell TC = new TableCell();
                            TR.Controls.Add(TC);
                            TC.Text = "&nbsp;";
                        }
                    }
                }

                return Shared.RenderControlToString(T);

            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }



        ////////////////////////////////////////////////////////////////////////////
        private Table AddTreeView()
        ////////////////////////////////////////////////////////////////////////////
        {
            Table T = new Table();
            T.Height = File.Height;
            T.Width = File.Width;
            TableRow TR = new TableRow();
            T.Controls.Add(TR);
            TableCell TC = new TableCell();
            TC.VerticalAlign = VerticalAlign.Top;
            TR.Controls.Add(TC);

            System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
            I.ImageUrl = File.Page.ClientScript.GetWebResourceUrl(File.GetType(), "DbNetLink.Shared.Resources.Images.FolderClosed.png");
            TC.Controls.Add(I);

            TC = new TableCell();
            TC.VerticalAlign = VerticalAlign.Top;
            TR.Controls.Add(TC);
            TC.Text = File.GetRootFolderAlias();

            return T;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected string AddMessage(string Msg)
        ////////////////////////////////////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            T.Controls.Add(TR);
            TableCell TC = new TableCell();
            TC.BackColor = System.Drawing.Color.LightYellow;
            TR.Controls.Add(TC);

            TC.Text = Msg;
            return Shared.RenderControlToString(T);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionListCollection ActionLists
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                DesignerActionListCollection AI = new DesignerActionListCollection();
                AI.Add(new DbNetFileDesignerActionList(this));
                return AI;
            }
        }

    }



    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetSpellControlDesigner : DbNetSuiteControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected DbNetSpell Spell;
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            Spell = (DbNetSpell)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {

            try
            {
                System.Web.UI.HtmlControls.HtmlGenericControl B = new System.Web.UI.HtmlControls.HtmlGenericControl("button");
                System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
                I.ImageUrl = Spell.Page.ClientScript.GetWebResourceUrl(typeof(DbNetLink.DbNetSuite.DbNetSpell), "DbNetLink.Resources.Images.spellcheck.png");
                B.Controls.Add(I);
                return Shared.RenderControlToString(B);
            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }
    }



    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetListControlDesigner : DbNetSuiteControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected DbNetList List;
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            List = (DbNetList)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                System.Web.UI.WebControls.Panel P = new System.Web.UI.WebControls.Panel();

                if (List.Width != Unit.Empty)
                    P.Width = List.Width;
                else
                    P.Width = new Unit("200px");
                if (List.Height != Unit.Empty)
                    P.Height = List.Height;
                else
                    P.Height = new Unit("200px");
                P.BorderWidth = 1;
                P.BorderColor = Color.Silver;

                return Shared.RenderControlToString(P);
            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }

    }


    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetComboControlDesigner : DbNetSuiteControlDesigner
    ////////////////////////////////////////////////////////////////////////////
    {
        protected DbNetCombo Combo;
        ////////////////////////////////////////////////////////////////////////////
        public override void Initialize(IComponent component)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.Initialize(component);
            Combo = (DbNetCombo)component;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override string GetDesignTimeHtml()
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                DropDownList DDL = new DropDownList();
                return Shared.RenderControlToString(DDL);
            }
            catch (Exception ex)
            {
                return String.Concat("<h3>Error</h3>Stack Trace:<br>", ex.StackTrace);
            }
        }

    }

    #endregion


    #region Type Converters
    ////////////////////////////////////////////////////////////////////////////
    internal class BaseConverter : StringConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        internal ListDictionary GetConnectionInfo(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return DesignUtility.ConnectionInfo(context);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class ServerControlConverter : BaseConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return true;
        }
        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return false;
        }
        ////////////////////////////////////////////////////////////////////////////
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            if ((context == null) || (context.Container == null))
            {
                return null;
            }
            Object[] serverControls = this.GetControls(context.Container);
            if (serverControls != null)
            {
                return new StandardValuesCollection(serverControls);
            }
            return null;
        }
        ////////////////////////////////////////////////////////////////////////////
        private object[] GetControls(IContainer container)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList availableControls = new ArrayList();
            foreach (IComponent component in container.Components)
            {
                System.Web.UI.Control serverControl = component as System.Web.UI.Control;
                if (serverControl != null &&
                     !(serverControl is Page) &&
                     serverControl.ID != null &&
                     serverControl.ID.Length != 0 &&
                     IncludeControl(serverControl)
                    )
                {
                    availableControls.Add(serverControl.ID);
                }
            }
            availableControls.Sort(Comparer.Default);
            return availableControls.ToArray();
        }
         //Override this method to customize which controls show up in the list
        ////////////////////////////////////////////////////////////////////////////
        protected virtual Boolean IncludeControl(System.Web.UI.Control serverControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            return true;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    internal class GridEditControlConverter : ServerControlConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        protected override Boolean IncludeControl(System.Web.UI.Control serverControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            return (serverControl is GridEdit);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetSuiteControlConverter : ServerControlConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        protected override Boolean IncludeControl(System.Web.UI.Control serverControl)
        ////////////////////////////////////////////////////////////////////////////
        {
            return (serverControl is DbNetLink.DbNetSuite.UI.Component);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class DbNetFileConverter : ServerControlConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        protected override Boolean IncludeControl(System.Web.UI.Control serverControl)
        {
            if (serverControl is DbNetFile)
                return true;
            else
                return false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class ConnectionStringConverter : BaseConverter
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return (context != null);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            StringCollection Names = new StringCollection();
            ListDictionary ConnectionInfo = this.GetConnectionInfo(context);
            foreach (string Key in ConnectionInfo.Keys)
                Names.Add(Key);

            return new StandardValuesCollection(Names);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    internal class FromPartConverter : BaseConverter
    ////////////////////////////////////////////////////////////////////////////
    {

        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            return (context != null);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        ////////////////////////////////////////////////////////////////////////////
        {
            List<string> DbObjects = new List<string>();

            DbNetSuite.UI.DatabaseControl Ctrl = null;

            if (context.Instance is DatabaseControl )
                Ctrl = (context.Instance as DatabaseControl);
            else if (context.Instance is GridEditDesignerActionList)
                Ctrl = (context.Instance as GridEditDesignerActionList).Ctrl;
            else if (context.Instance is DbNetSpellDesignerActionList)
                Ctrl = (context.Instance as DbNetSpellDesignerActionList).Ctrl;
            else if (context.Instance is DbNetListDesignerActionList)
                Ctrl = (context.Instance as DbNetListDesignerActionList).Ctrl;

            string Msg = "";

            DbNetLink.Data.DbNetData Db = DesignUtility.DbConnection(Ctrl.ConnectionString, context, ref Msg);

            if (Db == null)
            {
                DbObjects.Add( Msg );
                return new StandardValuesCollection(DbObjects);
            }

            DataTable Tables = Db.MetaDataCollection(DbNetLink.Data.MetaDataType.UserTables);

            foreach ( DataRow R in Tables.Rows )
                DbObjects.Add(R["table_name"].ToString());


            if (Ctrl is DbNetGrid)
            {
                DataTable Views = Db.MetaDataCollection(DbNetLink.Data.MetaDataType.Views);
                foreach (DataRow R in Views.Rows)
                    DbObjects.Add(R["table_name"].ToString());
            }

            Db.Close();
            DbObjects.Sort();
            return new StandardValuesCollection(DbObjects.ToArray());
        }
    }
#endregion
#region Designer Utilities
    ////////////////////////////////////////////////////////////////////////////
    internal class DesignUtility
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        internal static ListDictionary ConnectionInfo(IServiceProvider context)
        ////////////////////////////////////////////////////////////////////////////
        {
            ListDictionary ConnectionInfo = new ListDictionary();

            IWebApplication app = (IWebApplication)context.GetService(typeof(IWebApplication));

            if (app != null)
            {
                Configuration config = app.OpenWebConfiguration(true);
                if (config != null)
                {
                    foreach (ConnectionStringSettings css in config.ConnectionStrings.ConnectionStrings)
                    {
                        ConnectionInfo[css.Name] = css.ConnectionString;
                    }
                }
            }
            return ConnectionInfo;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal static DbNetLink.Data.DbNetData DbConnection(string ConnectionName, IServiceProvider Context, ref string Msg)
        ////////////////////////////////////////////////////////////////////////////
        {
            ListDictionary ConnectionInfo = DesignUtility.ConnectionInfo(Context);

            if (!ConnectionInfo.Contains(ConnectionName))
                return null;

            string CS = ConnectionInfo[ConnectionName].ToString();

            IWebApplication app = (IWebApplication)Context.GetService(typeof(IWebApplication));
            if (app != null)
                CS = Regex.Replace(CS, "data source=~", "data source=" + app.RootProjectItem.PhysicalPath, RegexOptions.IgnoreCase);

            DbNetLink.Data.DbNetData Db = new DbNetLink.Data.DbNetData(CS);

            try
            {
                Db.Open();
            }
            catch (Exception Ex)
            {
                Msg = Ex.Message;
                return null;
            }

            return Db;
        }
    }

#endregion

#region DesignerActionLists
    ////////////////////////////////////////////////////////////////////////////
    public class GridEditDesignerActionList : DesignerActionList
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetSuiteControlDesigner _Designer;
        ////////////////////////////////////////////////////////////////////////////
        internal GridEditDesignerActionList(DbNetSuiteControlDesigner Designer)
            : base(Designer.Component)
        ////////////////////////////////////////////////////////////////////////////
        {
            this._Designer = Designer;
			this.AutoShow = true;
		}

        ////////////////////////////////////////////////////////////////////////////
        internal GridEdit Ctrl
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return (GridEdit)this.Component; }
		}

        [TypeConverter(typeof(ConnectionStringConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string ConnectionString
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.ConnectionString; }
            set { this.SetProperty("ConnectionString", value); }
		}

        [TypeConverter(typeof(FromPartConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string FromPart
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.FromPart; }
            set { this.SetProperty("FromPart", value); }
        }


        ////////////////////////////////////////////////////////////////////////////
        public void EditColumns()
        ////////////////////////////////////////////////////////////////////////////
        {
            string ColumnPropName = Ctrl.GetType().Name.Replace("DbNet", "") + "Columns";
            EditorServiceContext.EditValue(this._Designer, Component, ColumnPropName);
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(string propertyName, object value)
        ////////////////////////////////////////////////////////////////////////////
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.Component)[propertyName];
            property.SetValue(this.Component, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionItemCollection GetSortedActionItems()
        ////////////////////////////////////////////////////////////////////////////
        {
			DesignerActionItemCollection AI = new DesignerActionItemCollection();

            AI.Add(new DesignerActionPropertyItem("ConnectionString", "ConnectionString"));
            AI.Add(new DesignerActionPropertyItem("FromPart", "FromPart"));

            string ColumnPropName = Ctrl.GetType().Name.Replace("DbNet","") + "Columns";

            AI.Add(new DesignerActionMethodItem(this, "EditColumns", "Columns...", ColumnPropName));

            return AI;
    	}
    }

    ////////////////////////////////////////////////////////////////////////////
    public class DbNetFileDesignerActionList : DesignerActionList
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetFileControlDesigner _Designer;
        ////////////////////////////////////////////////////////////////////////////
        internal DbNetFileDesignerActionList(DbNetFileControlDesigner Designer)
            : base(Designer.Component)
        ////////////////////////////////////////////////////////////////////////////
        {
            this._Designer = Designer;
            this.AutoShow = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        public string RootFolder
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.FileControl.RootFolder; }
            set { this.SetProperty("RootFolder", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        public string RootFolderAlias
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.FileControl.RootFolderAlias; }
            set { this.SetProperty("RootFolderAlias", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.DisplayStyles DisplayStyle
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.FileControl.DisplayStyle; }
            set { this.SetProperty("DisplayStyle", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetFile.SelectionModes SelectionMode
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.FileControl.SelectionMode; }
            set { this.SetProperty("SelectionMode", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(string propertyName, object value)
        ////////////////////////////////////////////////////////////////////////////
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.Component)[propertyName];
            property.SetValue(this.Component, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        internal DbNetFile FileControl
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return (DbNetFile)this.Component; }
        }

        public void EditColumns()
        {
            EditorServiceContext.EditValue(this._Designer, Component, "FileColumns");
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionItemCollection GetSortedActionItems()
        ////////////////////////////////////////////////////////////////////////////
        {
            DesignerActionItemCollection AI = new DesignerActionItemCollection();

            AI.Add(new DesignerActionPropertyItem("RootFolder", "RootFolder"));
            AI.Add(new DesignerActionPropertyItem("RootFolderAlias", "RootFolderAlias"));
            AI.Add(new DesignerActionPropertyItem("DisplayStyle", "DisplayStyle"));
            AI.Add(new DesignerActionPropertyItem("SelectionMode", "SelectionMode"));
            AI.Add(new DesignerActionMethodItem(this, "EditColumns", "Columns...", "Columns"));

            return AI;
        }

    }


    ////////////////////////////////////////////////////////////////////////////
    public class DbNetSpellDesignerActionList : DesignerActionList
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetSuiteControlDesigner _Designer;
        ////////////////////////////////////////////////////////////////////////////
        internal DbNetSpellDesignerActionList(DbNetSuiteControlDesigner Designer)
            : base(Designer.Component)
        ////////////////////////////////////////////////////////////////////////////
        {
            this._Designer = Designer;
            this.AutoShow = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal DbNetSpell Ctrl
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return (DbNetSpell)this.Component; }
        }

        [TypeConverter(typeof(ConnectionStringConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string ConnectionString
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.ConnectionString; }
            set { this.SetProperty("ConnectionString", value); }
        }

        [TypeConverter(typeof(FromPartConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string DictionaryTableName
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.DictionaryTableName; }
            set { this.SetProperty("DictionaryTableName", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(string propertyName, object value)
        ////////////////////////////////////////////////////////////////////////////
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.Component)[propertyName];
            property.SetValue(this.Component, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionItemCollection GetSortedActionItems()
        ////////////////////////////////////////////////////////////////////////////
        {
            DesignerActionItemCollection AI = new DesignerActionItemCollection();

            AI.Add(new DesignerActionPropertyItem("ConnectionString", "ConnectionString"));
            AI.Add(new DesignerActionPropertyItem("DictionaryTableName", "DictionaryTableName"));

            return AI;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class DbNetListDesignerActionList : DesignerActionList
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetSuiteControlDesigner _Designer;
        ////////////////////////////////////////////////////////////////////////////
        internal DbNetListDesignerActionList(DbNetSuiteControlDesigner Designer)
            : base(Designer.Component)
        ////////////////////////////////////////////////////////////////////////////
        {
            this._Designer = Designer;
            this.AutoShow = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal DbNetList Ctrl
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return (DbNetList)this.Component; }
        }

        [TypeConverter(typeof(ConnectionStringConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string ConnectionString
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.ConnectionString; }
            set { this.SetProperty("ConnectionString", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        public string Sql
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.Sql; }
            set { this.SetProperty("Sql", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(string propertyName, object value)
        ////////////////////////////////////////////////////////////////////////////
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.Component)[propertyName];
            property.SetValue(this.Component, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionItemCollection GetSortedActionItems()
        ////////////////////////////////////////////////////////////////////////////
        {
            DesignerActionItemCollection AI = new DesignerActionItemCollection();

            AI.Add(new DesignerActionPropertyItem("ConnectionString", "ConnectionString"));
            AI.Add(new DesignerActionPropertyItem("Sql", "Sql"));

            return AI;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class DbNetComboDesignerActionList : DesignerActionList
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetSuiteControlDesigner _Designer;
        ////////////////////////////////////////////////////////////////////////////
        internal DbNetComboDesignerActionList(DbNetSuiteControlDesigner Designer)
            : base(Designer.Component)
        ////////////////////////////////////////////////////////////////////////////
        {
            this._Designer = Designer;
            this.AutoShow = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal DbNetCombo Ctrl
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return (DbNetCombo)this.Component; }
        }

        [TypeConverter(typeof(ConnectionStringConverter))]
        ////////////////////////////////////////////////////////////////////////////
        public string ConnectionString
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.ConnectionString; }
            set { this.SetProperty("ConnectionString", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        public string Sql
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return this.Ctrl.Sql; }
            set { this.SetProperty("Sql", value); }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(string propertyName, object value)
        ////////////////////////////////////////////////////////////////////////////
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.Component)[propertyName];
            property.SetValue(this.Component, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        public override DesignerActionItemCollection GetSortedActionItems()
        ////////////////////////////////////////////////////////////////////////////
        {
            DesignerActionItemCollection AI = new DesignerActionItemCollection();

            AI.Add(new DesignerActionPropertyItem("ConnectionString", "ConnectionString"));
            AI.Add(new DesignerActionPropertyItem("Sql", "Sql"));

            return AI;
        }
    }



















    internal class EditorServiceContext : IWindowsFormsEditorService, ITypeDescriptorContext
    {
        private ComponentDesigner _designer;
        private IComponentChangeService _componentChangeSvc;
        private PropertyDescriptor _targetProperty;

        internal EditorServiceContext(ComponentDesigner designer)
        {
            this._designer = designer;
        }

        internal EditorServiceContext(ComponentDesigner designer, PropertyDescriptor prop)
        {
            this._designer = designer;
            this._targetProperty = prop;

            if (prop == null)
            {
                prop = TypeDescriptor.GetDefaultProperty(designer.Component);
                if (prop != null && typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                {
                    _targetProperty = prop;
                }
            }

            Debug.Assert(_targetProperty != null, "Need PropertyDescriptor for ICollection property to associate collectoin edtior with.");
        }

        internal EditorServiceContext(ComponentDesigner designer, PropertyDescriptor prop, string newVerbText)
            : this(designer, prop)
        {
            Debug.Assert(!string.IsNullOrEmpty(newVerbText), "newVerbText cannot be null or empty");
            _designer.Verbs.Add(new DesignerVerb(newVerbText, new EventHandler(this.OnEditItems)));
        }

        public static object EditValue(ComponentDesigner designer, object objectToChange, string propName)
        {
            // Get PropertyDescriptor
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(objectToChange)[propName];

            // Create a Context
            EditorServiceContext context = new EditorServiceContext(designer, descriptor);

            // Get Editor
            UITypeEditor editor = descriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;

            // Get value to edit
            object value = descriptor.GetValue(objectToChange);

            // Edit value
            object newValue = editor.EditValue(context, context, value);

            if (newValue != value)
            {
                try
                {
                    descriptor.SetValue(objectToChange, newValue);
                }
                catch (CheckoutException)
                {

                }
            }

            return newValue;
        }

        // Our caching property for the IComponentChangeService
        private IComponentChangeService ChangeService
        {
            get
            {
                if (_componentChangeSvc == null)
                {
                    _componentChangeSvc = (IComponentChangeService)((IServiceProvider)this).GetService(typeof(IComponentChangeService));
                }
                return _componentChangeSvc;
            }
        }

        // Self-explanitory interface impl.
        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                if (_designer.Component.Site != null)
                {
                    return _designer.Component.Site.Container;
                }
                return null;
            }
        }

        // Interface implementation
        void ITypeDescriptorContext.OnComponentChanged()
        {
            ChangeService.OnComponentChanged(_designer.Component, _targetProperty, null, null);
        }

        // Interface implementation
        bool ITypeDescriptorContext.OnComponentChanging()
        {
            try
            {
                ChangeService.OnComponentChanging(_designer.Component, _targetProperty);
            }
            catch (CheckoutException checkoutException)
            {
                if (checkoutException == CheckoutException.Canceled)
                {
                    return false;
                }
                throw;
            }
            return true;
        }

        // Interface implementation
        object ITypeDescriptorContext.Instance
        {
            get
            {
                return _designer.Component;
            }
        }

        // Interface implementation
        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return _targetProperty;
            }
        }

        // Interface implementation
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(ITypeDescriptorContext) ||
                serviceType == typeof(IWindowsFormsEditorService))
            {
                return this;
            }

            if (_designer.Component.Site != null)
            {
                return _designer.Component.Site.GetService(serviceType);
            }
            return null;
        }

        // Interface implementation
        void IWindowsFormsEditorService.CloseDropDown()
        {
            // we'll never be called to do this.
            //
            Debug.Fail("NOTIMPL");
            return;
        }

        // Interface implementation
        void IWindowsFormsEditorService.DropDownControl(System.Windows.Forms.Control control)
        {
            // nope, sorry
            //
            Debug.Fail("NOTIMPL");
            return;
        }

        // Interface implementation
        System.Windows.Forms.DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
        {
            IUIService uiSvc = (IUIService)((IServiceProvider)this).GetService(typeof(IUIService));
            if (uiSvc != null)
            {
                return uiSvc.ShowDialog(dialog);
            }
            else
            {
                return dialog.ShowDialog(_designer.Component as IWin32Window);
            }
        }

        // When the verb is invoked, use all the stuff above to show the dialog, etc.
        private void OnEditItems(object sender, EventArgs e)
        {
            object propertyValue = _targetProperty.GetValue(_designer.Component);
            if (propertyValue == null)
            {
                return;
            }
            CollectionEditor itemsEditor = TypeDescriptor.GetEditor(propertyValue, typeof(UITypeEditor)) as CollectionEditor;

            Debug.Assert(itemsEditor != null, "Didn't get a collection editor for type '" + _targetProperty.PropertyType.FullName + "'");
            if (itemsEditor != null)
            {
                itemsEditor.EditValue(this, this, propertyValue);
            }
        }
    }
    #endregion

}