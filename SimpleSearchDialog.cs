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
    internal class SimpleSearchDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public SimpleSearchDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite simple-search-dialog";
            T.ToolTip = Translate("SimpleSearch");

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            TextBox TB = new TextBox();
            TB.Width = new Unit("300px");
            TB.ID = "simpleSearchToken";
            TB.CssClass = "simple-search-token";
            C.Controls.Add(TB);

            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);

            return T;
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

            if (ParentControl.StandardSearch)
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

