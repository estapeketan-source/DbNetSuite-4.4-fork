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
    internal class UserProfileDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;

        ///////////////////////////////////////////////
        public UserProfileDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite user-profile-dialog";
            T.ToolTip = Translate("UserProfiles");

            AddProfileSave(T);
            AddProfileSelect(T);
            AddToolbar(T);

            ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private void AddProfileSave(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.GroupingText = Translate("SaveProfile");
            C.Controls.Add(P);

            Table T2 = new Table();
            P.Controls.Add(T2);

            R = new TableRow();
            T2.Rows.Add(R);
            C = new TableCell();
            C.ColumnSpan = 2;
            R.Controls.Add(C);

            TextBox TB = new TextBox();
            TB.Width = new Unit("400px");
            TB.ID = "userProfileName";
            TB.CssClass = "user-profile-name";
            C.Controls.Add(TB);

            R = new TableRow();
            T2.Rows.Add(R);

            C = new TableCell();
            R.Controls.Add(C);

            if (ParentControl.Database.ColumnExists(ParentControl.UserProfileTableName, ParentControl.UserProfileDefaultColumnName))
            {
                HtmlInputCheckBox Cb = new HtmlInputCheckBox();
                Cb.ID = ParentControl.Id + "_UserProfileDefaultDefaultCb";
                Cb.Attributes.Add("class","user-profile-default");
                Cb.Style.Add(HtmlTextWriterStyle.VerticalAlign, "text-bottom");
                C.Controls.Add(Cb);

                HtmlGenericControl Label = new HtmlGenericControl("label");
                Label.Attributes.Add("for",Cb.ID);
                Label.InnerText = ParentControl.Translate("Default");
                C.Controls.Add(Label);
            }

            C = new TableCell();
            R.Controls.Add(C);
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            C.Style.Add(HtmlTextWriterStyle.Width, "100%");
            C.Controls.Add(ParentControl.BuildButton("save", "Save", "save", ""));
        }

        ///////////////////////////////////////////////
        private void AddProfileSelect(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);
            Panel P = new Panel();
            P.GroupingText = Translate("SelectProfile");
            C.Controls.Add(P);

            Table T2 = new Table();
            P.Controls.Add(T2);
            R = new TableRow();

            T2.Rows.Add(R);
            C = new TableCell();
            C.ColumnSpan = 2;
            R.Controls.Add(C);

            DropDownList DDL = new DropDownList();
            DDL.Width = new Unit("400px");
            DDL.ID = "userProfileSelect";
            DDL.CssClass = "user-profile-select";
            C.Controls.Add(DDL);

            R = new TableRow();
            T2.Rows.Add(R);
            C = new TableCell();
            R.Controls.Add(C);
            C.Controls.Add(ParentControl.BuildButton("delete", "Delete", "delete", ""));

            C = new TableCell();
            R.Controls.Add(C);
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            C.Controls.Add(ParentControl.BuildButton("select", "Select", "apply", ""));
        }

        ///////////////////////////////////////////////
        private void AddToolbar(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);

            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            C.Style.Add(HtmlTextWriterStyle.PaddingRight, "5px");

            C.Controls.Add(ParentControl.BuildButton("close", "Close", "Cancel", ""));
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

