using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#if (!x64)
using System.Web.UI.DataVisualization.Charting;
#endif
using System.Drawing;
using System.Drawing.Text;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    ///////////////////////////////////////////////
    internal class ChartConfigDialog
    ///////////////////////////////////////////////
    {
        internal DbNetGrid ParentControl;

        ///////////////////////////////////////////////
        public ChartConfigDialog(DbNetGrid PC)
        ///////////////////////////////////////////////
        {
            this.ParentControl = PC;
        }

        ///////////////////////////////////////////////
        internal Table Build()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            T.CssClass = "dbnetsuite chart-config-dialog";
            T.ToolTip = Translate("ChartConfig");

            TableRow R = new TableRow();
            T.Rows.Add(R);
            TableCell C = new TableCell();
            R.Cells.Add(C);

            Panel P = new Panel();
            C.Controls.Add(P);
            P.CssClass = "chart-config-panel";

            P.Controls.Add(ChartPropertiesTable());

            AddButtonsRow(T);
            ParentControl.AddMessageRow(T);
            AddDialogRow(T);

            return T;
        }

        ///////////////////////////////////////////////
        internal Table ChartPropertiesTable()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            TableRow R;

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(ChartProperties());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(ChartSeries());
            R.Cells.Add(SeriesSelection());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(ChartAreaProperties());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(Chart3DProperties());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(BorderSkinProperties());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(ChartTitle());

            R = new TableRow();
            T.Rows.Add(R);
            R.Cells.Add(ChartLegend());


            return T;
        }

        ///////////////////////////////////////////////
        private TableCell ChartProperties()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Chart Properties";

            Table T = new Table();
            T.CssClass = "chart";
            T.Attributes.Add("MaxRows", "3");
            C.Controls.Add(T);

            AddChartProperty(T, "Width", "Width of the chart");
            AddChartProperty(T, "Height", "Height of the chart");
            AddChartProperty(T, "AutoLoad", "Render the chart automatically when the grid loads");

            AddChartProperty(T, "BackColor", "Background color of chart");
            AddChartProperty(T, "BackSecondaryColor", "Secondary background color chart (when Gradient selected)");
            AddChartProperty(T, "ChartPanel", "ID of the element inside which the chart will be rendered");

            AddChartProperty(T, "BackGradientStyle", "Background gradient of chart (use with Alt.Color)");
            AddChartProperty(T, "BackHatchStyle", "Background hatch style of chart");
            AddChartProperty(T);

            F.Controls.Add(T);

            return C;
        }

        ///////////////////////////////////////////////
        private TableCell ChartAreaProperties()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Chart Plotting Area Properties";

            Table T = new Table();
            T.CssClass = "chart-area";
            T.Attributes.Add("MaxRows", "3");
            C.Controls.Add(T);

            AddChartProperty(T, "BackColor", "Background color of chart plotting area");
            AddChartProperty(T, "BackSecondaryColor", "Secondary background color of chart plotting area (when Gradient selected)");
            AddChartProperty(T, "", "");

            AddChartProperty(T, "BorderColor", "Border color of chart plotting area");
            AddChartProperty(T, "BorderDashStyle", "Style of the border line of chart plotting area");
            AddChartProperty(T, "BorderWidth", "Width of the border line of chart plotting area");

            AddChartProperty(T, "BackGradientStyle", "Background gradient style of chart plotting area (use with Alt.Color)");
            AddChartProperty(T, "BackHatchStyle", "Background hatch style of chart plotting area");
            AddChartProperty(T, "", "");

            F.Controls.Add(T);

            return C;
        }

        ///////////////////////////////////////////////
        private TableCell ChartSeries()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Chart Series";

            Table T = new Table();
            T.CssClass = "series";
            C.Controls.Add(T);
            T.Attributes.Add("MaxRows", "3");

            AddChartProperty(T, "ChartType", "The type of chart");
            AddChartProperty(T, "Palette", "The color palette of the chart");
            AddChartProperty(T, "BorderColor", "The series border color");
            AddChartProperty(T, "IsValueShownAsLabel", "Show values as labels on chart");
            AddChartProperty(T, "", "");
            AddChartProperty(T, "", "");

            F.Controls.Add(T);

            return C;
        }


        ///////////////////////////////////////////////
        private TableCell Chart3DProperties()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "3D Properties";

            Table T = new Table();
            T.CssClass = "three-d";
            T.Attributes.Add("MaxRows", "3");

            C.Controls.Add(T);
            AddChartProperty(T, "Enable3D", "Toggles the 3D on and off for a chart area");
            AddChartProperty(T, "IsClustered", "Determines whether data series for a bar or column chart are clustered; that is, displayed along distinct rows");
            AddChartProperty(T, "IsRightAngleAxes", "Determines whether a chart area is displayed using an isometric projection");

            AddChartProperty(T, "Inclination", "Angle of rotation around the horizontal axes for 3D chart areas");
            AddChartProperty(T, "Rotation", "Angle of rotation around the vertical axes for 3D chart areas");
            AddChartProperty(T, "Perspective", "Percent of perspective for a 3D chart area");

            AddChartProperty(T, "LightStyle", "Type of lighting for a 3D chart area");
            AddChartProperty(T, "PointDepth", "Depth of data points displayed in a 3D chart area");
            AddChartProperty(T, "PointGapDepth", "Distance between series rows in a 3D chart area");
  //          AddChartProperty(T, "WallWidth", "Width of the walls displayed in a 3D chart area");

            F.Controls.Add(T);

            return C;
        }

        ///////////////////////////////////////////////
        private TableCell ChartTitle()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Chart Title";

            Table T = new Table();
            T.CssClass = "title";
            C.Controls.Add(T);
            T.Attributes.Add("MaxRows", "2");

            AddChartProperty(T, "Text", "Chart title text");
            AddChartProperty(T, "Family", "Chart title font");

            AddChartProperty(T, "ForeColor", "Chart title color");
            AddChartProperty(T, "BackColor", "Chart title background color");

            AddChartProperty(T, "Alignment", "Chart title alignment");
            AddChartProperty(T, "Docking", "Chart title docking");
  //          AddChartProperty(T, "IsDockedInsideChartArea", "Chart title docked inside chart area");

            F.Controls.Add(T);

            return C;
        }

        ///////////////////////////////////////////////
        private TableCell ChartLegend()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Chart Legend";

            Table T = new Table();
            T.CssClass = "legend";
            C.Controls.Add(T);
            T.Attributes.Add("MaxRows", "3");

            AddChartProperty(T, "Enabled", "Show chart legend");
            AddChartProperty(T, "ForeColor", "Chart legend color");
            AddChartProperty(T, "BackColor", "Chart legend background color");

            AddChartProperty(T, "BorderColor", "Border color of legend");
            AddChartProperty(T, "BorderDashStyle", "Style of the border line of legend");
            AddChartProperty(T, "BorderWidth", "Width of the border line of legend");

            AddChartProperty(T, "LegendStyle", "Chart legend style");
            AddChartProperty(T, "TableStyle", "Chart legend table style");
            AddChartProperty(T, "Docking", "Chart legend docking position");
             
            F.Controls.Add(T);

            return C;
        }


        ///////////////////////////////////////////////
        private TableCell BorderSkinProperties()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.ColumnSpan = 2;
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            C.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = "Border Skin Properties";

            Table T = new Table();
            T.CssClass = "border-skin";
            C.Controls.Add(T);
            T.Attributes.Add("MaxRows","1");

            AddChartProperty(T, "SkinStyle", "Sets the style of a border skin");

            F.Controls.Add(T);

            return C;
        }


        ///////////////////////////////////////////////
        private void AddChartProperty(Table T)
        ///////////////////////////////////////////////
        {
            AddChartProperty(T, "", "");
        }

        ///////////////////////////////////////////////
        private void AddChartProperty(Table T, string Class, string Title)
        ///////////////////////////////////////////////
        {
#if (!x64)
            string CssClass = Class;
            TableRow R = new TableRow();

            int MaxRows = Convert.ToInt32(T.Attributes["MaxRows"]);

            if (T.Rows.Count == MaxRows)
            {
                R = null;
                foreach (TableRow Row in T.Rows)
                    if (Row.Cells.Count < T.Rows[0].Cells.Count )
                    {
                        R = Row;
                        break;
                    }

                if (R == null)
                    R = T.Rows[0];
            }

            T.Controls.Add(R);
            TableCell C = new TableCell();
            R.Controls.Add(C);
            C.Text = Shared.GenerateLabel(Class).Replace("3 D", "3D").Replace(" ", "&nbsp;");

            switch (Class)
            {
                case "IsDockedInsideChartArea":
                    C.Text = "Dock Inside Chart";
                    break;
                case "BorderDashStyle":
                    C.Text = "Border Style";
                    break;
                case "IsValueShownAsLabel":
                    C.Text = "Show Value";
                    break;
                case "Family":
                    C.Text = "Font";
                    break;
                case "BackGradientStyle":
                case "BackHatchStyle":
                case "BackSecondaryColor":
                    C.Text = C.Text.Replace("Back&nbsp;", "");
                    break;
            }

            C.Text = C.Text.Replace("&nbsp;Style", "");
            C.Text = C.Text.Replace("Secondary", "Alt.");

            if (Class == ""){
                C = new TableCell();
                R.Controls.Add(C);
                return;
            }

            C = new TableCell();
            R.Controls.Add(C);
            
            HtmlSelect S;
            ListItem LI;

            switch (Class)
            {
                case "Width":
                case "Height":
                    S = new HtmlSelect();
                    for (var i = 100; i < 2000; i += 20)
                    {
                        S.Items.Add(i.ToString() + "px");
                        if (i == 500)
                            S.Items[S.Items.Count - 1].Selected = true;
                    }
                    C.Controls.Add(S);
                    break;
                case "Enable3D":
                case "IsClustered":
                case "IsRightAngleAxes":
                case "Enabled":
                case "IsValueShownAsLabel":
                case "IsDockedInsideChartArea":
                case "IsTextAutoFit":
                case "AutoLoad":
                    HtmlInputCheckBox CB = new HtmlInputCheckBox();
                    C.Controls.Add(CB);
                    break;
                case "LightStyle":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(LightStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "Inclination":
                case "Rotation":
                    S = new HtmlSelect();
                    for (int i = -40; i < 50; i += 10)
                    {
                        S.Items.Add(i.ToString());
                        if (i == 0)
                            S.Items[S.Items.Count - 1].Selected = true;
                    }
                    C.Controls.Add(S);
                    break;
                case "PointDepth":
                case "PointGapDepth":
                    S = new HtmlSelect();
                    for (decimal i = 0; i <= 1000; i += 100)
                    {
                        S.Items.Add(new ListItem(i.ToString() + "%", i.ToString()));
                        if (i == 100)
                            S.Items[S.Items.Count - 1].Selected = true;          
                    }
                    C.Controls.Add(S);
                    break;
                case "WallWidth":
                case "BorderWidth":
                    S = new HtmlSelect();
                    for (decimal i = 1; i <= 30; i += 1)
                        S.Items.Add(i.ToString());
                    C.Controls.Add(S);
                    break;
                case "ShadowOffset":
                    S = new HtmlSelect();
                    for (decimal i = 0; i <= 10; i += 1)
                        S.Items.Add(i.ToString());
                    C.Controls.Add(S);
                    break;
                case "Perspective":
                    S = new HtmlSelect();
                    for (int i = 0; i <= 100; i += 10)
                    {
                        S.Items.Add(new ListItem(i.ToString() + "%", i.ToString()));
                    }    
                    C.Controls.Add(S);
                    break;
                case "ChartType":
                    S = new HtmlSelect();

                    foreach (string CT in Enum.GetNames(typeof(SeriesChartType)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "Palette":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(ChartColorPalette)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "BackColor":
                case "PageColor":
                case "BorderColor":
                case "ForeColor":
                case "BackSecondaryColor":
                    S = new HtmlSelect();
                    CssClass += " colour-select";
                    S.Style.Add(HtmlTextWriterStyle.Width, "120px");
                    System.Reflection.PropertyInfo[] PI = typeof(System.Drawing.Color).GetProperties();
                    LI = new ListItem("");
                    S.Items.Add(LI);
                    foreach (System.Reflection.PropertyInfo I in PI)
                        if (I.PropertyType == typeof(System.Drawing.Color) && I.Name != "Transparent")
                        {
                            LI = new ListItem(I.Name, I.Name);
                            LI.Attributes.Add("style", "background-color:" + I.Name);
                            S.Items.Add(LI);
                        }
                    C.Controls.Add(S);
                    break;
                case "BackGradientStyle":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(GradientStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "Alignment":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(ContentAlignment)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "Docking":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(Docking)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "BorderDashStyle":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(ChartDashStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "SkinStyle":
                    S = new HtmlSelect();
                    foreach (string CT in Enum.GetNames(typeof(BorderSkinStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "BackHatchStyle":
                    S = new HtmlSelect();
                    S.Style.Add(HtmlTextWriterStyle.Width, "120px");
                    foreach (string CT in Enum.GetNames(typeof(ChartHatchStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "Family":
                    C.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                    S = new HtmlSelect();
                    InstalledFontCollection Fonts = new InstalledFontCollection();
                    foreach (FontFamily F in Fonts.Families)
                        S.Items.Add(new ListItem(F.Name,F.Name.ToLower()));
                    C.Controls.Add(S);
                    S.Style.Add(HtmlTextWriterStyle.Width, "120px");
                    S = new HtmlSelect();
                    S.Attributes.Add("class", "Size");
                    for (decimal i = 8; i <= 48; i += 2)
                        S.Items.Add(i.ToString());
                    C.Controls.Add(S);
                    break;
                case "Text":
                case "ChartPanel":
                    HtmlInputText TB = new HtmlInputText();
                    TB.Size = 20;
                    C.Controls.Add(TB);
                    break;
                case "LegendStyle":
                    S = new HtmlSelect();
                    S.Style.Add(HtmlTextWriterStyle.Width, "120px");
                    foreach (string CT in Enum.GetNames(typeof(LegendStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
                case "TableStyle":
                    S = new HtmlSelect();
                    S.Style.Add(HtmlTextWriterStyle.Width, "120px");
                    foreach (string CT in Enum.GetNames(typeof(LegendTableStyle)))
                        S.Items.Add(CT);
                    C.Controls.Add(S);
                    break;
            }

            if (C.Controls.Count > 0)
            {
                Control Ctrl = C.Controls[0];
                if (Ctrl is HtmlControl)
                {
                    (C.Controls[0] as HtmlControl).Attributes.Add("class", CssClass);
                    (C.Controls[0] as HtmlControl).Attributes.Add("title", Title);
                }
            }
#endif
        }

        ///////////////////////////////////////////////
        private TableCell SeriesSelection()
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            C.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

            HtmlTable T = new HtmlTable();
            T.CellPadding = 0;
            T.CellSpacing = 0;
            HtmlTableRow Row = new HtmlTableRow();
            T.Rows.Add(Row);
            C.Controls.Add(T);

            AddAxisSelection(Row, "x");
            AddAxisSelection(Row, "y");

            return C;
        }

        ///////////////////////////////////////////////
        private void AddAxisSelection(HtmlTableRow R, string Axis)
        ///////////////////////////////////////////////
        {
            HtmlTableCell Cell = new HtmlTableCell();
            R.Cells.Add(Cell);
            HtmlGenericControl F = new HtmlGenericControl("fieldset");
            Cell.Controls.Add(F);
            HtmlGenericControl L = new HtmlGenericControl("legend");
            F.Controls.Add(L);
            L.InnerText = Axis.ToUpper() + "-Axis";

            HtmlSelect S = new HtmlSelect();
            S.Size = 3;
            S.Multiple = (Axis == "y");
            S.Attributes.Add("class", Axis + "-axis-column");
            S.Style.Add(HtmlTextWriterStyle.Width, "100px");
            F.Controls.Add(S);
        }

        ///////////////////////////////////////////////
        private void AddButtonsRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);

            TableCell C = new TableCell();
            C.CssClass = "chart-config-dialog-toolbar";
            C.Style.Add("border-top", "1pt solid #AAAAAA");
            R.Controls.Add(C);

            Table T = new Table();
            T.CellPadding = 0;
            T.CellSpacing = 0;
            T.Width = new Unit("100%");

            C.Controls.Add(T);
            R = new TableRow();
            T.Rows.Add(R);

            C = new TableCell();
            R.Controls.Add(C);
            C.Controls.Add(ParentControl.BuildButton("serialize", "Serialize", "Serialize.gif", ""));

            C = new TableCell();
            R.Controls.Add(C);
            C.Wrap = false;
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            C.Controls.Add(ParentControl.BuildButton("apply", "Apply", "apply", ""));
            C.Controls.Add(ParentControl.BuildButton("cancel", "Close", "Cancel", ""));
        }

        ///////////////////////////////////////////////
        private void AddDialogRow(Table PT)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            PT.Controls.Add(R);
            R.Style.Add(HtmlTextWriterStyle.Display, "none");
            TableCell C = new TableCell();
            R.Controls.Add(C);

            Panel P = new Panel();
            P.Width = new Unit("600px");

            P.CssClass = "serialize-dialog";
            P.ToolTip = "Chart Configuration Serialization";
            C.Controls.Add(P);

            Panel P2 = new Panel();
            P.Controls.Add(P2);
            P2.Width = new Unit("580px");

            HtmlGenericControl L = new HtmlGenericControl("div");
            L.Style.Add(HtmlTextWriterStyle.Padding, "5px");
            L.Style.Add(HtmlTextWriterStyle.BackgroundColor, "whitesmoke");
            L.Style.Add("border", "1pt solid #AAAAAA");
            L.Style.Add("line-height", "150%");
            if (this.ParentControl.ServerId != "")
                L.InnerHtml = "To pre-configure the grid to display the chart designed in this dialog paste the <b>ChartConfig</b> property definition below into the DbNetGrid server control configuration code. If you want to display the chart on the same page as the grid use the <b>chartPanel</b> property to specify the ID of the element inside which the chart will be rendered.";
            else
                L.InnerHtml = "To pre-configure the grid to display the chart designed in this dialog paste the code below into the client-side script along with the rest of the grid configuration code. If you want to display the chart on the same page as the grid use the <b>chartPanel</b> property to specify the ID of the element inside which the chart will be rendered.";
            P2.Controls.Add(L);

            TextBox TB = new TextBox();
            TB.TextMode = TextBoxMode.MultiLine;
            TB.Height = new Unit("300px");
            TB.Width = new Unit("580px");
            TB.CssClass = "serialize-textbox";
            P.Controls.Add(TB);
        }

        ///////////////////////////////////////////////
        private string Translate(string Key)
        ///////////////////////////////////////////////
        {
            return ParentControl.Translate(Key);
        }
    }
}

