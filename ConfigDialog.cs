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
    internal class ConfigDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public ConfigDialog(DbNetGrid PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite config-dialog";
            T.ToolTip = Translate("Config");

            T.Controls.Add(PageSize());
            T.Controls.Add(OutputPageSize());
            T.Controls.Add(OutputCurrentPage());
            T.Controls.Add(CustomSave());

            AddButtonsRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        internal TableRow PageSize()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.Text = Translate("PageSize");

            C = new TableCell();
            R.Cells.Add(C);

            HtmlSelect S = new HtmlSelect();
            S.Attributes.Add( "class", "page-size-set" );

            ListItem I;

            for (int i=5; i<50; i+=5)
            {
                I = new ListItem(i.ToString(), i.ToString());
                S.Items.Add(I);
            }

            for (int i = 50; i < 100; i += 10)
            {
                I = new ListItem(i.ToString(), i.ToString());
                S.Items.Add(I);
            }

            for (int i = 100; i < 200; i += 20)
            {
                I = new ListItem(i.ToString(), i.ToString());
                S.Items.Add(I);
            }

            for (int i = 200; i < 1001; i += 100)
            {
                I = new ListItem(i.ToString(), i.ToString());
                S.Items.Add(I);
            }

            I = new ListItem(Translate("AllRows"), Int32.MaxValue.ToString());
            S.Items.Add(I);

            C.Controls.Add(S);

            return R;
        }

        ///////////////////////////////////////////////
        internal TableRow OutputPageSize()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.Text = Translate("OutputPageSize");

            C = new TableCell();
            R.Cells.Add(C);

            HtmlSelect S = new HtmlSelect();
            S.Attributes.Add("class", "output-page-size-set");

            for (int i = 0; i < 101; i +=5)
            {
                ListItem I = new ListItem(i.ToString(), i.ToString());
                if (i == 0)
                    I.Text = Translate("NoPaging");
                S.Items.Add(I);
            }
            C.Controls.Add(S);

            return R;
        }

        ///////////////////////////////////////////////
        internal TableRow OutputCurrentPage()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.Text = Translate("OutputCurrentPageOnly");

            C = new TableCell();
            R.Cells.Add(C);

            HtmlInputCheckBox CB = new HtmlInputCheckBox();
            CB.Attributes.Add("class", "output-current-page-only-set");
            C.Controls.Add(CB);

            return R;
        }

        ///////////////////////////////////////////////
        internal TableRow CustomSave()
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.Text = Translate("CustomSave");

            C = new TableCell();
            R.Cells.Add(C);

            HtmlInputCheckBox CB = new HtmlInputCheckBox();
            CB.Attributes.Add("class", "custom-save-set");
            C.Controls.Add(CB);

            return R;
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.ColumnSpan = 2;
            C.CssClass = "config-toolbar";

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

