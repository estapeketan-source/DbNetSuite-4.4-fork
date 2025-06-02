using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

namespace DbNetLink.DbNetSuite
{
    public class CSS : System.Web.UI.Page, IHttpHandler, IRequiresSessionState
    {
        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            HttpCachePolicy HCP = context.Response.Cache;
            HCP.SetCacheability(HttpCacheability.Public);

            context.Response.ContentType = "text/css";
            Shared.SetCulture(context.Request);

            List<string> CSSFiles = new List<string>();

            Dictionary<string, string> jQueryUiImages = LoadJQueryUIImages();

            const string jQueryUI_1_11_2 = "jquery-ui-1.11.2.custom";
            const string jQueryUI_1_13_2 = "jquery-ui-1.13.2.structure.min";

            string jqueryVersion = Shared.ConfigValue("jQueryVersion");

            string jQueryUI = jQueryUI_1_11_2;

            if (jqueryVersion == "3.6.1")
            {
                jQueryUI = jQueryUI_1_13_2;
            }

            string Mode = "";
            UI.Themes Theme = UI.Themes.Classic;

            if (context.Request.QueryString["mode"] != null)
                Mode = context.Request.QueryString["mode"].ToString().ToLower();

            string ThemeString = String.Empty;
            if (context.Request.QueryString["theme"] != null)
                ThemeString = context.Request.QueryString["theme"].ToString();
            else
                ThemeString = ConfigValue("Theme");

            if (ThemeString != String.Empty)
            {
                try
                {
                    Theme = (UI.Themes)Enum.Parse(typeof(UI.Themes), ThemeString, true);
                }
                catch (Exception) { }
            }

            //if (context.Session["DbNetSuiteTheme"] is string)
            //    if (context.Session["DbNetSuiteTheme"].ToString().ToLower() != Theme.ToString().ToLower())
            //        hours = 0;

            context.Session["DbNetSuiteTheme"] = Theme.ToString();

            HCP.SetMaxAge(new TimeSpan(12, 0, 0)); 

            CSSFiles.Add(jQueryUI);

            CSSFiles.Add("DbNetSuite");

            if (Theme != UI.Themes.Bootstrap)
            {
                CSSFiles.Add("DbNetGrid");
                CSSFiles.Add("DbNetEdit");
                CSSFiles.Add("DbNetFile");
                CSSFiles.Add("DbNetCombo");
                CSSFiles.Add("DbNetList");
            }
            CSSFiles.Add("SuggestLookup");
            CSSFiles.Add("jquery.placeholder");
            CSSFiles.Add("ViewDialog"); 
            
            StringBuilder CSS = new StringBuilder();
            foreach (string FileName in CSSFiles.ToArray())
            {
                string CSSText = GetCSSResource(FileName);

                switch (FileName)
                {
                    case jQueryUI_1_11_2:
                    case jQueryUI_1_13_2:
                        if (Shared.ConfigValue("jQueryUINoInclude").ToLower() == "true" || Mode == "nojquery")
                            continue;

                        //CSSText = Regex.Replace(CSSText, @"url\(images/ui-bg.*\)", "none");

                        foreach (string Key in jQueryUiImages.Keys)
                            CSSText = CSSText.Replace(Key, jQueryUiImages[Key]);
                        break;
                    default:
                        CSSText = ConvertImageUrls(CSSText);
                        break;
                }

                CSS.Append(CSSText);
                CSS.Append(Environment.NewLine);
            }

            string[] Controls = { "dbnetfile", "dbnetlist" };
            string[] ImageNames = {"tree-last-node-close","tree-last-node-open","tree-last-node","tree-node-close","tree-node-open","tree-node","tree-vertical-line"};

            foreach (string Control in Controls)
                foreach (string ImgName in ImageNames)
                {
                    CSS.Append("." + Control + " ." + ImgName + " {background-image:url('" + Page.ClientScript.GetWebResourceUrl(Page.GetType(), "DbNetLink.Resources.Images.Tree." + ImgName + ".gif").Replace(" ", "%20") + "');}" + Environment.NewLine);
                    if (ImgName == "tree-vertical-line")
                        CSS.Append("." + Control + " ." + ImgName + " {background-repeat:repeat-y;}" + Environment.NewLine);
                    else
                        CSS.Append("." + Control + " ." + ImgName + " {background-repeat:no-repeat;}" + Environment.NewLine);
                }

            if (Shared.ConfigValue("jQueryUINoInclude").ToLower() != "true" && Mode != "nojquery")
            {
                CSS.Append(".ui-dialog { border: 5px solid #4C94EA; background: #dcdcdc url(" + ImageUrl("jQueryUI.ui-bg_flat_75_dcdcdc_40x100.png") + @") 50% 50% repeat-x; }" + Environment.NewLine);
                CSS.Append(".ui-dialog .ui-dialog-content { padding: 2px; overflow: hidden; }" + Environment.NewLine);

                //CSS.Append(".ui-dialog .ui-widget-content { background-color:#dcdcdc; }" + Environment.NewLine);

                CSS.Append(".ui-dialog .ui-widget-content { background: #dcdcdc url(" + ImageUrl("jQueryUI.ui-bg_flat_75_dcdcdc_40x100.png") + @") 50% 50% repeat-x; }" + Environment.NewLine);
                
                CSS.Append(".ui-datepicker { z-index:99999; background-color:#eeeeee; border:1pt solid #999999;}" + Environment.NewLine);
                CSS.Append(".ui-datepicker { position: absolute !important; clip: auto; clip: auto; }" + Environment.NewLine);
                CSS.Append(".ui-widget-header { background-color: #CCCCCC; }" + Environment.NewLine);
                CSS.Append(".ui-widget-overlay { background: #aaaaaa url(" + ImageUrl("jQueryUI.ui-bg_flat_0_aaaaaa_40x100.png") + ") 50% 50% repeat-x; opacity: .30;filter:Alpha(Opacity=30); }" + Environment.NewLine);
                CSS.Append(".ui-menu { border:1pt solid #AAAAAA; }" + Environment.NewLine);
                CSS.Append(".ui-menu .ui-menu-item a {	padding:.2em .2em;	line-height:1;}" + Environment.NewLine);
            }

