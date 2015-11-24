using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public class UrlHelper
    {
        private readonly OwinContext _context;

        public UrlHelper(IDictionary<string, object> owinContext)
        {
            if (owinContext == null) throw new ArgumentNullException(nameof(owinContext));
            _context = new OwinContext(owinContext);
        }

        public string To(string relativePath)
        {
            return _context.Request.PathBase + relativePath;
        }

        public string Home()
        {
            return To("/");
        }
    }
}
