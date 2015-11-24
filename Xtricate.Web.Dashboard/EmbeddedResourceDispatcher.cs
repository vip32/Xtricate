using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    internal class EmbeddedResourceDispatcher : IRequestDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _resourceName;
        private readonly string _contentType;

        public EmbeddedResourceDispatcher(
            string contentType,
            Assembly assembly,
            string resourceName)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _assembly = assembly;
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
            using (var inputStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (inputStream == null)
                {
                    throw new ArgumentException(string.Format(
                        @"Resource with name {0} not found in assembly {1}.",
                        resourceName, assembly));
                }

                var buffer = new byte[Math.Min(inputStream.Length, 4096)];
                var readLength = inputStream.Read(buffer, 0, buffer.Length);
                while (readLength > 0)
                {
                    response.Write(buffer, 0, readLength);
                    readLength = inputStream.Read(buffer, 0, buffer.Length);
                }
            }
        }
    }
}