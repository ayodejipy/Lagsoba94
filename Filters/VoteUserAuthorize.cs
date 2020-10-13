using Lagsoba94.Helpers;
using Lagsoba94.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lagsoba94.Filters
{
    public class VoteUserAuthorize : AuthorizeAttribute
    {
        private protected HttpCookie _tokenCookie, _userIdCookie;
        public VoteUserAuthorize() { }

        private static bool SkipAuthorization(AuthorizationContext filterContext)
        {
            Contract.Assert(filterContext != null);

            return filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any()
                || filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (SkipAuthorization(filterContext)) return;

          isUserAuthorized(filterContext);
        }

        private void isUserAuthorized(AuthorizationContext filterContext)
        {
            // set token and userId cookie
            _tokenCookie = filterContext.HttpContext.Request.Cookies.Get("access");
            _userIdCookie = filterContext.HttpContext.Request.Cookies.Get("userId");

            // get request url
            string returnUrl = filterContext.HttpContext.Request.Url.AbsoluteUri;

           

            // if user's token is expired 
            if (_tokenCookie == null && _userIdCookie != null)
            {
                // check if it's ajax call
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 302; //Found Redirection to another page. Here- login page. Check Layout ajaxError() script.  
                    filterContext.HttpContext.Response.End();
                    SetResultPage(filterContext, "Home", "Index", returnUrl,
                   "Session expired. Please log in again");
                    return;
                }

                SetResultPage(filterContext, "Home", "Index", returnUrl,
                    "Session expired. Please log in again");
                return;
            }

            // if user is not logged in
            if (_tokenCookie == null)
            {
                SetResultPage(filterContext, "Home", "Index", returnUrl,
                    "Please login.");
                return;
            }

            // if user's token is present and there is a username
            if (_tokenCookie != null && _userIdCookie != null)
            {
                bool hasAccess = false;
                VoteUserManager user = new VoteUserManager();

                if (!string.IsNullOrEmpty(this.Roles))
                {
                    // split roles
                    string[] roles = this.Roles.Split(',');
                    for (int i = 0; i < roles.Length; i++)
                    {
                        roles[i] = roles[i].Trim();
                    }

                    // check roles
                    foreach (var role in roles)
                    {
                        if (user.HasRole(role))
                            hasAccess = true;
                    }
                }
                else
                {
                    hasAccess = true;
                }

                // return unauthorized if check failed
                if (!hasAccess)
                {
                    SetResultPage(filterContext, "Unauthorized", "Index");
                }
                else
                {
                    // check if user is a voter
                    if (user.HasRole("Voter"))
                    {
                        // validate if election is still on
                        using (DbContext db = new DbContext())
                        {
                            var endDate = db.Election.Select(x => x.EndDate).FirstOrDefault();
                            var startDate = db.Election.Select(x => x.StartDate).FirstOrDefault();

                            // init today
                            var today = DateTime.Now;

                            if(endDate < today)
                            {
                                SetResultPage(filterContext, "Unauthorized", "vote-over");
                            }
                            else if(today < startDate)
                            {
                                SetResultPage(filterContext, "Unauthorized", "vote-not-started");
                            }
                        }
                    }
                }
            }
        }

        private void SetResultPage(AuthorizationContext filterContext, string controller, string action, string returnUrl = null, string errorMsg = null)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", controller },
                    { "action", action },
                   {"returnUrl", returnUrl },
                   {"errorMsg", errorMsg }
               });
        }
    }
}