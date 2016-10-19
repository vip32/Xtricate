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
        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            var result = new Dictionary<string, string>()
            {
                {"datetime", DateTime.UtcNow.ToString() },
                {"appdomain", @AppDomain.CurrentDomain.FriendlyName },
                {"assembly", Assembly.GetExecutingAssembly().FullName },
                {"culture", Thread.CurrentThread.CurrentCulture.Name },
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
