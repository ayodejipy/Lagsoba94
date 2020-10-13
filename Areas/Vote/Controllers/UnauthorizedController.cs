using System.Web.Mvc;

namespace Lagsoba94.Areas.Vote.Controllers
{
    public class UnauthorizedController : Controller
    {
        // GET: Unauthorized
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("vote-over")]
        public ActionResult VoteOver()
        {
            return View("VoteOver");
        }

        [ActionName("vote-not-started")]
        public ActionResult VoteNotStarted()
        {
            return View("VoteNotStarted");
        }
    }
}