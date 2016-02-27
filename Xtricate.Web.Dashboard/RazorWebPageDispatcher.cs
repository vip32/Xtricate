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
        private readonly Func<Match, RazorWebTemplate> _pageFunc;

        public RazorWebPageDispatcher(Func<Match, RazorWebTemplate> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            // execute the template
            var page = _pageFunc(context.UriMatch);
            page.Assign(context);

            if (!string.IsNullOrEmpty(page.ContentType))
                owinContext.Response.ContentType = page.ContentType;
            return owinContext.Response.WriteAsync(page.ToString());
        }
    }
}