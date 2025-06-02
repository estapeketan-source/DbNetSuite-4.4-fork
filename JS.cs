using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Web.SessionState; 

namespace DbNetLink.DbNetSuite
{
    public class JS : System.Web.UI.Page, IHttpHandler 
    {
        HttpContext Context = null;
        ///////////////////////////////////////////////
        public void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            HttpCachePolicy HCP = context.Response.Cache;
            HCP.SetCacheability(HttpCacheability.Public);
            HCP.SetMaxAge(new TimeSpan(12, 0, 0)); 

            this.Context = context;

            string UserLanguage = context.Request.QueryString["userlanguage"];

            if (String.IsNullOrEmpty(UserLanguage))
                Shared.SetCulture(context.Request);
            else
                Shared.SetCulture(context.Request, UserLanguage);

            context.Response.ContentType = "text/javascript";
            string Mode = "";

            if (context.Request.QueryString["mode"] != null)
                Mode = context.Request.QueryString["mode"].ToString().ToLower();

            const string jQueryUI_1_11_2 = "jquery-ui-1.11.2.custom.min";
            const string jQueryUI_1_13_2 = "jquery-ui-1.13.2.min";
            const string jQuery_1_1_8 = "jquery-1.8.1.min";
            const string jQuery_3_6_1 = "jquery-3.6.1.min";

            string jqueryVersion = Shared.ConfigValue("jQueryVersion");

            string jQuery = jQuery_1_1_8;
            string jQueryUI = jQueryUI_1_11_2;

            if (jqueryVersion == "3.6.1")
            {
                jQuery = jQuery_3_6_1;
                jQueryUI = jQueryUI_1_13_2;
            }

            List<string> ScriptFiles = new List<string>();

            ScriptFiles.Add(jQuery);
            ScriptFiles.Add("jquery.browser");
            ScriptFiles.Add("jquery.bgiframe.min");
            ScriptFiles.Add(jQueryUI); 
            ScriptFiles.Add("jquery.tooltip"); 
            ScriptFiles.Add("jquery.maxlength"); 
            ScriptFiles.Add("jquery.fixedheader");
            ScriptFiles.Add("jquery.placeholder");
            ScriptFiles.Add("jquery.hotkeys");
            ScriptFiles.Add("json2");
            ScriptFiles.Add("class"); 
            ScriptFiles.Add("to-title-case"); 
            ScriptFiles.Add("nicEdit");
            ScriptFiles.Add("DbNetLink");
            ScriptFiles.Add("DbNetSuite");
            ScriptFiles.Add("GridEditControl"); 
            ScriptFiles.Add("DbNetGrid"); 
            ScriptFiles.Add("DbNetEdit"); 
            ScriptFiles.Add("DbNetFile"); 
            ScriptFiles.Add("DbNetSpell"); 
            ScriptFiles.Add("DbNetCombo"); 
            ScriptFiles.Add("DbNetList"); 
            ScriptFiles.Add("Dialog"); 
            ScriptFiles.Add("MessageBox"); 
            ScriptFiles.Add("SearchDialog");
            ScriptFiles.Add("SearchPanel");
            ScriptFiles.Add("LookupDialog"); 
            ScriptFiles.Add("EditDialog"); 
            ScriptFiles.Add("ErrorDialog");
            if (Shared.AjaxUpload)
                ScriptFiles.Add("AjaxUploadDialog");
            ScriptFiles.Add("UploadDialog"); 
            ScriptFiles.Add("TextEditor");
            ScriptFiles.Add("HtmlEditor"); 
            ScriptFiles.Add("FilePreviewDialog"); 
            ScriptFiles.Add("FileSearchDialog"); 
            ScriptFiles.Add("FileSearchResultsDialog"); 
            ScriptFiles.Add("SuggestLookup"); 
            ScriptFiles.Add("AdvancedSearchDialog");
            ScriptFiles.Add("SimpleSearchDialog");
            ScriptFiles.Add("NewFolderDialog");
            ScriptFiles.Add("PdfSettingsDialog");
            ScriptFiles.Add("DbNetSpellDialog");
            ScriptFiles.Add("UserProfileDialog");
            ScriptFiles.Add("BrowseDialog");
            ScriptFiles.Add("ColumnPickerDialog");
            ScriptFiles.Add("ColumnSortDialog");
            ScriptFiles.Add("ConfigDialog");
            ScriptFiles.Add("ChartConfigDialog");
            ScriptFiles.Add("ChartDialog");
            ScriptFiles.Add("console");
            ScriptFiles.Add("AuditDialog");
            ScriptFiles.Add("ViewDialog");
            ScriptFiles.Add("DataUploadDialog");
            ScriptFiles.Add("clipboard.min");
            ScriptFiles.Add("jquery-ui-timepicker-addon");

            if (Mode == "office")
                ScriptFiles.Add("office");

            JavaScriptSupport.JavaScriptMinifier MinJS = new JavaScriptSupport.JavaScriptMinifier();

