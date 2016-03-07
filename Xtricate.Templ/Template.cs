using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Xtricate.Templ
{
    public abstract class Template<TModel> : Template
    {
        public TModel Model { get; set; }
    }

    public abstract class Template
    {
        protected readonly StringBuilder Content = new StringBuilder();
        protected string Body;

        protected Template()
        {
            GenerationTime = Stopwatch.StartNew();
            Html = new HtmlHelper(this);
            Parameters = new Dictionary<string, string>();
            OutProperties = new Dictionary<string, string>();
            Texts = new Dictionary<string, IDictionary<string, string>>();
            ContentType = "text/html";
            Culture = Thread.CurrentThread.CurrentCulture.Name;
        }

        public string Culture { get; set; }
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

        public string Text(string key)
        {
            if (string.IsNullOrEmpty(Culture)) return null;
            IDictionary<string, string> values;
            Texts.TryGetValue(Culture, out values);
            if (values == null) return null;
            string value;
            values.TryGetValue(key, out value);
            return value;
        }

        public string OutProperty(string key, string value, bool output = false)
        {
            if (OutProperties.ContainsKey(key)) OutProperties.Remove(key);
            OutProperties.Add(key, value);
            return output ? value : string.Empty;
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