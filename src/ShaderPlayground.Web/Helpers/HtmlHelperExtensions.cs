using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShaderPlayground.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string SiteBuiltDate(this IHtmlHelper htmlHelper)
        {
            return File.GetLastWriteTime(typeof(HtmlHelperExtensions).Assembly.Location).ToString("yyyy-MM-dd");
        }
    }
}
