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
    internal class AuditDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public AuditDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite audit-dialog";
            T.ToolTip = "Audit History";

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            HtmlGenericControl Div = new HtmlGenericControl("div");
            Div.Attributes.Add("class", "audit-dialog-panel");
            C.Controls.Add(Div);

            AddButtonsRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "audit-toolbar";
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
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "undo", "Close"));
        }

    }
}

