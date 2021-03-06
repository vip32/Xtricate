using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Xtricate.Core.Templ
{
    public abstract class Template<TModel> : Template
    {
        public TModel Model { get; set; }
    }

    public abstract class Template
    {
        protected readonly StringBuilder Content = new StringBuilder();
        protected string Body { get; set; }

        protected Template()
        {
            GenerationTime = Stopwatch.StartNew();
            Html = new HtmlHelper(this);
            Parameters = new Dictionary<string, string>();
            OutProperties = new Dictionary<string, string>();
            Texts = new Dictionary<string, IDictionary<string, string>>();
            ContentType = "text/html";
#if NET45
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
#elif NET46
            Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
#else
            Culture = CultureInfo.CurrentCulture;
#endif
        }

        public CultureInfo Culture { get; set; }
        public string ContentType { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public IDictionary<string, string> OutProperties { get; set; }
        public IDictionary<string, IDictionary<string, string>> Texts { get; set; }
        public Stopwatch GenerationTime { get; protected set; }
        public HtmlHelper Html { get; protected set; }
        public Template Layout { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }

        public string Parameter(string key)
        {
            string value;
            Parameters.TryGetValue(key, out value);
            return value;
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(Culture.Name)) return null;
            if (string.IsNullOrEmpty(key)) return null;
            if (Texts == null || !Texts.Any()) return null;

            IDictionary<string, string> values;
            Texts.TryGetValue(Culture.Name, out values);
            if (values == null) return null;
            string value;
            values.TryGetValue(key, out value);
            return value;
        }

        public string SetOutProperty(string key, string value, bool output = false)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (OutProperties.ContainsKey(key)) OutProperties.Remove(key);
            OutProperties.Add(key, value);
            return output ? value : string.Empty;
        }

        public string GetOutProperty(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (OutProperties == null || !OutProperties.Any()) return null;
            if (!OutProperties.ContainsKey(key)) return null;

            return OutProperties[key];
        }

        public abstract void Execute();

        public override string ToString()
        {
#if NET45
            if (Culture == null || Thread.CurrentThread.CurrentCulture.Equals(Culture))
                return TransformText(null);
#elif NET46
            if (Culture == null || Thread.CurrentThread.CurrentCulture.Equals(Culture))
                return TransformText(null);
#else
            if (Culture == null || CultureInfo.CurrentCulture.Equals(Culture))
                return TransformText(null);
#endif

#if NET45
            var culture = Thread.CurrentThread.CurrentCulture;
            var uiculture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
#elif NET46
            var culture = Thread.CurrentThread.CurrentCulture;
            var uiculture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
#else
            var culture = CultureInfo.CurrentCulture;
            var uiculture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentCulture = Culture;
            CultureInfo.CurrentUICulture = Culture;
#endif

            var result = TransformText(null);
#if NET45
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = uiculture;
#elif NET46
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = uiculture;
#else
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiculture;
#endif
            return Regex.Replace(result, @"^\s+$[\r\n]*", "", RegexOptions.Multiline); // remove empty lines
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

        //public abstract void Assign(Template parentTemplate);
        public virtual void Assign(Template parentTemplate)
        {
            if (parentTemplate != null)
            {
                Texts = parentTemplate.Texts;
                Parameters = parentTemplate.Parameters;
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