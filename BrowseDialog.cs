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
    internal class BrowseDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public BrowseDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite browse-dialog";
            T.ToolTip = Translate("Browse");

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.Width = new Unit("300px");
            if (ParentControl.Req.ContainsKey("width"))
                if (ParentControl.Req["width"] != null)
                    P.Width = new Unit(ParentControl.Req["width"].ToString());

            P.ID = this.ParentControl.Id + "_browsePanel";
            P.CssClass = "browse-panel";
            C.Controls.Add(P);

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
                       R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "Cancel", "CloseTheBrowseWindow"));
        }



        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

