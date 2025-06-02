using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#if (!x64)
using System.Web.UI.DataVisualization.Charting;
#endif

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class ChartDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public ChartDialog(DbNetGrid PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            T.CssClass = "dbnetsuite chart-dialog";
            T.ToolTip = Translate("Chart");

            TableRow R = new TableRow();
            T.Rows.Add(R);
            TableCell C = new TableCell();
            R.Cells.Add(C);

            Panel P = new Panel();
            P.CssClass = "chart-dialog-image-panel";
            C.Controls.Add(P);

            Image I = new Image();
            I.CssClass = "chart-dialog-image";
            P.Controls.Add(I);

            AddButtonsRow(T);
  //          ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "chart-dialog-toolbar";
            R.Controls.Add(C);

            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "Cancel", ""));
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

