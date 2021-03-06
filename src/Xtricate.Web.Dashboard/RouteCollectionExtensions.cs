﻿using System;
using System.Text.RegularExpressions;

namespace Xtricate.Web.Dashboard
{
    public static class RouteCollectionExtensions
    {
        public static void Add(
            this RouteCollection routes,
            string pathTemplate,
            Func<Match, WebTemplate> pageFunc)
        {
            if (routes == null) throw new ArgumentNullException(nameof(routes));
            if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));
            if (pageFunc == null) throw new ArgumentNullException(nameof(pageFunc));

            routes.Add(pathTemplate, new TemplateRequestDispatcher(pageFunc));
        }
    }
}