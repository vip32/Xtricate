using System.Reflection;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class ResourceCollectionDispatcher : ResourceDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _baseNamespace;
        private readonly string[] _resourceNames;

        public ResourceCollectionDispatcher(string contentType, string baseNamespace, Assembly assembly,
            params string[] resourceNames)
            : base(contentType, null, assembly)
        {
            _assembly = assembly;
            _baseNamespace = baseNamespace;
            _resourceNames = resourceNames;
        }

        protected override void WriteResponse(IOwinResponse response)
        {
            foreach (var resourceName in _resourceNames)
            {
                WriteResource(
                    response,
                    _assembly,
                    string.Format("{0}.{1}", _baseNamespace, resourceName));
            }
        }
    }
}