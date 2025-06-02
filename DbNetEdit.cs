using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using DbNetLink.Data;

namespace DbNetLink.DbNetSuite
{
    public class DbNetEdit : GridEditControl
    {
        public string Mode = "";
        public bool InsertOnly = false;
        public int PlaceHolders = 0;
        //       public Dictionary<string, object> PrimaryKey = null;

        [
        CategoryAttribute("Layout"),
        DefaultValue(1),
        Description("Defines the number of columns over which the generated DbNetEdit layout is distributed.")
        ]
        public int LayoutColumns = 1;

        [
        Category("Toolbar"),
        DefaultValue(ToolbarOptions.Bottom),
        Description("Controls the location of the toolbar")
        ]
        public ToolbarOptions ToolbarLocation = ToolbarOptions.Bottom;


        /////////////////////////////////////////////// 
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            switch (Req["method"].ToString())
            {
                case "initialize":
                case "select-record":
                case "get-recordset":
                    this.OpenConnection();
                    break;
            }

            switch (Req["method"].ToString())
            {
                case "initialize":
                    ConfigureColumns();
                    GetLookupTables();
                    if (this.PlaceHolders > 0)
                        BuildInputControls();
                    else
                        BuildInputTable();

                    if (Req.ContainsKey("primaryKeys"))
                    {
                        object[] PrimaryKeys = (object[])Req["primaryKeys"];
                        foreach (object PrimaryKey in PrimaryKeys)
                            this.PrimaryKeyList.Add(PrimaryKey);
                        var primaryKey = (Dictionary<string, object>)PrimaryKeys[0];
                        if (Req.ContainsKey("primaryKey"))
                        {
                            primaryKey = (Dictionary<string, object>)Req["primaryKey"];
                        }
                        this.SelectRecord(primaryKey);
                    }
                    else
                        BuildPrimaryKeyList();

                    if (this.InsertOnly)
                    {
                        this.Search = false;
                        this.InsertRow = false;
                        this.DeleteRow = false;
                        this.Navigation = false;
                        this.PageInfo = false;
                    }

                    BuildToolbar();
                    if (Audit != AuditModes.None)
                        BuildRowAuditPanel();
                    if (SearchPanelId != "")
                        Resp["searchPanel"] = BuildSearchPanel();

                    ClientProperties["columns"] = SerialiseColumns();
                    break;
                case "get-recordset":
                    BuildPrimaryKeyList();
                    break;
                case "select-record":
                    //GetLookupTables();
                    SelectRecord();
                    break;
                case "validate-upload":
                    ValidateUpload();
                    break;
                case "upload":
                case "ajax-upload":
                    Upload();
                    break;
                case "browse-dialog":
                    BuildBrowseDialog();
                    break;
            }

            this.CloseConnection();

            switch (Req["method"].ToString())
            {
                case "thumbnail":
                case "upload":
                    break;
                default:
                    context.Response.Write(JSON.Serialize(Resp));
                    break;
            }
        }

        ///////////////////////////////////////////////
        private void BuildPrimaryKeyList()
        ///////////////////////////////////////////////
        {
            //         if (!this.Navigation || ToolbarLocation == ToolbarOptions.Hidden)
            //             return;

            QueryCommandConfig Query = BuildSQL(QueryBuildModes.PrimaryKeysOnly);

            if (Req.ContainsKey("primaryKey"))
            {
                Dictionary<string, object> PrimaryKey = (Dictionary<string, object>)Req["primaryKey"];
                AddPrimaryKeyFilter(Query, PrimaryKey, true);
            }

            int CurrentPage = (int)Req["currentPage"];
            try
            {
                Database.ExecuteQuery(Query);
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message, Database.CommandErrorInfo());
            }

            while (Database.Reader.Read())
            {
                Dictionary<string, object> PK = new Dictionary<string, object>();

                foreach (DbColumn C in Columns)
                {
                    if (!C.PrimaryKey)
                        continue;

                    if (!PK.ContainsKey(C.ColumnName))
                        PK.Add(C.ColumnName, Database.Reader[C.ColumnName]);
                }

                this.PrimaryKeyList.Add(PK);

                if (Req.ContainsKey("addedPrimaryKey"))
                    if (this.IsMatchingPrimaryKey((Dictionary<string, object>)Req["addedPrimaryKey"]))
                        CurrentPage = this.PrimaryKeyList.Count;

            }
            Database.Reader.Close();

