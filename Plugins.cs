
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Data;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using DbNetLink.Data;


///////////////////////////////////////////////
namespace DbNetLink.DbNetSuite
///////////////////////////////////////////////
{
    public interface IDatabaseConnection
    {
        DbNetData GetConnection(string ConnectionAlias);
    }

    ///////////////////////////////////////////////
    public class Plugins
    ///////////////////////////////////////////////
    {
        ArrayList PluginList = new ArrayList();
        HttpContext Context;

        ////////////////////////////////////////////////////////////////////////////
        public Plugins(HttpContext Context)
        ////////////////////////////////////////////////////////////////////////////
        {
            this.Context = Context;
        }

        ////////////////////////////////////////////////////////////////////////////
        public IDatabaseConnection GetConnectionPlugin()
        ////////////////////////////////////////////////////////////////////////////
        {
            return (IDatabaseConnection)LoadPlugin(typeof(IDatabaseConnection));
        }

        ////////////////////////////////////////////////////////////////////////////
        internal object LoadPlugin(Type PlugInType)
        ////////////////////////////////////////////////////////////////////////////
        {
            string PluginFolder = Shared.ConfigValue("PluginFolder");

            if (string.IsNullOrEmpty(PluginFolder))
            {
                PluginFolder = "bin";
            }

            string PluginPath = this.Combine(Context.Request.ApplicationPath, PluginFolder);
            string CurrentPath = Context.Request.MapPath(PluginPath);

            string AppFileName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());

            foreach (string FilePath in Directory.GetFiles(CurrentPath, "*.DLL"))
            {
                if (Path.GetFileName(FilePath).ToLower() == AppFileName.ToLower())
                    continue;

                switch (Path.GetFileName(FilePath).ToLower())
                {
                    case "itextsharp.dll":
                    case "system.web.extensions.dll":
                        continue;
                }

                try
                {
                    Assembly A = Assembly.LoadFile(FilePath);

                    if (A == null)
                        continue;

                    Type[] Types = A.GetTypes();
                    foreach (Type T in Types)
                    {
                        if (PlugInType.IsAssignableFrom(T))
                        {
                            object PlugIn = Activator.CreateInstance(T);
                            PluginList.Add(PlugIn);
                            return PlugIn;
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return null;
        }

        public string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }
    }
}


