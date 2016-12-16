using System.Web;
using System.Web.Optimization;

namespace FTPClient
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Content/js/jquery").Include(
                        "~/Content/js/Lib/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/Content/js/jqueryval").Include(
                        "~/Content/js/Lib/jquery.validate*",
                        "~/Content/js/Lib/jquery.unobtrusive*"
                        ));

            // 開発と学習には、Modernizr の開発バージョンを使用します。次に、実稼働の準備が
            // できたら、http://modernizr.com にあるビルド ツールを使用して、必要なテストのみを選択します。
            bundles.Add(new ScriptBundle("~/Content/js/modernizr").Include(
                        "~/Content/js/Lib/modernizr-*"));

            bundles.Add(new ScriptBundle("~/Content/js/bootstrap").Include(
                      "~/Content/js/Lib/bootstrap.js",
                      "~/Content/js/Lib/respond.js"));

            // jQuery UI
            bundles.Add(new ScriptBundle("~/Content/js/jquery-ui").Include(
                    "~/Content/js/Lib/jquery-ui.js"
                    ));

            bundles.Add(new StyleBundle("~/Content/css/jquery-ui").Include(
                    "~/Content/css/Lib/jquery-ui.css"
                ));

            // CSS
            bundles.Add(new StyleBundle("~/Content/css/common").Include(
                      "~/Content/css/Lib/bootstrap.css",
                      "~/Content/css/Lib/fontello.css",
                      "~/Content/css/Lib/font-awesome.min.css",
                      "~/Content/css/Common.css"));


            // JS
            // Common画面
            bundles.Add(new StyleBundle("~/Content/js/Common").Include(
                    "~/Content/js/Common.js"));

            // Connect画面
            bundles.Add(new StyleBundle("~/Content/js/Connect").Include(
                    "~/Content/js/Connect.js"));

            // Control画面
            bundles.Add(new StyleBundle("~/Content/js/Control").Include(
                    "~/Content/js/Control.js"));


        }
    }
}
