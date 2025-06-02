
using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;


///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    public class EditField : HtmlTable
    ///////////////////////////////////////////////
    {
        public enum ControlType
        {
            /// <summary>
            /// Control type is automatically assigned based on the underlying database column
            /// </summary>  
            Auto,
            /// <summary>
            /// Single-line text box
            /// </summary>  
            TextBox,
            /// <summary>
            /// Text box with a lookup button that opens a list selection dialog. Requires the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned.
            /// </summary>  
            TextBoxLookup,
            /// <summary>
            /// Text box with a lookup button that opens a list selection dialog that searches data defined by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned.
            /// </summary>  
            TextBoxSearchLookup,
            /// <summary>
            /// Forces a field to act as a boolean type
            /// </summary>  
            CheckBox,
            /// <summary>
            /// Edits HTML content with an WYSIWYG HTML editor
            /// </summary>  
            Html,
            /// <summary>
            /// Previews HTML content with button to open a WYSIWYG HTML editor
            /// </summary>  
            HtmlPreview,
            /// <summary>
            /// Drop-down list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
            /// </summary>  
            DropDownList,
            /// <summary>
            /// Radio button list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
            /// </summary>  
            RadioButtonList,
            /// <summary>
            /// Multi-line list of values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
            /// </summary>  
            ListBox,
            /// <summary>
            /// Displays data in a read-only label
            /// </summary>  
            Label,
            /// <summary>
            /// Multi-line text box
            /// </summary>  
            TextArea,
            /// <summary>
            /// Password field where field contents are obscured
            /// </summary>  
            Password,
            /// <summary>
            /// Binary file upload stored either as a path to a file or in a database BLOB field.
            /// </summary>  
            Upload,
            /// <summary>
            /// Google suggest style lookup searching values specified by the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property.
            /// </summary>  
            SuggestLookup,
            AutoCompleteLookup,
            /// <summary>
            /// Text box with a lookup button that opens a list selection dialog. Requires the <see cref="DbNetLink.DbNetSuiteVS.EditField.Lookup"/> or <see cref="DbNetLink.DbNetSuiteVS.Column.Lookup"/> property to be assigned. Allows selection of multiple values assigned as a comma separated list.
            /// </summary>  
            MultiValueTextBoxLookup,
            Selectmenu
        }

        internal GridEditControl ParentControl;
        internal WebControl InputControl;
        internal string DbNetEditID;
        internal DbColumn Column;
        internal TableRow Row;
        private bool isDataField = false;
        internal ListDictionary GridValue = null;
        internal Image UploadImage;
        internal HtmlGenericControl Updated;
        internal HtmlGenericControl UpdatedBy;
        internal System.Web.UI.WebControls.Image AuditHistory;

         ///////////////////////////////////////////////
        internal EditField( GridEditControl ParentControl, DbColumn Column, TableRow Row )
        ///////////////////////////////////////////////
        {
            this.ParentControl = ParentControl;
            this.Column = Column;
            this.Row = Row;

            if (Column.EditControlType == ControlType.TextBox)
                if (Column.Lookup != "")
                    Column.EditControlType = ControlType.TextBoxLookup;

            if (Column.EditControlType == ControlType.Auto)
            {
                Column.EditControlType = ControlType.TextBox;
                if (Column.Lookup != "")
                    Column.EditControlType = ControlType.DropDownList;
                if (Column.UploadRootFolder != "")
                    Column.EditControlType = ControlType.Upload;

                switch (Column.DataType)
                {
                    case "Boolean":
                        Column.EditControlType = ControlType.CheckBox;
                        break;
                    case "Byte[]":
                        Column.EditControlType = ControlType.Upload;
                        break;
                    default:
                        if (Column.ColumnSize > 255)
                            Column.EditControlType = ControlType.TextArea;
                        break;
                }
            }
        }

        ///////////////////////////////////////////////
        internal void Build()
        ///////////////////////////////////////////////
        {
            switch (Column.EditControlType)
            {
                case ControlType.CheckBox:
                    InputControl = new CheckBox();
                    InputControl.Style.Add(HtmlTextWriterStyle.Margin, "0px");
                    InputControl.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                    InputControl.Style.Add(HtmlTextWriterStyle.Height, "14px");
                    break;
                case ControlType.HtmlPreview:
                    InputControl = new Panel();
                    break;
                case ControlType.DropDownList:
                case ControlType.Selectmenu:
                    InputControl = new DropDownList();
                    break;
                case ControlType.RadioButtonList:
                    InputControl = new RadioButtonList();
                    break;
                case ControlType.ListBox:
                    InputControl = new ListBox();
                    break;
                default:
                    InputControl = new TextBox();
                    break;
            }

            this.CellPadding = 0;
            this.CellSpacing = 0;
            this.Attributes.Add("class","edit-field-table");

            HtmlTableRow R = new HtmlTableRow();
            HtmlTableCell C = new HtmlTableCell();

            this.Controls.Add(R);
            R.Controls.Add(C);
            C.Controls.Add(InputControl);

            System.Web.UI.AttributeCollection InputAttributes;

            switch (InputControl.GetType().ToString())
            {
                case "System.Web.UI.WebControls.CheckBox":
                    ((CheckBox)InputControl).InputAttributes.Add("editFieldType", "CheckBox");
                    InputAttributes = ((CheckBox)InputControl).InputAttributes;
                    InputAttributes.Add( "class", ParentControl.GetType().Name.ToLower() + "-input");
                    this.Column.IsBoolean = true;
                    break;
                default:
                    InputControl.Attributes.Add("editFieldType", Column.EditControlType.ToString());
                    InputControl.CssClass = ParentControl.GetType().Name.ToLower() + "-input";
                    InputAttributes = InputControl.Attributes;
                    break;
            }

            foreach (string Key in Column.EditControlProperties.Keys )
                this.ParentControl.SetProperty(InputControl, Key, Column.EditControlProperties[Key]);

            InputControl.ToolTip = Column.ToolTip;

            if (Column is EditColumn)
                if ((Column as EditColumn).TabIndex != 0)
                    InputControl.TabIndex = (Column as EditColumn).TabIndex;

            InputAttributes.Add("updateReadOnly", Column.UpdateReadOnly.ToString().ToLower());
            InputAttributes.Add("insertReadOnly", Column.InsertReadOnly.ToString().ToLower());
            InputAttributes.Add("required", Column.Required.ToString().ToLower());
            InputAttributes.Add("mandatory", Column.Required.ToString().ToLower());
            InputAttributes.Add("unique", Column.Unique.ToString().ToLower());
            InputAttributes.Add("initialValue", Column.InitialValue);
            InputAttributes.Add("spellCheck", Column.SpellCheck.ToString().ToLower());
            InputAttributes.Add("columnIndex", Column.ColumnIndex.ToString());
            InputAttributes.Add("columnName", Column.ColumnName);
            InputAttributes.Add("columnkey", Column.ColumnKey);
            InputAttributes.Add("dataType", Column.DataType);
            InputAttributes.Add("format", Column.Format);
            InputAttributes.Add("label", Column.Label);

            if ( Column.UploadFileNameColumn != String.Empty)
                InputAttributes.Add("uploadFileNameColumn", Column.UploadFileNameColumn);
            //InputAttributes.Add("encryption", Column.Encryption.ToString());

            if (Column.PlaceHolder != "")
                InputAttributes.Add("placeholder", Column.PlaceHolder);

            InputControl.ID = ParentControl.Id + "_" + Column.ColumnName.Replace(" ", "") + "_EF";

            if (this.Row != null)
                InputControl.ID += "_" + this.Row.Attributes["dataRowIndex"];

            InputControl.Enabled = (!Column.UpdateReadOnly);

            if (Column.EditControlType == ControlType.Upload)
            {
                // Column contains blob or file system data so add a container where the thumbnail
                // image will be displayed.

                isDataField = true;

                if (Column.DataType == "Byte[]")
                    ((HtmlTableCell)InputControl.Parent).Style.Add(HtmlTextWriterStyle.Display, "none");

                Image I = new Image();
                I.ID = "thumbnail" + Column.ColumnIndex;
                I.CssClass = "thumbnail-image";

                this.UploadImage = I;

                if (ParentControl is DbNetEdit)
                    I.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");

                Panel P = new Panel();
                C = new HtmlTableCell();
                C.Controls.Add(P);
                R.Controls.Add(C);
                P.Controls.Add(I);
                P.Attributes.Add("columnName", Column.ColumnName);
                P.CssClass = "thumbnail-panel";

                /*
                CheckBox CB = new CheckBox();
                CB.Style.Add(HtmlTextWriterStyle.Display, "none");
                C = new TableCell();
                C.Controls.Add(CB);
                R.Controls.Add(C);
                CB.CssClass = "file-modified";
                */

                if (!Column.ReadOnly)
                {
                    R.Controls.Add(MakeButton("image_add", "UploadDocument", "imageAdd"));
                    R.Controls.Add(MakeButton("image_delete", "DeleteDocument", "imageDelete"));
                    InputAttributes.Add("queryString", ParentControl.Context.Request.Url.AbsolutePath + "?mode=thumbnail&controlid=" + this.ParentControl.UniqueID + "&column=" + Column.ColumnIndex.ToString());
                }
            }

            if (InputControl.GetType() == typeof(TextBox))
            {

                TextBox tb = (TextBox)InputControl;

                switch (Column.EditControlType)
                {
                    case ControlType.TextArea:
                    case ControlType.Html:
                        tb.TextMode = TextBoxMode.MultiLine;
                        tb.Rows = 4;
                        tb.Columns = 40;
                        break;
                    case ControlType.Password:
                        tb.TextMode = TextBoxMode.Password;
                        SetTextBoxFieldSize();
                        break;
                    case ControlType.TextBoxLookup:
                    case ControlType.TextBoxSearchLookup:
                        if (LookupColumnCount(Column.ColumnKey) == 2)
                        {
                            InputControl.Attributes.Add("deleteToClear", "true");
                            if (InputControl.ToolTip == String.Empty)
                                InputControl.ToolTip = "Press Delete to clear";
                            tb.ReadOnly = true;
                        }
                        InputControl.Attributes.Add("size", "40");
                        break;
                    case ControlType.SuggestLookup:
                    case ControlType.AutoCompleteLookup:
                        InputControl.Attributes.Add("size", "40");
                        InputControl.Attributes.Add("LookupColumnCount", LookupColumnCount(Column.ColumnKey).ToString());
                        break;
                    default:
                        SetTextBoxFieldSize();
                        break;
                }

                if (Column.DataType == "String")
                    tb.Attributes.Add("maxlength", Column.ColumnSize.ToString());
            }

            
            // editButtonContainer contains all the "edit buttons".  This container can be hidden
            // as necessary if a field is marked as UpdateReadOnly.
        //    Label editButtonContainer = new Label();
        //    editButtonContainer.EnableViewState = false;
        //    Controls.Add(editButtonContainer);
        //    editButtonContainer.ID = "editButtonContainer";
        //    correctAttributesCollection.Add("editButtonContainerID", editButtonContainer.ClientID);

            if (InputControl.GetType() == typeof(TextBox))
            {

                if (Column.ColumnSize > 255 && !Column.ReadOnly && !isDataField && Column.EditControlType == ControlType.TextArea)
                {
                    R.Controls.Add( MakeButton( "edit", "OpenEditorWindow", "textEdit" ) );
                }              
                
                if (Column.DataType == "DateTime" && !Column.ReadOnly && !isDataField)
                {
                    R.Controls.Add(MakeButton("calendar", "SelectDate", "calendar"));
                }

                switch (Column.EditControlType)
                {
                    case ControlType.SuggestLookup:
                    case ControlType.AutoCompleteLookup:
                        break;
                    case ControlType.TextBoxLookup:
                    case ControlType.TextBoxSearchLookup:
                        if (!Column.Lookup.Equals("") && !Column.ReadOnly && !isDataField)
                            R.Controls.Add(MakeButton(LookupButton()));
                        break;
                    default:
                        if (!Column.Lookup.Equals("") && !Column.ReadOnly && !isDataField)
                        {
                            InputControl.Attributes.Add("disabled", "true");
                            R.Controls.Add(MakeButton(LookupButton()));
                        }
                        break;
                }
            }

            switch (Column.EditControlType)
            {
                case ControlType.Html:
                case ControlType.HtmlPreview:
                    InputControl.CssClass = "html-preview";

                    TextBox TB = new TextBox();
                    TB.Style.Add(HtmlTextWriterStyle.Display, "none");
                    C = new HtmlTableCell();
                    C.Controls.Add(TB);
                    R.Controls.Add(C);
                    TB.CssClass = "html-content";

                    if (!Column.ReadOnly)
                    {
                        R.Controls.Add(MakeButton("edit", "OpenEditorWindow", "htmlEdit"));
                    }
                    break;
            }

            PopulateDropDownLookupValues();
            AssignValue();

            foreach (string CSS in Column.Style.Split(';'))
                if (CSS.Split(':').Length == 2)
                    InputControl.Style.Add(CSS.Split(':')[0], CSS.Split(':')[1]);

            if (InputControl.GetType() == typeof(TextBox))
            {
                string DT;
                if (Column.Lookup.Equals(string.Empty))
                    DT = Column.DataType;
                else
                    DT = Column.LookupDataType;

                switch (DT.ToString())
                {
                    case "Byte":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                    case "Single":
                    case "Double":
                        InputControl.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                        break;
                }
            }

            if (this.Column.Audit != GridEditControl.AuditModes.None)
                AddAuditFields();
        }


        ///////////////////////////////////////////////
        internal void AddAuditFields()
        ///////////////////////////////////////////////
        {
            HtmlTableCell C = new HtmlTableCell();
            this.Rows[0].Cells.Add(C);
            HtmlGenericControl I = new HtmlGenericControl("div");
            C.Controls.Add(I);
            I.Attributes.Add("class", "updated-by-audit");
            I.Style.Add(HtmlTextWriterStyle.Display, "none");
            this.UpdatedBy = I;

            C = new HtmlTableCell();
            this.Rows[0].Cells.Add(C);
            I = new HtmlGenericControl("div");
            I.Attributes.Add("class", "updated-audit");
            I.Style.Add(HtmlTextWriterStyle.Display, "none");
            this.Updated = I;

            C.Controls.Add(I);

            if (this.Column.Audit == GridEditControl.AuditModes.Detail)
            {
                C = new HtmlTableCell();
                C.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                System.Web.UI.WebControls.Image Img = new System.Web.UI.WebControls.Image();
                Img.ImageUrl = ParentControl.GetImageUrl("zoom.png");
                Img.Attributes.Add("class", "audit-history");
                Img.ToolTip = "View audit history";
                Img.Style.Add(HtmlTextWriterStyle.Display, "none");
                C.Controls.Add(Img);
                this.AuditHistory = Img;
                this.Rows[0].Cells.Add(C);
            }
        }

        ///////////////////////////////////////////////
        internal int LookupColumnCount(string ColumnKey)
        ///////////////////////////////////////////////
        {
            int ColumnCount = 0;

            if (ParentControl.LookupTables.ContainsKey(ColumnKey))
            {
                DataTable DT = ParentControl.LookupTables[ColumnKey];
                ColumnCount = DT.Columns.Count;
            }

            return ColumnCount;
        }

        ///////////////////////////////////////////////
        internal HtmlTableCell MakeButton(string ImgName, string Title, string ButtonType)
        ///////////////////////////////////////////////
        {
            //string ImgUrl = ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName + ".png");
            DbNetButton B = new DbNetButton(ImgName, "", ParentControl.Translate(Title), ParentControl.Context.Request, GridEditControl.ToolButtonStyles.Image, ParentControl.PageRef);

            if (ParentControl.GetTheme() == UI.Themes.Bootstrap)
                B.Attributes.Add("class", "toolButton " + ButtonType + "-button btn");
            else
                B.Attributes.Add("class", "toolButton " + ButtonType + "-button");

            B.Attributes.Add("inputButtonType", ButtonType);

            B.Attributes.Add("tabIndex", InputControl.TabIndex.ToString());
            return MakeButton(B);

        }

        ///////////////////////////////////////////////
        internal HtmlTableCell MakeButton(DbNetButton B)
        ///////////////////////////////////////////////
        {
            HtmlTableCell C = new HtmlTableCell();
            if (this.ParentControl is DbNetEdit)
                C.Style.Add(HtmlTextWriterStyle.VerticalAlign, "bottom");
            C.Controls.Add(B);
            return C;
        }

        ///////////////////////////////////////////////
        internal void AssignValue()
        ///////////////////////////////////////////////
        {
            if (this.GridValue == null)
                return;

            if (this.GridValue["displayValue"] == System.DBNull.Value)
                this.GridValue["displayValue"] = "";
            if (this.GridValue["value"] == System.DBNull.Value)
                this.GridValue["value"] = "";

            string DisplayValue = this.GridValue["displayValue"].ToString();
            string Value = this.GridValue["value"].ToString();

            if (Column.DataType == "Boolean")
            {
                DisplayValue = DisplayValue.ToLower();
                Value = Value.ToLower();
            }

            if (this.GridValue["options"] is ListControl)
            {
                ListItemCollection ListItems = (this.GridValue["options"] as ListControl).Items;
                if (this.InputControl is ListControl)
                {
                    (this.InputControl as ListControl).Items.Clear();
                    foreach (ListItem LI in ListItems)
                        (this.InputControl as ListControl).Items.Add(LI);
                }
            }

            switch (Column.EditControlType)
            {
                case ControlType.CheckBox:
                    bool boolValue = false;
                    try
                    {
                        boolValue = (DisplayValue == "") ? false : Convert.ToBoolean(DisplayValue);
                    }
                    catch (Exception)
                    {
                        boolValue = (DisplayValue == "") ? false : Convert.ToBoolean(Convert.ToInt32(DisplayValue));
                    }
                    (InputControl as CheckBox).Checked = boolValue;
                    (InputControl as CheckBox).InputAttributes.Add("fieldValue", (InputControl as CheckBox).Checked.ToString().ToLower());
                    break;
                case ControlType.RadioButtonList:
                case ControlType.DropDownList:
                case ControlType.ListBox:
                    (InputControl as ListControl).SelectedValue = Value.TrimEnd();
                    InputControl.Attributes.Add("fieldValue", Value);
                    break;
                case ControlType.Upload:
                    if (Value != "")
                        this.UploadImage.ImageUrl = DisplayValue;
                    else
                        this.UploadImage.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    (InputControl as TextBox).Text = Value;
                    InputControl.Attributes.Add("fieldValue", Value);
                    break;
                case ControlType.HtmlPreview:
                case ControlType.Html:
                    HtmlTableRow R = InputControl.Parent.Parent as HtmlTableRow;
                    TextBox TB = R.Cells[R.Cells.Count - 2].Controls[0] as TextBox;
                    TB.Text = Value;
                    break;
                case ControlType.TextBoxLookup:
                case ControlType.TextBoxSearchLookup:
                case ControlType.SuggestLookup:
                case ControlType.AutoCompleteLookup:
                    (InputControl as TextBox).Text = DisplayValue;
                    InputControl.Attributes.Add("fieldValue", Value);
                    InputControl.Attributes.Add("dbValue", Value);
                    InputControl.Attributes.Add("displayValue", DisplayValue);
                    break;
                default:
                    InputControl.Attributes.Add("fieldValue", DisplayValue);
                    if (InputControl is TextBox)
                        (InputControl as TextBox).Text = DisplayValue;
                    break;
            }
        }

        ///////////////////////////////////////////////
        internal void AssignAuditInfo(ListDictionary AuditInfo)
        ///////////////////////////////////////////////
        {
            if (AuditInfo == null || this.Column.Audit == GridEditControl.AuditModes.None)
                return;

            if (!AuditInfo.Contains(Column.ColumnName))
                return;

            ListDictionary AuditData =  (ListDictionary)AuditInfo[Column.ColumnName];

            if (AuditData["updated"].ToString() != "")
            {
                Updated.InnerText = AuditData["updated"].ToString();
                Updated.Style.Add(HtmlTextWriterStyle.Display, "block");
            }

            if (AuditData["updated_by"].ToString() != "")
            {
                UpdatedBy.InnerText = AuditData["updated_by"].ToString();
                UpdatedBy.Style.Add(HtmlTextWriterStyle.Display, "block");
            }

            if (this.Column.Audit == GridEditControl.AuditModes.Detail)
                if (AuditData["updated"].ToString() != "" || AuditData["updated_by"].ToString() != "")
                    AuditHistory.Style.Add(HtmlTextWriterStyle.Display, "block");
        }

        ///////////////////////////////////////////////
        internal DbNetButton LookupButton()
        ///////////////////////////////////////////////
        {
            string ImageUrl = ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images.lookup.png");
            DbNetButton B = new DbNetButton(ImageUrl, "", this.ParentControl.Translate("OpenTheLookupWindow"), ParentControl.Context.Request, ParentControl.PageRef);

            if (ParentControl.GetTheme() == UI.Themes.Bootstrap)
            {
                B.Attributes.Add("class", "lookup-button btn");
                B.Style.Add(HtmlTextWriterStyle.Height, "30px");
                B.Style.Add(HtmlTextWriterStyle.Padding, "4px");
            }
            else
            {
                B.Attributes.Add("class", "lookup-button");
                B.Style.Add(HtmlTextWriterStyle.Height, "24px");
            }

             B.Attributes.Add("EditControlType", this.GetType().Name);
            return B;
        }

        ///////////////////////////////////////////////
        internal void PopulateDropDownLookupValues()
        ///////////////////////////////////////////////
        {
            if (Column.Lookup.Equals(""))
            {
                if (Column.DataType == "Boolean" && InputControl is ListControl)
                {
                    if (!Column.Required)
                        ((ListControl)InputControl).Items.Add(String.Empty);
                    ((ListControl)InputControl).Items.Add(new ListItem(this.ParentControl.Translate("No"), "false"));
                    ((ListControl)InputControl).Items.Add(new ListItem(this.ParentControl.Translate("Yes"), "true"));
                }

                return;
            }

            if (!Column.Lookup.StartsWith("["))
            {
                ListDictionary Params = this.ParentControl.Database.ParseParameters(Column.Lookup);

                if (Params.Count > 0)
                    foreach (string key in Params.Keys)
                    {
                        InputControl.Attributes.Add("parentColumn", key);
                        break;
                    }
            }

            switch( InputControl.GetType().Name)
            {
                case "DropDownList":
                case "ListBox":
                case "RadioButtonList":
                    break;
                default:
                    return;
            }

            if ( !Column.Required )
                ((ListControl)InputControl).Items.Add(new ListItem(ParentControl.EmptyOptionText, String.Empty));

            if (ParentControl.LookupTables.ContainsKey(Column.ColumnKey))
            {
                DataTable lookupTable = ParentControl.LookupTables[Column.ColumnKey];
                int TextIndex = (lookupTable.Columns.Count == 1) ? 0 : 1;
                foreach (DataRow row in lookupTable.Rows)
                {
                    ListItem LI = new ListItem(Convert.ToString(row[TextIndex]), Convert.ToString(row[0]));

                    for (int I = TextIndex + 1; I < row.Table.Columns.Count; I++)
                        LI.Attributes.Add(row.Table.Columns[I].ColumnName.ToLower(), Convert.ToString(row[I]));

                    ((ListControl)InputControl).Items.Add(LI);
                }
            }

            if (InputControl.GetType().Name == "DropDownList" && Column.Required)
                ((ListControl)InputControl).Items.Add(new ListItem(ParentControl.EmptyOptionText, ""));

        }


        ///////////////////////////////////////////////
        private void SetTextBoxFieldSize()
        ///////////////////////////////////////////////
        {          
            
            string size = "";

            switch (Column.DataType)
            {
                case "Byte":
                    size = "4";
                    break;
                case "Int16":
                case "Int32":  
                case "Int64":
                case "Decimal":
                case "Single":
                case "Double":
                    size = "10";
                    break;
                case "DateTime":
                    switch (Column.Format)
                    {
                        case "d":
                        case "m":
                        case "t":
                        case "T":
                            size = "11";
                            break;
                        case "g":
                        case "G":
                        case "s":
                        case "u":
                        case "y":
                            size = "21";
                            break;
                        case "D":
                        case "f":
                        case "F":
                        case "r":
                        case "U":
                            size = "30";
                            break;
                        default:
                            size = "12";
                            break;
                    }
                    break;
                case "String":
                    size = (Column.ColumnSize < 100) ? Column.ColumnSize.ToString() : "100";
                    break;
                case "Guid":
                    size = "40";
                    break;
                default:
                    size = "15";
                    break;
            }

            if (Convert.ToInt32(size) < 251)
                InputControl.Attributes.Add("size", size);

        }
    }
}
