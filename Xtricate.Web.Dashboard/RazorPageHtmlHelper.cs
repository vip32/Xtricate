using System;

namespace Xtricate.Web.Dashboard
{
    public class RazorPageHtmlHelper
    {
        private readonly RazorPage _page;

        public RazorPageHtmlHelper(RazorPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            _page = page;
        }

        public NonEscapedString RenderPartial(RazorPage partialPage)
        {
            partialPage.Assign(_page);
            return new NonEscapedString(partialPage.ToString());
        }
    }
}