            StringBuilder Script = new StringBuilder();
            foreach (string FileName in ScriptFiles.ToArray())
            {
                string Code = GetResourceString(FileName);

                switch(FileName)
                {
                    case "DbNetGrid":
                    case "DbNetEdit":
                        if (Mode == "compat")
                        {
                            string S = GetResourceString(FileName + ".Compat.Properties");
                            Code = Code.Replace("/* legacy properties */", S);
                            S = GetResourceString(FileName + ".Compat.Code");
                            Code = Code.Replace("/* legacy code */", S);
                        }
                        break;
                    case jQuery_1_1_8:
                    case jQuery_3_6_1:
                        if ((Shared.ConfigValue("jQueryNoInclude").ToLower() == "true" || Mode == "nojquery") && Mode != "office")
                            continue;
                        break;
                    case jQueryUI_1_11_2:
                    case jQueryUI_1_13_2:
                        if ((Shared.ConfigValue("jQueryUINoInclude").ToLower() == "true" || Mode == "nojquery") && Mode != "office")
                            continue;
                        break;
                }

                switch (FileName)
                {
                    case "DbNetGrid":
                    case "DbNetEdit":
                    case "DbNetFile":
                    case "DbNetList":
                        Code = Code.Replace("/* column property names */", ListColumnProperties(FileName));
                        break;
                    case "DbNetSuite":
                        if (!String.IsNullOrEmpty(UserLanguage))
                            Code = Code.Replace("this.userLanguage = null;", "this.userLanguage = '" + UserLanguage + "';");
                        break;
                }   

                bool Minify = true;

                if (context.Request.QueryString["compress"] != null)
                    if (context.Request.QueryString["compress"].ToString() == "false")
                        Minify = false;

#if (!DEBUG)
                if (Minify)
                {
                    switch (FileName)
                    {
                        case jQueryUI_1_11_2:
                        case jQueryUI_1_13_2:
                        case jQuery_1_1_8:
                        case jQuery_3_6_1:
                            break;
                        default:
                            Code = MinJS.Minify(Code);
                            break;
                    }
                }
#endif

                Script.Append(Code);
                Script.Append(Environment.NewLine);
            }

            Script.Append("jQuery.extend(jQuery.ui.dialog.prototype.options, { bgiframe: true, autoOpen: false });" + Environment.NewLine);

            Script.Append("var dbnetsuite = {};" + Environment.NewLine);
            Script.Append("dbnetsuite.spellCheckPng = '" + ImageUrl("spellcheck.png") + "';" + Environment.NewLine);
            Script.Append("dbnetsuite.requiredImgUrl = '" + ImageUrl("Warning.gif") + "';" + Environment.NewLine);
            Script.Append("dbnetsuite.nicEditGif = '" + ImageUrl("nicEditorIcons.gif") + "';" + Environment.NewLine);
            Script.Append("dbnetsuite.mailMergeHta = '" + Page.ClientScript.GetWebResourceUrl(Page.GetType(), "DbNetLink.Resources.HTA.mailmerge.hta") + "';" + Environment.NewLine);
            Script.Append("dbnetsuite.serverTimeZoneOffset = " + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes.ToString() + ";" + Environment.NewLine);
            Script.Append("dbnetsuite.applicationPath = '" + context.Request.ApplicationPath + ((context.Request.ApplicationPath == "/") ? "" : "/") + "';" + Environment.NewLine);

            GetTranslations(Script);
            WriteDatePickerLocalisation(Script);
            WriteTinyMCELocation(Script);

            context.Response.Write(Script.ToString() + Environment.NewLine);
      
            context.Response.Write("//(c) 2009 Michael Manning (actingthemaggot.com)" + Environment.NewLine);
            context.Response.Write("jQuery.getAttributes=function(F,C){var F=((typeof F==='string')?jQuery(F)[0]:F[0]),D=0,F=F.attributes,B=F.length,E=['abort','blur','change','click','dblclick','error','focus','keydown','keypress','keyup','load','mousedown','mousemove','mouseout','mouseover','mouseup','reset','resize','select','submit','unload'],A={};for(D;D<B;D++){if(C||!C&&jQuery.inArray(F[D].nodeName.replace(/^on/,''),E)==-1){A[F[D].nodeName]=F[D].nodeValue}}return A}" + Environment.NewLine);

            if (Shared.ConfigValue("jQueryNoConflict").ToLower() == "true" )
                context.Response.Write("jQuery(document).ready(function(){jQuery.noConflict();});" + Environment.NewLine);
  
        }


