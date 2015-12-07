using System.Collections.Generic;

namespace Xtricate.Web.Dashboard
{
    public class EmbeddedResources
    {
        public readonly List<string> JavascriptEmbeddedResources = new List<string>
        {
            "jquery.min.js",
            "bootstrap.min.js",
            "lodash.min.js",
            "knockout.min.js"
        };

        public readonly List<string> StylesheetEmbeddedResources = new List<string>
        {
            "bootstrap.min.css",
            //"bootstrap-theme.min.css",
            "dashboard.css"
        };

        public EmbeddedResources(
            string[] javascriptEmbeddedResources = null,
            string[] stylesheetEmbeddedResources = null)
        {
            if (javascriptEmbeddedResources != null) JavascriptEmbeddedResources.AddRange(javascriptEmbeddedResources);
            if (stylesheetEmbeddedResources != null) StylesheetEmbeddedResources.AddRange(stylesheetEmbeddedResources);
        }
    }
}