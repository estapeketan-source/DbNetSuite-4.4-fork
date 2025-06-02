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
    internal class HtmlEditor
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public HtmlEditor(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Panel Build()
        ///////////////////////////////////////////////
        {
            Panel P = new Panel();
            P.CssClass = "dbnetsuite html-editor";
            P.ToolTip = ParentControl.Translate("HTMLEditor");

            AddEditor(P, "simple");
            AddEditor(P, "advanced");

            Panel Toolbar = new Panel();
            P.Controls.Add(Toolbar);
            Toolbar.CssClass = "html-editor-toolbar";

            DbNetButton B = ParentControl.BuildButton("advanced", "Advanced", "", "SwitchToAdvancedEditing");
            B.Style.Add("float", "left");
            Toolbar.Controls.Add(B);
            B = ParentControl.BuildButton("simple", "Simple", "", "SwitchToSimpleEditing");
            B.Style.Add("float", "left");
            B.Style.Add(HtmlTextWriterStyle.Display, "none");
            Toolbar.Controls.Add(B);


            B = ParentControl.BuildButton("spellCheck", "", "spellcheck", "CheckSpelling");
            B.Style.Add("float", "left");
            B.Style.Add(HtmlTextWriterStyle.MarginLeft,"10px");
            Toolbar.Controls.Add(B);

            B = ParentControl.BuildButton("apply", "Ok", "apply", "");
            B.Style.Add("float", "right");
            Toolbar.Controls.Add(B);

            B = ParentControl.BuildButton("cancel", "Close", "undo", "");
            B.Style.Add("float", "right");
            Toolbar.Controls.Add(B);

            return P;
        }

        ///////////////////////////////////////////////
        internal void AddEditor(Panel T, string EditorType)
        ///////////////////////////////////////////////
        {
            Panel P = new Panel();
            T.Controls.Add(P);
            P.CssClass = EditorType + "Panel";

            if (EditorType == "advanced")
                P.Style.Add(HtmlTextWriterStyle.Display, "none");

            HtmlGenericControl D = new HtmlGenericControl("textarea");
            P.Controls.Add(D);
            D.Attributes.Add("class", EditorType + "HtmlEditor");
            D.Style.Add(HtmlTextWriterStyle.Height, "100px");
            D.Style.Add(HtmlTextWriterStyle.Width, "100px");
        }

    }
}

