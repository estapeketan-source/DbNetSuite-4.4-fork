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
    internal class FilePreviewDialog
    ///////////////////////////////////////////////
    {
        internal Shared ParentControl;

        ///////////////////////////////////////////////
        public FilePreviewDialog(Shared PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Panel Build()
        ///////////////////////////////////////////////
        {
            Panel MP = new Panel();
            MP.CssClass = "dbnetsuite file-preview";
            MP.ToolTip = ParentControl.Translate("FilePreview");
            MP.ID = this.ParentControl.AssignID("preview_dialog_panel");

            HtmlGenericControl F = new HtmlGenericControl("iframe");
            F.ID = this.ParentControl.AssignID("preview_dialog_frame");
            F.Attributes.Add("name", this.ParentControl.AssignID("preview_dialog_frame"));
            F.Style.Add(HtmlTextWriterStyle.Width, this.ParentControl.Req["frameWidth"].ToString() + "px");
            F.Style.Add(HtmlTextWriterStyle.Height, this.ParentControl.Req["frameHeight"].ToString() + "px");
            F.Style.Add(HtmlTextWriterStyle.BackgroundColor, "white");
            F.Attributes.Add("frameborder", "0");
            this.ParentControl.MakeIframe508Compliant(F, "Empty");

            MP.Style.Add(HtmlTextWriterStyle.Width, "100%");
            MP.Style.Add(HtmlTextWriterStyle.Height, "100%");
            MP.Controls.Add(F);

            Panel P = new Panel();
            P.ID = "caption";
            P.CssClass = "caption";
            MP.Controls.Add(P);

            P = new Panel();
            P.Style.Add("border-top", "1pt solid silver");
            P.Controls.Add(Toolbar());
            MP.Controls.Add(P);

            P = new Panel();
            P.Controls.Add(MessageLine());
            MP.Controls.Add(P);

            return MP;
        }

        ///////////////////////////////////////////////
        private Table Toolbar()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.Width = new Unit("100%");

            TableRow TR = new TableRow();
            T.Controls.Add(TR);

            TableCell TC = new TableCell();
            TC.Text = "&nbsp;";
            TC.Width = new Unit("100%");
            TR.Controls.Add(TC);

            ParentControl.AddToolButton(TR, "download", "download", "Download");
            ParentControl.AddToolButton(TR, "print", "printer", "Print");
            ParentControl.AddToolButton(TR, "window", "window", "OpenInNewWindow");

            if (ParentControl.Context.Request.Browser.Browser == "IE")
                ParentControl.AddToolButton(TR, "copy", "copy", "CopyToClipboard");

            return T;
        }


        ///////////////////////////////////////////////
        private Table MessageLine()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.Width = new Unit("100%");
            this.ParentControl.AddMessageRow(T);

            return T;
        }
    }
}

