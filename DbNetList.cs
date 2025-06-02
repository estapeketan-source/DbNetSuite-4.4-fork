using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using DbNetLink.Data;


namespace DbNetLink.DbNetSuite
{

    public class DbNetList : Shared
    {
        public string Sql = "";
        public bool HeaderRow = true;
        public bool Checkbox = false;
        public bool NestedChildList = false;
        public int NestedLevel = 0;
        public string TreeImageUrl = "";
        internal string[] SelectColumns; 
        System.Drawing.Bitmap TreeImage;

        internal Dictionary<string, object> ColumnProperties = new Dictionary<string, object>();
        internal ListColumnCollection Columns = new ListColumnCollection();

        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            switch (Req["method"].ToString())
            {
                case "load":
                    Load();
                    break;
            }

            context.Response.Write(JSON.Serialize(Resp));
        }

        ///////////////////////////////////////////////
        internal void Load()
        ///////////////////////////////////////////////
        {
            List<object> Items = new List<object>();
            Dictionary<string, object> Parameters = (Dictionary<string, object>)Req["parameters"];

            this.OpenConnection();

            if (this.TreeImageUrl != "")
                TreeImage = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(Context.Request.MapPath(this.TreeImageUrl));

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = this.Sql;

            this.SelectColumns = GetSelectColumns(this.Sql);

            foreach (string Key in Parameters.Keys)
                Query.Params.Add(Key, Parameters[Key]);

            AssignColumnProperties();

            Database.ExecuteQuery(Query);

            Table T = new Table();
            T.CssClass = "dbnetlist-" + this.NestedLevel.ToString();
            T.CssClass += " dbnetlist-" + (this.NestedList() ? "tree" : "grid");

            if (this.GetTheme() == UI.Themes.Bootstrap && this.NestedList() == false)
                T.CssClass += " table-condensed";

            if (this.HeaderRow)
                AddHeaderRow(T);

            while (Database.Reader.Read())
                AddDataRow(T);

            if (this.NestedList())
                if (T.Rows.Count > 0)
                {
                    TableCell C = T.Rows[T.Rows.Count-1].Cells[0];
                    C.CssClass = C.CssClass.Replace("tree-node", "tree-last-node");
                }

            this.CloseConnection();

            Resp["html"] = RenderControlToString(T);
        }

        ///////////////////////////////////////////////
        internal void AddHeaderRow(Table T)
        ///////////////////////////////////////////////
        {
            TableHeaderRow R = new TableHeaderRow();
            R.CssClass = "header-row";


            for (var i = 0; i < Database.Reader.FieldCount; i++)
            {
                ListColumn LC = GetColumn(i);

                if (!LC.Display)
                    continue;

                TableHeaderCell C = new TableHeaderCell();
                C.Text = GenerateLabel(Database.Reader.GetName(i));

                if (LC.Label != "")
                    C.Text = LC.Label;

                C.CssClass = "header-cell";
                C.Attributes.Add("columnname", Database.Reader.GetName(i));
                R.Cells.Add(C);
            }
            R.TableSection = TableRowSection.TableHeader;
            AddCheckBoxAndImages(R);
            T.Rows.Add(R);
        }

