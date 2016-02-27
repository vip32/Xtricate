using System;

namespace Xtricate.Web.Dashboard
{
    public class HtmlHelper
    {
        private readonly Template _template;

        public HtmlHelper(Template template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            _template = template;
        }

        public NonEscapedString RenderPartial(Template partialTemplate)
        {
            partialTemplate.Assign(_template);
            return new NonEscapedString(partialTemplate.ToString());
        }
    }
}