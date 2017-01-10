using System.Web;
using System.Web.Optimization;

namespace DMP
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    //"~/Scripts/jquery.min.js",
                    "~/plugins/metismenu/js/jquery.metisMenu.js",
                    "~/plugins/blockui-master/js/jquery-ui.js",
                    "~/plugins/blockui-master/js/jquery.blockUI.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                    "~/Scripts/jquery.validate*"));


            bundles.Add(new ScriptBundle("~/bundles/dataTables").Include(
                    "~/plugins/datatables/js/jquery.dataTables.min.js",
                    "~/plugins/datatables/js/dataTables.bootstrap.min.js",
                    "~/plugins/datatables/extensions/Buttons/js/dataTables.buttons.min.js",
                    "~/plugins/datatables/js/dataTables-script.js",
                    "~/plugins/datatables/js/jszip.min.js",
                    "~/plugins/datatables/js/pdfmake.min.js",
                    "~/plugins/datatables/js/vfs_fonts.js",
                    "~/plugins/datatables/extensions/Buttons/js/buttons.html5.js",
                    "~/plugins/datatables/extensions/Buttons/js/buttons.colVis.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/loader").Include(
              "~/Scripts/loader.js"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
               "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/functions.js"));
            //"~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/entypo.css",
                        "~/Content/font-awesome.min.css",
                      "~/Content/bootstrap.min.css",
                      "~/Content/integral-core.css",
                      "~/Content/integral-forms.css",
                      "~/plugins/datatables/css/jquery.dataTables.css",
                      "~/plugins/datatables/extensions/Buttons/css/buttons.dataTables.css",
                      "~/plugins/datepicker/css/bootstrap-datepicker.css",
                        "~/plugins/nouislider/css/nouislider.css",
                        "~/Content/Site.css"));

            bundles.Add(new ScriptBundle("~/bundles/CreateDMPJs").Include(
                      "~/plugins/blockui-master/js/jquery-ui.js",
                      "~/plugins/blockui-master/js/jquery.blockUI.js",
                      //"~/plugins/nouislider/js/nouislider.min.js",
                      "~/plugins/jasny/js/jasny-bootstrap.min.js",
                      "~/plugins/select2/js/select2.full.min.js",
                      "~/plugins/colorpicker/js/bootstrap-colorpicker.min.js",
                      "~/plugins/datepicker/js/bootstrap-datepicker.js",
                      "~/Scripts/form-advanced-script.js",
                      "~/Scripts/jquery.validate.min.js",
                      "~/plugins/wizard/js/jquery.bootstrap.wizard.min.js",
                      "~/plugins/wizard/js/wizard-script.js", 
                      "~/Scripts/functions.js",
                      "~/Scripts/loader.js"));


                        bundles.Add(new StyleBundle("~/bundles/CreateDMPCss").Include(
                         "~/plugins/datepicker/css/bootstrap-datepicker.css",
                         "~/plugins/colorpicker/css/bootstrap-colorpicker.css",
                         "~/plugins/nouislider/css/nouislider.css",
                         "~/plugins/select2/css/select2.css",
                         "~/css/integral-forms.css"
                         ));
        }
    }
}
