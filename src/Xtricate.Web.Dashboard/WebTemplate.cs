using Microsoft.Owin;
using Xtricate.Templ;

namespace Xtricate.Web.Dashboard
{
#pragma warning disable SA1402 // File may only contain a single class
    public abstract class WebTemplate<TModel> : WebTemplate
#pragma warning restore SA1402 // File may only contain a single class
    {
        public TModel Model { get; set; }
    }

    public abstract class WebTemplate : Template
    {
        public string[] Scripts { get; set; }

        public string[] TypeScripts { get; set; }

        public string[] Stylesheets { get; set; }

        public DashboardOptions Options { get; set; }

        public string AppPath { get; internal set; }

        public IOwinRequest Request { get; set; }

        public IOwinResponse Response { get; set; }

        public string RequestPath => Request.Path.Value;

        public string RequestFullPath => Request.GetFullPath();

        public bool IsPost => Request.IsPost();

        public UrlHelper Url { get; protected set; }

        public string Query(string key) => Request.Query[key];

        public override void Assign(Template parentTemplate)
        {
            var webParentPage = parentTemplate as WebTemplate;
            if (webParentPage != null)
            {
                Culture = webParentPage.Culture;
                Url = webParentPage.Url;
                Request = webParentPage.Request;
                Options = webParentPage.Options;
                Response = webParentPage.Response;
                AppPath = webParentPage.AppPath;
                Scripts = webParentPage.Scripts;
                TypeScripts = webParentPage.TypeScripts;
                Stylesheets = webParentPage.Stylesheets;
            }
            base.Assign(parentTemplate);
        }

        public void Assign(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);
            Url = new UrlHelper(context.OwinEnvironment);
            Name = context.Name;
            Options = context.Options;
            Request = owinContext.Request;
            Response = owinContext.Response;
            AppPath = context.AppPath;
        }
    }
}