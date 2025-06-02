using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security;
using System.Security.Permissions;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Net;


[assembly: TagPrefix("DbNetLink.DbNetSuite", "DNL")]

////////////////////////////////////////////////////////////////////////////
namespace DbNetLink.DbNetSuite.UI
////////////////////////////////////////////////////////////////////////////
{

    ////////////////////////////////////////////////////////////////////////////
    public abstract partial class DatabaseControl : DbNetLink.DbNetSuite.UI.Component
    ////////////////////////////////////////////////////////////////////////////
    {
        private string _ConnectionString = "";
        [
        TypeConverter(typeof(ConnectionStringConverter)),
        Category("Database"),
        Description("The name of the connection string in the web.config file")
        ]
        ////////////////////////////////////////
        public String ConnectionString
        ////////////////////////////////////////
        {
            get { return _ConnectionString; }
            set
            {
                _ConnectionString = value;

                if (this.DesignMode && value != "")
                {
                    try
                    {
                        DbNetLink.Data.DbNetData Db = DesignUtility.DbConnection(value, Site, ref _DesignTimeErrorMessage);

                        if (Db != null)
                            Db.Close();
                    }
                    catch (Exception) { }
                }
            }
        }

        private int _CommandTimeout = 30;
        [
        Category("Database"),
        Description("The number of seconds before a database query will timeout")
        ]
        ////////////////////////////////////////
        public int CommandTimeout
        ////////////////////////////////////////
        {
            get { return _CommandTimeout; }
            set { _CommandTimeout = value; }
        }

        private bool _CaseInsensitiveSearch = false;
        [
        Category("Database"),
        Description("Makes text searching case-insensitive where supported")
        ]
        ////////////////////////////////////////
        public bool CaseInsensitiveSearch
        ////////////////////////////////////////
        {
            get { return _CaseInsensitiveSearch; }
            set { _CaseInsensitiveSearch = value; }
        }

        ////////////////////////////////////////////////////////////////////////////
        public DatabaseControl(HtmlTextWriterTag Tag)
            : base(Tag)
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void RenderContents(HtmlTextWriter Writer)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.RenderContents(Writer);
        }

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnPreRender(EventArgs e)
        ////////////////////////////////////////////////////////////////////////////
        {
            base.OnPreRender(e);
            base.CreateChildControls();
        }


    }
}