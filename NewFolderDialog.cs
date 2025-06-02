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
    internal class NewFolderDialog
    ///////////////////////////////////////////////
    {
        internal DbNetFile ParentControl;

        ///////////////////////////////////////////////
        public NewFolderDialog(DbNetFile PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite new-folder-dialog";
            T.ToolTip = Translate("NewFolder");
            T.Controls.Add( FolderNameContent() );
            T.Controls.Add( Toolbar() );
            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        protected TableRow FolderNameContent()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.Controls.Add(FolderNameInput());

            Panel P = new Panel();
            P.GroupingText = Translate("FolderName");
            TableCell TC = new TableCell();
            P.Controls.Add(T);
            TC.Controls.Add(P);
            TableRow TR = new TableRow();
            TR.Controls.Add(TC);

            return TR;
        }

        ///////////////////////////////////////////////
        private TableRow FolderNameInput()
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TableCell TC = new TableCell();
            TextBox TB = new TextBox();
            TB.ID = "new_folder_name";
            TB.CssClass = "new-folder-name";
            TB.Width = 300;
            TC.Controls.Add(TB);
            TR.Controls.Add(TC);
           
            return TR;
        }
 
        ///////////////////////////////////////////////
        private TableRow Toolbar()
        ///////////////////////////////////////////////
        {
            TableRow TR = new TableRow();
            TableCell TC = new TableCell();
            TC.CssClass = "toolbar-container";
            TC.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            TC.Controls.Add(ParentControl.BuildButton("apply", "Apply", "apply", ""));
            TC.Controls.Add(ParentControl.BuildButton("cancel", "Cancel", "undo", ""));
            TR.Controls.Add(TC);
            return TR;
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

