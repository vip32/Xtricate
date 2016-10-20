using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Xtricate.Core.Templ
{
    public class TemplateEngine : ITemplateEngine
    {
        private readonly ITemplateFactory _templateFactory;
        private readonly IDictionary<string, IDictionary<string, string>> _texts;

        public TemplateEngine(ITemplateFactory templateFactory,
            IDictionary<string, IDictionary<string, string>> texts = null)
        {
            _templateFactory = templateFactory;
            _texts = texts;
        }

        public string Render<TModel>(TModel model, string key, IEnumerable<string> tags = null, CultureInfo culture = null)
        {
            return GetTemplate(model, key, tags, culture).ToString();
        }

        public Template<TModel> GetTemplate<TModel>(TModel model, string key, IEnumerable<string> tags = null, string culture = null)
        {
            if (string.IsNullOrEmpty(culture))
#if NET45
                return GetTemplate(model, key, tags, Thread.CurrentThread.CurrentCulture);
#elif NET46
                return GetTemplate(model, key, tags, Thread.CurrentThread.CurrentCulture);
#else
                return GetTemplate(model, key, tags, CultureInfo.CurrentCulture);
#endif

            return GetTemplate(model, key, tags, new CultureInfo(culture));
        }

        public Template<TModel> GetTemplate<TModel>(TModel model, string key, IEnumerable<string> tags = null, CultureInfo culture = null)
        {
            var template = _templateFactory.Create<TModel>(key, tags);
            if (culture != null) template.Culture = culture;
            template.Model = model;
            template.Texts = _texts;
            return template;
        }

        public string Render(string key, IEnumerable<string> tags = null, CultureInfo culture = null)
        {
            return GetTemplate(key, tags, culture).ToString();
        }

        public Template GetTemplate(string key, IEnumerable<string> tags = null, string culture = null)
        {
            if (string.IsNullOrEmpty(culture))
#if NET45
                return GetTemplate(key, tags, Thread.CurrentThread.CurrentCulture);
#elif NET46
                return GetTemplate(key, tags, Thread.CurrentThread.CurrentCulture);
#else
                return GetTemplate(key, tags, CultureInfo.CurrentCulture);
#endif
            return GetTemplate(key, tags, new CultureInfo(culture));
        }

        public Template GetTemplate(string key, IEnumerable<string> tags = null, CultureInfo culture = null)
        {
            var template = _templateFactory.Create(key, tags);
            if (culture != null) template.Culture = culture;
            template.Texts = _texts;
            return template;
        }
    }
}