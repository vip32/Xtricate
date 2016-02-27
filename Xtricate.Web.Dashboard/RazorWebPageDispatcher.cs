using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
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

            // execute the page
            var page = _pageFunc(context.UriMatch);
            page.Assign(context);

            if (!string.IsNullOrEmpty(page.ContentType))
                owinContext.Response.ContentType = page.ContentType;
            return owinContext.Response.WriteAsync(page.ToString());
        }
    }
}