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
    internal class DataUploadDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public DataUploadDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite data-upload-dialog";
            T.ToolTip = ParentControl.Translate("DataUploadDialog");

            AddPanel("table",T);
            AddPanel("column",T);

            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private void AddPanel(string Name, Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);
            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.ID = this.ParentControl.Id + "_" + Name + "Panel";
            P.CssClass = Name + "-select-panel";
            C.Controls.Add(P);
            HtmlGenericControl HR = new HtmlGenericControl("hr");
            C.Controls.Add(HR);
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
                       R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("apply", "Apply", "apply", "AddDataUsingSelectedColumnMappings"));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Cancel", "undo", "Cancel"));
        }
    }
}

