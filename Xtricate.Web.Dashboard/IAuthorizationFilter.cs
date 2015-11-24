using System.Collections.Generic;

namespace Xtricate.Web.Dashboard
{
    public interface IAuthorizationFilter
    {
        bool Authorize(IDictionary<string, object> owinEnvironment);
    }
}
