using System.Collections.Generic;

namespace Xtricate.Templ
{
    public interface ITemplateFactory
    {
        Template Create(string key, IEnumerable<string> tags = null);
        Template<TModel> Create<TModel>(string key, IEnumerable<string> tags = null);
    }
}