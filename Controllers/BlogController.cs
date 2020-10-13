using Lagsoba94.Models;
using Microsoft.AspNet.Identity;
using Lagsoba94.Models;
using Lagsoba94.Models.Data;
using Lagsoba94.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lagsoba94.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        public ActionResult Index()
        {
            IEnumerable<NewsVM> model = NewsContext.GetAllNews();
            return View(model);
        }

        // GET: Blog/News/
        public ActionResult News(string slug, int id)
        {
            SingleNewsVM model = new SingleNewsVM();

            using (DbContext db = new DbContext())
            {
                model.News = db.News
                    .Where(x => x.Slug == slug && x.NewsId == id)
                    .Select(x => new NewsVM
                    {
                        NewsId = x.NewsId,
                        Slug = x.Slug,
                        Headline = x.Heading,
                        Heading = x.Heading,
                        NewsBody = x.NewsBody,
                        NewsCaster = x.NewsCaster,
                        Authorized = x.Authorized,
                        AuthorizedBy = x.AuthorizedBy,
                        UploadDate = x.UploadDate,
                        TypeId = x.TypeId
                    }).FirstOrDefault();

                // only admins can view news until it's published
                if ((model.News.Authorized == false|| model.News.Authorized == null) && !User.IsInRole("Admin"))
                    return new HttpUnauthorizedResult("The resource is not available");

                model.News.ImageSrc = GetImageFromNews(model.News.NewsBody);

                model.RecentNews = db.News.ToArray()
                    .Where(x => x.Authorized == true)
                       .Select(x => new RecentNews
                       {
                           NewsId = x.NewsId,
                           Slug = x.Slug,
                           Headline = x.Headline,
                           ImageSrc = GetImageFromNews(x.NewsBody),
                           Date = x.UploadDate
                       }).ToList().Take(4);
            }

            ViewBag.Title = model.News.Headline;
            return View(model);
        }

        // GET: Blog/add-news
        [Authorize]
        [ActionName("add-news")]
        public ActionResult AddNews()
        {
            NewsVM model = new NewsVM();
            model.NewsTypeList = new SelectList(NewsContext.GetAllNewsType(), "TypeId", "Description");

            return View("AddNews", model);
        }

        // POST: Blog/add-news
        [Authorize]
        [HttpPost]
        [ActionName("add-news")]
        public ActionResult AddNews(NewsVM model)
        {
            if (ModelState.IsValid)
            {
                NewsDTO newNews = new NewsDTO
                {
                    Slug = model.Headline.Replace(" ", "-").ToLower(),
                    Headline = model.Headline,
                    Heading = model.Heading,
                    NewsBody = model.NewsBody,
                    NewsCaster = User.Identity.GetUserId<int>(),
                    UploadDate = DateTime.UtcNow,
                    TypeId = model.TypeId
                };

                using (DbContext db = new DbContext())
                {
                    db.News.Add(newNews);
                    db.SaveChanges();
                }

                TempData["SM"] = "News added successfully, awaiting approval.";
                return RedirectToAction("add-news");
            }

            // if we get this far, there is an error
            model.NewsTypeList = new SelectList(NewsContext.GetAllNewsType(), "TypeId", "Description");
            return View("AddNews", model);
        }

        // GET:Profile/add-news
        [Authorize(Roles = "Admin")]
        [ActionName("news-management")]
        public ActionResult NewsManagement()
        {
            using (DbContext db = new DbContext())
            {
                var model = db.News.ToArray()
                      .OrderByDescending(x => x.UploadDate)
                      .Select(x => new AllNewsVM(x)).ToList();

                foreach (var news in model)
                {
                    if (news.NewsBody.Contains("<img"))
                    {
                        int index = news.NewsBody.IndexOf("<img");
                        int startIndex = news.NewsBody.IndexOf("src=", index);
                        int endIndex = news.NewsBody.IndexOf("\"", startIndex + 6);
                        news.ImageSrc = news.NewsBody.Substring(startIndex + 6, (endIndex - startIndex) - 6);
                    }
                }

                return View("NewsManagement", model);
            }

        }

        // GET:Profile/approveNews
        [Authorize(Roles = "Admin")]
        public string approveNews(int id)
        {
            try
            {
                using (DbContext db = new DbContext())
                {
                    // init newsDTO
                    NewsDTO newsDTO = db.News.Where(x => x.NewsId == id).FirstOrDefault();

                    // approve news
                    newsDTO.Authorized = true;
                    newsDTO.AuthorizedBy = User.Identity.GetUserId<int>();
                    db.SaveChanges();

                    // return string result
                    return "Successfully approved news.";
                }
            }
            catch (Exception ex)
            {
                return "Unable to approve news at the moment";
            }
        }


        // GET:Profile/approveNews
        [Authorize(Roles = "Admin")]
        public string declineNews(int id)
        {
            try
            {
                using (DbContext db = new DbContext())
                {
                    // init newsDTO
                    NewsDTO newsDTO = db.News.Where(x => x.NewsId == id).FirstOrDefault();

                    // approve news
                    newsDTO.Authorized = false;
                    newsDTO.AuthorizedBy = User.Identity.GetUserId<int>();
                    db.SaveChanges();

                    // return string result
                    return "Successfully declined news.";
                }
            }
            catch (Exception ex)
            {
                return "Unable to decline news at the moment";
            }
        }


        #region Helper
        public string GetImageFromNews(string body)
        {
            string imagesrc = "";
            if (body.Contains("<img"))
            {
                int index = body.IndexOf("<img");
                int startIndex = body.IndexOf("src=", index);
                int endIndex = body.IndexOf("\"", startIndex + 6);
                imagesrc = body.Substring(startIndex + 6, (endIndex - startIndex) - 6);
                if (!imagesrc.StartsWith("/"))
                    imagesrc = "/" + imagesrc;
            }
            return imagesrc;
        }
        #endregion
    }
}