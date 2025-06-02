using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Script.Serialization;
using System.IO;
using System.Configuration;
using System.Data;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Resources;
using System.Globalization;
using System.Web.SessionState;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DbNetLink.Data;
using DbNetLink.Web.UI;

namespace DbNetLink.DbNetSuite
{
    public class Shared : System.Web.UI.Page, IHttpHandler, IRequiresSessionState
    {
        public enum ToolButtonStyles
        {
            Image,
            Text,
            ImageAndText
        };

        [DllImport("msvcrt.dll", SetLastError = true)]
        static extern int _mkdir(string path);

        internal JavaScriptSerializer JSON = new JavaScriptSerializer();
        internal Dictionary<string, object> Resp = new Dictionary<string, object>();
        internal Dictionary<string, object> ClientProperties = new Dictionary<string, object>();
        internal Dictionary<string, object> Req;
        internal HttpContext Context;
        internal Page PageRef;
        internal ResourceManager RM = null;
        internal static Hashtable MimeTypes = new Hashtable();
        internal BindingFlags BF = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public string Id = "";
        internal Plugins PlugInModules = null;
        internal string IE11UserAgentPattern = @"(?:\b(MS)?IE\s+|\bTrident\/7\.0;.*\s+rv:)(\d+)";
        internal string EdgeUserAgentPattern = @"Edge\/\d+";

        internal DbNetData Database;
        internal DbNetData DatabaseLookup;
        [
        Category("Database"),
        Description("The number of seconds a query will run before timing out (default is 30 seconds)")
        ]
        public bool CaseInsensitiveSearch;
        [
        Category("Database"),
        Description("Makes text searching case-insensitive where supporte")
        ]
        public int CommandTimeout;
        [
        Category("Database"),
        Description("The name of the connection string in the web.config file")
        ]
        public string ConnectionString = "";

        [
        Category("Configuration"),
        Description("Sets the culture in place of the default browser language")
        ]
        public string UserLanguage = "";

        [
        Category("Configuration"),
        Description("Sets the display CSS theme for the components")
        ]
        public UI.Themes Theme = UI.Themes.Classic;

        public Dictionary<string, object> Browser = new Dictionary<string, object>();

        internal string Method = "";
        internal int CallbackIndex = 0;
        internal string RequestToken = "";
        internal string ServerId = "";
        internal int TimezoneOffset = 0;
        internal DbNetLicense License;
        internal CultureInfo Culture;
        internal bool AntiXSS = false;
        internal bool CheckUserToken = false;

        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            JSON.MaxJsonLength = Int32.MaxValue;
            this.Context = context;
            this.PageRef = Page;
            this.PlugInModules = new Plugins(context);

            switch (Context.Request.RequestType)
            {
                case "GET":
                case "HEAD":
                    Req = new Dictionary<string, object>();
                    foreach (string Key in Context.Request.QueryString.Keys)
                        Req.Add(Key, Context.Request.QueryString[Key]);

                    if (Req.ContainsKey("cs"))
                        if (this is GridEditControl)
                            (this as GridEditControl).ConnectionString = Req["cs"].ToString();
                    return;
            }

            if (Context.Request.Form["data"] == null)
            {
                StreamReader SR = new StreamReader(Context.Request.InputStream);
                Req = (Dictionary<string, object>)JSON.DeserializeObject(SR.ReadToEnd());
                SR.Close();
            }
            else
            {
                Req = (Dictionary<string, object>)JSON.DeserializeObject(Context.Request.Form["data"]);
            }

            this.DeserialiseRequest();

            try
            {
                CheckUserToken = Convert.ToBoolean(ConfigValue("CheckUserToken"));
            }
            catch (Exception)
            {
            }

            if (this.CheckUserToken)
                if (Shared.GetUserToken() != this.RequestToken)
                    if (this.RequestToken == "")
                        throw new Exception("User token not supplied. Use <%=DbNetLink.DbNetSuite.Shared.WriteUserToken()%> to add the User Token to your page.");
                    else
                        throw new Exception("Invalid user token");

            try
            {
                AntiXSS = Convert.ToBoolean(ConfigValue("AntiXSS"));
            }
            catch (Exception)
            {
            }

            SetCulture(context.Request, this.UserLanguage);
            Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

            this.ClientProperties["cultureName"] = this.Culture.Name;

            Resp["clientProperties"] = this.ClientProperties;

            if (Req.ContainsKey("callbackIndex"))
                Resp["callbackIndex"] = Req["callbackIndex"];

            AssignTheme(context);

            context.Response.ContentType = "text/plain";

            if (Req.ContainsKey("method"))
            {
                switch (Req["method"].ToString())
                {
                    case "message-box":
                        Resp["html"] = BuildMessageBox();
                        break;
                    case "error-dialog":
                        Resp["html"] = BuildErrorDialog();
                        break;
                    case "ajax-upload-dialog":
                        Resp["html"] = BuildAjaxUploadDialog();
                        break;
                    case "upload-dialog":
                        Resp["html"] = BuildUploadDialog();
                        break;
                }
            }

#if (DBNETTIME || DBNETOFFICE || DBNETBUG)
            Resp["licenseMessage"] = "";
#else
            License = new DbNetLicense("DbNetSuite");
            License.Request = Context.Request;
            License.ApplyLicenseKey();
            if (License.LicenseKey == "")
            {
                License.Message = "License key not applied to <b>web.config</b> application setting <b>DbNetSuiteLicenseKey</b><br/><br/>" + License.EvalKeyLink;
            }
            Resp["licenseMessage"] = License.Message;
#endif
        }

        ///////////////////////////////////////////////
        internal void DeserialiseRequest()
        ///////////////////////////////////////////////
        {
            foreach (string PropName in Req.Keys)
            {
                switch (PropName)
                {
                    case "columns":
                        if (this is DbNetGrid)
                            (this as DbNetGrid).Columns = (GridColumnCollection)this.DeserialiseColumns();
                        else if (this is DbNetEdit)
                            (this as DbNetEdit).Columns = (DbColumnCollection)this.DeserialiseColumns();
                        else if (this is DbNetFile)
                            (this as DbNetFile).Columns = (FileColumnCollection)this.DeserialiseColumns();
                        else if (this is DbNetList)
                            (this as DbNetList).Columns = (ListColumnCollection)this.DeserialiseColumns();
                        break;
                    case "id":
                        this.Id = Req["id"].ToString();
                        break;
                    case "connectionString":
                    case "fromPart":
                    case "sql":
                        SetProperty(this, PropName, DbNetLink.Util.Decrypt(Req[PropName].ToString()));
                        break;
                    default:
                        SetProperty(this, PropName, Req[PropName]);
                        break;
                }

                /*
                Regex DateString = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$");
                Regex MozillaDateString = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z$");
                switch (PropName)
                {
                    case "fixedFilterParams":
                        if (this is GridEditControl)
                        {
                            Dictionary<string, object> P = new Dictionary<string, object>();
                            GridEditControl C = (this as GridEditControl);
                            foreach (string Key in C.FixedFilterParams.Keys)
                            {
                                string V = C.FixedFilterParams[Key].ToString();
                                if (DateString.IsMatch(V) || MozillaDateString.IsMatch(V))
                                    P[Key] = Convert.ToDateTime(V);
                                else
                                    P[Key] = C.FixedFilterParams[Key];
                            }

                     //       C.FixedFilterParams = P;        
                                     
                        }
                        break;
                }
                */

            }
        }


        ///////////////////////////////////////////////
        internal ColumnCollection DeserialiseColumns()
        ///////////////////////////////////////////////
        {
            object[] Columns = (object[])Req["columns"];
            FileColumnCollection FileCols = null;
            EditColumnCollection EditCols = null;
            GridColumnCollection GridCols = null;
            ListColumnCollection ListCols = null;

            if (this is DbNetGrid)
                GridCols = new GridColumnCollection();
            else if (this is DbNetEdit)
                EditCols = new EditColumnCollection();
            else if (this is DbNetFile)
                FileCols = new FileColumnCollection();
            else if (this is DbNetList)
                ListCols = new ListColumnCollection();

            foreach (Dictionary<string, object> Col in Columns)
            {
                if (this is DbNetGrid)
                    GridCols.Add(DeserialiseColumn(Col) as GridColumn);
                else if (this is DbNetEdit)
                    EditCols.Add(DeserialiseColumn(Col) as EditColumn);
                else if (this is DbNetFile)
                    FileCols.Add(DeserialiseColumn(Col) as FileColumn);
                else if (this is DbNetList)
                    ListCols.Add(DeserialiseColumn(Col) as ListColumn);
            }
            if (this is DbNetGrid)
                return GridCols as GridColumnCollection;
            else if (this is DbNetEdit)
                return EditCols as EditColumnCollection;
            else if (this is DbNetList)
                return ListCols as ListColumnCollection;
            else
                return FileCols;
        }

