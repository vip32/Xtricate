using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;
using Serilog;
using Serilog.Events;
using Xtricate.DocSet.Serilog;
using Xtricate.Service.Dashboard;
using Xtricate.Service.Dashboard.Templates;
using Xtricate.Web.Dashboard;

namespace Xtricate.Service.Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            ConfigureWebApi(httpConfig);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace(LogEventLevel.Debug)
                .WriteTo.LiterateConsole(LogEventLevel.Debug)
                .WriteTo.DocSet("XtricateTestSqlDb", "StorageTests", LogEventLevel.Information)
                .Enrich.WithProperty("App", "SampleApp")
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Debug("started");

            app.UseDashboard(
                options: new DashboardOptions
                {
                    MenuRoutes = new[] {"/products"}
                },
                routes: new RouteCollectionBuilder(
                    new Dictionary<string, IRequestDispatcher>
                    {
                        {
                            "/products",
                            new RequestDispatcher(x => new ProductIndex {Culture = CultureInfo.GetCultureInfo("de-DE")})
                        },
                        {
                            "/products/(?<PageId>\\d+)",
                            new RequestDispatcher(x => new ProductDetails
                            {
                                Culture = CultureInfo.GetCultureInfo("de-DE"),
                                Parameters = new Dictionary<string, string>
                                {
                                    {"id", x.Groups["PageId"].Value}
                                }
                            })
                        },
                        {
                            "/js-treegrid", new ResourceCollectionDispatcher(
                                "application/javascript",
                                typeof (Root).Assembly,
                                RouteCollectionBuilder.GetResourceFolderNamespace(typeof (Root), "js"),
                                "jquery.treegrid.min.js", "jquery.treegrid.bootstrap3.js")
                        },
                        {
                            "/css-treegrid", new ResourceCollectionDispatcher(
                                "text/css",
                                typeof (Root).Assembly,
                                RouteCollectionBuilder.GetResourceFolderNamespace(typeof (Root), "css"),
                                "jquery.treegrid.css")
                        }
                    }).Routes);
            app.UseWebApi(httpConfig);
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}