            if (CurrentPage > this.PrimaryKeyList.Count)
                CurrentPage = this.PrimaryKeyList.Count;
            else if (CurrentPage < 1 && this.PrimaryKeyList.Count > 0)
                CurrentPage = 1;

            ClientProperties["currentPage"] = CurrentPage;
            ClientProperties["primaryKeyList"] = this.PrimaryKeyList;
            ClientProperties["sql"] = Query.Sql;
            ClientProperties["params"] = Query.Params;

            if (this.PrimaryKeyList.Count > 0)
                SelectRecord((Dictionary<string, object>)this.PrimaryKeyList[CurrentPage - 1]);
        }


        ///////////////////////////////////////////////
        internal void BuildRowAuditPanel()
        ///////////////////////////////////////////////
        {
            HtmlTable T = new HtmlTable();
            T.ID = AssignID("audit");
            T.Attributes.Add("class", "audit");

            T.Rows.Add(new HtmlTableRow());
            HtmlTableRow R = T.Rows[0];

            HtmlTableCell C = new HtmlTableCell();
            R.Cells.Add(C);
            C.InnerText = "Created";
            C.Attributes.Add("class", "created-audit-label");
            C.Style.Add(HtmlTextWriterStyle.Display, "none");

            R.Cells.Add(AddAuditCell("created-by"));
            R.Cells.Add(AddAuditCell("created"));

            C = new HtmlTableCell();
            R.Cells.Add(C);
            C.InnerHtml = "&nbsp;Updated";
            C.Attributes.Add("class", "updated-audit-label");
            C.Style.Add(HtmlTextWriterStyle.Display, "none");

            R.Cells.Add(AddAuditCell("updated-by"));
            R.Cells.Add(AddAuditCell("updated"));

            if (Audit == AuditModes.Detail)
            {
                C = new HtmlTableCell();
                Image I = new Image();
                I.ImageUrl = GetImageUrl("zoom.png");
                I.Style.Add(HtmlTextWriterStyle.Display, "none");
                I.Attributes.Add("class", "audit-history");
                I.ToolTip = "View audit history";
                C.Controls.Add(I);
                R.Cells.Add(C);
            }

            Resp["audit"] = RenderControlToString(T);
        }

        ///////////////////////////////////////////////
        private HtmlTableCell AddAuditCell(string Class)
        ///////////////////////////////////////////////
        {
            HtmlTableCell C = new HtmlTableCell();
            C.Attributes.Add("class", Class + "-audit");
            C.Style.Add(HtmlTextWriterStyle.Display, "none");
            C.InnerHtml = "&nbsp;";

            return C;
        }

        ///////////////////////////////////////////////
        private void BuildInputTable()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ID = this.Id + "_edit_input_table";
            T.CssClass = "dbnetsuite dbnetedit";

            if (this.GetTheme() == UI.Themes.Bootstrap)
                T.CssClass += " table-condensed";

            int ColIdx = 0;

            if (this.LayoutColumns < 1)
                this.LayoutColumns = 1;

            int MaxRows = Convert.ToInt32(this.Columns.Count / this.LayoutColumns);

            if (this.Columns.Count % LayoutColumns != 0)
                MaxRows++;

            if (this.EditColumnOrderAssigned)
                this.Columns.Sort(new EditColumnSort());

            if (LayoutColumns > 1)
            {
                DbColumnCollection HiddenColumns = new DbColumnCollection();
                DbColumnCollection ShownColumns = new DbColumnCollection();

                foreach (EditColumn C in this.Columns)
                {
                    if (!C.Display || !C.EditDisplay)
                        HiddenColumns.Add(C);
                    else
                        ShownColumns.Add(C);
                }

                this.Columns = ShownColumns;

                foreach (EditColumn C in HiddenColumns)
                {
                    this.Columns.Add(C);
                }
            }

            foreach (EditColumn C in this.Columns)
            {
                TableRow R;

                if (ColIdx < MaxRows)
                {
                    R = new TableRow();
                    R.CssClass = "edit-row";
                    T.Controls.Add(R);
                }

                R = T.Rows[(ColIdx % MaxRows)];

                AddEditInput(C, R);
                ColIdx++;
            }

            Resp["html"] = RenderControlToString(T);
        }

        ///////////////////////////////////////////////
        private void BuildInputControls()
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> EditFields = new Dictionary<string, object>();

            foreach (EditColumn C in this.Columns)
                EditFields[C.ColumnExpressionKey] = RenderControlToString(BuildEditField(C));

            Resp["editFields"] = EditFields;
        }

        public class EditColumnSort : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return (x as EditColumn).ColumnOrder - (y as EditColumn).ColumnOrder;
            }
        }

        ///////////////////////////////////////////////
        private void AddEditInput(EditColumn Col, TableRow R)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();

            if (!Col.Display || !Col.EditDisplay)
                C.Style.Add(HtmlTextWriterStyle.Display, "none");

            R.Controls.Add(C);
            C.CssClass = "edit-label";
            C.Text = Col.Label;

            C = new TableCell();

            if (!Col.Display || !Col.EditDisplay)
                C.Style.Add(HtmlTextWriterStyle.Display, "none");

            R.Controls.Add(C);

            C.Controls.Add(BuildEditField(Col));
        }

        ///////////////////////////////////////////////
        private Control BuildEditField(EditColumn C)
        ///////////////////////////////////////////////
        {
            EditField EF = new EditField(this, C, null);
            EF.Build();
            return EF;
        }

        ///////////////////////////////////////////////
        internal void BuildBrowseDialog()
        ///////////////////////////////////////////////
        {
            BrowseDialog SD = new BrowseDialog(this);
            Resp["html"] = RenderControlToString(SD.Build());
        }

        ///////////////////////////////////////////////
        private void SelectRecord()
        ///////////////////////////////////////////////
        {
            SelectRecord((Dictionary<string, object>)Req["primaryKey"]);
        }

        ///////////////////////////////////////////////
        private void SelectRecord(Dictionary<string, object> PrimaryKey)
        ///////////////////////////////////////////////
        {
            ArrayList EditFieldDataList = null;

            QueryCommandConfig QC = new QueryCommandConfig();

            List<string> PrimaryKeyColumns = new List<string>();

            QC.Sql = "select " + BuildSelectPart(QueryBuildModes.Edit) + " from " + FromPart;

            AddPrimaryKeyFilter(QC, PrimaryKey);

            EditFieldDataList = new ArrayList();

            bool RecordFound = false;

            try
            {
                RecordFound = Database.ExecuteSingletonQuery(QC);
            }
            catch (Exception ex)
            {
                ThrowException(ex.Message, Database.CommandErrorInfo());
            }

            if (!RecordFound)
                ThrowException("SelectRecord ==> Not found:", Database.CommandErrorInfo());

            for (int i = 0; i < Columns.Count; i++)
                EditFieldDataList.Add(GetEditFieldData(i, PrimaryKey));

            Resp["data"] = EditFieldDataList;
            Resp["record"] = JsonData(null);

            if (Audit != AuditModes.None || AuditColumns().Count > 0)
                Resp["auditinfo"] = AuditData(FromPart, PrimaryKey);
        }
    }

    /////////////////////////////////////////////// 
    public class EditColumn : DbColumn
    ///////////////////////////////////////////////
    {
        private bool _Browse = false;
        [
        CategoryAttribute("Edit"),
        DefaultValue(true),
        Description("Specifies that the column should be used to create a browsable list of records")
        ]
        public bool Browse
        {
            get { return _Browse; }
            set { _Browse = value; }
        }

        private short _TabIndex = 0;
        [
        CategoryAttribute("Edit"),
        DefaultValue(0),
        Description("Specifies that tab sequence for edit fields in a manual layout")
        ]
        public short TabIndex
        {
            get { return _TabIndex; }
            set { _TabIndex = value; }
        }

        private int _ColumnOrder = 0;
        [
        CategoryAttribute("Edit"),
        DefaultValue(0),
        Description("Overrides the default order in which columns are presented in the edit dialog")
        ]
        public int ColumnOrder
        {
            get { return _ColumnOrder; }
            set { _ColumnOrder = value; }
        }

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
        public int IndexOf(EditColumn column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }
    }

    ///////////////////////////////////////////////
    internal class UploadConfig
    ///////////////////////////////////////////////
    {
        public bool AllowOverwrite = false;
        public ArrayList AllowedFiles = new ArrayList();
        public int MaxFileSizeKb = 0;
        public string ExtFilter = "";
        public string SavePath = "";
        public string FileName = "";
        public string File = "";
        public bool ForceOverWrite = false;
        public string RenameFile = "";
        public bool SaveToBlob = false;
        public int ColumnIndex = 0;
        public int RowIndex = 1;
        public string Guid = "";
        public string Url = "";
        public WriteToValues WriteTo = WriteToValues.FileSystem;

        public enum WriteToValues
        {
            FileSystem,
            Session
        }

        ///////////////////////////////////////////////
        public UploadConfig()
        ///////////////////////////////////////////////
        {

        }
    }
}