        ///////////////////////////////////////////////
        internal Column DeserialiseColumn(Dictionary<string, object> Col)
        ///////////////////////////////////////////////
        {
            Column C = null;
            if (this is DbNetGrid)
                C = new GridColumn();
            else if (this is DbNetEdit)
                C = new EditColumn();
            else if (this is DbNetFile)
                C = new FileColumn();
            else if (this is DbNetList)
                C = new ListColumn();

            foreach (string PropName in Col.Keys)
            {
                switch (PropName.ToLower())
                {
                    case "columnexpression":
                        SetProperty(C, PropName, DbNetLink.Util.Decrypt(Col[PropName].ToString()));
                        SetProperty(C, "ColumnExpressionKey", Col[PropName].ToString());
                        break;
                    case "lookup":
                        SetProperty(C, PropName, DbNetLink.Util.Decrypt(Col[PropName].ToString()));
                        break;
                    case "columntype":
                        string Val = Col[PropName].ToString();
                        try
                        {
                            Enum.Parse(typeof(DbNetFile.ColumnTypes), Val, true);
                            SetProperty(C, PropName, Val);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                Enum.Parse(typeof(DbNetFile.WindowsSearchColumnTypes), Val, true);
                                SetProperty(C, "WindowsSearchColumnType", Val);
                                SetProperty(C, PropName, "WindowsSearch");
                            }
                            catch (Exception)
                            {
                                Enum.Parse(typeof(DbNetFile.IndexingServiceColumnTypes), Val, true);
                                SetProperty(C, "IndexingServiceColumnType", Val);
                                SetProperty(C, PropName, "IndexingService");
                            }
                        }

                        break;
                    default:
                        SetProperty(C, PropName, Col[PropName]);
                        break;
                }
            }
            return C;
        }

        ///////////////////////////////////////////////
        internal Object[] SerialiseColumns()
        ///////////////////////////////////////////////
        {
            ArrayList ColumnList = new ArrayList();

            ColumnCollection Cols = null;

            if (this is DbNetGrid)
                Cols = (this as DbNetGrid).Columns;
            else if (this is DbNetEdit)
                Cols = (this as DbNetEdit).Columns;
            else if (this is DbNetFile)
                Cols = (this as DbNetFile).Columns;
            else if (this is DbNetList)
                Cols = (this as DbNetList).Columns;

            foreach (Column C in Cols)
            {
                Dictionary<string, object> Col = new Dictionary<string, object>();

                foreach (FieldInfo FI in C.GetType().GetFields())
                {
                    string Name = ClientSideName(FI.Name);
                    if (FI.FieldType.IsEnum)
                        Col[Name] = FI.GetValue(C).ToString();
                    else
                        Col[Name] = FI.GetValue(C);
                }

                foreach (PropertyInfo PI in C.GetType().GetProperties())
                {
                    string Name = char.ToLower(PI.Name[0]) + PI.Name.Substring(1);

                    if (PI.PropertyType.IsEnum)
                        Col[Name] = PI.GetValue(C, null).ToString();
                    else
                        Col[Name] = PI.GetValue(C, null);
                }

                ColumnList.Add(Col);
            }
            return ColumnList.ToArray();
        }

        ///////////////////////////////////////////////
        internal string ClientSideName(string S)
        ///////////////////////////////////////////////
        {
            return char.ToLower(S[0]) + S.Substring(1);
        }


        ///////////////////////////////////////////////
        internal void ThrowException(string Msg)
        ///////////////////////////////////////////////
        {
            ThrowException(Msg, "");
        }

        ///////////////////////////////////////////////
        internal void ThrowException(string Msg, string Info)
        ///////////////////////////////////////////////
        {
            if (Info != "" && CustomErrorsModeOff())
                Msg += " [" + Info + "]";

            //Msg += " [" + VersionString() + "]";
            this.Context.Response.Write(HttpUtility.HtmlEncode(Msg));
            this.Context.Response.End();
        }

        ////////////////////////////////////////////////////////////////////////////
        internal string VersionString()
        ////////////////////////////////////////////////////////////////////////////
        {
            Assembly A = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo V = System.Diagnostics.FileVersionInfo.GetVersionInfo(A.Location);
            string VersionString = V.FileMajorPart.ToString() + "." + V.FileMinorPart.ToString() + "." + V.FileBuildPart.ToString() + "." + V.ProductVersion;
            return A.GetName().Version.Major.ToString() + "." + A.GetName().Version.Minor.ToString() + "." + A.GetName().Version.Build.ToString();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal object GetProperty(object Obj, string PropertyName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            PropertyInfo P = Obj.GetType().GetProperty(PropertyName, BF);
            Object Value = null;

            if (P != null)
                if (P.CanRead)
                    try
                    {
                        Value = P.GetValue(Obj, null);
                    }
                    catch (Exception)
                    {
                    }

            return Value;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void SetProperty(object Obj, string PropertyName, object PropertyValue)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            object P = Obj.GetType().GetProperty(PropertyName, BF);

            if (P == null)
            {
                P = Obj.GetType().GetField(PropertyName, BF);
                if (P == null)
                    return;
                //ThrowException("Cannot find property [" + Obj.GetType().Name + "." + PropertyName + "]");
            }

            if (P is PropertyInfo)
                if (!(P as PropertyInfo).CanWrite)
                    return;
            //ThrowException("Property [" + Obj.GetType().Name + "." + PropertyName + "] is read-only");

            try
            {
                if (P is PropertyInfo)
                {
                    PropertyInfo PI = (P as PropertyInfo);
                    PropertyValue = ConvertPropertyValue(PI.PropertyType, PropertyValue, PI.PropertyType.IsEnum);

                }
                else
                {
                    FieldInfo FI = (P as FieldInfo);
                    PropertyValue = ConvertPropertyValue(FI.FieldType, PropertyValue, FI.FieldType.IsEnum);
                }
            }
            catch (Exception)
            {
            }


            try
            {
                if (P is PropertyInfo)
                {
                    //                   PropertyValue = Convert.ChangeType(PropertyValue, (P as PropertyInfo).PropertyType);
                    (P as PropertyInfo).SetValue(Obj, PropertyValue, null);
                }
                else
                {
                    //                   PropertyValue = Convert.ChangeType(PropertyValue, (P as FieldInfo).FieldType);
                    (P as FieldInfo).SetValue(Obj, PropertyValue);
                }
            }
            catch (Exception Ex)
            {
                ThrowException(Ex.Message + "==> SetValue:[" + Obj.GetType().ToString() + "." + PropertyName + "] Value ==> " + PropertyValue.ToString());
            }
        }

        ///////////////////////////////////////////////
        public static object ConvertPropertyValue(Type T, object V, bool IsEnum)
        ///////////////////////////////////////////////
        {
            switch (T.Name)
            {
                case "ArrayList":
                    V = new ArrayList(V as object[]);
                    break;
                case "Boolean":
                    V = Convert.ToBoolean(V);
                    break;
                case "Unit":
                    V = new Unit(V.ToString());
                    break;
                case "Color":
                    V = System.Drawing.Color.FromName(V.ToString());
                    break;
                case "Font":
                    if (V is Dictionary<string, object>)
                    {
                        Dictionary<string, object> D = CaseInsensitiveDictionary(V as Dictionary<string, object>);
                        try
                        {
                            System.Drawing.Font F = new System.Drawing.Font(D["Family"].ToString(), Convert.ToSingle(D["Size"]));
                            V = F;
                        }
                        catch (Exception)
                        {
                            V = new System.Drawing.Font("verdana", 1);
                        }
                    }
                    break;
                default:
                    if (IsEnum)
                        V = Enum.Parse(T, V.ToString(), true);
                    else
                    {
                        T = GetNullableType(T);
                        V = Convert.ChangeType(V, T);
                    }
                    break;
            }

            return V;
        }

        ///////////////////////////////////////////////
        public static Type GetNullableType(Type type)
        ///////////////////////////////////////////////
        {
            if (Nullable.GetUnderlyingType(type) == null)
                return type;
            else
                return Nullable.GetUnderlyingType(type);
        }

        ///////////////////////////////////////////////
        protected bool IsLocalHost()
        ///////////////////////////////////////////////
        {
            return (Context.Request.ServerVariables["REMOTE_ADDR"] == Context.Request.ServerVariables["LOCAL_ADDR"]);
        }

        ///////////////////////////////////////////////
        protected bool CustomErrorsModeOff()
        ///////////////////////////////////////////////
        {
            if (IsLocalHost())
                return true;

            try
            {
                Configuration Config;
                CustomErrorsSection CESection;

                Config = WebConfigurationManager.OpenWebConfiguration("~");
                CESection = (CustomErrorsSection)Config.GetSection("system.web/customErrors");

                return (CESection.Mode == CustomErrorsMode.Off);
            }
            catch (Exception)
            {
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string GetUserToken()
        ////////////////////////////////////////////////////////////////////////////
        {
            string Token = "";
            if (HttpContext.Current.Session != null)
            {
                if (!(HttpContext.Current.Session["UserToken"] is string))
                    HttpContext.Current.Session["UserToken"] = HttpContext.Current.Session.SessionID;
                Token = HttpContext.Current.Session["UserToken"].ToString();
            }
            else
                Token = Environment.MachineName;

            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Token, "SHA1");
        }

        ////////////////////////////////////////////////////////////////////////////
        public static HtmlGenericControl UserTokenScriptControl()
        ////////////////////////////////////////////////////////////////////////////
        {
            return UserTokenScriptControl(string.Empty);
        }

        ////////////////////////////////////////////////////////////////////////////
        public static HtmlGenericControl UserTokenScriptControl(string nonce = "")
        ////////////////////////////////////////////////////////////////////////////
        {
            HtmlGenericControl Script = new HtmlGenericControl("script");
            Script.Attributes.Add("type", "text/javascript");
            Script.Attributes.Add("language", "JavaScript");
            if (string.IsNullOrEmpty(nonce) == false)
            {
                Script.Attributes.Add("nonce", nonce);
            }
            Script.InnerHtml = "if (typeof DbNetLink != \"undefined\") DbNetLink.requestToken = \"" + Shared.GetUserToken() + "\";";
            return Script;
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string WriteUserToken(string nonce)
        ////////////////////////////////////////////////////////////////////////////
        {
            return RenderControlToString(UserTokenScriptControl(nonce));
        }

        ///////////////////////////////////////////////
        internal string GetCSS(string[] CssNames)
        ///////////////////////////////////////////////
        {
            Assembly A = Assembly.GetExecutingAssembly();
            StringBuilder Css = new StringBuilder();

            foreach (string CssName in CssNames)
            {
                TextReader TR = new StreamReader(A.GetManifestResourceStream("DbNetLink.Resources.CSS." + CssName + ".css"));
                string S = TR.ReadToEnd();
                TR.Close();
                if (Culture.TextInfo.IsRightToLeft)
                    S = S.Replace("{", "{" + Environment.NewLine + "direction:rtl;" + Environment.NewLine);

                Css.Append(S);
            }
            return Css.ToString();
        }

        ///////////////////////////////////////////////
        internal string GetScript(string[] ScriptNames)
        ///////////////////////////////////////////////
        {
            Assembly A = Assembly.GetExecutingAssembly();
            StringBuilder Script = new StringBuilder();

            foreach (string ScriptName in ScriptNames)
            {
                TextReader TR = new StreamReader(A.GetManifestResourceStream("DbNetLink.Resources.Scripts." + ScriptName + ".js"));
                string S = TR.ReadToEnd();
                TR.Close();
                Script.Append(S);
            }
            return Script.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string ConfigValue(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            Key = "DbNetSuite" + Key;
            string V = ConfigurationManager.AppSettings[Key];

            if (V == null)
                V = "";

            return V;
        }


        ///////////////////////////////////////////////
        public static bool AjaxUpload
        ///////////////////////////////////////////////
        {
            get { return Shared.ConfigValue("AjaxUpload").ToLower() != "false"; }
        }

        ///////////////////////////////////////////////
        internal static string Capitalise(string Text)
        ///////////////////////////////////////////////
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Text);
        }

        ///////////////////////////////////////////////
        internal string FormatValue(object value, string format)
        ///////////////////////////////////////////////
        {
            return FormatValue(value, format, AntiXSS);
        }


        ///////////////////////////////////////////////
        internal string FormatValue(object value, DbColumn column)
        ///////////////////////////////////////////////
        {
            return FormatValue(value, column, AntiXSS);
        }

        ///////////////////////////////////////////////
        internal string FormatValue(object value, DbColumn column, bool ApplyAntiXSS)
        ///////////////////////////////////////////////
        {
            CultureInfo C = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (column == null)
                return FormatValue(value, "", ApplyAntiXSS);

            if (column.Culture != "")
                C = new CultureInfo(column.Culture);

            return FormatValue(value, column.Format, ApplyAntiXSS, C);
        }

        ///////////////////////////////////////////////
        internal string FormatValue(object value, string format, bool ApplyAntiXSS)
        ///////////////////////////////////////////////
        {
            return FormatValue(value, format, ApplyAntiXSS, System.Threading.Thread.CurrentThread.CurrentCulture);
        }

        ///////////////////////////////////////////////
        internal string FormatValue(object value, string format, bool ApplyAntiXSS, CultureInfo Culture)
        ///////////////////////////////////////////////
        {
            if (value == null)
                return "";

            if (value.GetType() == typeof(DBNull))
                return "";

            switch (value.GetType().ToString())
            {
                case "System.Byte":
                    return Convert.ToByte(value).ToString(format, Culture);
                case "System.Int16":
                    return Convert.ToInt16(value).ToString(format, Culture);
                case "System.Int32":
                    return Convert.ToInt32(value).ToString(format, Culture);
                case "System.Int64":
                    return Convert.ToInt64(value).ToString(format, Culture);
                case "System.Decimal":
                    return Convert.ToDecimal(value).ToString(format, Culture);
                case "System.Single":
                    return Convert.ToSingle(value).ToString(format, Culture);
                case "System.Double":
                    return Convert.ToDouble(value).ToString(format, Culture);
                case "System.DateTime":
                    return Convert.ToDateTime(value).ToString(format, Culture);
                default:
                    if (ApplyAntiXSS)
                        return AntiXssHtmlEncode(Convert.ToString(value));
                    else
                        return Convert.ToString(value);
            }
        }

        ///////////////////////////////////////////////
        internal string AntiXssHtmlEncode(string Text)
        ///////////////////////////////////////////////
        {
            return System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(Text, false);
        }

        ///////////////////////////////////////////////
        internal string Translate(string Key)
        ///////////////////////////////////////////////
        {
            string Translation = "";

            if (RM == null)
                RM = new ResourceManager("DbNetLink.Resources.Localisation.default", Assembly.GetExecutingAssembly(), null);

            RM.IgnoreCase = true;

            if (RM != null)
            {
                try
                {
                    Translation = RM.GetString(Key.Replace(" ", ""));
                }
                catch (Exception)
                {
                }
            }

            if (String.IsNullOrEmpty(Translation))
                return Key;
            else
                return Translation;
        }

        ///////////////////////////////////////////////
        public static void SetCulture(HttpRequest Request)
        ///////////////////////////////////////////////
        {
            SetCulture(Request, string.Empty);
        }

        ///////////////////////////////////////////////
        public static void SetCulture(HttpRequest Request, string UserLanguage)
        ///////////////////////////////////////////////
        {
            if (Request != null)
                if (ConfigValue("CultureSource").ToLower() != "server")
                    SetCultureFromUserLanguage(Request.UserLanguages, UserLanguage);
        }

        ///////////////////////////////////////////////
        public static void SetCultureFromUserLanguage(string[] UserLanguages, string UserLanguage)
        ///////////////////////////////////////////////
        {
            if (UserLanguages == null)
                UserLanguages = new string[0];

            ArrayList Languages = new ArrayList(UserLanguages);

            // If the UserLanguage property has been supplied then use this to make the first attempt to set the culture
            if (!String.IsNullOrEmpty(UserLanguage))
                Languages.Insert(0, UserLanguage);

            Languages.Add("en-US");
            foreach (string lang in Languages)
            {
                string LangCode = lang.Split(';')[0];
                if (SetCulture(LangCode))
                    break;
            }
        }

        ///////////////////////////////////////////////
        public static bool SetCulture(string Lang)
        ///////////////////////////////////////////////
        {
            try
            {
                CultureInfo Culture = CultureInfo.CreateSpecificCulture(Lang);
                System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = Culture;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        ///////////////////////////////////////////////
        internal string AssignID(string ID)
        ///////////////////////////////////////////////
        {
            return this.Id + "_" + ID;
        }

        ///////////////////////////////////////////////
        internal static Control FindControl(string id, Control root)
        ///////////////////////////////////////////////
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls)
            {
                Control findControl = FindControl(id, control);
                if (findControl != null)
                    return findControl;
            }

            return null;
        }

        ///////////////////////////////////////////////
        internal static Control FindControlByClassName(string Class, Control Container)
        ///////////////////////////////////////////////
        {
            if (Container is HtmlControl)
                if ((Container as HtmlControl).Attributes["class"] != null)
                    if ((Container as HtmlControl).Attributes["class"] == Class)
                        return Container;

            if (Container is WebControl)
                if ((Container as WebControl).CssClass == Class)
                    return Container;

            foreach (Control Ctrl in Container.Controls)
            {
                Control C = FindControlByClassName(Class, Ctrl);
                if (C != null)
                    return C;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal void MakeIframe508Compliant(HtmlGenericControl F, string PageName)
        ////////////////////////////////////////////////////////////////////////////
        {
            string Url = this.PageRef.ClientScript.GetWebResourceUrl(typeof(Shared), "DbNetLink.Resources.Html." + PageName + ".htm");
            F.Attributes.Add("src", Url);
            Literal L = new Literal();
            L.Text = "Your browser does not support inline frames or is currently configured not to display inline frames. Content can be viewed at actual source page: " + Url;
            F.Controls.Add(L);
        }

        ///////////////////////////////////////////////
        internal string BuildMessageBox()
        ///////////////////////////////////////////////
        {
            Table MessageBox = new Table();

            MessageBox.Width = new Unit("100%");
            MessageBox.ID = AssignID("messageBox");
            MessageBox.CssClass = "dbnetsuite-message-box";

            TableRow TR = new TableRow();
            MessageBox.Rows.Add(TR);
            TableCell C = new TableCell();
            TR.Controls.Add(C);

            Table Content = new Table();
            Content.Width = new Unit("100%");
            Content.CellPadding = 10;
            Content.CssClass = "dbnetsuite-message-box-content";
            C.Controls.Add(Content);
            TR = new TableRow();
            Content.Rows.Add(TR);

            C = new TableCell();
            TR.Cells.Add(C);

            string[] Images = { "Error", "Warning", "Info", "Question" };

            foreach (string ImgName in Images)
            {
                Image I = new Image();
                I.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName + "32.png");
                I.CssClass = "dbnetsuite-message-box-icon";
                I.Attributes.Add("iconType", ImgName.ToLower());
                C.Controls.Add(I);
            }

            C = new TableCell();
            TR.Cells.Add(C);
            C.CssClass = "dbnetsuite-message-box-text";

            TR = new TableRow();

            MessageBox.Rows.Add(TR);
            C = new TableCell();
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
            AddDialogButton(C, "yes", "Yes", "Yes");
            AddDialogButton(C, "no", "No", "Yes");
            TR.Cells.Add(C);

            return RenderControlToString(MessageBox);
        }

        ///////////////////////////////////////////////
        internal string BuildErrorDialog()
        ///////////////////////////////////////////////
        {
            Table T = new Table();

            T.Width = new Unit("100%");
            T.ID = AssignID("errorDialog");
            T.CssClass = "dbnetsuite error-dialog";
            T.ToolTip = "Error Dialog";

            TableRow TR = new TableRow();
            T.Rows.Add(TR);
            TableCell C = new TableCell();
            TR.Controls.Add(C);

            Panel P = new Panel();
            P.CssClass = "error-content";
            P.Width = new Unit("600px");
            P.Height = new Unit("600px");
            P.Style.Add(HtmlTextWriterStyle.Overflow, "auto");

            C.Controls.Add(P);
            TR = new TableRow();

            T.Rows.Add(TR);
            C = new TableCell();
            C.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
            AddDialogButton(C, "ok", "Ok", "Ok");
            TR.Cells.Add(C);

            return RenderControlToString(T);
        }

        ///////////////////////////////////////////////
        internal DbNetButton AddToolButton(TableRow R, string ButtonID, string ImgName, string TitleKey)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            R.Controls.Add(C);
            return AddToolButton(C, ButtonID, ImgName, TitleKey);
        }

        ///////////////////////////////////////////////
        internal DbNetButton AddToolButton(TableCell C, string ButtonID, string ImgName, string TitleKey)
        ///////////////////////////////////////////////
        {
            //string ImgUrl = Page.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName + ".png");

            string Text = "";
            GridEditControl.ToolButtonStyles ButtonStyle = GridEditControl.ToolButtonStyles.ImageAndText;

            if (this is GridEditControl)
            {
                Text = Translate(Capitalise(ButtonID));
                ButtonStyle = (this as GridEditControl).ToolbarButtonStyle;
            }

            if (this is DbNetFile)
            {
                Text = Translate(Capitalise(ButtonID));
                ButtonStyle = (this as DbNetFile).ToolbarButtonStyle;
            }

            DbNetButton B = new DbNetButton(ImgName, Text, Translate(TitleKey), Context.Request, ButtonStyle, Page);
            B.ID = this.AssignID(ButtonID + "Btn");
            B.Attributes.Add("ButtonStyle", ButtonStyle.ToString());

            string Class = "toolbar-button";

            if (Theme == UI.Themes.Bootstrap)
                Class += " btn";

            switch (ButtonID.ToLower())
            {
                case "apply":
                case "insertRow":
                case "updateRow":
                case "first":
                    C.Style.Add(HtmlTextWriterStyle.PaddingLeft, "6px");
                    break;
            }

            switch (ButtonID.ToLower())
            {
                case "next":
                case "prev":
                case "first":
                case "last":
                    Class += " navigation";
                    break;
                case "export":
                    B.Attributes.Add("type", "submit");
                    break;
            }

            B.Attributes.Add("class", Class);
            B.Attributes.Add("buttonType", ButtonID.ToLower());

            if (this is DbNetGrid)
                if (ButtonID == "update")
                    B.Attributes.Add("updateMode", (this as DbNetGrid).UpdateMode.ToString());

            C.Controls.Add(B);
            return B;
        }

        ///////////////////////////////////////////////
        internal void BuildButton(TableRow R, string ID, string Img, string Text, string Title)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            R.Controls.Add(C);
            C.Controls.Add(BuildButton(ID, Text, Img, Title));
        }

        ///////////////////////////////////////////////
        internal DbNetButton BuildButton(string ID, string Text, string Img, string Title)
        ///////////////////////////////////////////////
        {
            DbNetButton B = new DbNetButton(Img, Translate(Text), Translate(Title), this.Context.Request, this.PageRef);

            if (this.GetTheme() == UI.Themes.Bootstrap)
                B.Attributes.Add("class", ID + "-button btn");
            else
                B.Attributes.Add("class", ID + "-button");
            B.ID = ID + "Btn";
            return B;
        }

        ///////////////////////////////////////////////
        internal void AddPageInformation(TableCell PC)
        ///////////////////////////////////////////////
        {
            Table T = new Table();
            PC.Controls.Add(T);
            T.CellPadding = 0;
            T.CellSpacing = 0;

            TableRow R = new TableRow();
            T.Controls.Add(R);

            AddToolButton(R, "first", "first", "FirstPage");
            AddToolButton(R, "prev", "prev", "PreviousPage");

            AddPageInfo(R);

            AddToolButton(R, "next", "next", "NextPage");
            AddToolButton(R, "last", "last", "LastPage");

            if (this is DbNetEdit)
                if ((this as DbNetEdit).Browse)
                    AddToolButton(R, "browse", "Browse", "OpenBrowseDialog");

            foreach (TableCell C in R.Cells)
            {
                C.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                C.Style.Add(HtmlTextWriterStyle.Padding, "0px");
            }
        }

        ///////////////////////////////////////////////
        internal void AddPageInfo(TableRow R)
        ///////////////////////////////////////////////
        {
            TableCell C = new TableCell();
            R.Controls.Add(C);

            Table T = new Table();
            T.CssClass = "page-information";
            T.CellPadding = 0;
            T.CellSpacing = 0;

            C.Controls.Add(T);
            TableRow TR = new TableRow();
            T.Controls.Add(TR);

            C = new TableCell();
            C.Text = "&nbsp;";

            if (this is DbNetEdit)
                C.Text += Translate("Record");
            else
                C.Text += Translate("Page");

            C.Text += "&nbsp;";
            TR.Controls.Add(C);

            TableCell TC = new TableCell();
            TR.Controls.Add(TC);

            TextBox PSTB = new TextBox();
            TC.Controls.Add(PSTB);
            PSTB.ID = AssignID("pageSelect");
            PSTB.Width = 50;

            TC = new TableCell();
            TR.Controls.Add(TC);
            TC.Text = "&nbsp;" + Translate("Of") + "&nbsp;";

            TC = new TableCell();
            TR.Controls.Add(TC);
            TextBox TB = new TextBox();
            TC.Controls.Add(TB);
            TB.ID = AssignID("totalPages");
            TB.Width = Unit.Pixel(30);
            TB.ReadOnly = true;

            TC = new TableCell();
            TR.Controls.Add(TC);
            TC.Text += "&nbsp;";

            foreach (TableCell Cell in TR.Cells)
            {
                Cell.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                Cell.Style.Add(HtmlTextWriterStyle.Padding, "0px");
            }
        }


        ///////////////////////////////////////////////
        internal DbNetButton AddDialogButton(TableCell C, string ButtonID, string Text, string TitleKey)
        ///////////////////////////////////////////////
        {
            //          string ImgUrl = Page.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName );

            DbNetButton B = new DbNetButton("", Translate(Text), Translate(TitleKey), Context.Request, Page);
            B.ID = this.AssignID(ButtonID + "Btn");

            B.Attributes.Add("class", "dbnetsuite-dialog-button " + ButtonID + "-button");
            B.Attributes.Add("buttonType", ButtonID.ToLower());
            B.Style.Add(HtmlTextWriterStyle.Padding, "5px");

            C.Controls.Add(B);
            return B;
        }

        ///////////////////////////////////////////////
        internal string GetImageUrl(string ImgName)
        ///////////////////////////////////////////////
        {
            return Page.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName);
        }


        ///////////////////////////////////////////////
        public static string ImageUrl(string ImgName)
        ///////////////////////////////////////////////
        {
            Page P = HttpContext.Current.Handler as Page;

            if (P != null)
                return P.ClientScript.GetWebResourceUrl(typeof(DbNetButton), "DbNetLink.Resources.Images." + ImgName);
            else
                return "";
        }

        ///////////////////////////////////////////////
        static internal string RenderControlToString(Control control)
        ///////////////////////////////////////////////
        {
            StringWriter SW = new StringWriter();
            control.RenderControl(new HtmlTextWriter(SW));
            return SW.ToString();
        }

        ///////////////////////////////////////////////
        internal void AssignTheme(HttpContext context)
        ///////////////////////////////////////////////
        {
            if (context.Session["DbNetSuiteTheme"] is string)
            {
                try
                {
                    this.Theme = (UI.Themes)Enum.Parse(typeof(UI.Themes), context.Session["DbNetSuiteTheme"].ToString(), true);
                }
                catch (Exception)
                {
                }
            }
        }

        ///////////////////////////////////////////////
        internal UI.Themes GetTheme()
        ///////////////////////////////////////////////
        {
            return this.Theme;
        }

        ///////////////////////////////////////////////
        protected Type GetColumType(string TypeName)
        ///////////////////////////////////////////////
        {
            return Type.GetType("System." + TypeName);
        }

        ///////////////////////////////////////////////
        internal string ValidateDataTypeValue(string dataType, string value, string Format)
        ///////////////////////////////////////////////
        {
            return ValidateDataTypeValue(GetColumType(dataType), value, Format);
        }

        ///////////////////////////////////////////////
        internal string ValidateDataTypeValue(Type dataType, string value, string Format)
        ///////////////////////////////////////////////
        {
            try
            {
                switch (dataType.Name)
                {
                    case "Boolean":
                        break;
                    case "TimeSpan":
                        Convert.ChangeType(value, typeof(DateTime));
                        break;
                    case "Guid":
                        if (isGuid.IsMatch(value.Trim()))
                            return "";
                        else
                            return "Value is not a valid GUID";
                        break;
                    default:
                        Convert.ChangeType(value, dataType);
                        break;
                }
            }
            catch (Exception)
            {
                string Text = Translate("ExampleFormat");

                switch (dataType.Name)
                {
                    case "DateTime":
                    case "TimeSpan":
                        return Text + ": <b>" + System.DateTime.Now.ToString(Format) + "</b>";
                    default:
                        return Text + ": <b>" + 123.ToString(Format) + "</b>";
                }
            }

            return "";
        }
        ///////////////////////////////////////////////
        protected void CreateDirectory(string path)
        ///////////////////////////////////////////////
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    try
                    {
                        MkDir(path);
                    }
                    catch (Exception ex)
                    {
                        ThrowException(ex.Message);
                    }
                }
            }
        }

        ///////////////////////////////////////////////
        static public string GenerateLabel(string Label)
        ///////////////////////////////////////////////
        {
            if (ConfigValue("GenerateLabel").ToLower() == "false")
                return Label;

            Label = Regex.Replace(Label, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            return Shared.Capitalise(Label.Replace("_", " "));
        }

        ///////////////////////////////////////////////
        private void MkDir(string path)
        ///////////////////////////////////////////////
        {
            string[] splitPath = path.Split('\\');
            string currentPath = splitPath[0] + "\\";

            for (int i = 1; i < splitPath.Length; i++)
            {
                currentPath += "\\" + splitPath[i];

                if (Directory.Exists(currentPath))
                    continue;

                int returnCode = _mkdir(currentPath);

                if (returnCode != 0)
                    throw new Exception("[msvcrt.dll]:_mkdir(" + currentPath + "), error code: " + returnCode.ToString());
            }
        }

        ///////////////////////////////////////////////
        internal string BuildUploadDialog()
        ///////////////////////////////////////////////
        {
            UploadDialog D = new UploadDialog(this, false);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string BuildAjaxUploadDialog()
        ///////////////////////////////////////////////
        {
            UploadDialog D = new UploadDialog(this, true);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal string AssignHandler(string handler)
        ///////////////////////////////////////////////
        {
            return this.Context.Request.ApplicationPath + ((this.Context.Request.ApplicationPath == "/") ? "" : "/") + handler;
        }

        ///////////////////////////////////////////////
        internal List<object> GetComboItems(QueryCommandConfig Query)
        ///////////////////////////////////////////////
        {
            List<object> Items = new List<object>();

            bool ConnectionAlreadyOpen = this.OpenConnection();

            Database.ExecuteQuery(Query);

            int TextIdx = (Database.Reader.FieldCount > 1) ? 1 : 0;

            while (Database.Reader.Read())
            {
                Dictionary<string, object> Item = new Dictionary<string, object>();

                Item.Add("val", Database.Reader[0].ToString());
                Item.Add("text", Database.Reader[TextIdx].ToString());

                for (int I = 2; I < Database.Reader.FieldCount; I++)
                    Item.Add(Database.Reader.GetName(I).ToLower(), Database.Reader[I].ToString());

                Items.Add(Item);
            }

            if (!ConnectionAlreadyOpen)
                this.CloseConnection();

            return Items;
        }

        ///////////////////////////////////////////////
        internal string UploadFolder(object FolderInfo)
        ///////////////////////////////////////////////
        {
            string RootFolder = String.Empty;
            string SubFolder = String.Empty;

            if (FolderInfo is DbColumn)
            {
                RootFolder = (FolderInfo as DbColumn).UploadRootFolder;
                SubFolder = (FolderInfo as DbColumn).UploadSubFolder;
            }
            else if (FolderInfo is ThumbnailInfo)
            {
                RootFolder = (FolderInfo as ThumbnailInfo).UploadRootFolder;
                SubFolder = (FolderInfo as ThumbnailInfo).UploadSubFolder;
            }

            if (SubFolder == String.Empty)
                return RootFolder;
            else
                return (RootFolder + "/" + SubFolder).Replace("//", "/");
        }

        ///////////////////////////////////////////////
        internal void ValidateUpload()
        ///////////////////////////////////////////////
        {
            UploadConfig UC = new UploadConfig();

            if (this is GridEditControl)
            {
                if (Req.ContainsKey("column"))
                {
                    DbColumn Col = (DbColumn)DeserialiseColumn((Dictionary<string, object>)Req["column"]);
                    UC.SavePath = UploadFolder(Col);
                    UC.ExtFilter = Col.UploadExtFilter;
                }
                else
                {
                    GridEditControl Ctrl = (GridEditControl)this;
                    UC.SavePath = Ctrl.UploadDataFolder;
                    UC.ExtFilter = Ctrl.UploadExtFilter;
                }
            }

            if (this is DbNetFile)
            {
                DbNetFile FileControl = (DbNetFile)this;
                UC.SavePath = FileControl.GetRootFolder();
                string CurrentFolder = FileControl.CurrentFolder;
                if (CurrentFolder != "")
                    UC.SavePath += "/" + CurrentFolder;
                UC.ExtFilter = FileControl.UploadFileTypes;
            }

            string FileName = (string)Req["fileName"];

            if ((string)Req["alternateFileName"] != String.Empty)
                FileName = GetAlternateFileName((string)Req["alternateFileName"], FileName);

            bool Overwrite = Convert.ToBoolean(Req["overwrite"]);

            Resp["ok"] = true;

            if (UC.ExtFilter == "" || !ValidExtension(UC.ExtFilter, Path.GetExtension(FileName)))
            {
                Resp["message"] = Translate("InvalidFileType");
                Resp["ok"] = false;
            }

            if (!Overwrite)
            {
                if (UC.SavePath == "")
                    UC.SavePath = ".";

                UC.SavePath = Regex.Replace(UC.SavePath, "^~", this.Context.Request.ApplicationPath);
                try
                {
                    if (!Directory.Exists(UC.SavePath) || (UC.SavePath.StartsWith("/") && !UC.SavePath.StartsWith("//")))
                        UC.SavePath = this.Context.Request.MapPath(UC.SavePath);
                }
                catch (Exception Ex)
                {
                    Resp["message"] = Ex.Message;
                    Resp["ok"] = false;
                }
                UC.FileName = Path.GetFileName(FileName);

                if (File.Exists(UC.SavePath + "/" + UC.FileName))
                {
                    Resp["message"] = Translate("FileExists");
                    Resp["ok"] = false;
                }
            }
        }

        ///////////////////////////////////////////////
        internal string GetAlternateFileName(string AlternateFileName, string FileName)
        ///////////////////////////////////////////////
        {
            return AlternateFileName.Split('.')[0] + Path.GetExtension(FileName);
        }

        ///////////////////////////////////////////////
        internal bool ValidExtension(string ExtList, string Ext)
        ///////////////////////////////////////////////
        {
            string[] ExtAray = ExtList.Split(',');

            foreach (string E in ExtAray)
                if (E.Replace(".", "").ToLower().Trim() == Ext.Replace(".", "").ToLower())
                    return true;

            return false;
        }

        ///////////////////////////////////////////////
        internal void Upload()
        ///////////////////////////////////////////////
        {
            HttpPostedFile file = this.Context.Request.Files[this.Context.Request.Files.Count - 1];
            int fileSize = file.ContentLength;

            UploadConfig UC = new UploadConfig();
            DbColumn Col = null;

            UC.FileName = Path.GetFileName(file.FileName);

            if (!String.IsNullOrEmpty(this.Context.Request.Form["alternateFileName"]))
                UC.FileName = GetAlternateFileName(this.Context.Request.Form["alternateFileName"].ToString(), UC.FileName);

            if (this is GridEditControl)
            {
                if (Req.ContainsKey("column"))
                {
                    Col = (DbColumn)DeserialiseColumn((Dictionary<string, object>)Req["column"]);
                    UC.SavePath = UploadFolder(Col);
                    UC.MaxFileSizeKb = Col.UploadMaxFileSize;
                    UC.ExtFilter = Col.UploadExtFilter;

                    UC.ColumnIndex = Col.ColumnIndex;

                    if (Req.ContainsKey("rowIndex"))
                        UC.RowIndex = Convert.ToInt32(Req["rowIndex"]);

                    if (Col.DataType == "Byte[]")
                        UC.SaveToBlob = true;
                }
                else
                {
                    UC.SavePath = (this as GridEditControl).UploadDataFolder;
                    UC.ExtFilter = (this as GridEditControl).UploadExtFilter;
                    if (Session != null)
                        UC.FileName = Session.SessionID + "_" + UC.FileName;
                }
            }

            if (this is DbNetFile)
            {
                DbNetFile FileControl = (DbNetFile)this;
                UC.SavePath = FileControl.GetRootFolder();
                string CurrentFolder = FileControl.CurrentFolder;
                if (CurrentFolder != "")
                    UC.SavePath += "/" + CurrentFolder;
                UC.MaxFileSizeKb = FileControl.UploadMaxFileSizeKb;
                UC.ExtFilter = FileControl.UploadFileTypes;
            }

            if (UC.SavePath == "")
                UC.SavePath = ".";

            UC.SavePath = Regex.Replace(UC.SavePath, "^~", this.Context.Request.ApplicationPath);
            try
            {
                if (!UC.SavePath.StartsWith(@"\\"))
                    UC.SavePath = this.Context.Request.MapPath(UC.SavePath);
            }
            catch (Exception)
            {
            }

            string Msg = "";

            if (UC.MaxFileSizeKb > 0)
                if ((file.ContentLength / 1024) > UC.MaxFileSizeKb)
                    Msg = string.Format(Translate("FileExceedsMaxSize"), UC.MaxFileSizeKb.ToString());

            if (UC.ExtFilter == "" || !ValidExtension(UC.ExtFilter, Path.GetExtension(file.FileName)))
                Msg = Translate("InvalidFileType");

            bool saveComplete = false;

            if (Msg == "")
            {
                try
                {
                    if (HttpContext.Current.Session == null || this is DbNetFile || Col == null)
                    {
                        CreateDirectory(UC.SavePath);
                        file.SaveAs(UC.SavePath + "/" + UC.FileName);
                    }
                    else
                    {
                        Stream IS = file.InputStream;
                        BinaryReader BR = new BinaryReader(IS);

                        byte[] Buffer = BR.ReadBytes(Convert.ToInt32(IS.Length));
                        IS.Close();

                        Image TI;

                        if (DataUriSupported())
                        {
                            TI = new DataUriImage(this as GridEditControl, Buffer, Col, UC.FileName, true, true);
                            UC.Guid = (TI as DataUriImage).Key;
                        }
                        else
                        {
                            TI = new ThumbnailImage(this as GridEditControl, Col, Buffer, UC.FileName);
                            (TI as ThumbnailImage).Info.Persist = true;
                            UC.Guid = (TI as ThumbnailImage).Key;
                        }

                        UC.Url = TI.ImageUrl;

                    }
                    saveComplete = true;
                }
                catch (Exception ex)
                {
                    Msg = ex.Message;
                }
            }

            if (!saveComplete)
                if (Msg == "")
                    Msg = "Unknown error";

            if (Req["method"].ToString() == "ajax-upload")
            {
                Resp["uploadComplete"] = true;
                Resp["uploadOutcome"] = saveComplete.ToString().ToLower();
                Resp["uploadGuid"] = UC.Guid;
                Resp["uploadMessage"] = Msg;
                Resp["uploadUrl"] = UC.Url;
                Resp["uploadFileName"] = UC.FileName;
                Resp["uploadFileSize"] = fileSize.ToString();
            }
            else
            {
                string Vars = "";
                Vars += "var uploadComplete = true;";
                Vars += "var uploadOutcome = " + saveComplete.ToString().ToLower() + ";";
                Vars += "var uploadGuid = \"" + UC.Guid + "\";";
                Vars += "var uploadMessage = \"" + Msg + "\";";
                Vars += "var uploadUrl = \"" + UC.Url + "\";";
                Vars += "var uploadFileName = \"" + UC.FileName + "\";";
                Vars += "var uploadFileSize = \"" + fileSize.ToString() + "\";";

                Assembly A = Assembly.GetAssembly(typeof(Shared));
                Stream S = A.GetManifestResourceStream("DbNetLink.Resources.Html.UploadPage.htm");
                StreamReader SR = new StreamReader(S);
                string Html = SR.ReadToEnd();

                Html = Html.Replace("/* Vars */", Vars);
                this.Context.Response.Clear();
                this.Context.Response.ContentType = "text/html";
                this.Context.Response.Write(Html);
                this.Context.Response.End();
            }
        }

        ///////////////////////////////////////////////
        internal TableRow AddMessageRow(Table T)
        ///////////////////////////////////////////////
        {
            TableRow R = new TableRow();
            T.Controls.Add(R);

            TableCell C = new TableCell();
            R.Controls.Add(C);

            if (this.GetTheme() == UI.Themes.Bootstrap)
            {
                HtmlGenericControl D = new HtmlGenericControl("div");
                C.Controls.Add(D);
                D.Attributes.Add("class", "message-line alert alert-info");
                D.InnerHtml = "&nbsp;";
            }
            else
            {
                C.CssClass = "message-line";
                C.Text = "&nbsp;";
            }

            return R;
        }

        ///////////////////////////////////////////////
        internal HtmlTableRow AddMessageRow(HtmlTable T)
        ///////////////////////////////////////////////
        {
            HtmlTableRow R = new HtmlTableRow();
            T.Controls.Add(R);

            HtmlTableCell C = new HtmlTableCell();
            R.Controls.Add(C);

            if (this.GetTheme() == UI.Themes.Bootstrap)
            {
                HtmlGenericControl D = new HtmlGenericControl("div");
                C.Controls.Add(D);
                D.Attributes.Add("class", "message-line alert alert-info");
                D.InnerHtml = "&nbsp;";
            }
            else
            {
                C.Attributes.Add("class", "message-line");
                C.InnerHtml = "&nbsp;";
            }

            return R;
        }

        ///////////////////////////////////////////////
        internal string BuildFilePreviewDialog()
        ///////////////////////////////////////////////
        {
            FilePreviewDialog D = new FilePreviewDialog(this);
            return RenderControlToString(D.Build());
        }

        ///////////////////////////////////////////////
        internal object JsonValue(Object Val)
        ///////////////////////////////////////////////
        {
            if (Val == System.DBNull.Value)
                return "";

            if (Val is Byte[])
                return "";

            if (Val is DateTime)
            {
                DateTime d1 = new DateTime(1970, 1, 1);
                DateTime d2 = Convert.ToDateTime(Val).ToUniversalTime();
                TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

                Val = "/Date(" + Convert.ToInt64(ts.TotalMilliseconds).ToString() + ")/";
            }

            return Val;
        }


        ///////////////////////////////////////////////
        internal string[] GetSelectColumns(string Sql)
        ///////////////////////////////////////////////
        {
            Match M = Regex.Match(Sql, @"select (.*?) from", RegexOptions.IgnoreCase);
            string ColumnList = M.Groups[1].ToString();

            string[] Patterns = { @"(\'.*\')", @"(\(.*\))" };
            foreach (string P in Patterns)
            {
                M = Regex.Match(ColumnList, P);

                foreach (Group G in M.Groups)
                    if (!String.IsNullOrEmpty(G.Value))
                        ColumnList = ColumnList.Replace(G.Value, G.Value.Replace(",", "~"));
            }

            string[] ColumnExpressions = ColumnList.Split(',');

            for (int I = 0; I < ColumnExpressions.Length; I++)
                ColumnExpressions[I] = ColumnExpressions[I].Replace("~", ",");

            return ColumnExpressions;
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /////////////////////////      Database Control       /////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////



        ///////////////////////////////////////////////
        internal void ExecuteQuery()
        ///////////////////////////////////////////////dbnetlink
        {
            ArrayList Data = new ArrayList();

            if (ConfigValue("NoAdHocQuery").ToLower() == "true")
            {
                Resp["data"] = "AdHoc Query has been disabled";
                return;
            }

            this.OpenConnection();

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = Req["sql"].ToString().Trim();

            if (ConfigValue("AdHocUpdate").ToLower() != "true")
            {
                string[] Keywords = { "alter", "drop", "create", "insert", "delete", "update" };

                foreach (string Keyword in Keywords)
                    if (Regex.Match(Query.Sql, @"\b" + Keyword + @"\b", RegexOptions.IgnoreCase).Success)
                        throw new Exception("Invalid select statement");
            }

            Dictionary<string, object> Params = (Dictionary<string, object>)Req["params"];

            if (Database.GetCommandType(Query.Sql) == CommandType.StoredProcedure)
            {
                ListDictionary derivedParameters = new ListDictionary();
                try
                {
                    derivedParameters = Database.DeriveParameters(Query.Sql);
                }
                catch (Exception) { }

                foreach (string ParamName in Params.Keys)
                    Database.SetParamValue(derivedParameters, ParamName, Params[ParamName]);

                Query.Params = derivedParameters;
            }
            else
            {
                foreach (string ParamName in Params.Keys)
                {
                    object ParamValue = Params[ParamName];
                    if (ParamValue is DateTime)
                        ParamValue = Convert.ToDateTime(ParamValue).AddMinutes(0 - this.TimezoneOffset);

                    Query.Params.Add(ParamName, ParamValue);
                }
            }

            Database.ExecuteQuery(Query);

            while (Database.Reader.Read())
                Data.Add(JsonData(null));

            Resp["data"] = Data.ToArray();

            this.CloseConnection();
        }

        ///////////////////////////////////////////////
        internal Dictionary<string, object> JsonData(QueryCommandConfig Query)
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> Data = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            if (Query != null)
                if (!Database.ExecuteSingletonQuery(Query))
                    return Data;

            for (int i = 0; i < Database.Reader.FieldCount; i++)
            {
                if (!Database.Reader.IsDBNull(i))
                    if (Database.Reader.GetValue(i).GetType().Name == "Byte[]")
                        continue;

                Data[Database.Reader.GetName(i).ToLower()] = JsonValue(Database.Reader.GetValue(i));
            }
            return Data;
        }


        ///////////////////////////////////////////////
        protected object LookupValue(object Value, string LookupText)
        ///////////////////////////////////////////////
        {
            if (Value == System.DBNull.Value)
                return Value;

            Match M = Regex.Match(LookupText, @"select (.*) (from|union)", RegexOptions.IgnoreCase);

            string Columns = M.Groups[1].ToString();

            Regex RE = new Regex(@"\((.*)\)", RegexOptions.IgnoreCase);

            for (Match M2 = RE.Match(Columns); M2.Success; M2 = M2.NextMatch())
            {
                Columns = Columns.Replace(M2.Groups[0].ToString(), M2.Groups[0].ToString().Replace(",", "~"));
            }

            if (Columns.Split(',').Length == 1)
                return Value;

            if (Columns.Split(',').Length == 2)
                if (Columns.Split(',')[0].Trim() == Columns.Split(',')[1].Trim())
                    return Value;

            string ValueColumn = Columns.Split(',')[0].Replace("~", ",");
            ValueColumn = Regex.Replace(ValueColumn, "distinct ", "", RegexOptions.IgnoreCase);

            Columns = Columns.Replace("~", ",");

            string TextColumn = Columns.Replace(ValueColumn + ",", "");

            string Sql = LookupText.Replace(Columns, TextColumn);

            ListDictionary Params = Database.ParseParameters(LookupText);

            if (Params.Count > 0)
                Sql = Regex.Replace(Sql, "where (.*)", "", RegexOptions.IgnoreCase);

            Sql = Regex.Replace(Sql, "order by (.*)", "", RegexOptions.IgnoreCase);

            string KeyFilter = " where " + ValueColumn + " = " + Database.ParameterName("key");

            if (Regex.IsMatch(Sql, " where ", RegexOptions.IgnoreCase) && Params.Count == 0)
            {
                Sql = Regex.Replace(Sql, " where ", " where ", RegexOptions.IgnoreCase);
                Regex regex = new Regex(" where ");
                Sql = regex.Replace(Sql, KeyFilter + " and ", 1);
            }
            else
                Sql += KeyFilter;

            QueryCommandConfig QC = new QueryCommandConfig(Sql);
            QC.Params["key"] = Value;

            this.DatabaseLookup = this.OpenConnection(this.DatabaseLookup);

            if (this.DatabaseLookup.ExecuteSingletonQuery(QC))
                return this.DatabaseLookup.Reader[0].ToString();

            return Value.ToString();
        }


        ///////////////////////////////////////////////
        internal static Dictionary<string, object> CaseInsensitiveDictionary(Dictionary<string, object> D)
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> D2 = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            foreach (string K in D.Keys)
                D2.Add(K, D[K]);

            return D2;
        }

        ///////////////////////////////////////////////
        internal bool DataUriSupported()
        ///////////////////////////////////////////////        
        {
            return (this.GetBrowser() != "msie" || this.GetBrowserVersion() > 8);
        }

        ///////////////////////////////////////////////
        internal string GetBrowser()
        ///////////////////////////////////////////////
        {
            if (Regex.IsMatch(HttpContext.Current.Request.UserAgent, IE11UserAgentPattern))
                return "msie";

            if (Regex.IsMatch(HttpContext.Current.Request.UserAgent, EdgeUserAgentPattern))
                return "edge";

            foreach (string key in Browser.Keys)
                if (Browser[key] is Boolean)
                    if (Convert.ToBoolean(Browser[key]))
                        return key;

            return String.Empty;
        }

        ///////////////////////////////////////////////
        internal int GetBrowserVersion()
        ///////////////////////////////////////////////
        {
            if (Regex.IsMatch(HttpContext.Current.Request.UserAgent, IE11UserAgentPattern))
                return 11;
            else
                return HttpContext.Current.Request.Browser.MajorVersion;
        }


        ///////////////////////////////////////////////
        internal bool OpenConnection()
        ///////////////////////////////////////////////
        {
            this.Database = this.OpenConnection(this.Database);
            this.Database.InjectionDetectionEnabled = this.AntiXSS;
            return (this.Database != null);
        }

        ///////////////////////////////////////////////
        internal DbNetData OpenConnection(DbNetData DbInstance)
        ///////////////////////////////////////////////
        {
            if (DbInstance != null && DbInstance.Conn.State == ConnectionState.Open)
                return DbInstance;

            DbInstance = GetDbNetDataInstance();

            if (this.CommandTimeout > -1)
                if (DbInstance.Database != DatabaseType.SqlServerCE)
                    DbInstance.CommandTimeout = this.CommandTimeout;

            if (DbInstance == null)
                return DbInstance;

            Page.Unload += this.CloseConnection;

            return DbInstance;
        }

        ///////////////////////////////////////////////
        internal DbNetData GetDbNetDataInstance()
        ///////////////////////////////////////////////
        {
            DbNetData Db;
            IDatabaseConnection ConnectionPlugin = null;

#if (DBNETTIME)
            ConnectionString = "DbNetTime";
#endif
#if (DBNETBUG)
            ConnectionString = "DbNetBug";
#endif
#if (DBNETOFFICE)
            ConnectionString = "DbNetOffice";
#endif

            if (ConnectionString.Equals(string.Empty))
            {
                ThrowException("The ConnectionString property has not been set for the control ==> " + this.ID);
                return null;
            }

            ConnectionStringSettings CSS = ConfigurationManager.ConnectionStrings[ConnectionString];

            if (CSS == null && Session != null)
            {
                if (Session["DbNetSuiteConnections"] is ListDictionary)
                {
                    ListDictionary Connections = Session["DbNetSuiteConnections"] as ListDictionary;
                    if (Connections[ConnectionString] != null)
                        CSS = Connections[ConnectionString] as ConnectionStringSettings;
                }
            }

            if (CSS == null)
            {
                ConnectionPlugin = (IDatabaseConnection)PlugInModules.LoadPlugin(typeof(IDatabaseConnection));

                /*
                if (ConnectionPlugin == null)
                {
                    ThrowException("The ConnectionString alias ==> " + ConnectionString + " not defined in web.config connection strings");
                    return null;
                }
                */
            }

            try
            {
                if (ConnectionPlugin != null)
                    Db = ConnectionPlugin.GetConnection(ConnectionString);
                else if (CSS != null)
                    Db = new DbNetData(CSS);
                else
                    Db = new DbNetData(ConnectionString);

                Db.Open();

#if (DBNETTIME || DBNETOFFICE || DBNETBUG)
                switch (Db.Database)
                {
                    case DatabaseType.SqlServer:
                        Db.ExecuteNonQuery("set TRANSACTION ISOLATION LEVEL read uncommitted");
                        break;
                }
#endif

                switch (Db.Database)
                {
                    case DatabaseType.SqlServer:
                        if (!String.IsNullOrEmpty(ConfigValue("TransactionIsolationLevel")))
                            Db.ExecuteNonQuery(String.Format("SET TRANSACTION ISOLATION LEVEL {0}", ConfigValue("TransactionIsolationLevel")));
                        if (!String.IsNullOrEmpty(ConfigValue("SymmetricKeyName")) && !String.IsNullOrEmpty(ConfigValue("CertificateName")))
                            Db.ExecuteNonQuery(String.Format("OPEN SYMMETRIC KEY {0} DECRYPTION BY CERTIFICATE {1}", ConfigValue("SymmetricKeyName"), ConfigValue("CertificateName")));
                        break;
                    case DatabaseType.Oracle:
                        if (this.CaseInsensitiveSearch)
                        {
                            Db.ExecuteNonQuery("ALTER SESSION SET NLS_COMP=LINGUISTIC");
                            Db.ExecuteNonQuery("ALTER SESSION SET NLS_SORT=BINARY_CI");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (!this.DesignMode)
                    ThrowException("Error connecting to database ==>" + ex.Message + "[" + ConnectionString + "]");
                return null;
            }

            Db.VerboseErrorInfo = this.CustomErrorsModeOff();

            return Db;
        }

        ///////////////////////////////////////////////
        protected override void OnUnload(EventArgs e)
        ///////////////////////////////////////////////
        {
            base.OnUnload(e);
            CloseConnection();
        }

        ///////////////////////////////////////////////
        internal void CloseConnection()
        ///////////////////////////////////////////////
        {
            CloseConnection(null, null);
        }
        ///////////////////////////////////////////////
        internal void CloseConnection(Object Sender, EventArgs e)
        ///////////////////////////////////////////////
        {
            if (Database != null)
                Database.Close();
            if (DatabaseLookup != null)
                DatabaseLookup.Close();
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /////////////////////////    End  Database Control    /////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////
        public static string GetContentType(string Ext)
        ///////////////////////////////////////////////
        {
            LoadMimeTypes();

            if (MimeTypes[Ext] == null)
                return "text/plain";
            else
                return MimeTypes[Ext].ToString();
        }

        ///////////////////////////////////////////////
        public static void LoadMimeTypes()
        ///////////////////////////////////////////////
        {
            if (MimeTypes.Count > 0)
                return;

            MimeTypes["z"] = "application/x-compress";
            MimeTypes["aab"] = "application/x-authorware-bin";
            MimeTypes["aam"] = "application/x-authorware-map";
            MimeTypes["aas"] = "application/x-authorware-seg";
            MimeTypes["acc"] = "chemical/x-synopsys-accord";
            MimeTypes["ai"] = "application/postscript";
            MimeTypes["aif"] = "audio/x-aiff";
            MimeTypes["aifc"] = "audio/x-aiff";
            MimeTypes["aiff"] = "audio/x-aiff";
            MimeTypes["ano"] = "application/x-annotator";
            MimeTypes["apm"] = "application/studiom";
            MimeTypes["asc"] = "text/plain";
            MimeTypes["asd"] = "application/astound";
            MimeTypes["asn"] = "application/astound";
            MimeTypes["asp"] = "application/x-asap";
            MimeTypes["au"] = "audio/basic";
            MimeTypes["avi"] = "video/x-msvideo";
            MimeTypes["bcpio"] = "application/x-bcpio";
            MimeTypes["bin"] = "application/octet-stream";
            MimeTypes["bmp"] = "image/bmp";
            MimeTypes["cdf"] = "application/x-netcdf";
            MimeTypes["cgm"] = "image/cgm";
            MimeTypes["chat"] = "application/x-chat";
            MimeTypes["chm"] = "chemical/x-cs-chemdraw";
            MimeTypes["class"] = "application/octet-stream";
            MimeTypes["cmx"] = "image/x-cmx";
            MimeTypes["cod"] = "image/cis-cod";
            MimeTypes["config"] = "application/x-ns-proxy-autoconfig";
            MimeTypes["cpio"] = "application/x-cpio";
            MimeTypes["cpt"] = "application/mac-compactpro";
            MimeTypes["csh"] = "application/x-csh";
            MimeTypes["css"] = "text/css";
            MimeTypes["dcr"] = "application/x-director";
            MimeTypes["dir"] = "application/x-director";
            MimeTypes["dms"] = "application/octet-stream";
            MimeTypes["doc"] = "application/msword";
            MimeTypes["docm"] = "application/vnd.ms-word.document.macroEnabled.12";
            MimeTypes["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            MimeTypes["dotm"] = "application/vnd.ms-word.template.macroEnabled.12";
            MimeTypes["dotx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";


            MimeTypes["dvi"] = "application/x-dvi";
            MimeTypes["dwg"] = "application/autocad";
            MimeTypes["dxf"] = "application/dxf";
            MimeTypes["dxr"] = "application/x-director";
            MimeTypes["epb"] = "application/x-epublisher";
            MimeTypes["eps"] = "application/postscript";
            MimeTypes["es"] = "audio/echospeech";
            MimeTypes["etx"] = "text/x-setext";
            MimeTypes["evy"] = "application/envoy";
            MimeTypes["exe"] = "application/octet-stream";
            MimeTypes["ez"] = "application/andrew-inset";
            MimeTypes["faxmgr"] = "application/x-fax-manager";
            MimeTypes["faxmgrjob"] = "application/x-fax-manager-job";
            MimeTypes["fgd"] = "application/x-director";
            MimeTypes["fh"] = "image/x-freehand";
            MimeTypes["fh4"] = "image/x-freehand";
            MimeTypes["fh5"] = "image/x-freehand";
            MimeTypes["fh7"] = "image/x-freehand";
            MimeTypes["fhc"] = "image/x-freehand";
            MimeTypes["fif"] = "image/fif";
            MimeTypes["fm"] = "application/x-framemaker";
            MimeTypes["frame"] = "application/x-framemaker";
            MimeTypes["frm"] = "application/x-framemaker";
            MimeTypes["gif"] = "image/gif";
            MimeTypes["gtar"] = "application/x-gtar";
            MimeTypes["hdf"] = "application/x-hdf";
            MimeTypes["hqx"] = "application/mac-binhex40";
            MimeTypes["htm"] = "text/html";
            MimeTypes["html"] = "text/html";
            MimeTypes["ice"] = "x-conference/x-cooltalk";
            MimeTypes["ief"] = "image/ief";
            MimeTypes["igs"] = "model/iges";
            MimeTypes["imd"] = "application/immedia";
            MimeTypes["ims"] = "application/immedia";
            MimeTypes["ins"] = "application/x-insight";
            MimeTypes["insight"] = "application/x-insight";
            MimeTypes["inst"] = "application/x-install";
            MimeTypes["iv"] = "application/x-inventor";
            MimeTypes["ivr"] = "i-world/I-vrml";
            MimeTypes["jpe"] = "image/jpeg";
            MimeTypes["jpeg"] = "image/jpeg";
            MimeTypes["jpg"] = "image/jpeg";
            MimeTypes["js"] = "application/x-javascript";
            MimeTypes["latex"] = "application/x-latex";
            MimeTypes["lha"] = "application/octet-stream";
            MimeTypes["lic"] = "application/x-enterlicense";
            MimeTypes["licmgr"] = "application/x-licensemgr";
            MimeTypes["lzh"] = "application/octet-stream";
            MimeTypes["m3u"] = "";
            MimeTypes["m3u"] = "audio/x-mpegurl";
            MimeTypes["mail"] = "application/x-mailfolder";
            MimeTypes["maker"] = "application/x-framemaker";
            MimeTypes["man"] = "application/x-troff-man";
            MimeTypes["mcf"] = "image/vasa";
            MimeTypes["me"] = "application/x-troff-me";
            MimeTypes["mid"] = "audio/midi";
            MimeTypes["mid"] = "audio/x-midi";
            MimeTypes["mif"] = "application/vnd.mif";
            MimeTypes["mif"] = "application/x-mif";
            MimeTypes["mol"] = "chemical/x-mdl-molfile";
            MimeTypes["mov"] = "video/quicktime";
            MimeTypes["movie"] = "video/x-sgi-movie";
            MimeTypes["mp2"] = "audio/mpeg";
            MimeTypes["mp2a"] = "audio/x-mpeg2";
            MimeTypes["mp2v"] = "video/x-mpeg2";
            MimeTypes["mp3"] = "";
            MimeTypes["mp3"] = "audio/x-mpeg";
            MimeTypes["mp3url"] = "";
            MimeTypes["mpa2"] = "audio/x-mpeg2";
            MimeTypes["mpe"] = "video/mpeg";
            MimeTypes["mpeg"] = "video/mpeg";
            MimeTypes["mpg"] = "video/mpeg";
            MimeTypes["mpga"] = "audio/mpeg";
            MimeTypes["mps"] = "video/x-mpeg-system";
            MimeTypes["mpv2"] = "video/x-mpeg2";
            MimeTypes["ms"] = "application/x-troff-ms";
            MimeTypes["msh"] = "model/mesh";
            MimeTypes["mv"] = "video/x-sgi-movie";
            MimeTypes["nc"] = "application/x-netcdf";
            MimeTypes["oda"] = "application/oda";
            MimeTypes["pat"] = "audio/x-pat";
            MimeTypes["pbm"] = "image/x-portable-bitmap";
            MimeTypes["pcd"] = "image/x-photo-cd";
            MimeTypes["pdb"] = "chemical/x-pdb";
            MimeTypes["pdf"] = "application/pdf";
            MimeTypes["pgm"] = "image/x-portable-graymap";
            MimeTypes["pgn"] = "application/x-chess-pgn";
            MimeTypes["png"] = "image/png";
            MimeTypes["pnm"] = "image/x-portable-anymap";
            MimeTypes["potm"] = "application/vnd.ms-powerpoint.template.macroEnabled.12";
            MimeTypes["potx"] = "application/vnd.openxmlformats-officedocument.presentationml.template";
            MimeTypes["ppam"] = "application/vnd.ms-powerpoint.addin.macroEnabled.12";
            MimeTypes["ppsm"] = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
            MimeTypes["ppsx"] = "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
            MimeTypes["pptm"] = "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
            MimeTypes["pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            MimeTypes["ppm"] = "image/x-portable-pixmap";
            MimeTypes["ppt"] = "application/powerpoint";
            MimeTypes["ps"] = "application/postscript";
            MimeTypes["puz"] = "application/x-crossword";
            MimeTypes["qt"] = "video/quicktime";
            MimeTypes["ra"] = "audio/x-realaudio";
            MimeTypes["ram"] = "audio/x-pn-realaudio";
            MimeTypes["ras"] = "image/x-cmu-raster";
            MimeTypes["rgb"] = "image/x-rgb";
            MimeTypes["rm"] = "audio/x-pn-realaudio";
            MimeTypes["roff"] = "application/x-troff";
            MimeTypes["rpm"] = "audio/x-pn-realaudio-plugin";
            MimeTypes["rtf"] = "application/rtf";
            MimeTypes["rtf"] = "text/rtf";
            MimeTypes["rtx"] = "text/richtext";
            MimeTypes["rxn"] = "chemical/x-mdl-rxn";
            MimeTypes["sbk"] = "audio/x-sbk";
            MimeTypes["sc"] = "application/x-showcase";
            MimeTypes["sds"] = "application/x-onlive";
            MimeTypes["sgi-lpr"] = "application/x-sgi-lpr";
            MimeTypes["sgm"] = "text/x-sgml";
            MimeTypes["sgml"] = "text/sgml";
            MimeTypes["sgml"] = "text/x-sgml";
            MimeTypes["sh"] = "application/x-sh";
            MimeTypes["shar"] = "application/x-shar";
            MimeTypes["sho"] = "application/x-showcase";
            MimeTypes["show"] = "application/x-showcase";
            MimeTypes["showcase"] = "application/x-showcase";
            MimeTypes["sit"] = "application/x-stuffit";
            MimeTypes["skc"] = "chemical/x-mdl-isis";
            MimeTypes["skd"] = "application/x-koan";
            MimeTypes["skm"] = "application/x-koan";
            MimeTypes["skp"] = "application/x-koan";
            MimeTypes["skt"] = "application/x-koan";
            MimeTypes["slides"] = "application/x-showcase";
            MimeTypes["smd"] = "chemical/x-smd";
            MimeTypes["smi"] = "application/smil";
            MimeTypes["smi"] = "chemical/x-daylight-smiles";
            MimeTypes["snd"] = "audio/basic";
            MimeTypes["spl"] = "application/futuresplash";
            MimeTypes["spl"] = "application/x-futuresplash";
            MimeTypes["spr"] = "application/x-sprite";
            MimeTypes["sprite"] = "application/x-sprite";
            MimeTypes["src"] = "application/x-wais-source";
            MimeTypes["str"] = "audio/x-str";
            MimeTypes["sv4cpio"] = "application/x-sv4cpio";
            MimeTypes["sv4crc"] = "application/x-sv4crc";
            MimeTypes["svr"] = "x-world/x-svr";
            MimeTypes["swf"] = "application/x-shockwave-flash";
            MimeTypes["swf"] = "application/x-shockwave-flash";
            MimeTypes["sys"] = "video/x-mpeg-system";
            MimeTypes["t"] = "application/x-troff";
            MimeTypes["talk"] = "text/x-speech";
            MimeTypes["tar"] = "application/x-tar";
            MimeTypes["tardist"] = "application/x-tardist";
            MimeTypes["tcl"] = "application/x-tcl";
            MimeTypes["tex"] = "application/x-tex";
            MimeTypes["texi"] = "application/x-texinfo";
            MimeTypes["texinfo"] = "application/x-texinfo";
            MimeTypes["tif"] = "image/tiff";
            MimeTypes["tiff"] = "image/tiff";
            MimeTypes["tr"] = "application/x-troff";
            MimeTypes["tsi"] = "audio/tsplayer";
            MimeTypes["tsp"] = "application/dsptype";
            MimeTypes["tsv"] = "text/tab-separated-values";
            MimeTypes["tvm"] = "application/x-tvml";
            MimeTypes["tvml"] = "application/x-tvml";
            MimeTypes["txt"] = "text/plain";
            MimeTypes["ustar"] = "application/x-ustar";
            MimeTypes["vcd"] = "application/x-cdlink";
            MimeTypes["viv"] = "video/vivo";
            MimeTypes["vivo"] = "video/vivo";
            MimeTypes["vmd"] = "application/vocaltec-media-desc";
            MimeTypes["vmf"] = "application/vocaltec-media-file";
            MimeTypes["vox"] = "audio/voxware";
            MimeTypes["vrj"] = "x-world/x-vrt";
            MimeTypes["vrml"] = "x-world/x-vrml";
            MimeTypes["vrt"] = "x-world/x-vrt";
            MimeTypes["vdx"] = "application/vnd.ms-visio";
            MimeTypes["vsd"] = "application/vnd.ms-visio";
            MimeTypes["vss"] = "application/vnd.ms-visio";
            MimeTypes["vst"] = "application/vnd.ms-visio";
            MimeTypes["vsx"] = "application/vnd.ms-visio";
            MimeTypes["vtx"] = "application/vnd.ms-visio";
            MimeTypes["wav"] = "audio/x-wav";
            MimeTypes["wbmp"] = "image/vnd.wap.wbmp";
            MimeTypes["wkz"] = "application/x-wingz";
            MimeTypes["wml"] = "text/vnd.wap.wml";
            MimeTypes["wmlc"] = "application/vnd.wap.wmlc";
            MimeTypes["wmls"] = "text/vnd.wap.wmlscript";
            MimeTypes["wmlsc"] = "application/vnd.wap.wmlscriptc";
            MimeTypes["wrl"] = "model/vrml";
            MimeTypes["wrl"] = "x-world/x-vrml";
            MimeTypes["xar"] = "application/vnd.xara";
            MimeTypes["xar"] = "application/vnd.xara";
            MimeTypes["xbm"] = "image/x-xbitmap";
            MimeTypes["xls"] = "application/vnd.ms-excel";
            MimeTypes["xlam"] = "application/vnd.ms-excel.addin.macroEnabled.12";
            MimeTypes["xlsb"] = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
            MimeTypes["xlsm"] = "application/vnd.ms-excel.sheet.macroEnabled.12";
            MimeTypes["xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            MimeTypes["xltm"] = "application/vnd.ms-excel.template.macroEnabled.12";
            MimeTypes["xltx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
            MimeTypes["xml"] = "text/xml";
            MimeTypes["xpm"] = "image/x-xpixmap";
            MimeTypes["xwd"] = "image/x-xwindowdump";
            MimeTypes["xyz"] = "chemical/x-pdb";
            MimeTypes["zip"] = "application/x-zip-compressed";
            MimeTypes["zip"] = "application/zip";
            MimeTypes["ztardist"] = "application/x-ztardist";
        }
    }

    /////////////////////////////////////////////// 
    public class Column
    ///////////////////////////////////////////////
    {
        internal ColumnCollection ParentCollection;
        internal int ColumnIndex
        {
            get
            {
                if (ParentCollection == null)
                    return -1;
                else
                    return ParentCollection.IndexOf(this);
            }
        }
        public string ColumnKey = "";
        public string ColumnName = "";
        private string _Label = "";
        [
        CategoryAttribute("Appearance"),
        DefaultValue(""),
        Description("Text label for column")
        ]
        public string Label
        {
            get { return _Label; }
            set { _Label = value; }
        }
        ///////////////////////////////////////////////
        public Column()
        ///////////////////////////////////////////////
        {
        }
    }

    ///////////////////////////////////////////////
    public class ColumnCollection : CollectionBase
    ///////////////////////////////////////////////
    {
        ///////////////////////////////////////////////
        public void Add(Column column)
        ///////////////////////////////////////////////
        {
            this.List.Add(column);
        }

        ///////////////////////////////////////////////
        public int IndexOf(Column column)
        ///////////////////////////////////////////////
        {
            return this.List.IndexOf(column);
        }

        ///////////////////////////////////////////////
        public void Sort(IComparer comparer)
        ///////////////////////////////////////////////
        {
            this.InnerList.Sort(comparer);
        }
    }

}
