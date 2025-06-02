using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    public class ViewDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;
        internal int LayoutColumns = 1;
        internal string TemplatePath = "";

        ///////////////////////////////////////////////
        public ViewDialog(DbNetGrid C)
        ///////////////////////////////////////////////
        {
            this.ParentControl = C;
            this.LayoutColumns = this.ParentControl.ViewLayoutColumns;
        }

        ///////////////////////////////////////////////
        public HtmlTable Build()
        ///////////////////////////////////////////////
        {
            HtmlTable T = new HtmlTable();
            T.Attributes.Add("class", "dbnetsuite view-dialog");
            T.Attributes.Add("title", ParentControl.Translate("ViewDialogTitle"));

            HtmlTableRow R = new HtmlTableRow();
            T.Rows.Add(R);

            HtmlTableCell C = new HtmlTableCell();
            R.Cells.Add(C);

            HtmlGenericControl P = new HtmlGenericControl("div");
            P.Attributes.Add("class", "view-dialog-panel");
            P.ID = this.ParentControl.Id + "viewDialogPanel";

            if ( ParentControl.ViewDialogHeight != "")
            {
                P.Style.Add(HtmlTextWriterStyle.Height,ParentControl.ViewDialogHeight);
                P.Style.Add(HtmlTextWriterStyle.OverflowY, "scroll");
            }
            else
                P.Style.Add(HtmlTextWriterStyle.OverflowY, "hidden");

            if (ParentControl.ViewDialogWidth != "")
            {
                P.Style.Add(HtmlTextWriterStyle.Width, ParentControl.ViewDialogWidth);
                P.Style.Add(HtmlTextWriterStyle.OverflowX, "scroll");
            }
            else
                P.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");

            C.Controls.Add(P);

            if (TemplatePath != "")
                if (File.Exists(HttpContext.Current.Request.MapPath(TemplatePath)))
                {
                    string Html = File.ReadAllText(HttpContext.Current.Request.MapPath(TemplatePath));
                    P.InnerHtml = Html;
                }


            if (P.InnerHtml == "")
                P.Controls.Add(BuildViewColumns());

            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private HtmlTable BuildViewColumns()
        ///////////////////////////////////////////////
        {
            HtmlTable T = new HtmlTable();

            int ColIdx = 0;

            if (this.LayoutColumns < 1)
                this.LayoutColumns = 1;

            int MaxRows = Convert.ToInt32(this.ParentControl.ViewColumns().Count / this.LayoutColumns);

            if (this.ParentControl.ViewColumns().Count % LayoutColumns != 0)
                MaxRows++;

            foreach (GridColumn C in this.ParentControl.Columns)
            {
                if (C.View == false)
                    continue;

                HtmlTableRow R;

                if (ColIdx < MaxRows)
                    T.Controls.Add(new HtmlTableRow());

                R = T.Rows[(ColIdx % MaxRows)];

                AddViewControl(C, R);
                ColIdx++;
            }

            return T;
        }

        ///////////////////////////////////////////////
        private void AddViewControl(GridColumn Col, HtmlTableRow R)
        ///////////////////////////////////////////////
        {
            HtmlTableCell C = new HtmlTableCell();

            R.Controls.Add(C);
            C.Attributes.Add("class","view-label");
            C.InnerHtml = Col.Label;

            C = new HtmlTableCell();

            R.Controls.Add(C);

            string Class = "view-element";

            HtmlControl D = new HtmlGenericControl("div");

            if (Col.EditControlType == EditField.ControlType.Upload || Col.DataType == "Byte[]")
            {
                Class += " view-image";
                D = new HtmlImage();
            }
            else if (Col.DataType == "Boolean")
            {
                Class += " view-checkbox";
                D = new HtmlInputCheckBox();
            }
            else
                Class += " view-text";

            Class += " view-" + Col.ColumnName.Replace(" ","-").ToLower();

            D.Attributes.Add("columnName", Col.ColumnName);
            D.Attributes.Add("dataType", Col.DataType);
            D.Attributes.Add("label", Col.Label);
            D.Attributes.Add("class", Class);

            if (Col.Style != String.Empty)
                D.Style.Value = Col.Style;

            if (Col.Lookup == "")
                if (ParentControl.GetAlignment(Col.DataType) == HorizontalAlign.Right)
                    D.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(D);
        }


        ///////////////////////////////////////////////
        private void AddButtonsRow(HtmlTable T)
        ///////////////////////////////////////////////
        {
            HtmlTableRow R = new HtmlTableRow();
            T.Controls.Add(R);

            HtmlTableCell C = new HtmlTableCell();
            C.Attributes.Add("class","view-dialog-toolbar");
            R.Controls.Add(C);

            HtmlTable T1 = new HtmlTable();
            C.Controls.Add(T1);

            R = new HtmlTableRow();
            T1.Controls.Add(R);
            T1.Style.Add(HtmlTextWriterStyle.Width, "100%");

            C = new HtmlTableCell();
            R.Controls.Add(C);
            C.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");

            C.Controls.Add(ParentControl.AddToolbarButton("prev", "", "prev", "PrevRow"));
            C.Controls.Add(ParentControl.AddToolbarButton("next", "", "next", "NextRow"));

            C = new HtmlTableCell();
            R.Controls.Add(C);
            C.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            if (ParentControl.GetBrowser().Equals("msie"))
                C.Controls.Add(ParentControl.AddToolbarButton("copy", "Copy", "copy", "CopyToClipboard")); 
            if (ParentControl.ViewPrint)
                C.Controls.Add(ParentControl.AddToolbarButton("print", "Print", "printer", "PrintTheCurrentRecord"));
            C.Controls.Add(ParentControl.AddToolbarButton("close", "Close", "Cancel", "Close"));
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

