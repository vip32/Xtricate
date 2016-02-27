using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;

namespace Xtricate.Web.Dashboard
{
    public abstract class RazorTemplate<TModel> : RazorTemplate
    {
        public TModel Model { get; set; }
    }

    public abstract class RazorTemplate
    {
        protected readonly StringBuilder Content = new StringBuilder();
        protected string Body;

        protected RazorTemplate()
        {
            GenerationTime = Stopwatch.StartNew();
            Html = new HtmlHelper(this);
            Parameters = new Dictionary<string, string>();
            ContentType = "text/html";
            Culture = Thread.CurrentThread.CurrentCulture.Name;
        }

        public string Culture { get; set; }
        public string ContentType { get; set; }

        public IDictionary<string, string> Parameters { get; set; }
        public Stopwatch GenerationTime { get; protected set; }
        public HtmlHelper Html { get; protected set; }
        public RazorTemplate Layout { get; protected set; }

        public string Name { get; protected set; }
        public string Title { get; protected set; }

        public string Parameter(string key)
        {
            string value;
            Parameters.TryGetValue(key, out value);
            return value;
        }

        public abstract void Execute();
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Culture) || Thread.CurrentThread.CurrentCulture.Name.Equals(Culture))
                return TransformText(null);
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Culture);
            var result = TransformText(null);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            return result;
        }

        protected string WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend)) return "";
            Content.Append(textToAppend);
            return "";
        }

        protected virtual void WriteAttribute(string attr,
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

        protected string TransformText(string body)
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


        //public abstract void Assign(RazorTemplate parentTemplate);
        public virtual void Assign(RazorTemplate parentTemplate)
        {
            if (parentTemplate != null)
            {
                Culture = parentTemplate.Culture;
                Name = !string.IsNullOrEmpty(parentTemplate.Title)
                    ? $"{parentTemplate.Name} - {parentTemplate.Title}"
                    : parentTemplate.Name;
                GenerationTime = parentTemplate.GenerationTime;
            }

            OnAssigned();
        }

        public virtual void OnAssigned()
        {
        }
    }
}