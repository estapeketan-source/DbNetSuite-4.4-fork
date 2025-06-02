using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class SearchDialog
    ///////////////////////////////////////////////
    {
        internal GridEditControl ParentControl;
        internal bool Dialog = true;

        ///////////////////////////////////////////////
        public SearchDialog(GridEditControl PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        public Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            T.CssClass = "dbnetsuite search-dialog table-condensed";
            T.ToolTip = ParentControl.Translate("StandardSearch");

            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);

            Panel P = new Panel();
            P.CssClass = "search-dialog-scroll-container";

            if (ParentControl.SearchDialogHeight != "")
            {
                P.Height = new Unit(ParentControl.SearchDialogHeight);
                P.Style.Add(HtmlTextWriterStyle.OverflowY, "scroll");
                P.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden ");
            }

            P.Controls.Add(BuildInputTable());
            C.Controls.Add(P);

            ParentControl.AddSearchOptionsRow(T);
            AddButtonsRow(T);
            if (Dialog)
                ParentControl.AddMessageRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        private Control BuildInputTable()
        ///////////////////////////////////////////////
        {
            Table T = new Table();


            int ColIdx = 0;

            if (this.ParentControl.SearchLayoutColumns < 1)
                this.ParentControl.SearchLayoutColumns = 1;

            int MaxRows = Convert.ToInt32(this.ParentControl.SearchColumns().Count / this.ParentControl.SearchLayoutColumns);

            if (this.ParentControl.SearchColumns().Count % this.ParentControl.SearchLayoutColumns != 0)
                MaxRows++;

            if (ParentControl.SearchColumnOrderAssigned)
                ParentControl.Columns.Sort(new SearchColumnSort());

            foreach (DbColumn column in ParentControl.Columns)
            {
                if (!column.Search)
                    continue;

                if (this.ParentControl.SearchLayoutColumns == 1)
                    T.Controls.Add(BuildSearchInputRow(column));
                else
                {
                    TableRow R;

                    if (ColIdx < MaxRows)
                        T.Controls.Add(new TableRow());

                    R = T.Rows[(ColIdx % MaxRows)];
                    TableCell C = new TableCell();
                    R.Cells.Add(C);
                    Table T2 = new Table();
                    C.Controls.Add(T2);
                    T2.Controls.Add(BuildSearchInputRow(column));
                }
                ColIdx++;
            }

            return T;
        }

        public class SearchColumnSort : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return (x as DbColumn).SearchColumnOrder - (y as DbColumn).SearchColumnOrder;
            }
        }

        ///////////////////////////////////////////////
        private TableRow BuildSearchInputRow(DbColumn column)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();

            R.ID = "searchRow" + column.ColumnKey;
            R.CssClass = "search-row";
            R.Attributes.Add("columnIndex", column.ColumnIndex.ToString());
            R.Attributes.Add("columnKey", column.ColumnKey);
            R.Attributes.Add("columnName", column.ColumnName);

            if (column.Lookup != "" || column.SearchLookup != "")
                R.Attributes.Add("lookupMode", column.LookupSearchMode.ToString());
            else
                R.Attributes.Add("lookupMode", "none");

            TableCell C = new TableCell();
            R.Controls.Add(C);
            C.CssClass = "search-label";
            C.Text = column.Label;
            C.Wrap = false;

            C = new TableCell();
            R.Controls.Add(C);

            DropDownList D = new DropDownList();

            if (this.ParentControl.SearchValuesOnly)
                D.Style.Add(HtmlTextWriterStyle.Display, "none");

            C.Controls.Add(D);
            D.ID = "searchOperator" + column.ColumnKey;
            D.CssClass = "search-operator";
            D.Attributes.Add("dataType", column.DataType);

            D.Attributes.Add("columnIndex", column.ColumnIndex.ToString());
            D.Width = Unit.Pixel(150);

            AddSearchOperators(column, D);

            C = new TableCell();
//            C.Width = new Unit("400px");
            R.Controls.Add(C);
