using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class ResourceRequestDispatcher : IRequestDispatcher
    {
        private readonly Func<Match, ResourceDispatcher> _resourceFunc;

        public ResourceRequestDispatcher(Func<Match, ResourceDispatcher> resourceFunc)
        {
            _resourceFunc = resourceFunc;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            // execute the template
            var resource = _resourceFunc(context.UriMatch);
            return resource.Dispatch(context);
        }
    }
}