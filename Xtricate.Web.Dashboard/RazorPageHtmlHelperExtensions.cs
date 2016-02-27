namespace Xtricate.Web.Dashboard
{
    public static class RazorPageHtmlHelperExtensions
    {
        public static string LoremIpsum(this RazorPageHtmlHelper source)
        {
            return
                "Elit dolor, luctus placerat scelerisque euismod, iaculis eu lacus nunc mi elit, vehicula ut laoreet ac, aliquam sit amet justo nunc tempor, metus vel.";
        }
    }
}