        ///////////////////////////////////////////////
        internal void AddDataRow(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            R.CssClass = "data-row";
            
            if (!this.NestedList())
                R.CssClass += " " + ((T.Rows.Count % 2 == 0) ? "even" : "odd");

            for (var i = 0; i < Database.Reader.FieldCount; i++)
            {
                ListColumn LC = GetColumn(i);

                R.Attributes.Add(Database.Reader.GetName(i).ToLower(), Database.Reader.GetValue(i).ToString());

                if (!LC.Display)
                    continue;

                TableCell C = new TableCell();
                C.CssClass = "data-cell";
                C.Attributes.Add("columnname", Database.Reader.GetName(i));
                if (LC.Selectable)
                {
                    HyperLink H = new HyperLink();
                    if (LC.Lookup != String.Empty)
                        H.Text = LookupValue(Database.Reader.GetValue(i), LC.Lookup).ToString();
                    else
                        H.Text = FormatValue(Database.Reader.GetValue(i), LC.Format);
                    H.NavigateUrl = "#";
                    H.CssClass = "selectable-link";
                    C.Controls.Add(H);
                }
                else
                {
                    if (this.NestedList())
                    {
                        HtmlGenericControl Span = new HtmlGenericControl("span");
                        Span.Attributes.Add("class", "data-cell-text");
                        if (LC.Lookup != String.Empty)
                            Span.InnerText = LookupValue(Database.Reader.GetValue(i), LC.Lookup).ToString();
                        else
                            Span.InnerText = FormatValue(Database.Reader.GetValue(i), LC.Format);
                        C.Controls.Add(Span);
                    }
                    else
                    {
                        if (LC.Lookup != String.Empty)
                            C.Text = LookupValue(Database.Reader.GetValue(i), LC.Lookup).ToString();
                        else
                            C.Text = FormatValue(Database.Reader.GetValue(i), LC.Format);
                    }
                }

                switch (Database.Reader.GetValue(i).GetType().Name)
                {
                    case "Byte":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                    case "Single":
                    case "Double":
                        if (LC.Lookup == String.Empty)
                            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                        break;
                }

                R.Cells.Add(C);
            }
            AddCheckBoxAndImages(R);
            T.Rows.Add(R);
        }

        ///////////////////////////////////////////////
        private void AddCheckBoxAndImages(TableRow R)
        ///////////////////////////////////////////////
        {
            if (this.NestedList())
            {
                TableCell C;
                if (R is TableHeaderRow)
                    C = new TableHeaderCell();
                else
                    C = new TableCell();
                R.Cells.AddAt(0, C);
                C.CssClass = R.Cells[1].CssClass;

                if (R.CssClass != "header-row")
                {
                    C.CssClass = "tree-cell tree-node";

                    if (this.NestedChildList)
                        C.CssClass += "-open";
                }
                else
                {
                    C.CssClass = "tree-vertical-line header-tree-cell";
                }

                C.Text = "&nbsp;&nbsp;&nbsp;";
                C.Width = new Unit("16px");

                if (this.TreeImageUrl != "")
                {
                    if (R is TableHeaderRow)
                        C = new TableHeaderCell();
                    else
                        C = new TableCell();
                    R.Cells.AddAt(1, C);

                    if (R.CssClass != "header-row")
                    {
                        C.CssClass = "tree-image-cell";
                        C.Width = new Unit(this.TreeImage.Width);

                        System.Web.UI.WebControls.Image I = new System.Web.UI.WebControls.Image();
                        C.Controls.Add(I);
                        I.CssClass = "tree-node-image";
                        I.ImageUrl = this.TreeImageUrl;
                    }
                    else
                    {
                        C.CssClass = "header-tree-image-cell";
                        C.Text = "&nbsp;";
                    }
                }

                if (this.Checkbox)
                {
                    this.AddCheckbox(R);  
                }
            }
            else if (this.Checkbox)
            {
                this.AddCheckbox(R);
            }
        }

        ///////////////////////////////////////////////
        internal void AddCheckbox(TableRow R)
        ///////////////////////////////////////////////
        {
            TableCell C;

            if (R is TableHeaderRow)
                C = new TableHeaderCell();
            else
                C = new TableCell();

            if (this.NestedList())
            {
                for (int i = 0; i < R.Cells.Count; i++)
                    if (R.Cells[i].Attributes["columnname"] != null)
                    {
                        R.Cells.AddAt(i, C);
                        break;
                    }
            }
            else
                R.Controls.Add(C);

            C.Width = new Unit("14px");
            if (this.NestedList())
                C.CssClass = R.Cells[R.Cells.Count-1].CssClass;
            else
                C.CssClass = R.Cells[0].CssClass;

            C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

            switch (C.CssClass)
            {
                case "data-cell":
                case "header-cell":
                    HtmlInputCheckBox CB = new HtmlInputCheckBox();
                    C.Controls.Add(CB);
                    CB.Attributes.Add("class", "select-checkbox");
                    break;
            }
        }


