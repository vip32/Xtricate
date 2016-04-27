using System;

namespace Xtricate.Templ
{
    public class HtmlHelper
    {
        private readonly Template _template;

        public HtmlHelper(Template template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            _template = template;
        }

        public NonEscapedString Include(Template partialTemplate)
        {
            partialTemplate.Assign(_template);
            return new NonEscapedString(partialTemplate.ToString());
        }

        public bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}