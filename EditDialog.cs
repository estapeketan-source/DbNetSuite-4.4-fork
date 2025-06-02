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
    public class EditDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public EditDialog(DbNetGrid C)
        ///////////////////////////////////////////////
        {
            this.ParentControl = C;
        }

        ///////////////////////////////////////////////
        public Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite edit-dialog";
            T.ToolTip = ParentControl.Translate("EditDialogTitle");

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);

            Panel P = new Panel();
            P.CssClass = "edit-dialog-panel";
            P.ID = this.ParentControl.Id + "editDialogPanel";

            if ( ParentControl.EditDialogHeight != "") {
                P.Height = new Unit(ParentControl.EditDialogHeight);
                P.Style.Add(HtmlTextWriterStyle.OverflowY, "scroll");
                P.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");
            }

            C.Controls.Add(P);

            AddAuditRow(T);
            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);

            return T;
        }


        ///////////////////////////////////////////////
        private void AddAuditRow(Table T)
        ///////////////////////////////////////////////
        {
            if (ParentControl.Audit == GridEditControl.AuditModes.None)
                return;

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "edit-dialog-audit-panel";
            R.Controls.Add(C);
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "edit-dialog-toolbar";
            R.Controls.Add(C);

            Table T1 = new Table();
            C.Controls.Add(T1);

            R = new TableRow();
            T1.Controls.Add(R);
            T1.Style.Add(HtmlTextWriterStyle.Width, "100%");

            C = new TableCell();
            R.Controls.Add(C);
            C.Wrap = false;


            C.Controls.Add(ParentControl.AddToolbarButton("prev", "", "prev", "PreviousRow"));
            C.Controls.Add(ParentControl.AddToolbarButton("next", "", "next", "NextRow"));

            if (ParentControl.InsertRow)
            {
                Literal L = new Literal();
                L.Text = "&nbsp;";
                C.Controls.Add(L);
                C.Controls.Add(ParentControl.AddToolbarButton("insert", "", "insert", "AddANewRecord"));
            }

            if (ParentControl.SpellCheck)
            {
                Literal L = new Literal();
                L.Text = "&nbsp;";
                C.Controls.Add(L);
                C.Controls.Add(ParentControl.AddToolbarButton("spellCheck", "", "spellcheck", "CheckSpelling"));
            }

            C = new TableCell();
            R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.AddToolbarButton("apply", "Apply", "apply", "ApplyChangesToTheCurrentRecord"));
            C.Controls.Add(ParentControl.AddToolbarButton("cancel", "Cancel", "undo", "Cancel"));
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