        ///////////////////////////////////////////////
        internal void AssignColumnProperties()
        ///////////////////////////////////////////////
        {
            foreach (string ColumnName in this.ColumnProperties.Keys)
            {
                string ColName = DbNetLink.Util.Decrypt(ColumnName);
                Dictionary<string, object> Properties = (Dictionary<string, object>)this.ColumnProperties[ColumnName];

                ListColumn Col = this.Columns[ColName];

                if (Col == null)
                {
                    Col = new ListColumn();
                    Col.ColumnName = ColName;
                    this.Columns.Add(Col);
                }

                foreach (string Property in Properties.Keys)
                    SetProperty(Col, Property, Properties[Property]);
            }
        }

        ///////////////////////////////////////////////
        private ListColumn GetColumn(int ColumnIndex)
        ///////////////////////////////////////////////
        {
            ListColumn C = GetColumnByName(Database.Reader.GetName(ColumnIndex));

            if (C == null)
                if (this.SelectColumns != null)
                    if (this.SelectColumns.Length > ColumnIndex)
                        C = GetColumnByName(this.SelectColumns[ColumnIndex]);

            if (C == null)
                C = new ListColumn();

            return C;
        }

        ///////////////////////////////////////////////
        private ListColumn GetColumnByName(string ColumnName)
        ///////////////////////////////////////////////
        {
            foreach (ListColumn C in this.Columns)
                if (C.ColumnName.ToLower() == ColumnName.ToLower() || C.ColumnExpression.ToLower() == ColumnName.ToLower())
                    return C;

            return null;
        }  

        ///////////////////////////////////////////////
        private bool NestedList()
        ///////////////////////////////////////////////
        {
            return (this.NestedChildList || this.NestedLevel > 0);
        }  
  }

    /////////////////////////////////////////////// 
    public class ListColumn : Column
    ///////////////////////////////////////////////
    {
        internal string _ColumnExpression = "";
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Name of the column")
        ]
        public string ColumnExpression
        {
            get { return _ColumnExpression; }
            set { _ColumnExpression = value; }
        } 

        private bool _Display = true;
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Controls display of column")
        ]
        public bool Display
        {
            get { return _Display; }
            set { _Display = value; }
        }       
        
        private string _Format = "";
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Applies a .NET formatting string to the column value.")
        ]
        public string Format
        {
            get { return _Format; }
            set { _Format = value; }
        }

        private string _Lookup = "";
        [
        CategoryAttribute("Database"),
        DefaultValue(""),
        Description("Associates a coded value with a defined list of descriptive values and converts the coded value to the descriptive value.")
        ]
        public string Lookup
        {
            get { return _Lookup; }
            set { _Lookup = value; }
        }

        private bool _Selectable = false;
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Controls creation of a selectable link")
        ]
        public bool Selectable
        {
            get { return _Selectable; }
            set { _Selectable = value; }
        }  

        ///////////////////////////////////////////////
        public ListColumn()
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public ListColumn(string ColumnExpression)
        ///////////////////////////////////////////////
        {
            this.ColumnExpression = ColumnExpression;
        }
    }

    ///////////////////////////////////////////////
    public class ListColumnCollection : ColumnCollection
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

        public ListColumn this[string index]
        {
            get
            {
                foreach (ListColumn C in this)
                    if (C.ColumnName.ToLower() == index.ToLower())
                        return (ListColumn)C;
                return null;
            }
        }

        ///////////////////////////////////////////////
        public void Add(ListColumn column)
        ///////////////////////////////////////////////
        {
            this.List.Add(column);
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
}