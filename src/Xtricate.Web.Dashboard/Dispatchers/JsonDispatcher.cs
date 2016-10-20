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
    public class JsonDispatcher : IRequestDispatcher
    {
        private readonly JsonSerializerSettings _settings;

        public JsonDispatcher(JsonSerializerSettings settings = null)
        {
            if (settings != null)
            {
                _settings = settings;
            }
            else
            {
                _settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
                };
            }
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            var serialized = JsonConvert.SerializeObject(CreateResponseObject(owinContext), _settings);
            owinContext.Response.ContentType = "application/json";
            owinContext.Response.WriteAsync(serialized);

            return Task.FromResult(true);
        }

        public virtual Dictionary<string, object> CreateResponseObject(OwinContext owinContext)
        {
            return new Dictionary<string, object>()
                {
                    {"datetime", DateTime.UtcNow },
                    {"totalSeconds", DateTime.UtcNow.TimeOfDay.TotalSeconds },
                    {"appdomain", @AppDomain.CurrentDomain.FriendlyName },
                    {"assembly", Assembly.GetExecutingAssembly() },
                    {"culture", Thread.CurrentThread.CurrentCulture },
                    {"bool", true },
                };
        }
    }
}
