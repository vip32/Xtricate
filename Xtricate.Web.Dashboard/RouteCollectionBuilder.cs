using System;
using System.Collections.Generic;
using System.Linq;
using Xtricate.Web.Dashboard.Models;
using Xtricate.Web.Dashboard.Templates;
namespace Xtricate.Web.Dashboard
{
    public class RouteCollectionBuilder
    {
        private readonly IDictionary<string, IRequestDispatcher> _dispatchers;
        private readonly EmbeddedResources _resources;
        private RouteCollection _routes;

        public RouteCollectionBuilder(
            IDictionary<string, IRequestDispatcher> dispatchers = null) : this(new EmbeddedResources(), dispatchers)
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
                _routes.AddRazorPage("/", x => new HomeIndex());
                _routes.AddRazorPage("/info", x => new InformationIndex
                {
                    //Culture = "de-DE",
                    Model = new InformationViewModel(_routes,
                    _resources.JavascriptEmbeddedResources,
                    _resources.StylesheetEmbeddedResources, new { DynProp = " |dynamic prop test|" })
                });

                _routes.Add("/img/logo", new ResourceDispatcher(
                    "image/png",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("img", "logo.png")));

                _routes.Add("/js", new ResourceCollectionDispatcher(
                    "application/javascript",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceFolderNamespace("js"),
                    _resources.JavascriptEmbeddedResources.ToArray()));

                _routes.Add("/css", new ResourceCollectionDispatcher(
                    "text/css",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceFolderNamespace("css"),
                    _resources.StylesheetEmbeddedResources.ToArray()));

                _routes.Add("/fonts/glyphicons-halflings-regular/eot", new ResourceDispatcher(
                    "application/vnd.ms-fontobject",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("fonts", "glyphicons-halflings-regular.eot")));

                _routes.Add("/fonts/glyphicons-halflings-regular/svg", new ResourceDispatcher(
                    "image/svg+xml",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("fonts", "glyphicons-halflings-regular.svg")));

                _routes.Add("/fonts/glyphicons-halflings-regular/ttf", new ResourceDispatcher(
                    "application/octet-stream",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("fonts", "glyphicons-halflings-regular.ttf")));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff", new ResourceDispatcher(
                    "application/font-woff",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("fonts", "glyphicons-halflings-regular.woff")));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff2", new ResourceDispatcher(
                    "application/font-woff2",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetResourceName("fonts", "glyphicons-halflings-regular.woff2")));

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

        public static string GetResourceFolderNamespace(string resourceFolder)
        {
            return GetResourceFolderNamespace(typeof (RouteCollectionBuilder), resourceFolder);
        }

        public static string GetResourceFolderNamespace(Type type, string resourceFolder)
        {
            return string.Format("{0}.Resources.{1}", type.Namespace, resourceFolder);
        }

        public static string GetResourceName(string resourceFolder, string name)
        {
            return GetResourceName(typeof (RouteCollectionBuilder), resourceFolder, name);
        }

        public static string GetResourceName(Type type, string resourceFolder, string name)
        {
            return string.Format("{0}.{1}", GetResourceFolderNamespace(type, resourceFolder), name);
        }
    }
}