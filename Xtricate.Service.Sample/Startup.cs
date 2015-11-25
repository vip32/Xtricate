using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;
using Xtricate.Web.Dashboard;
using Xtricate.Web.Dashboard.Pages;

namespace Xtricate.Service.Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            ConfigureWebApi(httpConfig);

            app.UseDashboard(
                routes: new RouteCollectionBuilder(
                    //resources: new EmbeddedResources(
                    //    new[]
                    //    {
                    //        "myscript.js"
                    //    },
                    //    new[]
                    //    {
                    //        "mycss.css"
                    //    }),
                    dispatchers:
                        new Dictionary<string, IRequestDispatcher>
                        {
                            {
                                "/mypage", new RazorPageDispatcher(x => new HomePage())
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
