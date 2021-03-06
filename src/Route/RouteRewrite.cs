﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class RouteRewrite
    {

        public RouteRewrite(HttpApiServer server)
        {
            mServer = server;
            this.UrlIgnoreCase = mServer.ServerConfig.UrlIgnoreCase;
        }

        public bool UrlIgnoreCase { get; set; }

        private Dictionary<int, RouteGroup> mRoutes = new Dictionary<int, RouteGroup>();

        private HttpApiServer mServer;

        public void AddRegion(UrlRoute[] routes)
        {
            if (routes != null)
                foreach (UrlRoute item in routes)
                {
                    Add(item);
                }
        }

        private void Add(UrlRoute item)
        {
            if (mServer.EnableLog(EventArgs.LogType.Info))
            {
                mServer.Log(EventArgs.LogType.Info, "rewrite setting {0} to {1}", item.Url, item.Rewrite);
            }
            item.UrlIgnoreCase = this.UrlIgnoreCase;
            item.Init();

            RouteGroup rg = null;
            mRoutes.TryGetValue(item.ID, out rg);
            if (rg == null)
            {
                rg = new RouteGroup();
                rg.Ext = item.Ext;
                mRoutes[item.ID] = rg;
            }
            rg.Routes.Add(item);
        }

        public void Add(string pattern, string rewritePattern)
        {
            UrlRoute route = new UrlRoute { Rewrite = rewritePattern, Url = pattern };
            Add(route);
        }

        public bool Match(HttpRequest request, ref RouteMatchResult result, QueryString queryString)
        {
            RouteGroup rg = null;
            if (mRoutes.TryGetValue(request.Path.GetHashCode(), out rg))
            {
                if (string.Compare(rg.Ext, request.Ext, true) == 0)
                {
                    if (rg.Match(request.BaseUrl, ref result, queryString))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

