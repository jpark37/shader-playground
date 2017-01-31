using System.Web.Optimization;

namespace OnlineHlslCompiler
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror").Include(
                "~/Scripts/CodeMirror/lib/codemirror.js",
                "~/Scripts/CodeMirror/addon/edit/matchbrackets.js",
                "~/Scripts/CodeMirror/addon/selection/active-line.js",
                "~/Scripts/CodeMirror/mode/clike/clike.js",
                "~/Scripts/codemirror-mode-hlsl.js",
                "~/Scripts/codemirror-mode-llvm-ir.js"));

            bundles.Add(new ScriptBundle("~/bundles/spin").Include(
                "~/Scripts/spin.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/normalize.css",
                "~/Scripts/CodeMirror/lib/codemirror.css",
                "~/Scripts/CodeMirror/theme/neat.css",
                "~/Content/site.css"));
        }
    }
}
