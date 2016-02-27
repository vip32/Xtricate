using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Xtricate.Web.Dashboard
{
    public abstract class RazorPage<TModel> : RazorPage
    {
        public TModel Model { get; set; }
    }

    public abstract class RazorPage
    {
        protected readonly StringBuilder Content = new StringBuilder();
        protected string Body;

        protected RazorPage()
        {
            GenerationTime = Stopwatch.StartNew();
            Html = new HtmlHelper(this);
            Parameters = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Parameters { get; set; }
        public Stopwatch GenerationTime { get; set; }
        public HtmlHelper Html { get; protected set; }
        public RazorPage Layout { get; protected set; }
        public UrlHelper Url { get; protected set; }

        public string Name { get; set; }
        public string Title { get; set; }

        public string Parameter(string key)
        {
            string value;
            Parameters.TryGetValue(key, out value);
            return value;
        }

        public abstract void Execute();
        public override string ToString() => TransformText(null);

        public string WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend)) return "";
            Content.Append(textToAppend);
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
            Content.Append(token1.Item1 + value + token2.Item1);
        }

        protected virtual void Write(object value)
        {
            if (value == null) return;
            var html = value as NonEscapedString;
            WriteLiteral(html != null ? html.ToString() : Encode(value.ToString()));
        }

        protected virtual object RenderBody() => new NonEscapedString(Body);

        public string TransformText(string body)
        {
            Body = body;

            Execute();

            if (Layout != null)
            {
                Layout.Assign(this);
                return Layout.TransformText(Content.ToString());
            }

            return Content.ToString();
        }

        private static string Encode(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : WebUtility.HtmlEncode(text);
        }

        //public abstract void Assign(RazorPage parentPage);
        public virtual void Assign(RazorPage parentPage)
        {
            Name = !string.IsNullOrEmpty(parentPage.Title)
                ? $"{parentPage.Name} - {parentPage.Title}"
                : parentPage.Name;
            Url = parentPage.Url;
            GenerationTime = parentPage.GenerationTime;

            OnAssigned();
        }

        public virtual void OnAssigned()
        {
        }

        public virtual void Assign(RequestDispatcherContext context)
        {
            Name = context.Name;
            Url = new UrlHelper(context.OwinEnvironment);
            OnAssigned();
        }
    }
}