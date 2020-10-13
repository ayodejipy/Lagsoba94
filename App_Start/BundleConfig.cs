using System.Web;
using System.Web.Optimization;

namespace Lagsoba94
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                    "~/Content/assets/vendors/slick/slick.js",
                    "~/Content/assets/vendors/datatables/jquery.dataTables.min.js",
                    "~/Content/assets/vendors/datatables/dataTables.bootstrap4.min.js",
                    "~/Content/assets/js/theme.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/assets/vendors/datatables/dataTables.bootstrap4.min.css"));
            
            bundles.Add(new StyleBundle("~/dashboard/css").Include(
                      "~/Content/dashboard/assets/vendors/mdi/css/materialdesignicons.min.css",
                      "~/Content/dashboard/assets/vendors/flag-icon-css/css/flag-icon.min.css",
                      "~/Content/dashboard/assets/vendors/css/vendor.bundle.base.css",
                      "~/Content/dashboard/assets/vendors/font-awesome/css/font-awesome.min.css",
                      "~/Content/dashboard/assets/css/style.css"));

            bundles.Add(new ScriptBundle("~/dashboard/js").Include(
                    "~/Content/assets/vendors/datatables/jquery.dataTables.min.js",
                    "~/Content/assets/vendors/datatables/dataTables.bootstrap4.min.js",
                    "~/Content/dashboard/assets/vendors/js/vendor.bundle.base.js",
                    "~/Content/dashboard/assets/vendors/chart.js/Chart.min.js",
                    "~/Content/dashboard/assets/vendors/jquery-circle-progress/js/circle-progress.min.js",
                    "~/Content/dashboard/assets/js/off-canvas.js",
                    "~/Content/dashboard/assets/js/hoverable-collapse.js",
                    "~/Content/dashboard/assets/js/misc.js",
                    "~/Content/dashboard/assets/js/dashboard.js"));
        }
    }
}
