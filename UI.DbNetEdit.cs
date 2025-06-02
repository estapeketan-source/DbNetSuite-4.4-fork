using System;
using System.Collections;
using System.ComponentModel; 
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design; 
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms.Design;
using System.Xml;


[assembly: TagPrefix ("DbNetLink.DbNetEdit" , "DNL") ]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{
	////////////////////////////////////////////////////////////////////////////
	class HtmlContentConverter : TypeConverter
	////////////////////////////////////////////////////////////////////////////
	{

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			else
				return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if ((destinationType == typeof(string)) |
				(destinationType == typeof(InstanceDescriptor)))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value == null)
				return new Xml();

			// if the source is a string then convert to our type
			if (value is string)
			{
				// get strongly typed value
				string StringValue = value as string;

				// if empty string then return again a new instance of Xml
				if (StringValue.Length <= 0)
					return new Xml();

				Xml Content = new Xml();
				Content.DocumentContent = StringValue;
					
				return Content;
			}

			else
				return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			// check that the value passed along is of our type
			if (value != null)
				if (!(value is Xml))
					throw new Exception("WrongType");

			// convert to a string
			if (destinationType == typeof(string))
			{
				// no Xml instance so we return an empty string
				if (value == null)
					return String.Empty;

				// get strongly type value
				Xml Content = value as Xml;

				return Content.Document.OuterXml;
			}

			// convert to a instance descriptor
			if (destinationType == typeof(InstanceDescriptor))
			{
				// no Xml instance
				if (value == null)
					return null;

				// get strongly type value
				Xml Content = value as Xml;

				// used to describe the constructor
				MemberInfo Member = null;
				object[] Arguments = null;

				// get the constructor of our Xml type
				Member = typeof(Xml).GetConstructor(new Type[] {});

				// the arguments to pass along to the Xml constructor
				Arguments = new object[] {};

				// return the instance descriptor or null if we could not find a constructor
				if (Member != null)
					return new InstanceDescriptor(Member, Arguments);
				else
					return null;
			}

			// call base converter to convert
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	[
    Designer(typeof(DbNetEditControlDesigner)),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxData("<{0}:DbNetEdit runat=\"server\" ConnectionString=\"\" FromPart=\"\"></{0}:DbNetEdit>")
    ]

	////////////////////////////////////////////////////////////////////////////
    public class DbNetEdit : DbNetSuite.UI.GridEdit
	////////////////////////////////////////////////////////////////////////////
    {
        public enum ClientEvents
        {
            onBeforeFilePreview,
            onBeforeFileUploadValidate,
            onBeforeFileUploaded,
            onBeforeInitialized,
            onBeforeRecordDeleted,
            onBeforeRecordValidated,
            onBeforeTinyMceInit,
            onBeforeUserProfileRestored,
            onBeforeUserProfileSaved,
            onColumnMappingConfigured,
            onColumnValidate,
            onDataUploaded,
            onDependentListLoaded,
            onFieldValueChanged,
            onFileUploaded,
            onInitialized,
            onInsertInitialize,
            onPageLoaded,
            onParentFilterAssigned,
            onRecordInserted,
            onRecordInsertError,
            onRecordSelected,
            onRecordUpdated,
            onRecordUpdateError,
            onRecordDeleted,
            onRecordDeleteError,
            onRecordValidate,
            onSearchDialogApply,
            onSearchPanelInitialized,
            onToolbarConfigured,
            onUniqueConstraintViolated,
            onValidationFailed
        };

		[
		Browsable(false)
		]
		public string DefaultTemplate =
				"<table>" + Environment.NewLine +
				"\t<tr>" + Environment.NewLine +
				"\t\t<td colspan=\"2\">" + Environment.NewLine +
				"\t\t\t<table>" + Environment.NewLine +
				"\t\t\t\t<tr>" + Environment.NewLine +
				"\t\t\t\t\t<td id=\"editNavigation\"></td>" + Environment.NewLine +
				"\t\t\t\t\t<td id=\"toolbar\"></td>" + Environment.NewLine +
				"\t\t\t\t</tr>" + Environment.NewLine +
				"\t\t\t</table>" + Environment.NewLine +
				"\t\t</td>" + Environment.NewLine +
				"\t</tr>" + Environment.NewLine +
				"\t<tr>" + Environment.NewLine +
				"\t\t<td>Label 1</td>" + Environment.NewLine +
				"\t\t<td><input class=\"dbnetedit\" id=\"column_name_1\"></input></td>" + Environment.NewLine +
				"\t</tr>" + Environment.NewLine +
				"\t<tr>" + Environment.NewLine +
				"\t\t<td>Label 2</td>" + Environment.NewLine +
				"\t\t<td><input class=\"dbnetedit\" id=\"column_name_2\"></input></td>" + Environment.NewLine +
				"\t</tr>" + Environment.NewLine +
				"\t<tr>" + Environment.NewLine +
				"\t\t<td colspan=\"2\">" + Environment.NewLine +
				"\t\t\t<table>" + Environment.NewLine +
				"\t\t\t\t<tr>" + Environment.NewLine +
				"\t\t\t\t\t<td id=\"navigatorStatus\"></td>" + Environment.NewLine +
				"\t\t\t\t\t<td id=\"messageLine\"></td>" + Environment.NewLine +
				"\t\t\t\t</tr>" + Environment.NewLine +
				"\t\t\t</table>" + Environment.NewLine +
				"\t\t</td>" + Environment.NewLine +
				"\t</tr>" + Environment.NewLine +
				"</table>"  + Environment.NewLine;


        private Xml _FormTemplate = null;
        [
        Category("Content"),
        Editor(typeof(DbNetEditLayoutEditor), typeof(UITypeEditor)),
        Description("HTML layout for edit control"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////
        public Xml FormTemplate
        ////////////////////////////////////////
        {
            get
            {
                if (_FormTemplate == null)
                    _FormTemplate = new Xml();
                return _FormTemplate;
            }
            set { _FormTemplate = value; }
        }


        private string _FormTemplatePath = "";
        [
        Category("Content"),
        Description("Virtual path to XML template file")
        ]
        ////////////////////////////////////////
        public string FormTemplatePath
        ////////////////////////////////////////
        {
            get { return _FormTemplatePath; }
            set { _FormTemplatePath = value; }
        }

        private string _BrowseDialogHeight = "300";
        [
        Category("Browse"),
        Description("Height of the browse dialog")
        ]
        ////////////////////////////////////////
        public string BrowseDialogHeight
        ////////////////////////////////////////
        {
            get { return _BrowseDialogHeight; }
            set { _BrowseDialogHeight = value; }
        }

        private string _BrowseDialogWidth = "300";
        [
        Category("Browse"),
        Description("Width of the browse dialog")
        ]
        ////////////////////////////////////////
        public string BrowseDialogWidth
        ////////////////////////////////////////
        {
            get { return _BrowseDialogWidth; }
            set { _BrowseDialogWidth = value; }
        }

        ArrayList _EditClientEvents = new ArrayList();
        [
        Category("Events"),
        Description("Client-side events"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(EditClientEventCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList EditClientEvents
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_EditClientEvents == null)
                {
                    _EditClientEvents = new ArrayList();
                }
                return _EditClientEvents;
            }
        }

        private EditColumnCollection _EditColumns = new EditColumnCollection();
        [
        Category("Display"),
        Description("Grid Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        Editor(typeof(EditColumnCollectionEditor), typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]

        ////////////////////////////////////////////////////////////////////////////
        public EditColumnCollection EditColumns
        ////////////////////////////////////////////////////////////////////////////
        {
            get
            {
                if (_EditColumns == null)
                {
                    _EditColumns = new EditColumnCollection();
                }
                return _EditColumns;
            }
        }

        private bool _InsertOnly = false;
        [
        Category("Insert"),
        DefaultValue(false),
        Description("When set to true the control will be automatically put into insert mode and will remain so after a record is inserted into the datanase")
        ]
        ////////////////////////////////////////
        public bool InsertOnly
        ////////////////////////////////////////
        {
            get { return _InsertOnly; }
            set { _InsertOnly = value; }
        }

        private int _LayoutColumns = 1;
        [
        CategoryAttribute("Layout"),
        DefaultValue(1),
        Description("Defines the number of columns over which the generated DbNetEdit layout is distributed.")
        ]
        ////////////////////////////////////////
        public int LayoutColumns
        ////////////////////////////////////////
        {
            get { return _LayoutColumns; }
            set { _LayoutColumns = value; }
        }

        public delegate void FormTemplateParsedDelegate(object sender, EventArgs args);
        public event FormTemplateParsedDelegate FormTemplateParsed;


        #region Legacy Properties

        [Obsolete("This method will be removed from future versions")]
        public string TableName = "";
        [Obsolete("This method will be removed from future versions")]
        public string PrimaryKeyName = "";
        [Obsolete("This method will be removed from future versions")]
        public string BrowseContainer = "";
        [Obsolete("This method will be removed from future versions")]
        internal string ForeignKeyColumn = "";
        [Obsolete("This method will be removed from future versions")]
        internal string RowValidation = "";
        [Obsolete("This method will be removed from future versions")]
        internal string OnEditApply = "";
     
        [Obsolete("This method will be removed from future versions")]
        public GridEditControl.SearchDialogModes SearchType = GridEditControl.SearchDialogModes.Standard;

        [Obsolete("This method will be removed from future versions")]
        public string SearchFields = "";
        [Obsolete("This method will be removed from future versions")]
        public string SearchLabels = "";
        [Obsolete("This method will be removed from future versions")]
        public string BrowseColumns = "";
        [Obsolete("This method will be removed from future versions")]
        public string BrowseTitles = "";

        private ArrayList _EditFieldProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList EditFieldProperties
        {
            get { if (_EditFieldProperties == null) { _EditFieldProperties = new ArrayList(); } return _EditFieldProperties; }
        }

        private ArrayList _SearchFieldProperties = new ArrayList();
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ArrayList SearchFieldProperties
        {
            get { if (_SearchFieldProperties == null) { _SearchFieldProperties = new ArrayList(); } return _SearchFieldProperties; }
        }

        private ArrayList _DetailForms;
        [
        Obsolete("This method will be removed from future versions"),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        ////////////////////////////////////////////////////////////////////////////
        public ArrayList DetailForms
        ////////////////////////////////////////////////////////////////////////////
        {
            get { if (_DetailForms == null) { _DetailForms = new ArrayList(); } return _DetailForms; }
        }


        ////////////////////////////////////////////////////////////////////////////
        // End of legacy Properties
        ////////////////////////////////////////////////////////////////////////////
        #endregion


        private DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions _ToolbarLocation = DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions.Bottom;
        [
        Category("Toolbar"),
        DefaultValue(DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions.Bottom),
        Description("Controls the location of the toolbar")
        ]
        ////////////////////////////////////////////////////////////////////////////
        public DbNetLink.DbNetSuite.DbNetGrid.ToolbarOptions ToolbarLocation
        ////////////////////////////////////////////////////////////////////////////
        {
            get { return _ToolbarLocation; }
            set { _ToolbarLocation = value; }
        }

		////////////////////////////////////////////////////////////////////////////
        public DbNetEdit()
		////////////////////////////////////////////////////////////////////////////
        {
        }


		////////////////////////////////////////////////////////////////////////////
		protected override void RenderContents(HtmlTextWriter Writer) 
		////////////////////////////////////////////////////////////////////////////
		{
			Writer.Write( RenderFormTemplate() );
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
        #region Convert Legacy Code
        ////////////////////////////////////////////////////////////////////////////
        protected void ConvertLegacyCode()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.TableName != "")
                this.FromPart = this.TableName;

            this.ConvertTemplate();

            if (this.SearchFields != "")
                ConfigLegacyColumns(this.SearchFields, this.SearchLabels, "Search");
            if (this.BrowseColumns != "")
                ConfigLegacyColumns(this.BrowseColumns, this.BrowseTitles, "Browse");

            if (this.TableName != "")
                this.FromPart = this.TableName;

            if (this.PrimaryKeyName != "")
                foreach (string PK in this.PrimaryKeyName.Split(','))
                    SetColumnProperty(PK.Trim(), "PrimaryKey", true);

            if (this.SearchType != GridEditControl.SearchDialogModes.Standard)
                this.SearchDialogMode = this.SearchType;

            this.ConvertColumnProperties(this.EditFieldProperties);

            if (this.ForeignKeyColumn != "")
                SetColumnProperty(this.ForeignKeyColumn, "ForeignKey", true);

            foreach (object O in this.DetailForms)
            {
                if (!(O is DetailForm))
                    continue;
                DetailForm DG = O as DetailForm;
                LinkedControl LC = new LinkedControl();
                LC.LinkedControlID = DG.FormID;

                this.LinkedControls.Add(LC);

                object Ctrl = DbNetFindControl(DG.FormID);

                if (Ctrl is DbNetEdit)
                {
                    DbNetEdit Edit = (Ctrl as DbNetEdit);
                    if (Edit.EditColumns.Count > 0)
                        Edit.SetColumnProperty(DG.ForeignKeyColumn, "ForeignKey", true);
                    else
                        Edit.ForeignKeyColumn = DG.ForeignKeyColumn;
                }
            }
            if (this.RowValidation != "")
                AddLegacyEventHandler(ClientEvents.onRecordValidate, this.RowValidation);
            if (this.OnEditApply != "")
            {
                AddLegacyEventHandler(ClientEvents.onRecordInserted, this.OnEditApply);
                AddLegacyEventHandler(ClientEvents.onRecordUpdated, this.OnEditApply);
            }
            /*
            if (this.OnPageLoaded != "")
                AddLegacyEventHandler(ClientEvents.onPageLoaded, this.OnPageLoaded);
            if (this.OnRowSelected != "")
                AddLegacyEventHandler(ClientEvents.onRowSelected, this.OnRowSelected);
            */

        }

        ////////////////////////////////////////////////////////////////////////////
        protected void AddLegacyEventHandler(ClientEvents EventType, string Handler)
        ////////////////////////////////////////////////////////////////////////////
        {
            EditClientEvent CE = new EditClientEvent();
            CE.EventName = EventType;
            CE.Handler = Handler;
            this.EditClientEvents.Add(CE);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void ConfigLegacyColumns(string _Columns, string _Labels, string ColumnType)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (ColumnType != "")
                ResetProperty(ColumnType);

            string[] Columns = _Columns.Split(',');
            string[] Labels = _Labels.Split(',');

            for (int I = 0; I < Columns.Length; I++)
            {
                EditColumn GC = this.FindColumn(Columns[I].Trim(), true);
                if (I < Labels.Length)
                    GC.Label = Labels[I].Trim();

                switch (ColumnType)
                {
                    case "Edit":
                    case "Search":
                    case "Browse":
                        SetColumnProperty(Columns[I].Trim(), ColumnType, true);
                        break;
                    default:
                        SetColumnProperty(Columns[I].Trim(), "Display", true);
                        SetColumnProperty(Columns[I].Trim(), "Search", true);
                        break;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void SetColumnProperty(string ColumnExpression, string PropertyName, object PropertyValue)
        ////////////////////////////////////////////////////////////////////////////
        {
            EditColumn GC = this.FindColumn(ColumnExpression, true);
            SetPropertyValue(GC, PropertyName, PropertyValue);
        }

        ////////////////////////////////////////////////////////////////////////////
        public EditColumn FindColumn(string ColumnName)
        ////////////////////////////////////////////////////////////////////////////
        {
            return FindColumn(ColumnName, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        public EditColumn FindColumn(string ColumnName, bool AddColumn)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.EditColumns == null)
                return null;

            EditColumn GC;

            foreach (Object O in this.EditColumns)
            {
                if (O is EditColumn)
                {
                    GC = (EditColumn)O;

                    if (GC.ColumnExpression.ToLower() == ColumnName.ToLower())
                        return GC;
                }
            }

            if (AddColumn)
            {
                GC = new EditColumn(ColumnName);
                GC.Search = false;
                EditColumns.Add(GC);

                return GC;
            }
            else
            {
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        protected void ConvertColumnProperties(ArrayList List)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (object O in List)
            {
                if (!(O is FieldProperty))
                    continue;
                FieldProperty CP = O as FieldProperty;

                switch (CP.Property.ToLower())
                {
                    case "validation":
                        AddLegacyEventHandler(ClientEvents.onColumnValidate, CP.Value);
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

        ////////////////////////////////////////////////////////////////////////////
        protected void ResetProperty(string Property)
        ////////////////////////////////////////////////////////////////////////////
        {
            foreach (EditColumn GC in this.EditColumns)
                SetPropertyValue(GC, Property, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        public void ConvertTemplate()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (!this.DesignMode && (FormTemplateContent() != "" || FormTemplatePath != ""))
            {
                XmlDocument Doc = new XmlDocument();

                try
                {
                    if (FormTemplatePath != "")
                        Doc.Load(HttpContext.Current.Request.MapPath(FormTemplatePath));
                    else
                        Doc.LoadXml(FormTemplateContent());
                }
                catch (Exception)
                {
                    return;
                }

                if (Doc.OuterXml != "")
                {
                    XmlNodeList Nodes = Doc.DocumentElement.SelectNodes("//node() | //namespace::*");

                    foreach (XmlNode Node in Nodes)
                    {
                        XmlNode Id = ElementAttribute(Node, "id");
                        XmlNode Class = ElementAttribute(Node, "class");
                        if (Id == null)
                            continue;
                        if (Class != null)
                            if (Class.InnerText == "dbnetedit")
                            {
                                XmlElement E = Doc.CreateElement("span");
                                XmlAttribute A = Doc.CreateAttribute("columnExpression");
                                A.InnerText = Id.InnerText;
                                AddEditColumn(Id.InnerText, null);
                                E.Attributes.Append(A);
                                Node.ParentNode.AppendChild(E);
                                Node.ParentNode.RemoveChild(Node);
                            }
                        if (Id.InnerText == "toolbar")
                            Id.InnerText = this.ID + "_toolbarPanel";
                        if (Id.InnerText == "messageLine")
                            Id.InnerText = this.ID + "_messagePanel";

                        if (this.BrowseContainer != "")
                            if (Id.InnerText == this.BrowseContainer)
                                Id.InnerText = this.ID + "_browsePanel";
                    }
                }
                FormTemplatePath = "";
                FormTemplate.Document = Doc;
            }
        }

        #endregion 

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnPreRender( EventArgs e )
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
            StringBuilder Script = new StringBuilder();
            Script.Append("<script language=\"JavaScript\"" + NonceAttribute+ ">" + Environment.NewLine + "jQuery(document).ready( function() {");

            ProcessFormTemplate();

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
            this.ProcessFormTemplate();
            MVC.Cache(MVC.CacheKeyNames.Html).Append("<div id=\"" + this.ID + "\">" + this.RenderFormTemplate() + "</div>");
            MVC.Cache(MVC.CacheKeyNames.Script).Append(this.GenerateClientScript());
            base.RenderInvocationScript(ParentControl);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected StringBuilder GenerateClientScript()
        ////////////////////////////////////////////////////////////////////////////
        {
            StringBuilder Script = new StringBuilder();

            if (this.BrowseContainer != "")
                Script.Append("jQuery('#" + this.BrowseContainer + "').attr('id','" + this.ClientID + "_browsePanel').css('overflow-y','auto');" + Environment.NewLine);

            Script.Append("var o = new " + ComponentName + "(\"" + this.ClientID + "\");" + Environment.NewLine);

            ArrayList ScriptLines = new ArrayList();

            PropertyInfo[] PI = typeof(DbNetEdit).GetProperties();
            DbNetEdit DefaultEdit = new DbNetEdit();

            foreach (PropertyInfo I in PI)
            {
                if (I.DeclaringType == typeof(System.Web.UI.Control))
                    continue;

                if (I.PropertyType == typeof(DbNetSpell))
                    continue;

                object PropertyValue;

                try
                {
                    if (I.GetValue(DefaultEdit, null) == null)
                    {
                        if (I.GetValue(this, null) == null)
                            continue;
                    }
                    else
                    {
                        if (I.GetValue(this, null).ToString() == I.GetValue(DefaultEdit, null).ToString())
                            continue;
                    }

                    PropertyValue = I.GetValue(this, null);
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

            if (this.DbNetSpell != null)
                DbNetSpellClientScript(ScriptLines, this.DbNetSpell);

            ScriptLines.AddRange(WriteColumnProperties(this));
            ScriptLines.AddRange(WriteParams(this.SearchFilterParams, "searchFilterParams"));
            ScriptLines.AddRange(WriteParams(this.FixedFilterParams, "fixedFilterParams"));
            ScriptLines.AddRange(WriteEditClientEvents(this));

            foreach (string Line in ScriptLines)
                Script.Append("o." + Line + Environment.NewLine);

            Script.Append("window." + ComponentName + "Array[\"" + this.ClientID + "\"] = o;" + Environment.NewLine);
            Script.Append("window.DbNetLink.components[\"" + this.ID + "\"] = o;" + Environment.NewLine);


            return Script;
        }

        ////////////////////////////////////////////////////////////////////////////
        protected ArrayList WriteEditClientEvents(DbNetEdit Edit)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Source = new ArrayList();

            if (Edit.EditClientEvents == null)
                return Source;

            StringBuilder S = new StringBuilder();

            foreach (Object O in Edit.EditClientEvents)
            {
                if (!(O is EditClientEvent))
                    continue;

                EditClientEvent E = (EditClientEvent)O;

                Source.Add("bind(\"" + E.EventName.ToString() + "\",\"" + E.Handler + "\");");
            }

            return Source;
        }


        ////////////////////////////////////////////////////////////////////////////
        public string FormTemplateContent()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (FormTemplate.Document != null)
                return FormTemplate.Document.OuterXml;
            else
                return "";
        }

        ////////////////////////////////////////////////////////////////////////////
        public void ProcessFormTemplate()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (!this.DesignMode && (FormTemplateContent() != "" || FormTemplatePath != ""))
            {
                XmlDocument Doc = new XmlDocument();

                try
                {
                    if (FormTemplatePath != "")
                        Doc.Load(HttpContext.Current.Request.MapPath(FormTemplatePath));
                    else
                        Doc.LoadXml(FormTemplateContent());
                }
                catch (Exception)
                {
                    return;
                }

                if (Doc.OuterXml != "")
                {
                    XmlNode Id = ElementAttribute(Doc.DocumentElement, "Id");

                    if (Id != null)
                        Id.InnerText = this.ClientID;
                    else
                    {
                        XmlAttribute IdAttr = Doc.CreateAttribute("id");
                        IdAttr.InnerText = this.ClientID;
                        Doc.DocumentElement.Attributes.Append(IdAttr);
                    }

                    XmlNodeList Nodes = Doc.DocumentElement.SelectNodes("//node() | //namespace::*");

                    foreach (XmlNode Node in Nodes)
                    {
                        XmlNode ColumnExpression = ElementAttribute(Node, "ColumnExpression");
                        if (ColumnExpression != null)
                        {
                            AddEditColumn(ColumnExpression.InnerText, ElementAttribute(Node, "Label"));
                            ColumnExpression.InnerText = DbNetLink.Util.Encrypt(ColumnExpression.InnerText);
                        }

                        Id = ElementAttribute(Node, "id");

                        if (Id != null)
                            if (Id.InnerText.StartsWith(this.ID + "_"))
                                Id.InnerText = Id.InnerText.Replace(this.ID + "_", this.ClientID + "_");

                    }
                }
                FormTemplatePath = "";
                FormTemplate.Document = Doc;

                if (FormTemplateParsed != null)
                    FormTemplateParsed(this, new EventArgs());

            }
        }

        ////////////////////////////////////////////////////////////////////////////
        public void AddEditColumn(string ColumnExpression, XmlNode Label)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this.FindColumn(ColumnExpression) != null)
                return;

            EditColumn EC = new EditColumn(ColumnExpression);
            if (Label != null)
                EC.Label = Label.InnerText;
            this.EditColumns.Add(EC);
        }

        ////////////////////////////////////////////////////////////////////////////
        public StringBuilder RenderFormTemplate()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (!this.DesignMode && (FormTemplateContent() != "" || FormTemplatePath != ""))
            {
                XmlDocument Doc = new XmlDocument();

                try
                {
                    if (FormTemplatePath != "")
                        Doc.Load(HttpContext.Current.Request.MapPath(FormTemplatePath));
                    else
                        Doc.LoadXml(FormTemplateContent());
                }
                catch (Exception Ex)
                {
                    return new StringBuilder("<div style='padding:10pt;border:1pt solid;background-color:gold'><B>Error Loading Form Template:</B> " + Ex.Message + "</div>");
                }
 
                return new StringBuilder(Doc.OuterXml);

            }
            else
                return new StringBuilder(FormTemplateContent());
        }

        ////////////////////////////////////////////////////////////////////////////
        public XmlNode ElementAttribute(XmlNode Node, string AttrName)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Node.NodeType != XmlNodeType.Element)
                return null;

            foreach (XmlNode N in Node.Attributes)
                if (N.Name.ToUpper() == AttrName.ToUpper())
                    return N;

            return null;
        } 

    }

    ////////////////////////////////////////////////////////////////////////////
    public class EditClientEvent
    ////////////////////////////////////////////////////////////////////////////
    {
        private DbNetEdit.ClientEvents _EventName;
        private string _Handler = "";

        public EditClientEvent()
        {
        }

        public EditClientEvent(DbNetEdit.ClientEvents EventName, string Handler)
        {
            this._EventName = EventName;
            this._Handler = Handler;
        }

        public DbNetEdit.ClientEvents EventName
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


    /////////////////////////////////////////////// 
    public class EditColumn : DbNetLink.DbNetSuite.EditColumn
    ///////////////////////////////////////////////
    {
        ///////////////////////////////////////////////
        public EditColumn()
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public EditColumn(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            this.ColumnExpression = ColumnExpression;
            //this.Label = Shared.GenerateLabel(ColumnExpression);
        }
    }


    ///////////////////////////////////////////////
    public class EditColumnCollection : DbColumnCollection
    ///////////////////////////////////////////////
    {
        public EditColumn this[int index]
        {
            get
            {
                return (EditColumn)this.List[index];
            }
            set
            {
                EditColumn column = (EditColumn)value;
                this.List[index] = column;
            }
        }

        ///////////////////////////////////////////////
        public void Add(EditColumn column)
        ///////////////////////////////////////////////
        {
            base.Add(column);
        }

        ///////////////////////////////////////////////
        public EditColumn Add(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            EditColumn C = new EditColumn(ColumnExpression);
            base.Add(C);
            return C;
        }

        ///////////////////////////////////////////////
        public EditColumn Add(string ColumnExpression, string Label)
        ///////////////////////////////////////////////
        {
            EditColumn C = new EditColumn(ColumnExpression);
            C.Label = Label;
            base.Add(C);
            return C;
        }

        ///////////////////////////////////////////////
        public int IndexOf(EditColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    public class EditColumnCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public EditColumnCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(EditColumn);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    public class EditClientEventCollectionEditor : CollectionEditor
    ////////////////////////////////////////////////////////////////////////////
    {
        public EditClientEventCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(EditClientEvent);
        }
    }

    
	////////////////////////////////////////////////////////////////////////////
	public class DbNetEditLayoutEditor : System.Drawing.Design.UITypeEditor 
	////////////////////////////////////////////////////////////////////////////
	{
		////////////////////////////////////////////////////////////////////////////
		public DbNetEditLayoutEditor() 
		////////////////////////////////////////////////////////////////////////////
		{ 
		}

		////////////////////////////////////////////////////////////////////////////
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value) 
		////////////////////////////////////////////////////////////////////////////
		{
			IWindowsFormsEditorService ies;
			DbNetEditLayoutEditorForm form;

			try 
			{
				ies = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
				if (ies == null)
					throw new Exception("Editor Service not available");

				form = new DbNetEditLayoutEditorForm();
				form.SetContextDetails(context, provider);
				if (value != null) 
				{
					form.TemplateXHTML = (String) value;
				}

				DialogResult diag = ies.ShowDialog(form);
				if (diag == DialogResult.Yes || diag == DialogResult.OK) 
				{
					return form.GetFinalDetails();
				}
			}
			catch(System.Exception ex) 
			{
				MessageBox.Show(ex.ToString(), "Error");
			}
			finally
			{
				form = null;
			}
			
			return value;
		}

		////////////////////////////////////////////////////////////////////////////
		public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) 
		////////////////////////////////////////////////////////////////////////////
		{
			return UITypeEditorEditStyle.Modal;
		}
	}

	////////////////////////////////////////////////////////////////////////////
	public class TemplateItem : System.Web.UI.Control, INamingContainer 
	////////////////////////////////////////////////////////////////////////////
	{
		private String _message	= null;
		public TemplateItem(String message) 
		{
			_message = message;
		}

		public String Message 
		{
			get {return _message;}
			set {_message = value;}
		}
	}


	////////////////////////////////////////////////////////////////////////////
	public class DbNetEditLayoutEditorForm : System.Windows.Forms.Form
	////////////////////////////////////////////////////////////////////////////
	{
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnClose;
		private System.ComponentModel.Container components = null;

		private ITypeDescriptorContext _context;
		private IServiceProvider _service;
		private System.Windows.Forms.Button btnOK;
		public string TemplateXHTML;

		////////////////////////////////////////////////////////////////////////////
		private void btnOK_Click(object sender, System.EventArgs e) 
		////////////////////////////////////////////////////////////////////////////
		{
			if ( textBox1.Text == "" ) 
			{
				MessageBox.Show("Layout HTML must be specified", "Content");
				this.DialogResult = DialogResult.Abort;
			}
			else 
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.LoadXml(textBox1.Text);

				if ( XmlDoc.OuterXml == "" )
				{
					MessageBox.Show("HTML must be valid X/HTML", "Content");
					this.DialogResult = DialogResult.Abort;
				}
				else
				{
					this.DialogResult = DialogResult.OK;
				}
			}
		}
		
		////////////////////////////////////////////////////////////////////////////
		public DbNetEditLayoutEditorForm()
		////////////////////////////////////////////////////////////////////////////
		{
			InitializeComponent();
		}

		////////////////////////////////////////////////////////////////////////////
		private void DbNetEditLayoutEditorForm_Load(object sender, System.EventArgs e) 
		////////////////////////////////////////////////////////////////////////////
		{
			this.textBox1.Text = TemplateXHTML;
		}

		////////////////////////////////////////////////////////////////////////////
		protected override void Dispose( bool disposing )
		////////////////////////////////////////////////////////////////////////////
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		////////////////////////////////////////////////////////////////////////////
		internal string GetFinalDetails() 
		////////////////////////////////////////////////////////////////////////////
		{
			return textBox1.Text;
		}

		////////////////////////////////////////////////////////////////////////////
		internal void SetContextDetails(ITypeDescriptorContext context, IServiceProvider service) 
		////////////////////////////////////////////////////////////////////////////
		{
			_context = context;
			_service = service;
		}

		////////////////////////////////////////////////////////////////////////////
		private void InitializeComponent()
		////////////////////////////////////////////////////////////////////////////
		{
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.textBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width * 0.8), Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height * 0.8));
			this.textBox1.TabIndex = 0;

			this.textBox1.Multiline = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.AcceptsReturn = true;
			this.textBox1.AcceptsTab = true;
			this.textBox1.WordWrap = true;

			// 
			// btnOK
			// 
//			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Location = new System.Drawing.Point(this.textBox1.Size.Width - 120, this.textBox1.Size.Height + 3);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(50, 24);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

			// 
			// btnClose
			// 
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
//			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Location = new System.Drawing.Point(this.textBox1.Size.Width - 60, this.textBox1.Size.Height + 3);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(50, 24);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "Close";

			// 
			// DbNetEditLayoutEditorForm
			// 
			this.ClientSize = new System.Drawing.Size( this.textBox1.Size.Width, this.textBox1.Size.Height + 30 );
			this.ControlBox = false;
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.textBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DbNetEditLayoutEditorForm";
			this.Text = "DbNetEdit Content Layout Editor";
			this.Load += new System.EventHandler(this.DbNetEditLayoutEditorForm_Load);
			this.textBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}

    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class FieldProperty
    ////////////////////////////////////////////////////////////////////////////
    {
        public string ColumnName = "";
        public string Property = "";
        public string Value = "";
    }
    [Obsolete("Deprecated")]
    ////////////////////////////////////////////////////////////////////////////
    public class DetailForm
    ////////////////////////////////////////////////////////////////////////////
    {
        public string FormID = "";
        public string ForeignKeyColumn = "";
    }
}