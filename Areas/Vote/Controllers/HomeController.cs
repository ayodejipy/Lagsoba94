using Lagsoba94.Areas.Vote.Models.Data;
using Lagsoba94.Areas.Vote.Models.ViewModel;
using Lagsoba94.Filters;
using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.ViewModel;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Lagsoba94.Areas.Vote.Controllers
{
    [VoteUserAuthorize]
    [RouteArea("Vote")]
    public class HomeController : Controller
    {
        private readonly DbContext db = new DbContext();
        private new readonly VoteUserManager User = new VoteUserManager();

        [AllowAnonymous]
        public ActionResult Index(string returnUrl, string errorMsg)
        {
            //UserManager userManger = new UserManager();
            if (User.IsAuthenticated())
                return RedirectToAction("Vote");

            ViewBag.ReturnUrl = returnUrl;
            if (!string.IsNullOrEmpty(errorMsg))
                ModelState.AddModelError("loginError", errorMsg);
            return View();
        }

        public ActionResult Vote()
        {
            // get all positions
            IEnumerable<PositionVM> model = User.GetActivePositions();

            // get logged in user 
            var user = db.VoteUsers.Where(x => x.Email == User.Email).FirstOrDefault();
            ViewBag.Name = user.FirstName + " " + user.LastName;

            // get all votes for this user
            List<int?> votedPositions = db.Votes.Where(x => x.VoterId == user.VoterId).Select(x => x.PositionId).ToList();

            // get all postions this user hasn't voted for
            List<int> notVotedPosition = model.Select(x => x.Id).ToList();

            foreach (var post in votedPositions)
            {
                int postId;
                if (post != null)
                {
                    postId = post ?? default;
                    notVotedPosition.Remove(postId);
                }
            }

            // wildly set active position if user has voted for all positions
            if (notVotedPosition.Count() < 1)
            {
                ViewBag.ActivePosition = "Finished";
                ViewBag.PositionId = 0;
                ViewBag.AllVoted = true;
            }

            // loop through names of postion to trim them
            foreach (var item in model)
            {
                // get the first unvoted position
                if (notVotedPosition.Count() > 0 && item.Id == notVotedPosition.First())
                {
                    //string activePosition = item.Name.Replace(" ", "%20");
                    ViewBag.ActivePosition = item.Name.Replace('.', ' ');
                    ViewBag.PositionId = item.Id;
                }
            }

            return View(model);
        }

        [Route("home/vote-for/{positionId}/{positionName?}")]
        public PartialViewResult VoteFor(int positionId, string positionName)
        {
            // check if vote is finished
            if (positionId == 0 && positionName.ToLower() == "finished")
            {
                // get all votes for user
                var userVotes = db.Votes.Where(x => x.VoterId == User.VoterId).ToList();

                // init voteResult
                List<VoteResultVM> voteResults = new List<VoteResultVM>();

                foreach (var vote in userVotes)
                {
                    var result = new VoteResultVM
                    {
                        PositionName = db.Position.Where(x => x.Id == vote.PositionId).Select(x => x.Name).FirstOrDefault()
                    };

                    if (vote.CandidateId == null)
                    {
                        result.CandidateName = "NO VOTE";
                    }
                    else
                    {
                        result.CandidateName = db.VoteUsers.Where(x => x.VoterId == vote.CandidateId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    }

                    //add result 
                    voteResults.Add(result);
                }

                // construct result mail message
                string resultMail = "";

                foreach (var result in voteResults)
                {
                    string votedFor = "<tr>" +
                     "<td style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\">" + result.PositionName +
                     "</td>" +
                     "<td style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\">" + result.CandidateName +
                     "</td>" +
                     "</tr>";

                    resultMail += votedFor;
                }

                // get username and date
                string userName = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                DateTime? voteDate = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.LastVoteDateUtc).FirstOrDefault();

                string date;
                if (voteDate != null)
                {
                    DateTime updatedVoteDate = voteDate ?? DateTime.Now;

                    var day = updatedVoteDate.Day;
                    var dayOfWeek = updatedVoteDate.DayOfWeek;
                    var month = updatedVoteDate.ToString("MMMM");
                    var year = updatedVoteDate.Year;

                    date = day + " " + month + ", " + year;
                }
                else
                {
                    date = "-";
                }

                string message = "<html><body>" +
                     "<div style=\"text-align:center;max-width:600px;\">" +

                     "<h3 style=\"background-color: #64608e;margin-bottom: 0px;padding: 10px 0px;color: #fff;\">" +
                     "My Voting Summary - LAGSOBA '94  " +
                     "</h3>" +

                     "<div style=\"border:2px solid #64608e\">" +
                     "<table style=\"width: 100%;margin-bottom: 1rem;\">" +

                     "<tr>" +
                     "<th style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\"> Position</th>" +
                     "<th style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\"> Candidate Voted For</th>" +
                     "</tr>" + resultMail +

                     "</table>" +

                     "<div style=\"margin-bottom: 20px;\">" +
                    "<h6>The Vote Summary Above was casted by:</h6>" +
                    "<p style=\"margin: 0px;\"><strong>Name: </strong> " + userName + "</p>" +
                    "<p><strong>Last Vote On: </strong> " + date + "</p>" +
                    "</div>" +

                     "<p style=\"color: #846d6dc7;\"><em>Thanks For Voting</em></p>" +

                     "</div>" +
                     "</div>" +
                     "</body></html>";

                ViewBag.MyVoteResult = message;

                return PartialView("_MyVoteResult");
            }


            // get user Id
            string voterId = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.VoterId).FirstOrDefault();

            VotingVM model = new VotingVM();

            // get position
            var postion = db.Position.Where(x => x.Id == positionId).FirstOrDefault();

            // get candidates 
            IEnumerable<CandidateVM> candidates = User.GetCandidatesFor(postion.Id);

            // init model.candidates
            model.Candidates = candidates.Select(x => new Candidates
            {
                Id = x.VoterId,
                Name = x.FirstName + " " + x.LastName,
                ImageUrl = x.ImageString,
                PositionId = x.PositionId
            }).ToList();

            // get postion name
            model.PostionName = db.Position.Where(x => x.Id == postion.Id).Select(x => x.Name).FirstOrDefault();

            // check if user has casted votes for this position
            model.VoteCasted = false;
            var voteCasted = db.Votes.Where(x => x.VoterId == voterId && x.PositionId == postion.Id).FirstOrDefault();
            if (voteCasted != null)
            {
                model.VoteCasted = true;
                model.VotedCandidateId = voteCasted.CandidateId;
            }

            // admin privileges
            model.IsAdmin = User.HasRole("Admin");

            // set user name
            ViewBag.VoterName = db.VoteUsers.Where(x => x.VoterId == voterId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

            return PartialView("_VoteForPosition", model);
        }

        [AllowAnonymous]
        public ActionResult GrantAccess(LoginVM model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // declare user Id
            string emailAddress;

            // check and authorize user from db
            var user = db.VoteUsers.Where(x => x.VoterId == model.VoterId && x.Email == model.Email).FirstOrDefault();
            if (user == null || HttpContext.User.Identity.Name != model.Email)
            {
                ModelState.AddModelError("loginError", "Check in atempt failed. Please use correct credentials.");
                return View("Index", model);
            }

            // set user id
            emailAddress = user.Email;

            SetCookies(emailAddress);

            // return to previous url if exist
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Vote");
        }

        [VoteUserAuthorize]
        public ActionResult Logout()
        {
            RemoveCookie();
            return Redirect("~/home");
        }

        [ActionName("vote-proof")]
        public ActionResult VoteProof(string candidateId)
        {
            // get vote and user details
            VoteProofVM model = new VoteProofVM();

            // init user & candidate
            var user = db.VoteUsers.Where(x => x.Email == User.Email).FirstOrDefault();
            var candidate = db.VoteUsers.Where(x => x.VoterId == candidateId).FirstOrDefault();

            // log user out if this is an invalid attempt.
            if (user == null || candidate == null)
            {
                RemoveCookie();
                string errorMsg = "Invalid or malicious attempt. You have been logged out.";
                return RedirectToAction("Index", new { errorMsg = errorMsg });
            }

            // fill model
            model.Name = user.FirstName + " " + user.LastName;
            model.Phone = user.Phone;

            // fill candidate
            model.Candidate = candidate;
            model.Position = db.Position.Where(x => x.Id == candidate.PositionId).Select(x => x.Name).FirstOrDefault();

            // get vote date
            DateTime votedate = db.Votes.Where(x => x.CandidateId == candidateId && x.VoterId == user.VoterId).Select(x => x.DateUtc).FirstOrDefault();
            model.VoteDate = votedate.ToLocalTime();

            // return view with model
            return View("VoteProof", model);
        }

        [ActionName("vote-proof-download")]
        public ActionResult VoteProofDownload(string candidateId)
        {
            // read parameters from the webpage
            string htmlString = GetProofPdfHtml(candidateId);
            string baseUrl = "";

            string pdf_page_size = PdfPageSize.A4.ToString();
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize),
                pdf_page_size, true);

            string pdf_orientation = PdfPageOrientation.Portrait.ToString();
            PdfPageOrientation pdfOrientation =
                (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation),
                pdf_orientation, true);

            int webPageWidth = 1024;
            try
            {
                webPageWidth = Convert.ToInt32("");
            }
            catch { }

            int webPageHeight = 0;
            try
            {
                webPageHeight = Convert.ToInt32("");
            }
            catch { }

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(htmlString, baseUrl);

            // save pdf document
            byte[] pdf = doc.Save();

            // close pdf document
            doc.Close();

            // return resulted pdf document
            FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            fileResult.FileDownloadName = "Vote-Proof-Download.pdf";
            return fileResult;
        }

        [HttpPost]
        [ActionName("cast-vote")]
        public ActionResult CastVote(string candidateId)
        {
            try
            {
                // get candidate
                var candidate = db.VoteUsers.Where(x => x.VoterId == candidateId).FirstOrDefault();
                int? positionId = db.VoteUsers.Where(x => x.VoterId == candidateId).Select(x => x.PositionId).FirstOrDefault();

                // log out user when cadidate is null
                if (candidate == null)
                {
                    RemoveCookie();
                    string errorMsg = "You attempted to vote for a candidate that doesn't exist. Please log in again.";
                    return Json(new { Status = 0, Url = Url.Action("Index", "Home", new { errorMsg = errorMsg }) });
                }

                if (User.HasRole("Candidate"))
                {
                    RemoveCookie();
                    string errorMsg = "You cannot cast a vote because you are a contestant. Please log in again.";
                    return Json(new { Status = 0, Url = Url.Action("Index", "Home", new { errorMsg = errorMsg }) });
                }

                // get user id 
                string email = User.Email;
                string voterId = db.VoteUsers.Where(x => x.Email == email).Select(x => x.VoterId).FirstOrDefault();

                // check if vote exist before
                var existingVote = db.Votes
                    .Where(x => x.VoterId == voterId && x.PositionId == candidate.PositionId)
                    .FirstOrDefault();

                if (existingVote != null)
                {
                    RemoveCookie();
                    string errorMsg = "You attempted to vote twice. This is not allowed! You have been logged out.";
                    return Json(new { Status = 0, Url = Url.Action("Index", "Home", new { errorMsg = errorMsg }) });
                }

                // cast vote for candidate
                var newVote = new VoteDTO
                {
                    VoterId = voterId,
                    CandidateId = candidateId,
                    PositionId = candidate.PositionId,
                    DateUtc = DateTime.UtcNow
                };

                // add vote to db and save
                db.Votes.Add(newVote);

                // save last vote date
                var user = db.VoteUsers.Where(x => x.VoterId == voterId).FirstOrDefault();
                user.LastVoteDateUtc = DateTime.UtcNow;

                // save db
                db.SaveChanges();


                // set tempdata
                TempData["SCC"] = "Vote Submitted!";
            }
            catch (Exception)
            {
                // set tempdata
                TempData["ERR"] = "Failed to cast vote, please try again.";
                return Json(new { Status = 3, Message = "Error" });
            }
            // return
            return Json(new { Status = 1, Message = "Success" });
        }

        [HttpPost]
        public ActionResult NoVote(string positionName)
        {
            // get the position
            var position = db.Position.Where(x => x.Name == positionName).FirstOrDefault();

            // return error null position
            if (position == null)
                return Json(new { Status = 0, Message = "Error" });

            // get user ID
            var voterId = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.VoterId).FirstOrDefault();

            // prepare a new vote
            var newVote = new VoteDTO
            {
                VoterId = voterId,
                CandidateId = null,
                PositionId = position.Id,
                DateUtc = DateTime.UtcNow
            };

            // cast a null vote for position
            db.Votes.Add(newVote);

            // save last vote date
            var user = db.VoteUsers.Where(x => x.VoterId == voterId).FirstOrDefault();
            user.LastVoteDateUtc = DateTime.UtcNow;

            // save to db
            db.SaveChanges();

            // set tempdata
            TempData["SCC"] = "Vote Submitted - No Vote";

            // return success message
            return Json(new { Status = 1, Message = "Success" });
        }

        [VoteUserAuthorize(Roles = "Electoral Admin, Electoral Supervisor")]
        [ActionName("election-results")]
        public ActionResult VoteResults()
        {
            // init count
            int count = 0;

            // init model
            VoteResult model = new VoteResult();
            //model.Votes = new List<SingleVote>();

            // init candidateScore
            List<CandidateScore> candidateScores = new List<CandidateScore>();

            // get votes
            var votes = db.Votes.ToArray();

            // get candidates
            var allCandidates = User.GetAllCandidates();

            // init candidate scores
            model.CandidateScores = allCandidates.Select(x => new CandidateScore
            {
                Id = x.VoterId,
                Name = x.FirstName + " " + x.LastName,
                VoteCount = 0,
                PositionId = x.PositionId
            }).ToList();

            // init all position ids 
            model.Positions = User.GetAllPositions();

            // loop through votes and get each voter details
            foreach (var item in votes)
            {
                // init count
                count++;

                // get user
                var user = db.VoteUsers.Where(x => x.VoterId == item.VoterId).FirstOrDefault();

                if (model.CandidateScores.Any(x => x.Id == item.CandidateId))
                {
                    var cand = model.CandidateScores.Where(x => x.Id == item.CandidateId).FirstOrDefault();
                    cand.VoteCount++;
                }

                // init singlevm
                SingleVote vote = new SingleVote
                {
                    SN = count,
                    VoterName = user.FirstName + " " + user.LastName,
                    VoterPhone = user.Phone,
                    VotedFor = db.VoteUsers.Where(x => x.VoterId == item.CandidateId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault()
                };

                //model.Votes.Add(vote);
            };

            model.Name = User.GetName();

            return View("VoteResults", model);
        }

        [ActionName("position-vote-result")]
        public PartialViewResult PositionVoteResult(int id)
        {
            // init count
            int count = 0;

            // init model
            List<SingleVote> model = new List<SingleVote>();

            // get all votes
            var allVotes = db.Votes.ToList().Where(x => x.PositionId == id);

            // loop through all votes and init model
            foreach (var item in allVotes)
            {
                // init count
                count++;

                // get user
                var user = db.VoteUsers.Where(x => x.VoterId == item.VoterId).FirstOrDefault();

                // init singlevm
                SingleVote vote = new SingleVote
                {
                    SN = count,
                    VoterName = user.FirstName + " " + user.LastName,
                    VoterPhone = user.Phone,
                    VotedFor = db.VoteUsers.Where(x => x.VoterId == item.CandidateId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault()
                };

                model.Add(vote);
            };

            ViewBag.PositionName = db.Position.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            ViewBag.PositionActive = db.Position.Where(x => x.Id == id).Select(x => x.IsActive).FirstOrDefault();

            return PartialView("_PositionVoteResult", model);
        }

        [ActionName("send-me-a-copy")]
        public ActionResult SendMeACopy()
        {
            // get userId
            var voterId = User.VoterId;

            // get all votes for user
            var userVotes = db.Votes.Where(x => x.VoterId == voterId).ToList();

            // init voteResult
            List<VoteResultVM> voteResults = new List<VoteResultVM>();

            foreach (var vote in userVotes)
            {
                var result = new VoteResultVM
                {
                    PositionName = db.Position.Where(x => x.Id == vote.PositionId).Select(x => x.Name).FirstOrDefault()
                };

                if (vote.CandidateId == null)
                {
                    result.CandidateName = "NO VOTE";
                }
                else
                {
                    result.CandidateName = db.VoteUsers.Where(x => x.VoterId == vote.CandidateId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                }

                //add result 
                voteResults.Add(result);
            }

            // construct result mail message
            string resultMail = "";

            foreach (var result in voteResults)
            {
                string votedFor = "<tr>" +
                 "<td style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\">" + result.PositionName +
                 "</td>" +
                 "<td style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\">" + result.CandidateName +
                 "</td>" +
                 "</tr>";

                resultMail += votedFor;
            }

            // Send an email with this link
            var mailModel = new EmailMessageVM();

            mailModel.ToAddress = User.Email;
            mailModel.Subject = "Voting Result - LAGSOBA '94";

            // get username and date
            string userName = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
            DateTime? voteDate = db.VoteUsers.Where(x => x.Email == User.Email).Select(x => x.LastVoteDateUtc).FirstOrDefault();

            string date;
            if (voteDate != null)
            {
                DateTime updatedVoteDate = voteDate ?? DateTime.Now;

                var day = updatedVoteDate.Day;
                var dayOfWeek = updatedVoteDate.DayOfWeek;
                var month = updatedVoteDate.ToString("MMMM");
                var year = updatedVoteDate.Year;

                date = day + " " + month + ", " + year;
            }
            else
            {
                date = "-";
            }

            string message = "<html><body>" +
                 "<div style=\"text-align:center;max-width:600px;\">" +

                 "<h3 style=\"background-color: #64608e;margin-bottom: 0px;padding: 10px 0px;color: #fff;\">" +
                 "My Voting Summary - LAGSOBA '94  " +
                 "</h3>" +

                 "<div style=\"border:2px solid #64608e\">" +
                 "<table style=\"width: 100%;margin-bottom: 1rem;\">" +

                 "<tr>" +
                 "<th style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\"> Position</th>" +
                 "<th style=\"padding: .75rem;vertical-align: top;border-top: 1px solid #dee2e6;\"> Candidate Voted For</th>" +
                 "</tr>" + resultMail +

                 "</table>" +

                 "<div style=\"margin-bottom: 20px;\">" +
                    "<h6>The Vote Summary Above was casted by:</h6>" +
                    "<p style=\"margin: 0px;\"><strong>Name: </strong> " + userName + "</p>" +
                    "<p><strong>Last Vote On: </strong> " + date + "</p>" +
                    "</div>" +


                 "<p style=\"color: #846d6dc7;\"><em>Thanks For Voting</em></p>" +
                 "</div>" +
                 "</div>" +
                 "</body></html>";

            mailModel.Message = message;

            // send email
            MailClient.SendEmail(mailModel);

            return Json(new { Status = 1, Message = "Your vote summary has been forwarded to your email" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Dash()
        {
            return View();
        }

        #region Helpers
        public ActionResult _LoginPartial()
        {
            LoginPartialVM model = new LoginPartialVM();
            model.Name = User.GetName();

            //model.IsAdmin = await User.HasRole("Electoral Admin");

            return PartialView(model);
        }

        // generate and set custom token cookie
        private void SetCookies(string emailAddress)
        {
            // set the forms authentication
            FormsAuthentication.SetAuthCookie(emailAddress, true);

            // remove current token
            HttpContext.Response.Cookies.Remove("access");

            // generate token
            Guid obj = Guid.NewGuid();
            string accessToken = obj.ToString();

            // prepare new token
            HttpCookie access = new HttpCookie("access", accessToken);
            access.HttpOnly = true;
            access.Expires = DateTime.Now.AddMinutes(15);

            // set token
            HttpContext.Response.Cookies.Add(access);

            // remove current username
            HttpContext.Response.Cookies.Remove("userId");

            // prepare new username
            HttpCookie userName = new HttpCookie("userId", emailAddress);
            userName.HttpOnly = true;

            // set username
            HttpContext.Response.Cookies.Add(userName);

        }

        private void RemoveCookie()
        {
            FormsAuthentication.SignOut();

            var _response = HttpContext.Response;

            HttpCookie access = new HttpCookie("access")
            {
                Expires = DateTime.Now.AddDays(-1) // or any other time in the past
            };
            _response.Cookies.Set(access);

            HttpCookie userId = new HttpCookie("userId")
            {
                Expires = DateTime.Now.AddDays(-1) // or any other time in the past
            };
            _response.Cookies.Set(userId);

            //HttpContext.Response.Cookies.Remove("access");
            //HttpContext.Response.Cookies.Remove("userId");

            //HttpCookie accessCookie = HttpContext.Request.Cookies["access"];
            //HttpCookie userIdCookie = HttpContext.Request.Cookies["userId"];
            //if (accessCookie != null)
            //{
            //    accessCookie.Expires = DateTime.Now.AddSeconds(-1);
            //    HttpContext.Response.Cookies.Add(accessCookie);
            //}

            //if (userIdCookie != null)
            //{
            //    userIdCookie.Expires = DateTime.Now.AddSeconds(-1);
            //    HttpContext.Response.Cookies.Add(userIdCookie);
            //}
        }

        private string GetProofPdfHtml(string candidateId)
        {
            string result;
            // get vote and user details
            VoteProofVM model = new VoteProofVM();

            // init user & candidate
            var user = db.VoteUsers.Where(x => x.Email == User.Email).FirstOrDefault();
            var candidate = db.VoteUsers.Where(x => x.VoterId == candidateId).FirstOrDefault();

            // log user out if this is an invalid attempt.
            if (user == null || candidate == null)
            {
                // compute result
                #region Error result
                result = @"<!DOCTYPE html>";
                result += "<html lang=\"en\">";
                result += "<head>";
                result += "<meta charset = \"UTF -8\">";


                result += "<meta name = \"viewport\" content = \"width=device-width, initial-scale=1.0\">";


                result += "<title> REFER.</title>";


                result += "<link rel = \"stylesheet\" href = \"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css\" integrity = \"sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk\" crossorigin = \"anonymous\">";

                result += " <link rel = \"stylesheet\" href = \"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.12.0-2/css/all.min.css\">";


                result += " <link href = \"~/Content/style.css\" rel = \"stylesheet\" />";
                result += "</head>";
                result += " <body>";


                result += " <section>";

                result += "<div class=\"container\">";
                result += " <div class=\"row my-3 vote-result\" style=\"position: relative; \">";

                result += "<div class=\"col -12 col-sm-12 h-100 \">";
                result += "<div class=\"d -flex flex-column align-items-center justify-content-center h-100\">";

                result += "  <h1 class=\"text -danger\">Invalid Proof.</h1>";

                result += " </div>";

                result += " </div>";

                result += "</div>";
                result += "</div>";
                result += " </section>";


                result += "    <script src = \"https://code.jquery.com/jquery-3.5.1.slim.min.js\" integrity=\"sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj\" crossorigin=\"anonymous\"></script>";
                result += "<script src = \"https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js\" integrity=\"sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo\" crossorigin=\"anonymous\"></script>";
                result += "<script src = \"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js\" integrity=\"sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI\" crossorigin=\"anonymous\"></script>";
                result += "</body>";
                result += " </html>";
                #endregion


                return result;
            }

            // fill model
            model.Name = user.FirstName + " " + user.LastName;
            model.Phone = user.Phone;

            // fill candidate
            model.Candidate = candidate;
            model.Position = db.Position.Where(x => x.Id == candidate.PositionId).Select(x => x.Name).FirstOrDefault();

            // get vote date
            DateTime votedate = db.Votes.Where(x => x.CandidateId == candidateId && x.VoterId == user.VoterId).Select(x => x.DateUtc).FirstOrDefault();
            model.VoteDate = votedate.ToLocalTime();


            // compute result
            #region Proof Pdf
            result = @"<!DOCTYPE html>";
            result += "<html lang=\"en\">";
            result += "<head>";
            result += "<meta charset = \"UTF -8\">";


            result += "<meta name = \"viewport\" content = \"width=device-width, initial-scale=1.0\">";


            result += "     <title> REFER.</title>";


            result += "<link rel = \"stylesheet\" href = \"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css\" integrity = \"sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk\" crossorigin = \"anonymous\">";

            result += " <link rel = \"stylesheet\" href = \"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.12.0-2/css/all.min.css\">";


            result += " <link href = \"~/Content/style.css\" rel = \"stylesheet\" />";
            result += "</head>";
            result += " <body>";


            result += " <section>";

            result += "<div class=\"container\">";
            result += " <div class=\"row my-3 vote-result\" style=\"position: relative; \">";

            result += "<div class=\"col -12 col-sm-12 h-100 \">";
            result += "<div class=\"d -flex flex-column align-items-center justify-content-center h-100\">";
            result += "<div class=\"text -center p-2 inline\">";
            result += "<h4>Thank's for casting a vote.</h4>";
            result += " <h5 class=\"text -muted\">Below is your vote result</h5>";
            result += " </div>";
            result += " <div class=\"card rounded-lg\" style=\"width: 18rem;\">";
            result += "    <img src = \"" + model.Candidate.ImageString + "\" class=\"card-img-top\" alt=\"...\">";
            result += "  <div class=\"card -body\">";
            result += "      <div>";
            result += "     <span class=\"title -label\">Name</span>";
            result += "<h5 class=\"card -title result-t-title\">" + model.Candidate.FirstName + " " + model.Candidate.LastName + "</h5>";
            result += " </div>";
            result += "  <div>";
            result += "     <span class=\"title -label\">Position</span>";
            result += "   <h5 class=\"card -title result-t-title\">" + model.Position + "</h5>";
            result += " </div>";
            result += " <div>";
            result += "    <span class=\"title -label\">Vote casted by</span>";
            result += "    <h5 class=\"card -title result-t-title\">" + model.Name + " - " + model.Phone + "</h5>";
            result += " </div>";
            result += "  <div>";
            result += "  <span class=\"title -label\">Date</span>";
            result += "  <h5 class=\"card -title result-t-title\">" + model.VoteDate + "</h5>";
            result += " </div>";
            result += "</div>";

            result += "<p class=\"text -uppercase text-muted text-center mt-3\">&copy; Copyright 2020</p>";


            result += "</div>";
            result += " </div>";

            result += " </div>";

            result += "</div>";
            result += "</div>";
            result += " </section>";


            result += "    <script src = \"https://code.jquery.com/jquery-3.5.1.slim.min.js\" integrity=\"sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj\" crossorigin=\"anonymous\"></script>";
            result += "<script src = \"https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js\" integrity=\"sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo\" crossorigin=\"anonymous\"></script>";
            result += "<script src = \"https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js\" integrity=\"sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI\" crossorigin=\"anonymous\"></script>";
            result += "</body>";
            result += " </html>";
            #endregion


            return result;
        }
        #endregion
    }
}
