using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.Core.Templ
{
    public class TemplateFactory : ITemplateFactory
    {
        private readonly IEnumerable<TemplateDefinition> _definitions;

        public TemplateFactory(IEnumerable<TemplateDefinition> definitions)
        {
            _definitions = definitions;
        }

        public Template<TModel> Create<TModel>(string key, IEnumerable<string> tags = null)
        {
            var definition = _definitions.FirstOrDefault(t =>
                t.Key.Equals(key, StringComparison.OrdinalIgnoreCase) &&
                t.Tags.NullToEmpty().Intersect(tags.NullToEmpty()).Count().Equals(tags.NullToEmpty().Count()));
            if (definition == null) throw new Exception($"no template found for name={key} and tags={tags}");
            var instance = (Template<TModel>)Activator.CreateInstance(definition.TemplateType, new object[] { });
            if (instance == null) throw new Exception($"template should be of type 'MailTemplate<{typeof(TModel).Name}>'");
            return instance;
        }

        public Template Create(string key, IEnumerable<string> tags = null)
        {
            var definition = _definitions.FirstOrDefault(t =>
                t.Key.Equals(key, StringComparison.OrdinalIgnoreCase) &&
                t.Tags.NullToEmpty().Intersect(tags.NullToEmpty()).Count().Equals(tags.NullToEmpty().Count()));
            if (definition == null) throw new Exception($"no template found for name={key} and tags={tags}");
            var instance = (Template)Activator.CreateInstance(definition.TemplateType, new object[] { });
            if (instance == null) throw new Exception("template should be of type 'MailTemplate'");
            return instance;
        }
    }
}