using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;
using Xtricate.Service.Dashboard;
using Xtricate.Service.Dashboard.Pages;
using Xtricate.Web.Dashboard;

namespace Xtricate.Service.Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            ConfigureWebApi(httpConfig);

            app.UseDashboard(new RouteCollectionBuilder(
                    //resources: new EmbeddedResources(
                    //    new[]
                    //    {
                    //        "jquery.treegrid.min.js", "jquery.treegrid.bootstrap3.js"
                    //    },
                    //    new[]
                    //    {
                    //        "jquery.treegrid.css"
                    //    }),
                    dispatchers:
                        new Dictionary<string, IRequestDispatcher>
                        {
                            {
                                "/products", new RazorPageDispatcher(x => new ProductIndex())
                            },
                            {
                                "/js-treegrid", new CombinedResourceDispatcher(
                                    "application/javascript",
                                    typeof (ProductIndex).Assembly,
                                    RouteCollectionBuilder.GetContentFolderNamespace(typeof (Root), "js"),
                                    "jquery.treegrid.min.js", "jquery.treegrid.bootstrap3.js")
                            },
                            {
                                "/css-treegrid", new CombinedResourceDispatcher(
                                    "text/css",
                                    typeof (ProductIndex).Assembly,
                                    RouteCollectionBuilder.GetContentFolderNamespace(typeof (Root), "css"),
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
