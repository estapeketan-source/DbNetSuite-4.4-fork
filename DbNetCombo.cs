using System;
using System.Web;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using DbNetLink.Data;


namespace DbNetLink.DbNetSuite
{

    public class DbNetCombo : Shared
    {
        [
        CategoryAttribute("Display"),
        DefaultValue(""),
        Description("Adds a Caption element to the top of the grid table.")
        ]
        public string Sql = "";


        ///////////////////////////////////////////////
        public override void ProcessRequest(HttpContext context)
        ///////////////////////////////////////////////
        {
            base.ProcessRequest(context);

            switch (Req["method"].ToString())
            {
                case "load":
                    Load();
                    break;
            }

            context.Response.Write(JSON.Serialize(Resp));
        }

        ///////////////////////////////////////////////
        internal void Load()
        ///////////////////////////////////////////////
        {
            Dictionary<string, object> Parameters = (Dictionary<string, object>)Req["parameters"];

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = this.Sql;

            foreach (string Key in Parameters.Keys)
                Query.Params.Add(Key, Parameters[Key]);

            Resp["items"] = GetComboItems(Query);
        }
    }
}