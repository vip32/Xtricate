using System;
using Xtricate.Web.Dashboard.Models;
using Xtricate.Web.Dashboard.Pages;

namespace Xtricate.Web.Dashboard
{
    public class DashboardRoutes
    {
        private static readonly string[] Javascripts =
        {
            "jquery.min.js",
            "bootstrap.min.js"
        };

        private static readonly string[] Stylesheets =
        {
            "bootstrap.min.css",
            //"bootstrap-theme.min.css",
            "dashboard.css"
        };

        public DashboardRoutes()
        {
            Routes = new RouteCollection();
            Routes.AddRazorPage("/", x => new HomePage());
            Routes.AddRazorPage("/info", x => new InfoPage
            {
                Model = new DashboardInfoViewModel(Routes, Javascripts, Stylesheets)
            });

            #region Embedded static content

            Routes.Add("/img/logo", new EmbeddedResourceDispatcher(
                "image/png",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("img", "logo.png")));

            Routes.Add("/js", new CombinedResourceDispatcher(
                "application/javascript",
                typeof(DashboardRoutes).Assembly,
                GetContentFolderNamespace("js"),
                Javascripts));

            Routes.Add("/css", new CombinedResourceDispatcher(
                "text/css",
                typeof(DashboardRoutes).Assembly,
                GetContentFolderNamespace("css"),
                Stylesheets));

            Routes.Add("/fonts/glyphicons-halflings-regular/eot", new EmbeddedResourceDispatcher(
                "application/vnd.ms-fontobject",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("fonts", "glyphicons-halflings-regular.eot")));

            Routes.Add("/fonts/glyphicons-halflings-regular/svg", new EmbeddedResourceDispatcher(
                "image/svg+xml",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("fonts", "glyphicons-halflings-regular.svg")));

            Routes.Add("/fonts/glyphicons-halflings-regular/ttf", new EmbeddedResourceDispatcher(
                "application/octet-stream",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("fonts", "glyphicons-halflings-regular.ttf")));

            Routes.Add("/fonts/glyphicons-halflings-regular/woff", new EmbeddedResourceDispatcher(
                "application/font-woff",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("fonts", "glyphicons-halflings-regular.woff")));

            Routes.Add("/fonts/glyphicons-halflings-regular/woff2", new EmbeddedResourceDispatcher(
                "application/font-woff2",
                typeof(DashboardRoutes).Assembly,
                GetContentResourceName("fonts", "glyphicons-halflings-regular.woff2")));

            #endregion
        }

        public RouteCollection Routes { get; }

        public static string GetContentFolderNamespace(string contentFolder)
        {
            return GetContentFolderNamespace(typeof(DashboardRoutes), contentFolder);
        }

        public static string GetContentFolderNamespace(Type type, string contentFolder)
        {
            return string.Format("{0}.Content.{1}", type.Namespace, contentFolder);
        }

        public static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return GetContentResourceName(typeof(DashboardRoutes), contentFolder, resourceName);
        }

        public static string GetContentResourceName(Type type, string contentFolder, string resourceName)
        {
            return string.Format("{0}.{1}", GetContentFolderNamespace(type, contentFolder), resourceName);
        }
    }
}