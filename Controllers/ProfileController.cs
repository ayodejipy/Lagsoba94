using Lagsoba94.Areas.Vote.Models.ViewModel;
using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.Data;
using Lagsoba94.Models.ViewModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lagsoba94.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private SignInManager _signInManager;
        private UserManager _userManager;

        private readonly DbContext Db = new DbContext();

        public ProfileController()
        {
        }

        public ProfileController(UserManager userManager, SignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public SignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<SignInManager>();
            private set => _signInManager = value;
        }

        public UserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            private set => _userManager = value;
        }

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your profile has been updated."
                : "";

            var userId = User.Identity.GetUserId<int>();
            string userImage;
            User user;
            AddressesDTO userAddress;
            using (DbContext Db = new DbContext())
            {
                user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();
                userAddress = Db.Address.Where(x => x.AddressId == user.AddressId).FirstOrDefault();
                userImage = Db.Images.Where(x => x.UserId == user.Id).Select(x => x.Image1).FirstOrDefault();
            }

            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId.ToString()),
                Profile = user,
                UserAddresss = userAddress,
                UserImage = userImage
            };

            ViewBag.Title = model.Profile.FirstName + " " + model.Profile.LastName + " " + model.Profile.OtherNames;
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId<int>(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId<int>(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId<int>(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId<int>(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId<int>(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId<int>(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId<int>(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId<int>());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        // GET:Profile/edit-profile
        //
        [ActionName("edit-profile")]
        public ActionResult EditProfile(int userId)
        {
            var model = Db.Users.ToArray().Where(x => x.Id == userId).Select(x => new EditProfile(x)).FirstOrDefault();

            string regionCode = string.Empty;

            // init the phone number country
            ViewBag.PhoneCountryCode = model.Phone.Substring(0, model.Phone.IndexOf(' ')).Trim();

            // init country and state
            ViewBag.Country = model.Country;
            ViewBag.State = model.State;

            // init title list
            IEnumerable<SelectListModel> titles = new List<SelectListModel>()
            {
               new SelectListModel { Text="Mr", Value="Mr"},
               new SelectListModel { Text = "Doctor", Value = "Doctor" },
               new SelectListModel { Text = "Engineer", Value = "Engineer" },
               new SelectListModel { Text = "Architect", Value = "Architect" }
            };
            model.TitleList = new SelectList(titles, "Value", "Text", titles.Where(x => x.Value == model.Title));


            ViewBag.Title = model.FirstName + " " + model.LastName + " " + model.OtherNames;

            return View("EditProfile", model);
        }

        // GET:Profile/edit-profile
        //
        [HttpPost]
        [ActionName("edit-profile")]
        public ActionResult EditProfile(EditProfile model)
        {
            // check model state
            if (!ModelState.IsValid)
            {
                string regionCode = string.Empty;

                // init the phone number country
                try
                {
                    ViewBag.PhoneCountryCode = model.PhoneNumberRaw.Substring(0, model.Phone.IndexOf(' ')).Trim();
                }
                catch (Exception) { }

                // init title list
                IEnumerable<SelectListModel> titles = new List<SelectListModel>()
            {
               new SelectListModel { Text="Mr", Value="Mr"},
               new SelectListModel { Text = "Doctor", Value = "Doctor" },
               new SelectListModel { Text = "Engineer", Value = "Engineer" },
               new SelectListModel { Text = "Architect", Value = "Architect" }
            };
                model.TitleList = new SelectList(titles, "Value", "Text", titles.Where(x => x.Value == model.Title));

                IEnumerable<SelectListModel> genders = new List<SelectListModel>()
            {
                new SelectListModel { Text = "Male", Value = "Male" },
                new SelectListModel { Text = "Female", Value = "Female" }
            };
                model.GenderList = new System.Web.Mvc.SelectList(genders, "Value", "Text", genders.Where(x => x.Value == model.Gender));


                ViewBag.Title = model.FirstName + " " + model.LastName + " " + model.OtherNames;

                return View("EditProfile", model);
            }

            // get phone number
            try
            {
                PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

                // Parse the number to check into a PhoneNumber object.
                PhoneNumber phoneNumber = _phoneUtil.Parse(model.PhoneNumberRaw, model.CountryCodeSelected);

                if (_phoneUtil.IsValidNumberForRegion(phoneNumber, model.CountryCodeSelected))
                {
                    model.Phone = _phoneUtil.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
                }
                else
                {
                    string regionCode = string.Empty;

                    // init the phone number country
                    ViewBag.PhoneCountryCode = model.Phone.Substring(0, model.Phone.IndexOf(' ')).Trim();

                    // init title list
                    IEnumerable<SelectListModel> titles = new List<SelectListModel>()
            {
               new SelectListModel { Text="Mr", Value="Mr"},
               new SelectListModel { Text = "Doctor", Value = "Doctor" },
               new SelectListModel { Text = "Engineer", Value = "Engineer" },
               new SelectListModel { Text = "Architect", Value = "Architect" }
            };
                    model.TitleList = new SelectList(titles, "Value", "Text", titles.Where(x => x.Value == model.Title));

                    IEnumerable<SelectListModel> genders = new List<SelectListModel>()
            {
                new SelectListModel { Text = "Male", Value = "Male" },
                new SelectListModel { Text = "Female", Value = "Female" }
            };
                    model.GenderList = new System.Web.Mvc.SelectList(genders, "Value", "Text", genders.Where(x => x.Value == model.Gender));


                    ViewBag.Title = model.FirstName + " " + model.LastName + " " + model.OtherNames;

                    ModelState.AddModelError("Phone", "Invalid phone number for country");
                    return View("EditProfile", model);
                }

            }
            catch (NumberParseException npex)
            {
                string regionCode = string.Empty;

                // init the phone number country
                ViewBag.PhoneCountryCode = model.Phone.Substring(0, model.Phone.IndexOf(' ')).Trim();

                // init title list
                IEnumerable<SelectListModel> titles = new List<SelectListModel>()
            {
               new SelectListModel { Text="Mr", Value="Mr"},
               new SelectListModel { Text = "Doctor", Value = "Doctor" },
               new SelectListModel { Text = "Engineer", Value = "Engineer" },
               new SelectListModel { Text = "Architect", Value = "Architect" }
            };
                model.TitleList = new SelectList(titles, "Value", "Text", titles.Where(x => x.Value == model.Title));

                IEnumerable<SelectListModel> genders = new List<SelectListModel>()
            {
                new SelectListModel { Text = "Male", Value = "Male" },
                new SelectListModel { Text = "Female", Value = "Female" }
            };
                model.GenderList = new System.Web.Mvc.SelectList(genders, "Value", "Text", genders.Where(x => x.Value == model.Gender));

                ViewBag.Title = model.FirstName + " " + model.LastName + " " + model.OtherNames;

                ModelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
                return View("EditProfile", model);
            }

            // set image url if user did not provide image
            if (string.IsNullOrEmpty(model.ImageString))
            {
                if (model.Gender.ToLower() == "male")
                {
                    model.ImageString = System.IO.File.ReadAllText(Path.Combine(Server.MapPath("~/Content/assets"), "DefaultMaleImage.txt"));
                }
                else
                {
                    model.ImageString = System.IO.File.ReadAllText(Path.Combine(Server.MapPath("~/Content/assets"), "DefaultFemaleImage.txt"));
                }
            }

            // get existing user
            var user = Db.Users.Where(x => x.Id == model.UserId).FirstOrDefault();

            // update info
            user.Title = model.Title;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.OtherNames = model.OtherNames;
            user.Gender = model.Gender;
            user.Profession = model.Profession;
            user.Profile = model.Profile;
            user.DOB = model.DOB;
            user.PhoneNumber = model.Phone;

            // get existing user address
            var address = Db.Address.Where(x => x.AddressId == user.AddressId).FirstOrDefault();

            // update user address
            address.Address = model.Address;
            address.City = model.City;
            address.State = model.State;
            address.Country = model.Country;
            address.Zip = model.Zip;

            // get existing user image
            var image = Db.Images.Where(x => x.UserId == user.Id).FirstOrDefault();

            // update user image
            image.Image1 = model.ImageString;

            // save the request to database
            Db.SaveChanges();

            // return to profile page on success
            return RedirectToAction("Index", new { Message = ManageMessageId.UpdateProfile });
        }

        // GET:Profile/add-news
        //
        [Authorize(Roles = "Admin")]
        [ActionName("news-management")]
        public ActionResult NewsManagement()
        {
            return View("NewsManagement");
        }

        // GET:Profile/all-users
        [Authorize(Roles = "Admin")]
        [ActionName("all-users")]
        public ActionResult AllUsers()
        {
            // get logged in user id
            int userId = User.Identity.GetUserId<int>();
            List<UsersList> model = new List<UsersList>();

            // get all users
            var usersWithRoles = (from user in Db.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      FullName = user.FirstName + " " + user.LastName + " " + user.OtherNames,
                                      Phone = user.PhoneNumber,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in Db.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).Where(x => x.Email != "support@bytesintel.com")
                                  .ToList().Select(p => new UsersList()
                                  {
                                      UserId = p.UserId,
                                      FullName = p.FullName,
                                      Phone = p.Phone,
                                      Email = p.Email,
                                      RoleNames = string.Join(", ", p.RoleNames)
                                  }).OrderBy(x => x.FullName);
            // init count
            int count = 0;
            foreach (var user in usersWithRoles)
            {
                count++;

                var item = user;
                item.Count = count;
                model.Add(item);
            }

            return View("AllUsers", model);
        }

        // GET:Profile/user-profile
        //
        [ActionName("user-profile")]
        public async Task<ActionResult> UserProfile(string memberName, int userId)
        {
            string userImage;
            User user;
            AddressesDTO userAddress = new AddressesDTO();
            user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();
            userAddress = Db.Address.Where(x => x.AddressId == user.AddressId).FirstOrDefault();
            userImage = Db.Images.Where(x => x.UserId == user.Id).Select(x => x.Image1).FirstOrDefault();


            var model = new IndexViewModel
            {
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                isAdmin = await UserManager.IsInRoleAsync(userId, "Admin") || await UserManager.IsInRoleAsync(userId, "ChapelAdmin"),
                Profile = user,
                UserAddresss = userAddress,
                UserImage = userImage,
                UserRoles = await UserManager.GetRolesAsync(userId),
                IsExecutive = await UserManager.IsInRoleAsync(userId, "Executive")
            };

            ViewBag.Title = model.Profile.FirstName + " " + model.Profile.LastName + " " + model.Profile.OtherNames;
            return View("UserProfile", model);
        }


        //GET:Profile/make-executive
        //
        [Authorize(Roles = "Admin")]
        [ActionName("make-executive")]
        public string MakeExecutive(int userId, int officeId)
        {
            try
            {
                UserManager.AddToRole(userId, "Executive");

                // get user
                var user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();

                // get office
                var office = Db.Office.Find(officeId);

                if (user == null || office == null)
                {
                    return "Error: unable to make user an executive";
                }

                user.OfficeId = officeId;
                Db.SaveChanges();

                return "Success: user is now an executive";
            }
            catch
            {
                return "Error: unable to make user an executive";
            }
        }

        //GET:Profile/remove-executive
        //
        [Authorize(Roles = "Admin")]
        [ActionName("remove-executive")]
        public string RemoveExecutive(int userId)
        {
            try
            {
                UserManager.RemoveFromRole(userId, "Executive");

                // get user
                var user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();
                user.OfficeId = 1;
                return "Success: user has been removed as an executive";
            }
            catch
            {
                return "Error: unable to remove user from an executive";
            }
        }

        //GET:Profile/MakeElectAdmin
        //
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult MakeElectAdmin(int userId)
        {
            try
            {
                // get role id
                var roleId = Db.Roles.Where(x => x.Name == "Electoral Admin").Select(x => x.Id).FirstOrDefault();

                // check if admin count is up
                var roleUsers = Db.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(roleId)).ToList();

                if (roleUsers.Count() >= 3)
                {
                    return Json(new { Status = 0, Message = "Error: There cannot be more than 2 Electoral Admins" });
                }

                UserManager.AddToRole(userId, "Electoral Admin");

                return Json(new { Status = 1, Message = "Success: User is now an Electoral Admin" });
            }
            catch (Exception)
            {
                return Json(new { Status = 0, Message = "Error: Unable to make user an Electoral Admin" });
            }
        }

        //GET:Profile/RemoveElectAdmin
        //
        [Authorize(Roles = "Admin")]
        public string RemoveElectAdmin(int userId)
        {
            try
            {
                UserManager.RemoveFromRole(userId, "Electoral Admin");

                // get user
                var user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();
                user.OfficeId = 1;
                return "Success: user has been removed as an executive";
            }
            catch
            {
                return "Error: unable to remove user from an executive";
            }
        }


        //GET:Profile/MakeElectSuper
        //
        [Authorize(Roles = "Admin, Electoral Admin")]
        [HttpPost]
        public ActionResult MakeElectSuper(int userId)
        {
            try
            {
                // get role id
                var roleId = Db.Roles.Where(x => x.Name == "Electoral Supervisor").Select(x => x.Id).FirstOrDefault();

                // check if admin count is up
                var roleUsers = Db.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(roleId)).ToList();

                if (roleUsers.Count() >= 5)
                {
                    return Json(new { Status = 0, Message = "Error: There cannot be more than 5 Electoral Supervisors" });
                }

                UserManager.AddToRole(userId, "Electoral Supervisor");

                return Json(new { Status = 1, Message = "Success: User is now an Electoral Supervisor" });
            }
            catch
            {
                return Json(new { Status = 0, Message = "Error: Unable to make user an Electoral Supervisor" });
            }
        }

        //GET:Profile/RemoveElectSuper
        //
        [Authorize(Roles = "Admin, Electoral Admin")]
        public string RemoveElectSuper(int userId)
        {
            try
            {
                UserManager.RemoveFromRole(userId, "Electoral Supervisor");

                // get user
                var user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();
                user.OfficeId = 1;
                return "Success: user has been removed as an Electoral Supervisor";
            }
            catch
            {
                return "Error: unable to remove user from an Electoral Supervisor";
            }
        }

        //GET:Profile/MakeVoter
        //
        [Authorize(Roles = "Admin, Electoral Admin, Electoral Supervisor")]
        public string MakeVoter(int userId)
        {
            try
            {
                UserManager.AddToRole(userId, "Voter");

                // get user
                var user = Db.Users.Where(x => x.Id == userId).FirstOrDefault();

                // add user to voter user table
                var newVoter = new VoterVM()
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName + " " + user.OtherNames,
                    Email = user.Email,
                    Phone = user.PhoneNumber
                };

                var manager = new VoteUserManager();
                _ = manager.CreateUserAsync(newVoter);

                return "Success: user is now a Voter";
            }
            catch
            {
                return "Error: unable to make user a Voter";
            }
        }

        //GET:Profile/RemoveVoter
        //
        [Authorize(Roles = "Admin, Electoral Admin, Electoral Supervisor")]
        public string RemoveVoter(int userId)
        {
            try
            {
                UserManager.RemoveFromRole(userId, "Voter");

                // get user
                var user = Db.VoteUsers.Where(x => x.UserId == userId).FirstOrDefault();

                // remove 
                Db.VoteUsers.Remove(user);
                Db.SaveChanges();

                return "Success: user has been removed as a Voter";
            }
            catch
            {
                return "Error: unable to remove user from a Voter";
            }
        }

        //GET:Profile/make-executive
        //
        [ActionName("make-admin")]
        public string MakeAdmin(int userId)
        {
            try
            {
                UserManager.AddToRole(userId, "Admin");
                return "Success: user is now an admin";
            }
            catch
            {
                return "Error: unable to make user an admin";
            }
        }

        // GET:Profile/all-users
        [Authorize(Roles = "Admin, Electoral Admin, Electoral Supervisor")]
        [ActionName("all-voters")]
        public ActionResult AllVoters()
        {
            // get logged in user id
            int userId = User.Identity.GetUserId<int>();
            List<UsersList> model = new List<UsersList>();

            // get all users
            var usersWithRoles = (from user in Db.Users
                                  select new
                                  {
                                      UserId = user.Id,
                                      FullName = user.FirstName + " " + user.LastName + " " + user.OtherNames,
                                      Phone = user.PhoneNumber,
                                      Email = user.Email,
                                      RoleNames = (from userRole in user.Roles
                                                   join role in Db.Roles on userRole.RoleId
                                                   equals role.Id
                                                   select role.Name).ToList()
                                  }).Where(x => x.Email != "support@bytesintel.com" && !x.RoleNames.Contains("Candidate"))
                                  .ToList().Select(p => new UsersList()
                                  {
                                      UserId = p.UserId,
                                      FullName = p.FullName,
                                      Phone = p.Phone,
                                      Email = p.Email,
                                      RoleNames = string.Join(", ", p.RoleNames)
                                  }).OrderBy(x => x.FullName);

            // init count
            int count = 0;
            foreach (var user in usersWithRoles)
            {
                count++;
                var item = user;
                item.Count = count;
                model.Add(item);
            }

            return View("AllVoters", model);
        }

        public ActionResult _ElectionDatePartial()
        {
            // get the current expiry date into viewbag
            var election = Db.Election.ToArray().Select(x => new ElectionVM(x)).FirstOrDefault();

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
            var election = Db.Election.FirstOrDefault();

            // update end date
            election.StartDate = model.StartDate;
            election.EndDate = model.EndDate;

            // save db
            Db.SaveChanges();

            return Json(new { Status = 1, Message = "Success" });
        }

        [ActionName("executive-office")]
        public ActionResult ExecutiveOffice()
        {
            var model = Db.Office.ToArray()
                .Where(x => x.OfficeId != 1)
                .Select(x => new OfficeVM(x))
                .ToList().OrderBy(x => x.Preference);

            return View("ExecutiveOffice", model);
        }

        [HttpPost]
        [ActionName("executive-office")]
        public ActionResult ExecutiveOffice(int[] officeIds)
        {
            int preference = 1;
            foreach (int id in officeIds)
            {
                var office = Db.Office.Find(id);
                office.Preference = preference;
                Db.SaveChanges();
                preference += 1;
            }

            var model = Db.Office.ToArray().Select(x => new OfficeVM(x)).ToList().OrderBy(x => x.Preference);

            return RedirectToAction("executive-office", model);
        }

        public ActionResult _NewOfficePartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult _NewOfficePartial(OfficeVM model)
        {
            if (!ModelState.IsValid)
            {
                string errors = "";
                foreach (var values in ModelState.Values)
                {
                    foreach (var err in values.Errors)
                    {
                        errors += err + "\n";
                    }
                }

                return Json(new { Status = 0, Message = errors });
            }

            // get last preference
            int lastPref = Db.Office.Select(x => x.Preference).ToList().Last();

            // init newOffice
            var newOffice = new OfficeDTO
            {
                Name = model.Name,
                Description = model.Description,
                Preference = lastPref + 1
            };

            // add and save to db
            Db.Office.Add(newOffice);
            Db.SaveChanges();

            return Json(new { Status = 1, Message = "Success" });
        }


        public ActionResult _EditOfficePartial(int id)
        {
            var model = Db.Office.ToArray().Where(x => x.OfficeId == id).Select(x=> new OfficeVM(x)).FirstOrDefault();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _EditOfficePartial(OfficeVM model)
        {
            if (!ModelState.IsValid)
            {
                string errors = "";
                foreach (var values in ModelState.Values)
                {
                    foreach (var err in values.Errors)
                    {
                        errors += err + "\n";
                    }
                }

                return Json(new { Status = 0, Message = errors });
            }

            // get office
            var office = Db.Office.Find(model.OfficeId);
            office.Name = model.Name;
            office.Description = model.Description;

            // save to db
            Db.SaveChanges();

            return Json(new { Status = 1, Message = "Success" });
        }

        [ActionName("delete-office")]
        public ActionResult DeleteOffice(int id)
        {
            // find and remove all users with office record
            var users = Db.Users.Where(x => x.OfficeId == id).ToList();
            foreach(var user in users)
            {
                user.OfficeId = 1;
                UserManager.RemoveFromRole(user.Id, "Executive");

            }
            Db.SaveChanges();

            // find and remove office record
            var office = Db.Office.Find(id);
            Db.Office.Remove(office);
            Db.SaveChanges();

            return RedirectToAction("executive-office");
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            UpdateProfile,
            Error
        }
        #endregion
    }
}