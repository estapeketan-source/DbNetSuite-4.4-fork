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
    internal class DbNetSpellDialog
    ///////////////////////////////////////////////
    {
        internal DbNetSpell ParentControl;

        ///////////////////////////////////////////////
        public DbNetSpellDialog(DbNetSpell PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite dbnetspell-dialog";
            T.ToolTip = Translate("SpellCheck");

            TableRow R = new TableRow();
            T.Rows.Add(R);
            TableCell C = new TableCell();
            R.Cells.Add(C);
            C.Text = "Not found";

            C = new TableCell();
            R.Cells.Add(C);
            TextBox TB = new TextBox();
            TB.CssClass = "not-found";
            TB.Width = new Unit(150);
            TB.ReadOnly = true;
            C.Controls.Add(TB);

            C = new TableCell();
            R.Cells.Add(C);
            C.RowSpan = 3;
            C.VerticalAlign = VerticalAlign.Top;
            C.Controls.Add(Buttons());

            R = new TableRow();
            T.Rows.Add(R);
            C = new TableCell();
            R.Cells.Add(C);
            C.Text = "Change To";

            C = new TableCell();
            R.Cells.Add(C);
            TB = new TextBox();
            TB.Width = new Unit(150);
            TB.CssClass = "change-to";
            C.Controls.Add(TB);

            R = new TableRow();
            T.Rows.Add(R);
            C = new TableCell();
            R.Cells.Add(C);
            C.Text = "Suggestions";
            C.VerticalAlign = VerticalAlign.Top;

            C = new TableCell();

            R.Cells.Add(C);
            ListBox LB = new ListBox();
            LB.Width = new Unit(150);
            LB.Height = new Unit(150);
            LB.CssClass = "suggestions";
            C.Controls.Add(LB);

            R = ParentControl.AddMessageRow(T);
            R.Cells[0].ColumnSpan = 3;

            return T;
        }


        ///////////////////////////////////////////////
        private Table Buttons()
        ///////////////////////////////////////////////
        {
            string[] ButtonNames = {"Replace", "Replace All", "Skip", "Skip All", "Add", "Cancel" };
            Table T = new Table();

            foreach (string B in ButtonNames)
            {
                TableRow R = new TableRow();
                T.Rows.Add(R);
                TableCell C = new TableCell();
                R.Cells.Add(C);

                Button Btn = new Button();
                Btn.Attributes.Add("type", "button");
                Btn.CssClass = B.ToLower().Replace(" ", "-");
                Btn.Width = new Unit(100);
                Btn.Text = B;
               
                C.Controls.Add(Btn);
            }

            return T;
        }



        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

