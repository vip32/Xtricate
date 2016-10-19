using System;
using System.Collections.Generic;
using System.Linq;
using Xtricate.Web.Dashboard.Models;
using Xtricate.Web.Dashboard.Pages;

namespace Xtricate.Web.Dashboard
{
    public class RouteCollectionBuilder
    {
        private readonly IDictionary<string, IRequestDispatcher> _dispatchers;
        private readonly EmbeddedResources _resources;
        private RouteCollection _routes;

        public RouteCollectionBuilder(
            IDictionary<string, IRequestDispatcher> dispatchers = null) 
            : this(new EmbeddedResources(), dispatchers)
        {
        }

        public RouteCollectionBuilder(
            EmbeddedResources resources,
            IDictionary<string, IRequestDispatcher> dispatchers = null)
        {
            _resources = resources;
            _dispatchers = dispatchers;
        }

        public RouteCollection Routes
        {
            get
            {
                if (_routes != null) return _routes;

                _routes = new RouteCollection();
                _routes.Add("/", x => new HomeIndex());
                _routes.Add("/info", x => new InformationIndex
                {
                    Model = new InformationViewModel(_routes,
                        _resources.JavascriptEmbeddedResources,
                        _resources.StylesheetEmbeddedResources)
                });
                _routes.Add("/appstats", new JsonApplicationStats());

                //_routes.Add(@"/ts/(?<ScriptName>\S+)",
                //    new ResourceRequestDispatcher(x => new ResourceDispatcher(
                //        "application/typescript",
                //        GetResourcesFolderNamespace(typeof (RouteCollectionBuilder), "ts") + "." +
                //        x.Groups["ScriptName"].Value + ".ts"))
                //    );
                //_routes.Add(@"/js/(?<ScriptName>\S+)",
                //    new ResourceRequestDispatcher(x => new ResourceDispatcher(
                //        "application/javascript",
                //        GetResourcesFolderNamespace(typeof (RouteCollectionBuilder), "js") + "." +
                //        x.Groups["ScriptName"].Value + ".js"))
                //    );
                _routes.Add("/img/logo", new ResourceDispatcher(
                    "image/png",
                    GetResourceName("img", "logo.png"), typeof (RouteCollectionBuilder).Assembly));

                _routes.Add("/js", new ResourceCollectionDispatcher(
                    "application/javascript",
                    GetResourcesFolderNamespace("js"),
                    typeof (RouteCollectionBuilder).Assembly, _resources.JavascriptEmbeddedResources.ToArray()));

                _routes.Add("/ts", new ResourceCollectionDispatcher(
                    "application/javascript",
                    GetResourcesFolderNamespace("js"),
                    typeof (RouteCollectionBuilder).Assembly, _resources.TypescriptEmbeddedResources.ToArray()));

                _routes.Add("/css", new ResourceCollectionDispatcher(
                    "text/css",
                    GetResourcesFolderNamespace("css"),
                    typeof (RouteCollectionBuilder).Assembly, _resources.StylesheetEmbeddedResources.ToArray()));

                _routes.Add("/fonts/glyphicons-halflings-regular/eot", new ResourceDispatcher(
                    "application/vnd.ms-fontobject",
                    GetResourceName("fonts", "glyphicons-halflings-regular.eot"),
                    typeof (RouteCollectionBuilder).Assembly));

                _routes.Add("/fonts/glyphicons-halflings-regular/svg", new ResourceDispatcher(
                    "image/svg+xml",
                    GetResourceName("fonts", "glyphicons-halflings-regular.svg"),
                    typeof (RouteCollectionBuilder).Assembly));

                _routes.Add("/fonts/glyphicons-halflings-regular/ttf", new ResourceDispatcher(
                    "application/octet-stream",
                    GetResourceName("fonts", "glyphicons-halflings-regular.ttf"),
                    typeof (RouteCollectionBuilder).Assembly));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff", new ResourceDispatcher(
                    "application/font-woff",
                    GetResourceName("fonts", "glyphicons-halflings-regular.woff"),
                    typeof (RouteCollectionBuilder).Assembly));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff2", new ResourceDispatcher(
                    "application/font-woff2",
                    GetResourceName("fonts", "glyphicons-halflings-regular.woff2"),
                    typeof (RouteCollectionBuilder).Assembly));

                if (_dispatchers != null && _dispatchers.Any())
                {
                    foreach (var dispatcher in _dispatchers
                        .Where(dispatcher => dispatcher.Key != null && dispatcher.Value != null))
                    {
                        _routes.Add(dispatcher.Key, dispatcher.Value);
                    }
                }

                return _routes;
            }
        }

        public static string GetResourcesFolderNamespace(string folder)
        {
            return GetResourcesFolderNamespace(typeof (RouteCollectionBuilder), folder);
        }

        public static string GetResourcesFolderNamespace(Type type, string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return string.Format("{0}.Resources", type.Namespace);
            return string.Format("{0}.Resources.{1}", type.Namespace, folder);
        }

        public static string GetPagesFolderNamespace(string folder)
        {
            return GetPagesFolderNamespace(typeof(RouteCollectionBuilder), folder);
        }

        public static string GetPagesFolderNamespace(Type type, string folder = null)
        {
            if(string.IsNullOrEmpty(folder))
                return string.Format("{0}.Pages", type.Namespace);
            return string.Format("{0}.Pages.{1}", type.Namespace, folder);
        }

        public static string GetResourceName(string resourceFolder, string name)
        {
            return GetResourceName(typeof (RouteCollectionBuilder), resourceFolder, name);
        }

        public static string GetResourceName(Type type, string resourceFolder, string name)
        {
            return string.Format("{0}.{1}", GetResourcesFolderNamespace(type, resourceFolder), name);
        }
    }
}