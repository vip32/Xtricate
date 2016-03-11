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
            "knockout.min.js",
            "knockout.mapping.js",
            "sprintf.min.js"
        };

        public readonly List<string> TypescriptEmbeddedResources = new List<string>
        {
            "typescriptservices.js",
            "typescripttranspiler.js"
        };

        public readonly List<string> StylesheetEmbeddedResources = new List<string>
        {
            "bootstrap.min.css",
            "dashboard-dark.css"
        };

        public EmbeddedResources(
            string[] javascriptEmbeddedResources = null,
            string[] typescriptEmbeddedResources = null,
            string[] stylesheetEmbeddedResources = null)
        {
            if (javascriptEmbeddedResources != null) JavascriptEmbeddedResources.AddRange(javascriptEmbeddedResources);
            if (typescriptEmbeddedResources != null) TypescriptEmbeddedResources.AddRange(typescriptEmbeddedResources);
            if (stylesheetEmbeddedResources != null) StylesheetEmbeddedResources.AddRange(stylesheetEmbeddedResources);
        }
    }
}