using System.Collections.Generic;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class LocalRequestsOnlyAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            var context = new OwinContext(owinEnvironment);
            var remoteAddress = context.Request.RemoteIpAddress;

            // if unknown, assume not local
            if (string.IsNullOrEmpty(remoteAddress))
                return false;

            // check if localhost
            if (remoteAddress == "127.0.0.1" || remoteAddress == "::1")
                return true;

            // compare with local address
            if (remoteAddress == context.Request.LocalIpAddress)
                return true;

            return false;
        }
    }
}