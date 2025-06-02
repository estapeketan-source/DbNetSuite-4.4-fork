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
    internal class LookupDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public LookupDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Control Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite lookup-dialog";
            T.ToolTip = ParentControl.Translate("Lookup");

            T.Controls.Add(SearchRow());

            TableRow R = new TableRow();
            T.Controls.Add(R);
            R.Height = Unit.Percentage(100);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            C.ID = "listBoxContainer";
            C.CssClass = "list-box-container";

            R = new TableRow();
            T.Controls.Add(R);
            R.Height = Unit.Percentage(0);

            C = new TableCell();
            R.Controls.Add(C);

            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("select","Select","apply",""));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "undo", ""));

            return T;
        }

        ///////////////////////////////////////////////
        private TableRow SearchRow()
        ///////////////////////////////////////////////
        {
            TableRow SR = new TableRow();
            SR.ID = "searchRow";
            SR.CssClass = "search-row";
            SR.Height = Unit.Percentage(100);
            TableCell C = new TableCell();
            SR.Controls.Add(C);

            Table T = new Table();
            T.CellPadding = 0;
            T.CellSpacing = 0;

            T.Width = Unit.Percentage(100);
            C.Controls.Add(T);

            TableRow R = new TableRow();

            T.Controls.Add(R);

            for (int I = 0; I < 3; I++)
            {
                C = new TableCell();
                R.Cells.Add(new TableCell());
            }

            R.Cells[0].Style.Add(HtmlTextWriterStyle.PaddingRight, "5px");
            R.Cells[0].Width = Unit.Percentage(100);
            TextBox TB = new TextBox();
            TB.ID = "searchLookupToken";
            TB.CssClass = "search-token-input";
            TB.Width = Unit.Percentage(100);
            R.Cells[0].Controls.Add(TB);
            R.Cells[1].Text = "&nbsp;";

            DbNetButton B = ParentControl.BuildButton("search", "Search", "find", "");
            B.Style.Add(HtmlTextWriterStyle.Height, "24px");
            R.Cells[2].Controls.Add(B);

            return SR;
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