//            C.ColumnSpan = 3;
            C.Wrap = false;

            Table T = new Table();
            T.CellSpacing = 0;
            T.CellPadding = 0;
            C.Controls.Add(T);
            TableRow R1 = new TableRow();
            R1.CssClass = "search-criteria";
            T.Controls.Add(R1);

            InputControl(R1, 1, column);

            C = new TableCell();
            C.Style.Add(HtmlTextWriterStyle.Display, "none");
            C.CssClass = "between-info";
            R1.Controls.Add(C);

            Label L = new Label();
            C.Controls.Add(L);
            L.Text = "&nbsp;" + Translate("And") + "&nbsp;";

            InputControl(R1, 2, column);

            return R;
        }

        ///////////////////////////////////////////////
        private void InputControl(TableRow R, int Idx, DbColumn column)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            R.Controls.Add(C);

            if (Idx == 2)
            {
                C.Style.Add(HtmlTextWriterStyle.Display, "none");
                C.CssClass = "between-info";
            }

            C.Wrap = false;

            TextBox TB = new TextBox();
            C.Controls.Add(TB);
            TB.ID = this.ParentControl.Id + "_search_input" + Idx.ToString() + column.ColumnKey;
            TB.CssClass = "input-control";

            TB.Attributes.Add("columnIndex", column.ColumnIndex.ToString());
            TB.Attributes.Add("columnKey", column.ColumnKey);
            TB.Attributes.Add("dataType", column.DataType);
            TB.Attributes.Add("alt", column.Label);
            TB.Attributes.Add("lookupType", column.EditControlType.ToString());
            TB.Attributes.Add("format", column.Format);

            if (column.DataType == "Boolean")
                TB.Style.Add(HtmlTextWriterStyle.Display, "none");

            TB.Width = Unit.Pixel(240);

            if (column.DataType.Equals("DateTime"))
                R.Controls.Add(AddButton(BuildButton("calendar", "SelectDate"), Idx));

            if ((column.Lookup != "" || column.SearchLookup != "") && column.LookupSearchMode == DbColumn.LookupSearchModeValues.SearchValue)
                R.Controls.Add(AddButton(BuildButton("lookup", "OpenTheLookupWindow"), Idx));

        }

        ///////////////////////////////////////////////
        internal TableCell AddButton(DbNetButton B, int Idx)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.Controls.Add(B);
            if (Idx == 2)
            {
                C.Style.Add(HtmlTextWriterStyle.Display, "none");
                C.CssClass = "between-info";
            }
            return C;
        }

        ///////////////////////////////////////////////
        internal DbNetButton BuildButton(string ID, string Title)
        ///////////////////////////////////////////////
        {
            //string ImageUrl = this.ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ID + ".png");
            DbNetButton B = new DbNetButton(ID, "", Translate(Title), this.ParentControl.Context.Request, this.ParentControl.PageRef);

            if (ParentControl.GetTheme() == UI.Themes.Bootstrap)
            {
                B.Style.Add(HtmlTextWriterStyle.Height, "30px");
                B.Attributes.Add("class", ID + "-button btn");
            }
            else
            {
                B.Style.Add(HtmlTextWriterStyle.Height, "24px");
                B.Attributes.Add("class", ID + "-button");
            }
            return B;
        }

        ///////////////////////////////////////////////
        internal DbNetButton DatePickerButton()
        ///////////////////////////////////////////////
        {
            //string ImageUrl = this.ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images.calendar.png");
            DbNetButton B = new DbNetButton("calendar", "", Translate("SelectDate"), this.ParentControl.Context.Request, this.ParentControl.PageRef);
            B.Attributes.Add("class", "calendar-button");
            return B;
        }

        ///////////////////////////////////////////////
        internal DbNetButton LookupButton(TextBox InputControl)
        ///////////////////////////////////////////////
        {
            //string ImageUrl = this.ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images.lookup.png");
            DbNetButton B = new DbNetButton("lookup", "", Translate("OpenTheLookupWindow"), this.ParentControl.Context.Request, this.ParentControl.PageRef);
            B.Attributes.Add("class", "lookup-button");
            return B;
        }


        ///////////////////////////////////////////////
        private void AddButtonsRow(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);
            R.CssClass = "search-toolbar-row";

            TableCell C = new TableCell();
            C.CssClass = "search-toolbar";
            R.Controls.Add(C);

            Table T1 = new Table();
            C.Controls.Add(T1);

            R = new TableRow();
            T1.Controls.Add(R);
            T1.Style.Add(HtmlTextWriterStyle.Width, "100%");

            C = new TableCell();
            R.Controls.Add(C);
            C.Controls.Add(AddToolbarButton("clear", "Clear", "refresh", "ClearTheSearchCriteria"));

            if (Dialog)
            {
                if (!ParentControl.Req.ContainsKey("advancedSearchDialog"))
                {
                    if (ParentControl.SimpleSearch)
                        R.Controls.Add(ParentControl.AddSearchDialogLink("Simple"));

                    if (ParentControl.AdvancedSearch)
                        R.Controls.Add(ParentControl.AddSearchDialogLink("Advanced"));
                }
            }

            C = new TableCell();
            R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            if (ParentControl.Req.ContainsKey("advancedSearchDialog"))
                C.Controls.Add(AddToolbarButton("apply", "Apply", "apply", "ApplyTheEnteredSearchCriteria"));
            else
                C.Controls.Add(AddToolbarButton("apply", "Search", "apply", "ApplyTheEnteredSearchCriteria"));

            if (Dialog)
                C.Controls.Add(AddToolbarButton("cancel", "Close", "undo", "CloseTheSearchWindow"));
        }


        ///////////////////////////////////////////////
        private DbNetButton AddToolbarButton(string ID, string Text, string Img, string Title)
        ///////////////////////////////////////////////
        {
            //string ImageUrl = this.ParentControl.PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + Img + ".png");
            DbNetButton B = new DbNetButton(Img, Translate(Text), Translate(Title), this.ParentControl.Context.Request,this.ParentControl.PageRef);

            if (ParentControl.GetTheme() == UI.Themes.Bootstrap)
                B.Attributes.Add("class", ID + "-button btn");
            else        
                B.Attributes.Add("class", ID + "-button");
            B.ID = ID + "Btn";
            return B;
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }


        ///////////////////////////////////////////////
        private void AddSearchOperators(DbColumn Column, DropDownList D)
        ///////////////////////////////////////////////
        {
            string DT = Column.DataType;

            if (Column.Lookup != "" || Column.SearchLookup != "")
                if (Column.LookupSearchMode == DbColumn.LookupSearchModeValues.SearchText)
                    DT = Column.LookupDataType;

            AddOperator("", "", D, "");

            if (ParentControl.IsXmlDataType(Column))
            {
                if (Column.XmlElementName == String.Empty)
                {
                    AddOperator("contains", Translate("Contains"), D, "like {0}");
                    AddOperator("does_not_contain", Translate("DoesNotContain"), D, "not like {0}");
                }
                else
                {
                    AddStringOperators(D);
                }
            }

            switch (Column.DbDataType.ToLower())
            {
                default:
                    switch (DT)
                    {
                        case "String":
                            AddStringOperators(D);
                            break;
                    }

                    switch (DT)
                    {
                        case "Byte":
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "Decimal":
                        case "Single":
                        case "Double":
                        case "DateTime":
                        case "TimeSpan":
                        case "String":
                        case "Guid":
                            AddOperator("equal_to", Translate("EqualTo"), D, "= {0}");
                            AddOperator("in", Translate("In"), D, "in ({0})");

                            //if (DT != "String")
                            //{
                                AddOperator("less_than", Translate("LessThan"), D, "< {0}");
                                AddOperator("greater_than", Translate("GreaterThan"), D, "> {0}");
                                AddOperator("between", Translate("Between"), D, "between {0} and {1}");
                            //}

                            AddOperator("not_equal_to", Translate("NotEqualTo"), D, "<> {0}");

                            //if (DT != "String")
                            //{
                                AddOperator("not_less_than", Translate("NotLessThan"), D, ">= {0}");
                                AddOperator("not_greater_than", Translate("NotGreaterThan"), D, "<= {0}");
                                AddOperator("not_between", Translate("NotBetween"), D, "not between {0} and {1}");
                            //}

                            AddOperator("not_in", Translate("NotIn"), D, "not in ({0})");
                            AddOperator("is_null", Translate("IsNull"), D, "is null");
                            AddOperator("is_not_null", Translate("IsNotNull"), D, "is not null");
                            break;
                        case "Boolean":
                            AddOperator("true", BooleanText("True", Column), D, "<> {0}");
                            AddOperator("false", BooleanText("False", Column), D, "= {0}");
                            break;
                    }
                    break;
                }
        }

        ///////////////////////////////////////////////
        private void AddStringOperators(DropDownList D)
        ///////////////////////////////////////////////
        {
            AddOperator("contains", Translate("Contains"), D, "like {0}");
            AddOperator("starts_with", Translate("StartsWith"), D, "like {0}");
            AddOperator("ends_with", Translate("EndsWith"), D, "like {0}");
            AddOperator("does_not_contain", Translate("DoesNotContain"), D, "not like {0}");
            AddOperator("does_not_start_with", Translate("DoesNotStartWith"), D, "not like {0}");
            AddOperator("does_not_end_with", Translate("DoesNotEndWith"), D, "not like {0}");
        }

        ///////////////////////////////////////////////
        private string BooleanText(string Key, DbColumn Column)
        ///////////////////////////////////////////////
        {
            if (Column is GridColumn)
            {
                GridColumn GC = Column as GridColumn;
                if (GC.BooleanDisplayMode != GridColumn.BooleanDisplayModeValues.TrueFalse)
                    return (Key == "True" ? Translate("Yes") : Translate("No"));
            }

            return Translate(Key);
        }

        ///////////////////////////////////////////////
        private void AddOperator(string value, string text, DropDownList D, string Operator)
        ///////////////////////////////////////////////
        {
            ListItem item = new ListItem(text, value);

            item.Attributes.Add("operator", Operator);

            if (!D.Items.Contains(item))
                D.Items.Add(item);
        }


    }
}