        ///////////////////////////////////////////////
        internal string ListColumnProperties(string FileName)
        ///////////////////////////////////////////////
        {
            PropertyInfo[] PI = null;
            Column DefaultColumn = null;

            switch (FileName)
            {
                case "DbNetGrid":
                    PI = typeof(GridColumn).GetProperties();
                    DefaultColumn = new GridColumn();
                    break;
                case "DbNetEdit":
                    PI = typeof(EditColumn).GetProperties();
                    DefaultColumn = new EditColumn();
                    break;
                case "DbNetFile":
                    PI = typeof(FileColumn).GetProperties();
                    DefaultColumn = new FileColumn();
                    break;
                case "DbNetList":
                    PI = typeof(ListColumn).GetProperties();
                    DefaultColumn = new ListColumn();
                    break;
            }

            List<string> Properties = new List<string>();

            foreach (PropertyInfo I in PI)
            {
                string PropertyName = char.ToLower(I.Name[0]) + I.Name.Substring(1);
                Properties.Add(PropertyName);
            }

            return "this.columnPropertyNames = ['" + String.Join("','", Properties.ToArray()) + "'];";
        }

        ///////////////////////////////////////////////
        internal string GetResourceString(string FileName)
        ///////////////////////////////////////////////
        {
            try
            {
                Assembly A = Assembly.GetExecutingAssembly();
                TextReader TR = new StreamReader(A.GetManifestResourceStream("DbNetLink.Resources.Scripts." + FileName + ".js"));
                string S = TR.ReadToEnd();
                TR.Close();
                return S;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message + " [" + FileName + "]");
            }
        }

        ///////////////////////////////////////////////
        internal void GetTranslations(StringBuilder Script)
        ///////////////////////////////////////////////
        {
            ResourceManager RM = new ResourceManager("DbNetLink.Resources.Localisation.default", Assembly.GetExecutingAssembly(), null);
            Dictionary<string, string> Translations = new Dictionary<string, string>();
            ResourceSet RS = RM.GetResourceSet(new System.Globalization.CultureInfo(""),true,true);
            Script.Append("var DbNetSuiteText = {};" + Environment.NewLine);
            foreach (DictionaryEntry E in RS)
                Script.Append("DbNetSuiteText[\"" + E.Key.ToString() + "\"] = \"" + RM.GetString(E.Key.ToString()).Replace("\"", "'") + "\";" + Environment.NewLine);
        }

        ///////////////////////////////////////////////
        public bool IsReusable
        ///////////////////////////////////////////////
        {
            get { return false; }
        }

        ///////////////////////////////////////////////
        private void WriteDatePickerLocalisation(StringBuilder Script)
        ///////////////////////////////////////////////
        {
            System.Globalization.CultureInfo C = System.Threading.Thread.CurrentThread.CurrentUICulture;
 
            Script.Append(@"
DbNetLink.dateOptions = {    
    changeMonth : true,
    changeYear: true,
    yearRange: '-99:+10',
    closeText: '',
    prevText: '',
    nextText: '',
    currentText: ''," + Environment.NewLine);

            Script.Append("monthNames: ['" + String.Join("','", C.DateTimeFormat.MonthNames) + "']," + Environment.NewLine);
            Script.Append("monthNamesShort: ['" + String.Join("','", C.DateTimeFormat.AbbreviatedMonthNames) + "']," + Environment.NewLine);
            Script.Append("dayNames: ['" + String.Join("','", C.DateTimeFormat.DayNames) + "']," + Environment.NewLine);
            Script.Append("dayNamesMin: ['" + String.Join("','", C.DateTimeFormat.AbbreviatedDayNames) + "']," + Environment.NewLine);
            Script.Append("dateFormat: '" + C.DateTimeFormat.ShortDatePattern.ToLower().Replace("yyyy", "yy") + "'," + Environment.NewLine);
            Script.Append("isRTL: " + C.TextInfo.IsRightToLeft.ToString().ToLower() + "," + Environment.NewLine);

            Script.Append(@"
    firstDay: 1};");
            
            Script.Append(@"
jQuery(function(jQuery){
    jQuery.datepicker.setDefaults(DbNetLink.dateOptions);
    }
);" + Environment.NewLine);
        }

        ///////////////////////////////////////////////
        private void WriteTinyMCELocation(StringBuilder Script)
        ///////////////////////////////////////////////
        {
            string Folder = Shared.ConfigValue("TinyMCELocation");

            if (Folder == "")
                Folder = "~/tiny_mce/";

            string[] Files = { "tiny_mce_gzip.js", "tiny_mce.js", "tinymce.min.js" };

            foreach( string F in Files)
            {
                string Path = Folder + "/" + F;
                Path = Path.Replace("~", this.Context.Request.ApplicationPath).Replace("//", "/");

                if (File.Exists(this.Context.Request.MapPath(Path)))
                {
                    Script.Append("var DbNetSuiteTinyMCELocation = \"" + Path + "\";" + Environment.NewLine);
                    break;
                }
            }
        }

        ///////////////////////////////////////////////
        public string ImageUrl(string ImageName)
        ///////////////////////////////////////////////
        {
            return Page.ClientScript.GetWebResourceUrl(Page.GetType(), "DbNetLink.Resources.Images." + ImageName);
        }
    }
}
