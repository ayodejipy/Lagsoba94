using Lagsoba94.Areas.Vote.Models.Data;
using Lagsoba94.Areas.Vote.Models.ViewModel;
using Lagsoba94.Models;
using Lagsoba94.Models.ViewModel;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace Lagsoba94.Helpers
{
    public class VoteUserManager
    {
        private string Token { get; set; }
        public string Email { get; private set; }
        public string VoterId { get; private set; }
        //private List<string> Roles { get; set; }

        private DbContext _db = new DbContext();

        private ResultVM Result = new ResultVM();

        private UserManager _userManager;


        public VoteUserManager()
        {
            // init token
            string token = string.Empty;
            if (HttpContext.Current.Request.Cookies.Get("access") != null)
            {
                this.Token = HttpContext.Current.Request.Cookies.Get("access").Value;
            }

            // init userId
            string userId = string.Empty;
            if (HttpContext.Current.Request.Cookies.Get("userId") != null)
            {
                this.Email = HttpContext.Current.Request.Cookies.Get("userId").Value;
            }

            // init roles 
            if (this.Token != null && this.IsAuthenticated())
            {
                // init userid
                this.VoterId = _db.VoteUsers.Where(x => x.Email == this.Email).Select(x => x.VoterId).FirstOrDefault();
            }
        }

        public VoteUserManager(UserManager userManager, SignInManager signInManager)
        {
            UserManager = userManager;
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public async Task<ResultVM> CreateUserAsync(VoterVM model)
        {
            var existingUser = _db.VoteUsers.Where(x => x.Email == model.Email).FirstOrDefault();
            if (existingUser != null)
            {
                if (await IsInRole(existingUser.UserId, "Voter"))
                {
                    Result.Success = false;
                    Result.ErrorMessage = "Voter with email: " + existingUser.Email + " already exist";
                    return Result;
                }
            }

            // generate userId
            string newVoterId = GenerateUserId();

            VoteUserDTO user = new VoteUserDTO
            {
                UserId = model.UserId,
                VoterId = newVoterId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                DateAddedUtc = DateTime.UtcNow
            };

            _db.VoteUsers.Add(user);
            _db.SaveChanges();

            // add user to role
            //this.AddToRole(user.UserId, "Voter");

            // notify user
            NotifyNewVoter(user);

            Result.Success = true;
            return Result;
        }

        internal ResultVM CreateMultipleUsers(List<VoterVM> fileData)
        {
            throw new NotImplementedException();
        }

       
        public ResultVM CreateCandidate(CandidateVM model)
        {
            Result.Success = false;


            // generate userId
            string newVoterId = GenerateUserId();

            var candidate = new VoteUserDTO
            {
                UserId = model.UserId,
                VoterId = newVoterId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                PositionId = model.PositionId,
                ImageString = model.ImageString,
                DateAddedUtc = DateTime.UtcNow
            };



            _db.VoteUsers.Add(candidate);
            _db.SaveChanges();

            // add user to role
            AddToRole(candidate.UserId, "Candidate");
            //this.AddToRole(candidate.UserId, "Candidate");

            Result.Success = true;
            return Result;
        }

        public ResultVM RemoveUser(int userId)
        {
            Result.Success = false;
            var user = _db.VoteUsers.Where(x => x.UserId == userId).FirstOrDefault();
            if (user == null)
            {
                Result.ErrorMessage = "User does not exist";
                return Result;
            }

            // remove user
            _db.VoteUsers.Remove(user);
            _db.SaveChanges();

            Result.Success = true;
            return Result;
        }

        public void AddToRole(int userId, string roleName)
        {
            UserManager.AddToRoleAsync(userId, roleName).Wait();
        }

        public void RemoveFromRole(int userId, string roleName)
        {
            UserManager.RemoveFromRoleAsync(userId, roleName).Wait();
        }

        public bool HasRole(string role)
        {
            int userId = _db.Users.Where(x => x.UserName == this.Email).Select(x => x.Id).FirstOrDefault();
            return (UserManager.IsInRoleAsync(userId, role).Result);
        }

        public async Task<bool> IsInRole(int userId, string role)
        {
            bool task = await UserManager.IsInRoleAsync(userId, role);
            return task;
        }

        public bool IsAuthenticated()
        {
            // validate user in db
            var user = _db.VoteUsers.Where(x => x.Email == this.Email).FirstOrDefault();
            if (user == null)
                return false;

            if (this.Token == null)
                return false;

            return true;
        }

        public string GetName()
        {
            return _db.VoteUsers.Where(x => x.Email == this.Email).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault() ?? string.Empty; ;
        }

        public IEnumerable<VoterVM> GetAllVoters()
        {
            // init candidate vm
            var result = new List<VoterVM>();

            var allUsers = _db.VoteUsers.ToList();

            foreach (var user in allUsers)
            {
                if (UserManager.IsInRoleAsync(user.UserId, "Voter").Result)
                {
                    var voter = new VoterVM(user);
                    result.Add(voter);
                }
            }

            //return result
            return result;
        }

        public IEnumerable<NewCandidatesVM> UsersForNewCandidateAsync()
        {

            var result = (from user in _db.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      FullName = user.FirstName + " " + user.LastName + " " + user.OtherNames,
                                      Phone = user.PhoneNumber,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in _db.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).Where(x => x.Email != "support@bytesintel.com" && !x.RoleNames.Contains("Candidate"))
                                  .Select(x => new NewCandidatesVM()
                                  {
                                      UserId = x.UserId,
                                      Name = x.FullName,
                                      Email = x.Email,
                                      Phone = x.Phone,
                                      IsVoter = x.RoleNames.Contains("Voter")

                                  });

            var allUsers = _db.Users
                .Select(x => new NewCandidatesVM()
                {
                    UserId = x.Id,
                    Name = x.FirstName + " " + x.LastName + " " + x.OtherNames,
                    Email = x.Email,
                    Phone = x.PhoneNumber,
                    //IsVoter = UserManager.IsInRoleAsync(x.Id, "Voter").Result

                }).ToList();

            return result;
        }

        #region Candidate
        public IEnumerable<CandidateVM> GetAllCandidates()
        {
            // init candidate vm
            var result = new List<CandidateVM>();

            var allUsers = _db.VoteUsers.Where(x => x.PositionId != 0 && x.PositionId != null).ToList();

            foreach (var user in allUsers)
            {
                if (UserManager.IsInRoleAsync(user.UserId, "Candidate").Result)
                {
                    var candidate = new CandidateVM(user);
                    result.Add(candidate);
                }
            }

            //return result
            return result;
        }
        public IEnumerable<CandidateVM> GetCandidatesFor(int positionId)
        {
            // init candidate vm
            var result = new List<CandidateVM>();

            var allUsers = _db.VoteUsers.Where(x => x.PositionId == positionId).ToList();
            foreach (var user in allUsers)
            {
                if (UserManager.IsInRoleAsync(user.UserId, "Candidate").Result)
                {
                    var candidate = new CandidateVM(user);
                    result.Add(candidate);
                }
            }

            //return result
            return result;
        }

        public ResultVM AddCandidate(int userId, int positionId)
        {
            Result.Success = false;

            // update userPosition
            var user = _db.VoteUsers.Where(x => x.UserId == userId).FirstOrDefault();
            if (user == null)
            {
                Result.ErrorMessage = "Invalid Candidate";
                return Result;
            }

            user.PositionId = positionId;
            _db.SaveChanges();

            UserManager.RemoveFromRoleAsync(userId, "Voter");
            UserManager.AddToRoleAsync(userId, "Candidate");

            //return 
            return Result;
        }

        public ResultVM RemoveCandidate(string userId)
        {
            Result.Success = false;

            // update userPosition
            var user = _db.VoteUsers.Where(x => x.VoterId == userId).FirstOrDefault();
            if (user == null)
            {
                Result.ErrorMessage = "Invalid Candidate";
                return Result;
            }

            user.PositionId = 0;
            user.ImageString = "";

            RemoveFromRole(user.UserId, "Candidate");

            // remove all candidate votes 
            var candidatesVote = _db.Votes.Where(x => x.CandidateId == userId).ToList();
            _db.Votes.RemoveRange(candidatesVote);

            // add user to voter role
           AddToRole(user.UserId, "Voter");

            _db.SaveChanges();

            //return 
            Result.Success = true;
            return Result;
        }
        #endregion

        // postion management
        #region Positions
        public ResultVM CreatePosition(PositionVM model)
        {
            Result.Success = false;

            //format the position name
            string toLower = CultureInfo.CurrentCulture.TextInfo.ToLower(model.Name);
            string formattedName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(toLower);

            // check if position is unique
            var position = _db.Position.Where(x => x.Name == formattedName).FirstOrDefault();
            if (position != null)
            {
                Result.ErrorMessage = "Position already exist.";
                return Result;
            }

            // mockup new position 
            var newPosition = new PositionDTO
            {
                Name = formattedName,
                RequiredAge = model.RequiredAge,
                Qualification = model.Qualification,
                IsActive = true,
            };

            // add and save position
            _db.Position.Add(newPosition);
            _db.SaveChanges();

            // return
            Result.Success = true;
            return Result;
        }

        public IEnumerable<PositionVM> GetAllPositions()
        {
            return _db.Position.ToArray().Select(x => new PositionVM(x)).ToList();
        }

        public IEnumerable<PositionVM> GetActivePositions()
        {
            return _db.Position.ToArray().Where(x => x.IsActive == true).Select(x => new PositionVM(x)).ToList();
        }

        public void DeletePosition(int positionId)
        {
            var position = _db.Position.Where(x => x.Id == positionId).FirstOrDefault();
            if (position == null)
                return;

            // get all candidate in position
            var candidates = _db.VoteUsers.Where(x => x.PositionId == positionId).ToList();

            // remove all candidates from postion
            foreach (var user in candidates)
            {
                user.PositionId = 0;
                UserManager.RemoveFromRoleAsync(user.UserId, "Candidate");
                UserManager.AddToRoleAsync(user.UserId, "Voter");
            }

            _db.Position.Remove(position);
            _db.SaveChanges();
        }

        public void DeactivatePosition(int positionId)
        {
            var position = _db.Position.Where(x => x.Id == positionId).FirstOrDefault();
            if (position == null)
                return;

            position.IsActive = false;
            _db.SaveChanges();
        }

        public void ActivatePosition(int positionId)
        {
            var position = _db.Position.Where(x => x.Id == positionId).FirstOrDefault();
            if (position == null)
                return;

            position.IsActive = true;
            _db.SaveChanges();
        }
        #endregion

        #region Helpers
        private string GenerateUserId()
        {
            string result = GetRandomString();

            // make sure id is unique
            while (_db.VoteUsers.Where(x => x.VoterId == result).Any())
            {
                result = GetRandomString();
            }

            // return
            return result;
        }

        private static string GetRandomString()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string num = "0123456789";
            string userId, letter, number;

            // generate random 3 digits
            letter = new string(Enumerable.Repeat(chars, 3)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            // generate random 4 characters
            number = new string(Enumerable.Repeat(num, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            userId = number + letter;
            return userId;
        }

        private void NotifyNewVoter(VoteUserDTO voter)
        {
            // Send an email with this link
            var mailModel = new EmailMessageVM();

            mailModel.ToAddress = voter.Email;
            mailModel.Subject = "Account Created on LAGSOBA '94";

            var callbackUrl = GetBaseUrl();

            string message = "Hello," +
                "<br>An account has been created for you on the LAGSOBA '94 voting platform." +
                "<br> Use this as your password to log in: " + voter.VoterId +
                "<br> <html><body><a href='" + callbackUrl + "' style=\"padding: 8px 12px; border: 1px solid #5ed863;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#5ed863;text-decoration: none;font-weight:bold;display: inline-block;\">Log in now</a></body></html>" +
                "<br><br><br>" +
                "<html><body><p><em>If you you got this email as an error, kindly ignore.</em></p></body></html>";

            mailModel.Message = message;

            // send email
            MailClient.SendEmail(mailModel);

        }

        public string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (appUrl != "/")
                appUrl = "/" + appUrl;

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }
        #endregion
    }
}
