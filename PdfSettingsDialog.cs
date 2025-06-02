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
    internal class PdfSettingsDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public PdfSettingsDialog(DbNetGrid PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite pdf-settings-dialog";
            T.ToolTip = Translate("PdfSettings");

            T.Controls.Add(Orientation());
            T.Controls.Add(Font());

            AddButtonsRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        internal TableRow Orientation()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Controls.Add(C);

            Panel P = new Panel();
            C.Controls.Add(P);
            P.GroupingText = Translate("Orientation");

            RadioButtonList RBL = new RadioButtonList();
            RBL.CssClass = "orientation";
            RBL.RepeatDirection = RepeatDirection.Horizontal;
            P.Controls.Add(RBL);
            RBL.Items.Add(new ListItem(Translate("Portrait"), "portrait"));
            RBL.Items.Add(new ListItem(Translate("Landscape"), "landscape"));

            RBL.Items[0].Selected = true;

            return R;
        }

        ///////////////////////////////////////////////
        internal TableRow Font()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Controls.Add(C);

            Panel P = new Panel();
            C.Controls.Add(P);
            P.GroupingText = Translate("Font");

            Table T = new Table();
            P.Controls.Add(T);

            T.Controls.Add(new TableRow());

            C = new TableCell();
            C.Text = Translate("FontFamily");
            T.Rows[0].Controls.Add(C);

            C = new TableCell();
            T.Rows[0].Controls.Add(C);

            DropDownList DDL = new DropDownList();
            DDL.CssClass = "font-family";
            C.Controls.Add(DDL);
            DDL.Items.Add(new ListItem("Helvetica"));
            DDL.Items.Add(new ListItem("Times Roman"));
            DDL.Items.Add(new ListItem("Courier"));

            DDL.Items[0].Selected = true;

            T.Controls.Add(new TableRow());

            C = new TableCell();
            C.Text = Translate("FontSize");
            T.Rows[1].Controls.Add(C);

            C = new TableCell();
            T.Rows[1].Controls.Add(C);

            DDL = new DropDownList();
            DDL.CssClass = "font-size";
            C.Controls.Add(DDL);

            for (int i = 1; i < 25; i++)
                DDL.Items.Add(new ListItem(i.ToString()));

            DDL.Items[9].Selected = true;

            return R;
        }


        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "pdf-settings-toolbar";
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

            C.Controls.Add(ParentControl.BuildButton("apply", "Apply", "apply", ""));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Cancel", "undo", ""));
        }



        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

