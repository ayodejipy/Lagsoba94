using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.Data;
using Lagsoba94.Models.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lagsoba94.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        string Error;
        private DbContext db = new DbContext();

        // GET: Settings
        public ActionResult Index()
        {
            // declare model
            var model = db.PageContent.Select(x => new EditSettings()
            {
                ChairmanSpeech = x.ChairmanSpeech,
                Story = x.Story,
                AimsAndObjectives = x.AimsAndObjectives,
                Mission = x.Mission,
                Vision = x.Vision,
                Achievements = x.Achievements
            }).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(EditSettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // get existing data
            var existingContent = db.PageContent.FirstOrDefault();

            bool isError = false;

            // validate and update each content
            if (ValidateInput(model.ChairmanSpeech, 5, 350))
            {
                existingContent.ChairmanSpeech = model.ChairmanSpeech;
            }
            else
            {
                ViewBag.ChairError = Error;
                isError = true;
            }

            if (ValidateInput(model.AimsAndObjectives, 5, 85))
            {
                existingContent.AimsAndObjectives = model.AimsAndObjectives;
            }
            else
            {
                ViewBag.AimsError = Error;
                isError = true;
            }

            if (ValidateInput(model.Story, 10, 500))
            {
                existingContent.Story = model.Story;
            }
            else
            {
                ViewBag.StoryError = Error;
                isError = true;
            }

            if (ValidateInput(model.Mission, 5, 80))
            {
                existingContent.Mission = model.Mission;
            }
            else
            {
                ViewBag.MissionError = Error;
                isError = true;
            }

            if (ValidateInput(model.Vision, 5, 80))
            {
                existingContent.Vision = model.Vision;
            }
            else
            {
                ViewBag.VissionError = Error;
                isError = true;
            }


            existingContent.Achievements = model.Achievements;

            // save changes
            db.SaveChanges();

            // set success msg
            if (isError)
            {
                TempData["ERR"] = "Update completed with some errors.";
            }
            else
            {
                TempData["SCC"] = "Update Successful";
            }

            return View(model);
        }

        public ActionResult Gallery()
        {
            var model = db.Gallery.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddImage()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];

                        // get file name
                        DateTime date = DateTime.UtcNow;
                        string fname = "image-" 
                            + date.ToShortDateString().Replace('/', '-')
                            + "-" 
                            + date.ToLongTimeString().Replace(':',' ') 
                            + Path.GetExtension(file.FileName);

                        // Get the complete folder path and store the file inside it.  
                        var imgPath = Path.Combine(Server.MapPath("~/Content/assets/img/gallery/"), fname);
                        file.SaveAs(imgPath);

                        // save record
                        var galleryRecord = new GalleryDTO { ImageUrl = "/Content/assets/img/gallery/" + fname };
                        db.Gallery.Add(galleryRecord);
                        db.SaveChanges();
                    }

                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        [HttpPost]
        public ActionResult RemoveImage(int id)
        {
            try
            {
                // get image record
                var image = db.Gallery.Where(x => x.GalleryId == id).FirstOrDefault();

                // remove from server
                string fullPath = Request.MapPath("~" + image.ImageUrl);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                //remove from db
                db.Gallery.Remove(image);
                db.SaveChanges();

                return Json(new { Status = 1, Message = "File Removed Successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = "Error occurred. Error details: " + ex.Message });
            }
        }

        #region Helpers
        public bool ValidateInput(string input, int min, int max)
        {
            // count array
            var wordCount = HtmlStringHelper.GetWordCount(input);

            // validate count
            if (wordCount > max)
            {
                Error = "Error: The words has exceeded " + max + " words";
                return false;
            }
            else if (wordCount <= min)
            {
                Error = "Error: There must me a  minimum of " + min + " words";
                return false;
            }

            return true;
        }
        #endregion
    }
}