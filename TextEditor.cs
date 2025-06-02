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
    internal class TextEditor
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public TextEditor(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Panel Build()
        ///////////////////////////////////////////////
        {
            Panel P = new Panel();
            P.CssClass = "dbnetsuite text-editor";
            P.ToolTip = ParentControl.Translate("TextEditor");

            Panel P2 = new Panel();
            P.Controls.Add(P2);

            TextBox TA = new TextBox();
            P2.Controls.Add(TA);
            TA.ID = ParentControl.AssignID("textEditor");
            TA.CssClass = "text-editor";
            TA.Height = new Unit("400px");
            TA.Width = new Unit("800px");
            TA.TextMode = TextBoxMode.MultiLine;

            Panel P3 = new Panel();

            P.Controls.Add(P3);
            P3.CssClass = "text-editor-toolbar";
            P3.Height = new Unit("24px");
            P3.Width = new Unit("800px");

            DbNetButton B = ParentControl.BuildButton("spellCheck", "", "spellcheck", "CheckSpelling");
            B.Style.Add("float", "left");
            P3.Controls.Add(B);

            B = ParentControl.BuildButton("apply", "Ok", "apply", "");
            B.Style.Add("float", "right");
            P3.Controls.Add(B);

            B = ParentControl.BuildButton("cancel", "Close", "undo", "");
            B.Style.Add("float", "right");
            P3.Controls.Add(B);

            return P;
        }
    }
}

