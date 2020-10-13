using Lagsoba94.Areas.Vote.Models.ViewModel;
using Lagsoba94.Filters;
using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lagsoba94.Areas.Vote.Controllers
{
    [VoteUserAuthorize(Roles = "Electoral Admin, Electoral Supervisor")]
    [RouteArea("Vote")]
    public class CandidateController : Controller
    {
        private readonly DbContext db = new DbContext();
        private new readonly VoteUserManager User = new VoteUserManager();
        private ResultVM Result { get; set; }

        // GET: Candidate
        //[Route("candidate/for/{positionName?}")]
        public ActionResult Index()
        {
            //if (!string.IsNullOrEmpty(positionName))
            //{
            //    // get position Id
            //    int positionId = db.Position.Where(x => x.Name == positionName).Select(x => x.Id).FirstOrDefault();
            //    IEnumerable<CandidateVM> candidates = User.GetCandidatesFor(positionId);

            //    ViewBag.Title = "Candidates for " + positionName;
            //    ViewBag.PositionId = positionId;

            //    return View("CandidatesForPosition", candidates);
            //}

            // get all positions
            var model = User.GetAllPositions();
            return View("Index", model);
        }

        [ActionName("candidate-for")]
        public async Task<ActionResult> CandidatesFor(string positionName)
        {
            // get position Id
            int positionId = db.Position.Where(x => x.Name == positionName).Select(x => x.Id).FirstOrDefault();
            IEnumerable<CandidateVM> candidates = User.GetCandidatesFor(positionId);

            ViewBag.Title = "Candidates for " + positionName;

            TempData["PositionId"] = positionId;
            TempData["PositionName"] = positionName;

            return View("CandidatesForPosition", candidates);
        }

        public ActionResult CandidatesForPosition(IEnumerable<CandidateVM> model)
        {
            return View(model);
        }

        public ActionResult _CreatePositionPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult _CreatePositionPartial(PositionVM model)
        {
            if (!ModelState.IsValid)
            {
                // mock up error messages to display in json
                string errors = "";

                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(x => x.Errors);

                foreach (var item in allErrors)
                {
                    errors += item.ErrorMessage + "\n";
                }

                ViewBag.Errors = errors;

                return PartialView(model);
            }

            Result = User.CreatePosition(model);

            if (!Result.Success)
            {
                ModelState.AddModelError("error", Result.ErrorMessage);

                return Json(new { Status = 0, Message = Result.ErrorMessage });
                //return PartialView(model);
            }

            TempData["SCC"] = "Position created!";
            return PartialView();
        }

        [ActionName("edit-position")]
        public ActionResult EditPosition(int positionId)
        {
            // get position
            var position = db.Position.ToArray().Where(x => x.Id == positionId).Select(x => new PositionVM(x)).FirstOrDefault();

            // return view with model
            return View("EditPosition", position);
        }

        [HttpPost]
        [ActionName("edit-position")]
        public ActionResult EditPosition(PositionVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditPosition", model);
            }

            // get position
            var position = db.Position.Where(x => x.Id == model.Id).FirstOrDefault();

            //update position
            position.IsActive = model.IsActive;
            position.Name = model.Name;
            position.Qualification = model.Qualification;
            position.RequiredAge = model.RequiredAge;

            // save changes
            db.SaveChanges();

            // return with status
            TempData["SCC"] = "Position updated!";

            return View("EditPosition");
        }

        public ActionResult DeletePosition(int positionId)
        {
            User.DeletePosition(positionId);

            TempData["SCC"] = "Postion deleted successfully";

            return RedirectToAction("Index");
        }

        public ActionResult ActivatePostion(int positionId)
        {
            User.ActivatePosition(positionId);

            TempData["SCC"] = "Postion activated!";

            return RedirectToAction("Index");
        }

        public ActionResult DeactivatePostion(int positionId)
        {
            User.DeactivatePosition(positionId);

            TempData["SCC"] = "Postion deactivated!";

            return RedirectToAction("Index");
        }

        [ActionName("select-new-candidate")]
        public ActionResult SelectNewCandidate(int positionId)
        {
            IEnumerable<NewCandidatesVM> model = User.UsersForNewCandidateAsync();

            TempData["PositionId"] = positionId;
            TempData["PositionName"] = db.Position.Where(x => x.Id == positionId).Select(x => x.Name).FirstOrDefault();

            return View("SelectNewCandidate", model);
        }

        [HttpPost]
        public async Task<ActionResult> MakeNewCandidate(int userId, int positionId)
        {
            if (userId == null || positionId == null)
            {
                return Json(new { Status = 0, Message = "Invalid attempt" });
            }

            // get brief user info
            var user = db.Users.Where(x => x.Id == userId).FirstOrDefault();
            string userImage = db.Images.Where(x => x.UserId == user.Id).Select(x => x.Image1).FirstOrDefault();

            // remove user from voter role
            if (await User.IsInRole(user.Id, "Voter"))
            {
                // get existing voter user 
                var voteUser = db.VoteUsers.Where(x => x.UserId == user.Id).FirstOrDefault();

                // update user image in voteusers table
                voteUser.PositionId = positionId;
                voteUser.ImageString = userImage;

                // save db
                db.SaveChanges();

                // remove user from voter role
                User.RemoveFromRole(user.Id, "Voter");
                User.AddToRole(user.Id, "Candidate");

                // return success Json
                return Json(new { Status = 1, Message = "Created Candidate Successfully" });
            }

            // create new vote User profile
            var newVoteUser = new CandidateVM()
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName + " " + user.OtherNames,
                Phone = user.PhoneNumber,
                Email = user.Email,
                PositionId = positionId,
                ImageString = userImage
            };

            // add new profile
            //db.VoteUsers.Add(newVoteUser);

            // 
            Result = User.CreateCandidate(newVoteUser);

            //db.SaveChanges();

            // return  Json status
            if (Result.Success)
            {
                return Json(new { Status = 1, Message = "Created Candidate Successfully" });
            }
            else
            {
                return Json(new { Status = 0, Message = "Error: Unable to create Candidate" });
            }
        }

        [ActionName("create-candidate")]
        public ActionResult CreateCandidate(string voterId, int positionId)
        {
            CandidateVM model;
            if (string.IsNullOrEmpty(voterId))
            {
                model = new CandidateVM()
                {
                    PositionId = positionId
                };
            }
            else
            {
                model = db.VoteUsers.Where(x => x.VoterId == voterId).Select(x => new CandidateVM()
                {
                    VoterId = x.VoterId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PositionId = positionId
                }).FirstOrDefault();
            }

            ViewBag.Postion = positionId;
            ViewBag.PositionName = db.Position.Where(x => x.Id == positionId).Select(x => x.Name).FirstOrDefault();
            return View("CreateCandidate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("create-candidate")]
        public ActionResult CreateCandidate(CandidateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateCandidate", model);
            }

            Result = User.CreateCandidate(model);

            if (!Result.Success)
            {
                ModelState.AddModelError("duplicate", Result.ErrorMessage);
                return View("CreateCandidate", model);
            }

            TempData["SM"] = "Candidate added successfully";
            return RedirectToAction("create-candidate", new { positionId = model.PositionId });
        }

        [ActionName("select-from-voters")]
        public async Task<ActionResult> SelectFromVoters(int positionId)
        {
            IEnumerable<VoterVM> model = User.GetAllVoters();

            ViewBag.PositionId = positionId;
            return View("SelectFromVoters", model);
        }

        [ActionName("edit-candidate")]
        public ActionResult EditCandidate(string Id)
        {
            var candidate = db.VoteUsers.Where(x => x.VoterId == Id).FirstOrDefault();
            if (candidate == null)
                return RedirectToAction("Index");

            var model = new CandidateVM(candidate);

            return View("EditCandidate", model);
        }

        [HttpPost]
        [ActionName("edit-candidate")]
        public ActionResult EditCandidate(CandidateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCandidate", model);
            }

            var candidate = db.VoteUsers.Where(x => x.VoterId == model.VoterId).FirstOrDefault();
            if (candidate == null)
                return RedirectToAction("Index");

            candidate.FirstName = model.FirstName;
            candidate.LastName = model.LastName;
            candidate.Email = model.Email;
            candidate.Phone = model.Phone;
            candidate.ImageString = model.ImageString;

            db.SaveChanges();

            TempData["SM"] = "Changes Saved";

            return RedirectToAction("edit-candidate", new { Id = model.VoterId });
        }

        [ActionName("delete-candidate")]
        public ActionResult DeleteCandidate(string Id, int positionId)
        {
            Result = User.RemoveCandidate(Id);

            // get position name
            string positionName = db.Position.Where(x => x.Id == positionId).Select(x => x.Name).FirstOrDefault();

            if (!Result.Success)
            {
                TempData["ERR"] = "Error removing candidate now.";
                return RedirectToAction("candidate-for", new { positionName = positionName });
            }

            TempData["SM"] = "Candidate removed successfully.";
            return RedirectToAction("candidate-for", new { positionName = positionName });
        }

        public ActionResult _ElectionDatePartial()
        {
            // get the current expiry date into viewbag
            var election = db.Election.ToArray().Select(x => new ElectionVM(x)).FirstOrDefault();

            // compute date
            string date, limitDate, startDate;
            if (election.EndDate != null)
            {
                var day = election.EndDate.Day;
                var dayOfWeek = election.EndDate.DayOfWeek;
                var month = election.EndDate.ToString("MMMM");
                var year = election.EndDate.Year;

                date = day + " " + month + ", " + year;


                startDate = election.StartDate.Day + " " + election.StartDate.ToString("MMM") + ", " + election.StartDate.Year;

                DateTime now = DateTime.Now;
                string mon = now.Month.ToString("MMM");
                string da = now.Day.ToString();
                if (mon.Length < 2)
                {
                    mon = '0' + mon;
                }

                if (da.Length < 2)
                {
                    da = '0' + da;
                }

                limitDate = now.Year + "-" + mon + "-" + da;
            }
            else
            {
                date = "-";
                limitDate = "";
                startDate = "";
            }

            ViewBag.StartDate = startDate;

            // set current end date
            ViewBag.EndDate = date;

            // set limit date
            ViewBag.Limit = limitDate;

            return PartialView(election);
        }

        [HttpPost]
        public ActionResult _ElectionDatePartial(ElectionVM model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }

            // make sure dates are correct
            if (model.EndDate < model.StartDate)
            {
                return Json(new { Status = 0, Message = "End date should be the future of the start date" });
            }

            // get election
            var election = db.Election.FirstOrDefault();

            // update end date
            election.StartDate = model.StartDate;
            election.EndDate = model.EndDate;

            // save db
            db.SaveChanges();

            return Json(new { Status = 1, Message = "Success" });
        }
    }
}