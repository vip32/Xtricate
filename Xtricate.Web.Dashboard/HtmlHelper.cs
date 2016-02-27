using System;

namespace Xtricate.Web.Dashboard
{
    public class HtmlHelper
    {
        private readonly RazorTemplate _template;

        public HtmlHelper(RazorTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            _template = template;
        }

        public NonEscapedString RenderPartial(RazorTemplate partialTemplate)
        {
            partialTemplate.Assign(_template);
            return new NonEscapedString(partialTemplate.ToString());
        }
    }
}