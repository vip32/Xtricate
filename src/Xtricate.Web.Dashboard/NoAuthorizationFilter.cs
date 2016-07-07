using System.Collections.Generic;

namespace Xtricate.Web.Dashboard
{
    public class NoAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            return true;
        }
    }
}