            switch (Theme)
            {
                case UI.Themes.Light:
                case UI.Themes.Dark:
                case UI.Themes.Bootstrap:
                    CSS.Append(ConvertImageUrls(GetCSSResource("Themes." + Theme.ToString().ToLower())));
                    CSS.Append(Environment.NewLine);

                    if (Theme == UI.Themes.Bootstrap)
                    {
                        CSS.Append(ConvertImageUrls(GetCSSResource("Themes.bootstrap.mod")));
                        CSS.Append(Environment.NewLine);
                    }

                    break;
            }

            context.Response.Write(CSS.ToString());
        }

        ///////////////////////////////////////////////
        public string ConvertImageUrls(string CSS)
        ///////////////////////////////////////////////
        {
            MatchCollection Matches = Regex.Matches(CSS.ToString(), @"url\((.*)\)", RegexOptions.IgnoreCase);
            foreach (Match M in Matches)
            {
                string ImgName = M.Groups[1].Value; 
                if (!ImgName.Contains("/"))
                    CSS = CSS.Replace("(" + ImgName + ")", "(" + ImageUrl(ImgName) + ")");
            }

            return CSS;
        }

        ///////////////////////////////////////////////
        public bool IsReusable
        ///////////////////////////////////////////////
        {
            get { return false; }
        }

        ///////////////////////////////////////////////
        public string UpdateImageUrls(TextReader TR)
        ///////////////////////////////////////////////
        {
            string CSS = TR.ReadToEnd();

            CSS = CSS.Replace("images/treeview-default.gif", ImageUrl("treeview-default.gif"));
            CSS = CSS.Replace("images/treeview-default-line.gif", ImageUrl("treeview-default-line.gif"));
            CSS = CSS.Replace("images/treeview-default.gif", ImageUrl("treeview-default.gif"));
            CSS = CSS.Replace("images/folder.gif", ImageUrl("FolderOpen.png"));
            CSS = CSS.Replace("images/folder-closed.gif", ImageUrl("FolderClosed.png"));
            CSS = CSS.Replace("images/file.gif", ImageUrl("Ext.default.gif"));

            return CSS;
        }

        ///////////////////////////////////////////////
        public string GetCSSResource(string FileName)
        ///////////////////////////////////////////////
        {
            System.Globalization.CultureInfo C = System.Threading.Thread.CurrentThread.CurrentUICulture;
            Assembly A = Assembly.GetExecutingAssembly();

            Stream St = A.GetManifestResourceStream("DbNetLink.Resources.CSS." + FileName + ".css");
            if (St == null)
                return "";

            TextReader TR = new StreamReader(St);

            string S = TR.ReadToEnd();
            TR.Close();

            if (C.TextInfo.IsRightToLeft)
                S = S.Replace("{", "{" + Environment.NewLine + "direction:rtl;" + Environment.NewLine);
            return S;
        }

        ///////////////////////////////////////////////
        public string ImageUrl(string ImageName)
        ///////////////////////////////////////////////
        {
            return Page.ClientScript.GetWebResourceUrl(Page.GetType(), "DbNetLink.Resources.Images." + ImageName).Replace(" ","%20");
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string ConfigValue(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            Key = "DbNetSuite" + Key;
            string V = ConfigurationManager.AppSettings[Key];
            return (V == null) ? String.Empty : V;
        }

        ///////////////////////////////////////////////
        private Dictionary<string, string> LoadJQueryUIImages()
        ///////////////////////////////////////////////
        {
            List<string> ImageNames = new List<string>();
            Dictionary<string, string> jQueryUiImages = new Dictionary<string, string>();

            ImageNames.Add("ui-bg_flat_0_aaaaaa_40x100.png");
            ImageNames.Add("ui-bg_flat_75_ffffff_40x100.png");
            ImageNames.Add("ui-bg_glass_55_fbf9ee_1x400.png");
            ImageNames.Add("ui-bg_glass_75_dadada_1x400.png");
            ImageNames.Add("ui-bg_glass_75_e6e6e6_1x400.png");
            ImageNames.Add("ui-bg_glass_95_fef1ec_1x400.png");
            ImageNames.Add("ui-bg_highlight-soft_75_cccccc_1x100.png");
            ImageNames.Add("ui-icons_2e83ff_256x240.png");
            ImageNames.Add("ui-icons_454545_256x240.png");
            ImageNames.Add("ui-icons_888888_256x240.png");
            ImageNames.Add("ui-icons_cd0a0a_256x240.png");

            ImageNames.Add("ui-icons_222222_256x240.png");
            ImageNames.Add("ui-bg_glass_65_ffffff_1x400.png");

            foreach (string Key in ImageNames)
                jQueryUiImages.Add("images/" + Key, Page.ClientScript.GetWebResourceUrl(Page.GetType(), "DbNetLink.Resources.Images.jQueryUI." + Key).Replace(" ", "%20"));

            return jQueryUiImages;
        }
    }
}
