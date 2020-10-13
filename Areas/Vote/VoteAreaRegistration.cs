using System.Web.Mvc;

namespace Lagsoba94.Areas.Vote
{
    public class VoteAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Vote";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Vote_default",
                "vote/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Lagsoba94.Areas.Vote.Controllers" }
            );
        }
    }
}