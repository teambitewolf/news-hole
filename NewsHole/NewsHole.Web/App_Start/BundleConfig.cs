using System.Web.Optimization;

namespace NewsHole.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/jquery")
                .Include("~/Scripts/jquery-2.1.3.js"));

            bundles.Add(new ScriptBundle("~/Scripts/validation")
                .Include("~/Scripts/jquery-validate.js")
                .Include("~/Scripts/jquery-validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/Scripts/bootstrap")
                .Include("~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/bootstrap")
                .Include("~/Content/bootstrap.css")
                .Include("~/Content/bootstrap-theme.css"));
        }
    }
}