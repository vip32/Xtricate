using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class RazorWebPageDispatcher : IRequestDispatcher
    {
        private readonly Func<Match, RazorWebPage> _pageFunc;

        public RazorWebPageDispatcher(Func<Match, RazorWebPage> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);
            owinContext.Response.ContentType = "text/html";

            // execute the page
            var page = _pageFunc(context.UriMatch);
            page.Assign(context);

            return owinContext.Response.WriteAsync(page.ToString());
        }
    }
}