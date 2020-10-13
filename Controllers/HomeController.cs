using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.Data;
using Lagsoba94.Models.ViewModels;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lagsoba94.Controllers
{
    public class HomeController : Controller
    {
        // init the db context
        private readonly DbContext db = new DbContext();
        private UserManager _userManager;

        public HomeController() { }

        public HomeController(UserManager userManager)
        {
            UserManager = userManager;
        }

        public UserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            private set => _userManager = value;
        }

        public async Task<ActionResult> Index()
        {
            //NewsContext News = new NewsContext();
            List<User> users = db.Users.Where(x => x.Email != "support@bytesintel.com").ToList();

            HomeVM model = new HomeVM();
            List<MembersVM> allMembers = new List<MembersVM>();

            // get users in exeutive role
            foreach (var user in users)
            {
                bool isExecutive = await UserManager.IsInRoleAsync(user.Id, "Executive");
                if (isExecutive)
                {
                    MembersVM member = new MembersVM()
                    {
                        Title = user.Title,
                        FullName = user.FirstName + " " + user.LastName + " " + user.OtherNames,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        OtherNames = user.OtherNames,
                        Profession = user.Profession,
                        Office = db.Office.Where(x=>x.OfficeId == user.OfficeId).Select(x=>x.Name).FirstOrDefault(),
                        Preference = db.Office.Where(x => x.OfficeId == user.OfficeId).Select(x => x.Preference).FirstOrDefault(),
                        Image = db.Images.Where(x => x.UserId == user.Id).Select(x => x.Image1).FirstOrDefault(),
                        UserId = user.Id,
                    };

                    allMembers.Add(member);
                }
            }

            // get news 
            model.News = NewsContext.GetAllNews().Take(7);

            // trim news heading
            foreach (var news in model.News)
            {
                if (news.Heading.Length >= 250)
                {
                    int splitIndex = news.Heading.IndexOf(' ', 220);
                    news.Heading = news.Heading.Substring(0, splitIndex) + "...";
                }
            }

            // get members
            model.Members = allMembers;

            // set the chaiman's speech
            TempData["ChairmanSpeech"] = db.PageContent.Select(x => x.ChairmanSpeech).FirstOrDefault();

            return View(model);
        }

        public ActionResult About()
        {
            // get pageContent
            var pageContent = db.PageContent.FirstOrDefault();

            // init model
            var model = new AboutVM
            {
                Achievements = pageContent.Achievements,
                AimsAndObjectives = pageContent.AimsAndObjectives,
                Mission = pageContent.Mission,
                Vision = pageContent.Vision
            };

            // populate story and divide into two
            if (pageContent.Story.Length >= 1600)
            {
                int searchIndex = 1000;
                int splitIndex = 0;
                var found = false;
                while (!found)
                {
                    splitIndex = pageContent.Story.IndexOf('<', searchIndex);
                    if (pageContent.Story[splitIndex + 1] != '/')
                    {
                        found = true;
                    }
                    else
                    {
                        searchIndex = splitIndex + 1;
                    }
                }

                model.Story_First = pageContent.Story.Substring(0, splitIndex);
                model.Story_Second = pageContent.Story.Substring(splitIndex);
            }
            else
            {
                model.Story_First = pageContent.Story;
                model.Story_Second = "";
            }

            // return model
            return View(model);
        }

        public async Task<ActionResult> Members()
        {
            List<User> users = db.Users.Where(x => x.Email != "support@bytesintel.com").ToList();
            List<MembersVM> membersModel = new List<MembersVM>();

            foreach (var user in users)
            {
                MembersVM member = new MembersVM()
                {
                    Title = user.Title,
                    FullName = user.FirstName + " " + user.LastName + " " + user.OtherNames,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OtherNames = user.OtherNames,
                    Profession = user.Profession,
                    Office = db.Office.Where(x => x.OfficeId == user.OfficeId).Select(x => x.Name).FirstOrDefault(),
                    Preference = db.Office.Where(x => x.OfficeId == user.OfficeId).Select(x => x.Preference).FirstOrDefault(),
                    Image = db.Images.Where(x => x.UserId == user.Id).Select(x => x.Image1).FirstOrDefault(),
                    UserId = user.Id,
                    IsExecutive = await UserManager.IsInRoleAsync(user.Id, "Executive")
                };

                membersModel.Add(member);
            }

            

            return View(membersModel);
        }

        public ActionResult Projects()
        {

            return View();
        }

        public ActionResult Gallery()
        {

            return View();
        }

        public ActionResult News()
        {

            return View();
        }

        public ActionResult Events()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

        //GET:/Home/EventsNavPartial
        public ActionResult EventsNavPartial()
        {
            var endDate = db.Election.Select(x => x.EndDate).FirstOrDefault();
            var startDate = db.Election.Select(x => x.StartDate).FirstOrDefault();

            // init today
            var today = DateTime.Now;

            if (endDate > today && today > startDate && HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.VoteActive = true;
            }

            return PartialView();
        }

        public string _VisionStatement()
        {
            return db.PageContent.Select(x => x.Vision).FirstOrDefault();
        }
    }
}