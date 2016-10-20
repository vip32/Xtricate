using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xtricate.Web.Dashboard
{
    public class RequestDispatcherContext
    {
        public RequestDispatcherContext(
            DashboardOptions options,
            string name,
            string appPath,
            IDictionary<string, object> owinEnvironment,
            Match uriMatch)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (appPath == null) throw new ArgumentNullException(nameof(appPath));
            if (owinEnvironment == null) throw new ArgumentNullException(nameof(owinEnvironment));
            if (uriMatch == null) throw new ArgumentNullException(nameof(uriMatch));

            Options = options;
            Name = name;
            AppPath = appPath;
            OwinEnvironment = owinEnvironment;
            UriMatch = uriMatch;
        }

        public DashboardOptions Options { get; private set; }

        public string Name { get; private set; }

        public string AppPath { get; private set; }

        public IDictionary<string, object> OwinEnvironment { get; private set; }

        public Match UriMatch { get; private set; }
    }
}