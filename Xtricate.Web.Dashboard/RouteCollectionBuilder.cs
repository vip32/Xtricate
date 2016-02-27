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
                _routes.AddRazorPage("/", x => new HomePage());
                _routes.AddRazorPage("/info", x => new InfoPage
                {
                    //Culture = "de-DE",
                    Model = new DashboardInfoViewModel(_routes,
                    _resources.JavascriptEmbeddedResources,
                    _resources.StylesheetEmbeddedResources, new { DynProp = " |dynamic prop test|" })
                });

                _routes.Add("/img/logo", new EmbeddedResourceDispatcher(
                    "image/png",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("img", "logo.png")));

                _routes.Add("/js", new CombinedResourceDispatcher(
                    "application/javascript",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentFolderNamespace("js"),
                    _resources.JavascriptEmbeddedResources.ToArray()));

                _routes.Add("/css", new CombinedResourceDispatcher(
                    "text/css",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentFolderNamespace("css"),
                    _resources.StylesheetEmbeddedResources.ToArray()));

                _routes.Add("/fonts/glyphicons-halflings-regular/eot", new EmbeddedResourceDispatcher(
                    "application/vnd.ms-fontobject",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("fonts", "glyphicons-halflings-regular.eot")));

                _routes.Add("/fonts/glyphicons-halflings-regular/svg", new EmbeddedResourceDispatcher(
                    "image/svg+xml",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("fonts", "glyphicons-halflings-regular.svg")));

                _routes.Add("/fonts/glyphicons-halflings-regular/ttf", new EmbeddedResourceDispatcher(
                    "application/octet-stream",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("fonts", "glyphicons-halflings-regular.ttf")));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff", new EmbeddedResourceDispatcher(
                    "application/font-woff",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("fonts", "glyphicons-halflings-regular.woff")));

                _routes.Add("/fonts/glyphicons-halflings-regular/woff2", new EmbeddedResourceDispatcher(
                    "application/font-woff2",
                    typeof (RouteCollectionBuilder).Assembly,
                    GetContentResourceName("fonts", "glyphicons-halflings-regular.woff2")));

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

        public static string GetContentFolderNamespace(string contentFolder)
        {
            return GetContentFolderNamespace(typeof (RouteCollectionBuilder), contentFolder);
        }

        public static string GetContentFolderNamespace(Type type, string contentFolder)
        {
            return string.Format("{0}.Content.{1}", type.Namespace, contentFolder);
        }

        public static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return GetContentResourceName(typeof (RouteCollectionBuilder), contentFolder, resourceName);
        }

        public static string GetContentResourceName(Type type, string contentFolder, string resourceName)
        {
            return string.Format("{0}.{1}", GetContentFolderNamespace(type, contentFolder), resourceName);
        }
    }
}