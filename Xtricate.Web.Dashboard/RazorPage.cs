using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    public abstract class RazorPage<TModel> : RazorPage
    {
        public TModel Model { get; set; }
    }

    public abstract class RazorPage
    {
        private readonly StringBuilder _content = new StringBuilder();
        private string _body;

        protected RazorPage()
        {
            GenerationTime = Stopwatch.StartNew();
            Html = new HtmlHelper(this);
            Parameters = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Parameters { get; set; }
        public string[] Scripts { get; set; }
        public string[] Stylesheets { get; set; }
        public RazorPage Layout { get; protected set; }
        public HtmlHelper Html { get; private set; }
        public UrlHelper Url { get; private set; }
        public DashboardOptions Options { get; private set; }
        public string Name { get; private set; }
        public string Title { get; set; }
        public string AppPath { get; internal set; }
        public Stopwatch GenerationTime { get; private set; }
        public IOwinRequest Request { get; set; }
        public IOwinResponse Response { get; set; }

        public string RequestPath => Request.Path.Value;

        public string RequestFullPath =>
            $"{Request.Scheme}://{Request.Uri.Host}{(Request.Uri.IsDefaultPort ? string.Empty : ":" + Request.Uri.Port)}{Request.Uri.AbsolutePath}";

        public string Parameter(string key)
        {
            string value;
            Parameters.TryGetValue(key, out value);
            return value;
        }

        public abstract void Execute();

        public string Query(string key) => Request.Query[key];

        public override string ToString() => TransformText(null);

        public void Assign(RazorPage parentPage)
        {
            Request = parentPage.Request;
            Response = parentPage.Response;
            Options = parentPage.Options;
            Name = !string.IsNullOrEmpty(parentPage.Title)
                ? $"{parentPage.Name} - {parentPage.Title}"
                : parentPage.Name;
            AppPath = parentPage.AppPath;
            Url = parentPage.Url;
            Scripts = parentPage.Scripts;
            Stylesheets = parentPage.Stylesheets;
            GenerationTime = parentPage.GenerationTime;

            OnAssigned();
        }

        internal void Assign(RequestDispatcherContext context)
        {
            var owinContext = new OwinContext(context.OwinEnvironment);
            Request = owinContext.Request;
            Response = owinContext.Response;
            Options = context.Options;
            Name = context.Name;
            AppPath = context.AppPath;
            Url = new UrlHelper(context.OwinEnvironment);
            OnAssigned();
        }

        public virtual void OnAssigned()
        {
        }

        public string WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend)) return "";
            _content.Append(textToAppend);
            return "";
        }

        public virtual void WriteAttribute(string attr,
            Tuple<string, int> token1,
            Tuple<string, int> token2,
            Tuple<Tuple<string, int>, Tuple<object, int>, bool> token3)
        {
            object value;
            if (token3 != null)
                value = token3.Item2.Item1;
            else
                value = string.Empty;
            _content.Append(token1.Item1 + value + token2.Item1);
        }

        protected virtual void Write(object value)
        {
            if (value == null) return;
            var html = value as NonEscapedString;
            WriteLiteral(html != null ? html.ToString() : Encode(value.ToString()));
        }

        protected virtual object RenderBody() => new NonEscapedString(_body);

        private string TransformText(string body)
        {
            _body = body;

            Execute();

            if (Layout != null)
            {
                Layout.Assign(this);
                return Layout.TransformText(_content.ToString());
            }

            return _content.ToString();
        }

        private static string Encode(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : WebUtility.HtmlEncode(text);
        }
    }
}