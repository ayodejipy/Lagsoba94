using Microsoft.Azure.Mobile.Server.Config;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Lagsoba94.Filters
{
    public class CanonicalizedFilter : ActionFilterAttribute
    {
        private IAppConfiguration _config;

        public CanonicalizedFilter()
        {
            this._config = DependencyResolver.Current.GetService<IAppConfiguration>();
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContextBase Context = filterContext.HttpContext;

            string path = Context.Request.Url.AbsolutePath ?? "/";

            string query = Context.Request.Url.Query;

            // don't 'rewrite' POST requests or you'll loose values:
            if (Context.Request.RequestType == "GET")
            {
                // check for any upper-case letters:
                if (path != path.ToLower(CultureInfo.InvariantCulture))
                {
                    this.Redirect(Context, path, query);
                    return;
                }

                // make sure request ends with a "/"
                if (!path.EndsWith("/"))
                {
                    this.Redirect(Context, path + "/", query);
                    return;
                }



                // perform hostname checks (unless working in dev):
                //if (this._config.IsProductionServer)
                //{
                string hostName = Context.Request.Url.Host.ToLower(CultureInfo.InvariantCulture);

                if (!hostName.Contains("localhost"))
                {
                    this.Redirect(Context, path, query);
                    return;
                }

                // don't allow host-name only connections (i.e., force 'www'):
                if (hostName.StartsWith("www."))
                {
                    this.Redirect(Context, path, query);
                    return;
                }
                //}
            }
            base.OnActionExecuting(filterContext);
        }



        // correct as many 'rules' as possible per redirect to avoid
        // issuing too many redirects per request.
        private void Redirect(HttpContextBase context, string path, string query)
        {
            string newLocation = "https://localhost:44376" + path;

            if (!newLocation.EndsWith("/"))
                newLocation += "/";

            newLocation = newLocation.ToLower(CultureInfo.InvariantCulture);

            if(!context.Response.IsRequestBeingRedirected)
                context.Response.RedirectPermanent(newLocation + query, true);
        }
    }
}