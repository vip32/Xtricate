using System.Collections.Generic;
using System.Globalization;

namespace Xtricate.Core.Templ
{
    public interface ITemplateEngine
    {
        string Render<TModel>(TModel model, string key, IEnumerable<string> tags = null, CultureInfo culture = null);

        Template<TModel> GetTemplate<TModel>(TModel model, string key, IEnumerable<string> tags = null, string culture = null);

        Template<TModel> GetTemplate<TModel>(TModel model, string key, IEnumerable<string> tags = null, CultureInfo culture = null);

        string Render(string key, IEnumerable<string> tags = null, CultureInfo culture = null);

        Template GetTemplate(string key, IEnumerable<string> tags = null, string culture = null);

        Template GetTemplate(string key, IEnumerable<string> tags = null, CultureInfo culture = null);
    }
}