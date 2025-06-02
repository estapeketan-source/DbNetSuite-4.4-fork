using System;
using System.Web;
using System.Web.SessionState;

namespace DbNetLink.DbNetSuite
{
    public class SessionStateManager : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_PostMapRequestHandler);
        }

        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            var request = app.Context.Request;
            string pageName = request.Url.Segments[request.Url.Segments.Length - 1].ToLower();

            switch (pageName)
            {
                case "dbnetgrid.ashx":
                    if (request.Headers.HasKeys())
                    {
                        switch (request.Headers["method"] ?? String.Empty)
                        {
                            case "load-data":
#if NET40
                                app.Context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
#endif
                                break;
                        }
                    }
                    break;
            }
        }
    }
}


