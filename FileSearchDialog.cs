using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class FileSearchDialog
    ///////////////////////////////////////////////
    {
        internal DbNetFile ParentControl;
        internal TableRow ContentsSearchRow;

        ///////////////////////////////////////////////
        public FileSearchDialog(DbNetFile PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        public Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.ToolTip = Translate("FileSearch");
            T.CssClass = "dbnetsuite file-search-dialog";

            T.Controls.Add(FileAttributeSearch());
            T.Controls.Add(FileContentSearch());
            T.Controls.Add(FileSearchOptions());
            T.Controls.Add(AddToolbar());
            ParentControl.AddMessageRow(T);
            return T;
        }

        ///////////////////////////////////////////////
        protected Control FileAttributeSearch()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            foreach (FileColumn FC in ParentControl.Columns)
            {
                if (FC.Search)
                    T.Controls.Add(FilePropertySearch(FC));
            }

            Panel P = new Panel();
            P.GroupingText = Translate("FileAttributes");
            TableCell TC = new TableCell();
            P.Controls.Add(T);
            TC.Controls.Add(P);
            TableRow TR = new TableRow();
            TR.Controls.Add(TC);

            return TR;
        }

        ///////////////////////////////////////////////
        protected Control FileSearchOptions()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            TableRow TR = new TableRow();
            T.Controls.Add(TR);

            AddSearchOption("include_sub_folders", TR);
            AddSearchOption("search_files_only", TR);

            Panel P = new Panel();
            P.GroupingText = Translate("Options");
            TableCell TC = new TableCell();
            P.Controls.Add(T);
            TC.Controls.Add(P);
            TR = new TableRow();
            TR.Controls.Add(TC);

            return TR;
        }

        ///////////////////////////////////////////////
        private void AddSearchOption(string Id, TableRow TR)
        ///////////////////////////////////////////////
        {
            TableCell TC = new TableCell();
            TC.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");

            switch (Id)
            {
                case "include_sub_folders":
                    TC.Text = Translate("Includesub-folders");
                    break;
                case "search_files_only":
                    TC.Text = Translate("Searchfilesonly");
                    TC.Visible = (ParentControl.SearchMode != DbNetFile.SearchModes.IndexingService);
                    break;
            }

            TR.Controls.Add(TC);

            TC = new TableCell();
            TC.Visible = TR.Controls[0].Visible;
            HtmlGenericControl CB = new HtmlGenericControl("input");
            CB.ID = Id;
            CB.Attributes.Add("type", "checkbox");
            CB.Attributes.Add("class", "search-criteria");
            CB.Attributes.Add("checked", "true");

            TC.Controls.Add(CB);
            TR.Controls.Add(TC);
        }

        ///////////////////////////////////////////////
        protected Control FileContentSearch()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.Controls.Add(AddFileContentSearch());

            Panel P = new Panel();
            P.GroupingText = Translate("FileContent");
            TableCell TC = new TableCell();
            P.Controls.Add(T);
            TC.Controls.Add(P);
            TableRow TR = new TableRow();
            TR.Controls.Add(TC);

            this.ContentsSearchRow = TR;
            SetContentSearchVisibility();

            return TR;
        }

        ///////////////////////////////////////////////
        internal void SetContentSearchVisibility()
        ///////////////////////////////////////////////
        {
            this.ContentsSearchRow.Style.Add(HtmlTextWriterStyle.Display, (ParentControl.SearchMode == DbNetFile.SearchModes.FileSystem) ? "none" : "");
        }

        ///////////////////////////////////////////////
        private TableRow AddFileContentSearch()
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TableCell TC = new TableCell();
            TR.Controls.Add(TC);

            Table T = new Table();
            T.CellPadding = 0;
            T.CellSpacing = 0;
            T.CssClass = "file-search-dialog-criteria";
            TC.Controls.Add(T);
            TR = new TableRow();
            T.Controls.Add(TR);

            TC = new TableCell();
            TextBox TB = new TextBox();
            TB.ID = "content_search_token";
            TB.CssClass = "search-criteria";
            TB.Width = 500;
            TC.Controls.Add(TB);
            TR.Controls.Add(TC);

            this.ParentControl.AddToolButton(TR, "help", "Help", "Content search help");

            return (TableRow)TR.Parent.Parent.Parent;
        }

        ///////////////////////////////////////////////
        private TableRow FilePropertySearch(FileColumn FC)
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TableCell TC = new TableCell();
            TC.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            TC.Text = FC.Label;
            TR.Controls.Add(TC);

            TC = new TableCell();
            TR.Controls.Add(TC);

            Table T = new Table();
            T.CellPadding = 0;
            T.CellSpacing = 0;
            T.CssClass = "file-search-dialog-criteria";
            TC.Controls.Add(T);
            TR = new TableRow();
            T.Controls.Add(TR);

            TC = new TableCell();
            DropDownList LB = new DropDownList();
            LB.ID = FC.ColumnID + "_search_operator";
            LB.CssClass = "search-criteria search-criteria-operator";

            switch (FC.ColumnDataType)
            {
                case "String":
                    LB.Items.Add(new ListItem(Translate("Contains"), "contains", true));
                    LB.Items.Add(new ListItem(Translate("Equal To"), "equalto"));
                    LB.Items.Add(new ListItem(Translate("Starts With"), "startswith"));
                    LB.Items.Add(new ListItem(Translate("Ends With"), "endswith"));
                    break;
                case "DateTime":
                    LB.Items.Add(new ListItem(Translate("Equal To"), "equalto"));
                    LB.Items.Add(new ListItem(Translate("Before"), "lessthan", true));
                    LB.Items.Add(new ListItem(Translate("After"), "greaterthan"));
                    LB.Items.Add(new ListItem(Translate("Between"), "between"));
                    break;
                default:
                    LB.Items.Add(new ListItem(Translate("Equal To"), "equalto"));
                    LB.Items.Add(new ListItem(Translate("Less Than"), "lessthan", true));
                    LB.Items.Add(new ListItem(Translate("Greater Than"), "greaterthan"));
                    LB.Items.Add(new ListItem(Translate("Between"), "between"));
                    break;
            }

            TC.Controls.Add(LB);
            TR.Controls.Add(TC);

            AddTokenInput(TR, "1", FC);

            if (FC.ColumnDataType != typeof(String).Name)
                AddTokenInput(TR, "2", FC);

            return (TableRow)TR.Parent.Parent.Parent;
        }

        ///////////////////////////////////////////////
        private void AddTokenInput(TableRow TR, string Index, FileColumn FC)
        ///////////////////////////////////////////////
        {
            if (Index == "2")
                AddAndLiteral(TR);

            TableCell TC = new TableCell();
            TextBox TB = new TextBox();
            TB.ID = FC.ColumnID + "_search_token_" + Index;

            TB.Attributes.Add("dataType", FC.ColumnDataType);

            if (FC.ColumnDataType == typeof(String).Name)
                TB.Width = 300;
            else
                TB.Width = 80;

            if (FC.ColumnDataType == typeof(Int32).Name)
                TB.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            TB.CssClass = "search-criteria" + (Index == "2" ? " between" : "");
            if (Index == "2")
                TB.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
            TC.Controls.Add(TB);
            TR.Controls.Add(TC);

            if (FC.ColumnType == DbNetFile.ColumnTypes.Size)
            {
                TC = new TableCell();
                DropDownList LB = new DropDownList();
                LB.ID = FC.ColumnID + "_search_unit_" + Index;
                LB.CssClass = "search-criteria size-unit" + (Index == "2" ? " between" : "");
                if (Index == "2")
                    LB.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                LB.Items.Add(new ListItem(Translate("Bytes"), "bytes", true));
                LB.Items.Add(new ListItem(Translate("KB"), "kb"));
                LB.Items.Add(new ListItem(Translate("MB"), "mb"));
                TC.Controls.Add(LB);
                TR.Controls.Add(TC);
            }

            if (FC.ColumnDataType == typeof(DateTime).Name)
            {
                DbNetButton B = this.ParentControl.AddToolButton(TR, "calendar", "calendar", "SelectDate");

                if (Index == "2")
                {
                    B.Attributes.Add("class", "between");
                    B.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                }
            }
        }

        ///////////////////////////////////////////////
        private void AddAndLiteral(TableRow TR)
        ///////////////////////////////////////////////
        {
            TableCell TC = new TableCell();
            Panel P = new Panel();
            P.Controls.Add(new LiteralControl("and"));
            P.CssClass = "between";
            P.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
            TC.Controls.Add(P);
            TR.Controls.Add(TC);
        }


        ///////////////////////////////////////////////
        private TableRow AddToolbar()
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TableCell TC = new TableCell();
            TC.CssClass = "toolbar-container";
            TC.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            TC.Controls.Add(AddToolbarButton("apply", "Apply", "apply", ""));
            TC.Controls.Add(AddToolbarButton("cancel","Cancel","undo",""));

            TR.Controls.Add(TC);
            return TR;
        }

        ///////////////////////////////////////////////
        private DbNetButton AddToolbarButton( string ID, string Text, string Img, string Title)
        ///////////////////////////////////////////////
        {
            string ImageUrl = this.ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + Img + ".png");
            DbNetButton B = new DbNetButton(ImageUrl, Translate(Text), Translate(Title), this.ParentControl.Context.Request, this.ParentControl.PageRef);
            B.Attributes.Add("class", ID + "-button");
            B.ID = ID + "Btn";
            return B;
        }

        ///////////////////////////////////////////////
        private string Translate( string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }

        ///////////////////////////////////////////////
        internal void ValidateParameters()
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> Parameters = (Dictionary<string, object>)ParentControl.Req["searchCriteria"];

            string Message = "";
            string ElementID = "";

            foreach (FileColumn FC in ParentControl.Columns)
            {
                if (FC.Search == false)
                    continue;

                string[] Tokens = { "", "" };
                string Operator = Parameters[FC.ColumnID + "_search_operator"].ToString();

                Tokens[0] = Parameters[FC.ColumnID + "_search_token_1"].ToString();

                if (Operator == "between")
                    Tokens[1] = Parameters[FC.ColumnID + "_search_token_2"].ToString();

                int Idx = 0;
                foreach (string Token in Tokens)
                {
                    Idx++;
                    if (Token == "")
                        continue;

                    if (FC.ColumnDataType == typeof(DateTime).Name)
                    {
                        try
                        {
                            DateTime.Parse(Token);
                        }
                        catch (Exception)
                        {
                            Message = Translate("DateNotValid");
                        }
                    }

                    if (FC.ColumnDataType == typeof(Int32).Name)
                    {
                        try
                        {
                            Int32.Parse(Token);
                        }
                        catch (Exception)
                        {
                            Message = Translate("IntegerNotValid");
                        }
                    }

                    if (Message != "")
                    {
                        ElementID = FC.ColumnID + "_search_token_" + Idx.ToString();
                        break;
                    }

                }

                if (Message == "" && Operator == "between")
                    Message = ValidateRange(FC, Tokens);

                if (Message != "")
                {
                    if (ElementID == "")
                        ElementID = FC.ColumnID + "_search_token_1";
                    break;
                }
            }

            ParentControl.Resp.Add("message", Message);
            ParentControl.Resp.Add("elementId", ElementID);
        }

        ///////////////////////////////////////////////
        protected string ValidateRange(FileColumn FC, string[] Tokens)
        ///////////////////////////////////////////////
        {
            string Message = "";

            if (Tokens[0] + Tokens[1] == "")
                return "";

            if (Tokens[0] == "" || Tokens[1] == "")
                Message = Translate("InvalidRange");
            else
            {
                if (FC.ColumnDataType == typeof(DateTime).Name)
                    if (DateTime.Parse(Tokens[0]) > DateTime.Parse(Tokens[1]))
                        Message = Translate("InvalidRange");

                if (FC.ColumnDataType == typeof(Int32).Name)
                    if (Int32.Parse(Tokens[0]) > Int32.Parse(Tokens[1]))
                        Message = Translate("InvalidRange");
            }

            return Message;
        }
    }
}