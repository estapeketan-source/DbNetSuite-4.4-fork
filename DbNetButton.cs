using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System;
using System.Text.RegularExpressions;

///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{

    ///////////////////////////////////////////////
    internal class DbNetButton : HtmlButton
    ///////////////////////////////////////////////
    {
        public string ImageUrl = "";
        public string Title = "";
        public string Text = "";

        ///////////////////////////////////////////////
        public DbNetButton(string ImgUrl, System.Web.HttpRequest Request, Page PageRef)
            : this(ImgUrl, "", "", Request, PageRef)
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public DbNetButton(string ImageUrl, string Text, string Title, System.Web.HttpRequest Request, Page PageRef)
            : this(ImageUrl, Text, Title, Request, (Text == String.Empty) ? GridEditControl.ToolButtonStyles.Image : GridEditControl.ToolButtonStyles.ImageAndText, PageRef)
        ///////////////////////////////////////////////
        {
        }

        ///////////////////////////////////////////////
        public DbNetButton(string ImageUrl, string Text, string Title, System.Web.HttpRequest Request, GridEditControl.ToolButtonStyles ButtonStyle, Page PageRef)
        ///////////////////////////////////////////////
        {
            this.Attributes.Add("type", "button");

            this.ImageUrl = ImageUrl;
            this.Text = Text;
            this.Title = Title;

            string Browser = (HttpContext.Current.Handler as Shared).GetBrowser();

            switch(Browser)
            {
                case "msie":
                    break;
                case "mozilla":
                    this.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                    break;
                default:
                    this.Style.Add(HtmlTextWriterStyle.Padding, "2px");
                    break;
            }                        


            this.Attributes.Add("title", Title);

            if (ImageUrl == "")
            {
                Label L = new Label();
                L.EnableViewState = false;
                L.Text = Text;
                Controls.Add(L);
                return;
            }

            if (ImageUrl.Length > 20 || IsMobileDevice(Request))
            {
                if (ImageUrl.Length <= 20)
                    ImageUrl = PageRef.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImageUrl + ".png");

                Table T = new Table();
                T.CssClass = "button-table";

                this.Controls.Add(T);
                T.CellPadding = 0;
                T.CellSpacing = 0;

                TableRow row = new TableRow();
                T.Controls.Add(row);

                TableCell C = new TableCell();
                row.Controls.Add(C);
                C.EnableViewState = false;

                Image IM = new Image();
                C.Controls.Add(IM);
                IM.ImageUrl = ImageUrl;
                IM.AlternateText = Title;

                switch (ButtonStyle)
                {
                    case GridEditControl.ToolButtonStyles.Text:
                        C.Style.Add(HtmlTextWriterStyle.Display, "none");
                        break;
                    case GridEditControl.ToolButtonStyles.ImageAndText:
                        if (Text != "")
                        {
                            C = new TableCell();
                            row.Controls.Add(C);
                            C.Style.Add(HtmlTextWriterStyle.Padding, "3px");
                        }
                        break;
                }

                C = new TableCell();
                row.Controls.Add(C);
                C.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                C.CssClass = "tool-button-text";

                switch (ButtonStyle)
                {
                    case GridEditControl.ToolButtonStyles.ImageAndText:
                    case GridEditControl.ToolButtonStyles.Text:
                        C.Text = Text;
                        break;
                }
            }
            else
            {
                HtmlGenericControl Span = new HtmlGenericControl("div");

                if (ButtonStyle == Shared.ToolButtonStyles.Image)
                {
                    Span.Style.Add(HtmlTextWriterStyle.Width, "16px");
                    Span.Style.Add(HtmlTextWriterStyle.Height, "16px");
                }

                Span.Style.Add(HtmlTextWriterStyle.Margin, "0px");
                Span.Style.Add(HtmlTextWriterStyle.Padding, "0px");

                if (ButtonStyle != Shared.ToolButtonStyles.Text)
                    Span.Attributes.Add("class", "tool-button-text sprite sprite-" + ImageUrl);

                Span.InnerHtml = "&nbsp;";

                switch (ButtonStyle)
                {
                    case GridEditControl.ToolButtonStyles.Text:
                        Span.InnerHtml = Text;
                        break;
                    case GridEditControl.ToolButtonStyles.ImageAndText:
                        Span.Style.Add(HtmlTextWriterStyle.PaddingLeft, "20px"); 
                        Span.InnerHtml = Text;
                        break;
                }
                this.Controls.Add(Span);
                if (ButtonStyle == Shared.ToolButtonStyles.Image)
                {
                    this.Style.Add(HtmlTextWriterStyle.Width, "24px");
                    this.Style.Add(HtmlTextWriterStyle.Height, "24px");
                }

                switch (Browser)
                {
                    case "msie":
                        this.Style.Add(HtmlTextWriterStyle.Padding, "2px");
                        break;
                    case "mozilla":
                        this.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                        if (ButtonStyle == Shared.ToolButtonStyles.Image)
                        {
                            this.Style.Add(HtmlTextWriterStyle.Width, "26px");
                            this.Style.Add(HtmlTextWriterStyle.Height, "26px");
                        }
                        break;
                    default:
                        this.Style.Add(HtmlTextWriterStyle.Padding, "2px");
                        break;
                }

                if ((HttpContext.Current.Handler as Shared).GetTheme() == UI.Themes.Bootstrap)
                {
                    this.Style.Add(HtmlTextWriterStyle.PaddingLeft, "4px");

                    switch (ButtonStyle)
                    {
                        case GridEditControl.ToolButtonStyles.Text:
                        case GridEditControl.ToolButtonStyles.ImageAndText:
                            this.Style.Add(HtmlTextWriterStyle.PaddingRight, "4px");
                            break;
                    }

                    this.Style.Add(HtmlTextWriterStyle.Height, "30px");
                }
            }
        }

        ///////////////////////////////////////////////
        public bool IsMobileDevice(System.Web.HttpRequest Request)
        ///////////////////////////////////////////////
        {
            string UserAgent = Request.UserAgent.ToLower();
            return (UserAgent.Contains("blackberry") || UserAgent.Contains("iphone") || UserAgent.Contains("android") || UserAgent.Contains("webos") || UserAgent.Contains("opera mini") || UserAgent.Contains("opera mobi") || UserAgent.Contains("windows phone os"));
        }
    }
}