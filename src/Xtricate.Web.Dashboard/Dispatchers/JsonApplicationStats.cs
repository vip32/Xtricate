using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Xtricate.Web.Dashboard
{
    public class JsonApplicationStats : IRequestDispatcher
    {
        private RouteCollection _routes;

        public JsonApplicationStats(RouteCollection routes)
        {
            this._routes = routes;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            var result = new Dictionary<string, object>()
            {
                {"datetime", DateTime.UtcNow },
                {"totalSeconds", DateTime.UtcNow.TimeOfDay.TotalSeconds },
                {"appdomain", @AppDomain.CurrentDomain.FriendlyName },
                {"assembly", Assembly.GetExecutingAssembly() },
                {"culture", Thread.CurrentThread.CurrentCulture },
                {"bool", true },
                {"routes", _routes?.Dispatchers },
            };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
            };
            var serialized = JsonConvert.SerializeObject(result, settings);
            owinContext.Response.ContentType = "application/json";
            owinContext.Response.WriteAsync(serialized);

            return Task.FromResult(true);
        }
    }
}
