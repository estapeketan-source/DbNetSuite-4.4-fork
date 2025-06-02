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
    internal class AdvancedSearchDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public AdvancedSearchDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite advanced-search-dialog";
            T.ToolTip = Translate("AdvancedSearch");

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.Height = new Unit("200px");
            P.Style.Add(HtmlTextWriterStyle.OverflowY, "auto");

            P.CssClass = "search-criteria-panel";

            P.Controls.Add(AddSearchCriteriaTable());

            C.Controls.Add(P);

            ParentControl.AddSearchOptionsRow(T);
            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private Table AddSearchCriteriaTable()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "search-criteria-table";
            TableRow R = new TableRow();
            T.Rows.Add(R);
            TableCell C = new TableCell();
            C.CssClass = "join-operator";
            R.Cells.Add(C);

            C = new TableCell();
            Panel P = new Panel();
            P.CssClass = "filter-text";
            P.Width = new Unit("400px");
            P.Style.Add(HtmlTextWriterStyle.OverflowY, "auto");
            P.BorderColor = System.Drawing.Color.Silver;
            P.BorderStyle = BorderStyle.Solid;
            P.BorderWidth = new Unit("1px");
            P.BackColor = System.Drawing.Color.White;
            C.Controls.Add(P);
            R.Cells.Add(C);

            R.Cells.Add(AddHyperLink("Edit"));
            R.Cells.Add(AddHyperLink("Clear"));

            return T;
        }

        ///////////////////////////////////////////////
        internal TableCell AddHyperLink(string LinkType)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            HyperLink H = new HyperLink();

            H.ID = LinkType.ToLower() + "Link";
            H.CssClass = LinkType.ToLower() + "-link";
            H.Text = Translate(LinkType);
            H.NavigateUrl = "#";
            C.Controls.Add(H);
            return C;
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "search-toolbar";
            R.Controls.Add(C);

            Table T = new Table();
            T.CellPadding = 0;
            T.CellSpacing = 0;

            T.Width = new Unit("100%");
            C.Controls.Add(T);

            R = new TableRow();
            T.Controls.Add(R);

            C = new TableCell();
            R.Controls.Add(C); 
            C.Controls.Add(ParentControl.BuildButton( "add", "Add", "new_search", "AddNewSearch"));

            R.Controls.Add(ParentControl.AddSearchDialogLink("Standard"));

            C = new TableCell();
            R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("apply", "Search", "apply", "ApplyTheEnteredSearchCriteria"));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "undo", "CloseTheSearchWindow"));
        }



        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

