using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class RazorPageDispatcher : IRequestDispatcher
    {
        private readonly Func<Match, RazorPage> _pageFunc;

        public RazorPageDispatcher(Func<Match, RazorPage> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);
            owinContext.Response.ContentType = "text/html";

            var page = _pageFunc(context.UriMatch);
            page.Assign(context);

            return owinContext.Response.WriteAsync(page.ToString());
        }
    }
}