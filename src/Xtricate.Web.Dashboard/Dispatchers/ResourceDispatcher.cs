using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class ResourceDispatcher : IRequestDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _contentType;
        private readonly string _resourceName;
        private IEnumerable<Assembly> _assemblies;

        public ResourceDispatcher(string contentType, string resourceName, Assembly assembly = null)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            //if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _assembly = assembly;
            if (assembly == null)
                _assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList().Where(a => !a.IsDynamic);
            _resourceName = resourceName;
            _contentType = contentType;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            owinContext.Response.ContentType = _contentType;
            owinContext.Response.Expires = DateTime.Now.AddYears(1);

            WriteResponse(owinContext.Response);

            return Task.FromResult(true);
        }

        protected virtual void WriteResponse(IOwinResponse response)
        {
            WriteResource(response, _assembly, _resourceName);
        }

        protected void WriteResource(IOwinResponse response, Assembly assembly, string resourceName)
        {
            if (assembly != null) _assemblies = new[] {assembly};
            var found = false;

            foreach (var a in _assemblies)
            {
                using (var inputStream = a.GetManifestResourceStream(resourceName))
                {
                    if (inputStream == null) continue;

                    found = true;
                    var buffer = new byte[Math.Min(inputStream.Length, 4096)];
                    var readLength = inputStream.Read(buffer, 0, buffer.Length);
                    while (readLength > 0)
                    {
                        response.Write(buffer, 0, readLength);
                        readLength = inputStream.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            if (!found)
            {
                throw new ArgumentException(string.Format(
                    @"Resource with name {0} not found in any assembly.", resourceName));
            }
        }
    }
}