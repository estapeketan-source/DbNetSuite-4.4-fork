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
    internal class ColumnPickerDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public ColumnPickerDialog(DbNetGrid PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            T.CssClass = "dbnetsuite column-picker-dialog";
            T.ToolTip = Translate("ColumnPicker");

            AddColumnSelection(T);
            AddButtonsRow(T);
            this.ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private void AddColumnSelection(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);
            TableCell C = new TableCell();
            R.Controls.Add(C);

            Table T = new Table();
            C.Controls.Add(T);
            R = new TableRow();
            T.Controls.Add(R);

            AddSelectionCell(R, "Available");
            AddSelectionCell(R, "Selected");
        }

        ///////////////////////////////////////////////
        private void AddSelectionCell(TableRow R, string CellType)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.GroupingText = Translate(CellType);
            C.Controls.Add(P);
            Table T = new Table();
            P.Controls.Add(T);
            R = new TableRow();
            T.Controls.Add(R);
            C = new TableCell();
            R.Controls.Add(C);
            HtmlSelect S = new HtmlSelect();
            S.Multiple = true;
            S.Size = 20;
            S.Attributes.Add("class", CellType.ToLower() + "-columns");

            S.Style.Add(HtmlTextWriterStyle.Width, "150px");
            C.Controls.Add(S);

            C = new TableCell();
            C.VerticalAlign = VerticalAlign.Middle;
            R.Controls.Add(C);

            if (CellType == "Available")
            {
                this.AddToolButton(C, "add", "Right", "Move column from available to selected");
                C.Controls.Add(new HtmlGenericControl("P"));
                this.AddToolButton(C, "remove", "Left", "Move column from selected to available");
            }
            else
            {
                this.AddToolButton(C, "up", "Up", "Move selected column up");
                C.Controls.Add(new HtmlGenericControl("P"));
                this.AddToolButton(C, "down", "Down", "Move selected column down");
            }
        }

        ///////////////////////////////////////////////
        private DbNetButton AddToolButton(TableCell Cell, string ButtonID, string ImageName, string TitleKey)
        ///////////////////////////////////////////////
        {
            DbNetButton B = this.ParentControl.AddToolButton(Cell, ButtonID, ImageName, TitleKey);
            B.ID = this.ParentControl.Id + "_ColumnPickerDialog_" + ButtonID + "Btn";
            B.Attributes.Add("class",ButtonID + "-toolbutton");
            B.Style.Add(HtmlTextWriterStyle.Width, "28px");

            (B.Controls[0] as HtmlGenericControl).InnerHtml = "&nbsp;";
            return B;
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "column-picker-toolbar";
            R.Controls.Add(C);

            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("apply", "Apply", "apply", ""));
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

