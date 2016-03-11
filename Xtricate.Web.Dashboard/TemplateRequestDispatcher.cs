using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class TemplateRequestDispatcher : IRequestDispatcher
    {
        private readonly Func<Match, WebTemplate> _templateFunc;

        public TemplateRequestDispatcher(Func<Match, WebTemplate> templateFunc)
        {
            _templateFunc = templateFunc;
        }

        public Task Dispatch(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);

            // execute the template
            var template = _templateFunc(context.UriMatch);
            template.Assign(context);

            if (!string.IsNullOrEmpty(template.ContentType))
                owinContext.Response.ContentType = template.ContentType;
            return owinContext.Response.WriteAsync(template.ToString());
        }
    }
}