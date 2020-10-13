﻿using Lagsoba94.Helpers;
using Lagsoba94.Models;
using Lagsoba94.Models.Data;
using Lagsoba94.Models.ViewModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
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
    public class AccountController : Controller
    {
        // init the db context
        private readonly DbContext db = new DbContext();

        private SignInManager _signInManager;
        private UserManager _userManager;


        public AccountController()
        {
        }

        public AccountController(UserManager userManager, SignInManager signInManager)
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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return Redirect("~/profile/index");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    // Check if logged in user is still active in the membership requests table
                    var requestActive = db.MembershipRequest.Where(x => x.Email == model.Email && x.UserApproved == true).FirstOrDefault();

                    // deactivate if found
                    if (requestActive != null && requestActive.Active == true)
                    {
                        requestActive.Active = false;
                        db.SaveChanges();
                    }

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MembershipRequestVM model)
        {
            // check model state
            if (!ModelState.IsValid)
            {
                return View(model);
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
                    ModelState.AddModelError("Phone", "Invalid phone number for country");
                    return View(model);
                }

            }
            catch (NumberParseException npex)
            {
                ModelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
                return View(model);
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

            // init membershipRequest
            MembershipRequestDTO membershipRequest = new MembershipRequestDTO()
            {
                Title = model.Title,
                FirstName = model.FirstName,
                LastName = model.LastName,
                OtherNames = model.OtherNames,
                Gender = model.Gender,
                DOB = model.DOB.ToUniversalTime(),
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                Zip = model.Zip,
                Profession = model.Profession,
                Profile = model.Profile,
                FirstYear = model.FirstYear,
                FinalYear = model.FinalYear,
                NickName = model.NickName,
                ImageString = model.ImageString,
                Date = DateTime.UtcNow,
                Active = true
            };

            // save the request to database
            db.MembershipRequest.Add(membershipRequest);
            db.SaveChanges();

            // Set TempData message
            TempData["SM"] = "Your membership request has been sent successfully. A confirmation email will be sent to you.";

            // send mail notification to admin
            EmailMessageVM mailModel = new EmailMessageVM();

            // init redirect link from email
            var request = Request.Url.Authority.ToString().ToLower();
            var link = request + "/account/requests/" + membershipRequest.Id;

            // get admin email
            var adminEmails = (from user in db.Users
                               select new
                               {
                                   Email = user.Email,
                                   RoleNames = (from userRole in user.Roles
                                                join role in db.Roles on userRole.RoleId
                                                equals role.Id
                                                select role.Name).ToList()
                               }).Where(x => x.RoleNames.Contains("Admin")).ToList();

            mailModel.Subject = "New Membership Registration Request - LAGSOBA '94";
            string message = "A New member has requested to register to the LAGSOBA '94 Portal. Below are the details.";
            message += "<br><br>Name: " + model.Title + " " + model.FirstName.ToUpper() + " " + model.LastName + " " + model.OtherNames;
            message += "<br>Profession: " + model.Profession;
            message += "<br>First Year Group: " + model.FirstYear;
            message += "<br>Last Year Group: " + model.FinalYear;
            message += "<br>Date Submitted: " + model.Date;
            message += "<br><br> <html><body><a href='https://" + link + "' style=\"padding: 8px 12px; border: 1px solid #1993e8;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#1993e8;text-decoration: none;font-weight:bold;display: inline-block;\">See Full Details Here</a></body></html>";

            mailModel.Message = message;

            // send mail to all admins
            foreach (var item in adminEmails)
            {
                mailModel.ToAddress = item.Email;
                MailClient.SendEmail(mailModel);
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            IdentityResult result;
            try
            {
                result = await UserManager.ConfirmEmailAsync(userId, code);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.
                ViewBag.errorMessage = ioe.Message;
                return View("Error");
            }

            if (result.Succeeded)
            {
                return View();
            }

            // If we got this far, something failed.
            AddErrors(result);
            ViewBag.errorMessage = "ConfirmEmail failed";
            return View("Error");


            //if (userId == 0 || code == null)
            //{
            //    return View("Error");
            //}
            //var result = await UserManager.ConfirmEmailAsync(userId, code);
            //return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        [ActionName("forgot-password")]
        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ActionName("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("reset-password", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">HERE</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View("ForgotPassword", model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        [ActionName("reset-password")]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View("ResetPassword");
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ActionName("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ResetPassword", model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);

            return View("ResetPassword");
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private static string GeneratePassword()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstwxyz";
            const string num = "0123456789";
            string result;

            // generate random 2 characters
            result = new string(Enumerable.Repeat(chars, 2)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            // generate random 3 numbers
            result += new string(Enumerable.Repeat(num, 3)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            // add 2 more characters
            result += new string(Enumerable.Repeat(chars, 2)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return LGSecurity.Encrypt(result);
        }
        #endregion


        //GET:/Account/UserNavPartial
        public ActionResult UserNavPartial()
        {
            // Get username
            int userId = User.Identity.GetUserId<int>();

            ViewBag.ProfileName = db.Users.Where(x => x.Id == userId).Select(x => x.FirstName).FirstOrDefault();

            return PartialView();
        }

        // GET:/Account/Requests
        [Authorize(Roles = "Admin")]
        public ActionResult Requests(int? id)
        {
            // init members
            IEnumerable<MembershipRequestVM> memberRequests;
            using (DbContext DB = new DbContext())
            {
                memberRequests = DB.MembershipRequest.ToArray().Where(x => x.Active == true)
                    .Select(x => new MembershipRequestVM(x)).ToList();
            }

            // return all requests if id == null
            if (id != null)
            {
                memberRequests = memberRequests.Where(x => x.Id == id).ToList();
            }

            // return view with memberRequests
            return View(memberRequests);
        }


        //POST:/Account/Approve
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Approve(int id)
        {
            if (ModelState.IsValid)
            {
                // init requestDTO
                MembershipRequestDTO requestDTO =
                    db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

                // get logged in user id
                int currentUserId = User.Identity.GetUserId<int>();

                // init new user
                var user = new User
                {
                    UserName = requestDTO.Email,
                    Email = requestDTO.Email,
                    Title = requestDTO.Title,
                    FirstName = requestDTO.FirstName,
                    LastName = requestDTO.LastName,
                    OtherNames = requestDTO.OtherNames,
                    Profile = requestDTO.Profile,
                    Gender = requestDTO.Gender,
                    DOB = requestDTO.DOB,
                    Profession = requestDTO.Profession,
                    Active = true
                };

                // save the user address
                AddressesDTO userAddress = new AddressesDTO
                {
                    Address = requestDTO.Address,
                    City = requestDTO.City,
                    State = requestDTO.State,
                    Zip = requestDTO.Zip,
                    Country = requestDTO.Country
                };
                db.Address.Add(userAddress);
                db.SaveChanges();

                // set user address
                user.AddressId = userAddress.AddressId;

                // init defaultPassword
                string defaultPassword = GeneratePassword();

                // create user
                var result = await UserManager.CreateAsync(user, LGSecurity.Decrypt(defaultPassword));
                if (result.Succeeded)
                {
                    // assign the user as a member, can be promoted late
                    UserManager.AddToRole(user.Id, "Member");

                    // save phone number for the user
                    await UserManager.SetPhoneNumberAsync(user.Id, requestDTO.Phone);

                    // save user image
                    ImagesDTO userImage = new ImagesDTO
                    {
                        Image1 = requestDTO.ImageString,
                        UserId = user.Id
                    };

                    db.Images.Add(userImage);

                    // Send an email with confirmation link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    var message = "Hello " + user.FirstName.ToUpper() + ",";
                    message += "<br>Your request to join LAGSOBA '94 Portal has just been approved. This is your password <strong>" + LGSecurity.Decrypt(defaultPassword) + "</strong>";
                    message += "<br>Kindly click the button below to confirm your email address and log in with the password above.";

                    message += "<br><br> <html><body><a href='" + callbackUrl + "' style=\"padding: 8px 12px; border: 1px solid #f3ae1a;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#f3ae1a;text-decoration: none;font-weight:bold;display: inline-block;\">Confirm account and login</a></body></html>";

                    await UserManager.SendEmailAsync(user.Id, "LAGSOBA '94 - Registration Request Approved.", message);

                    // update the user in membership request
                    requestDTO.DefaultPassword = defaultPassword;
                    requestDTO.UserApproved = true;
                    db.SaveChanges();

                    TempData["SM"] = "Member has been approved successfully.";
                }
                else
                {
                    // delete the saved address
                    var deleteAddress = db.Address.Find(userAddress.AddressId);
                    if (deleteAddress != null)
                        db.Address.Remove(deleteAddress);

                    // collect errors
                    string errMsg = "";
                    foreach (var error in result.Errors)
                    {
                        if (!error.ToString().Contains("Name"))
                            errMsg += error.ToString() + ", ";
                    }

                    TempData["SM"] = "Error: " + errMsg;
                }
            }

            // If we got this far, something failed, redisplay page
            return RedirectToAction("Requests");
        }

        //POST:/Account/Decline
        [Authorize(Roles = "Admin")]
        public string Decline(int id, string reason)
        {
            // get logged in user id
            int currentUserId = User.Identity.GetUserId<int>();

            // init memberDTO
            MembershipRequestDTO memberDTO =
                    db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            // deactivate the member 
            memberDTO.UserApproved = false;
            memberDTO.Comment = reason;

            db.SaveChanges();

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = memberDTO.Email;
            mailModel.Subject = "Membership Registration Declined";

            var message = "Hello " + memberDTO.FirstName.ToUpper() + ",<br/>";
            message += "Your request to join LAGSOBA '94 Portal has been declined.<br/>";
            message += "Note: " + reason + "<br/>";
            message += "Please contact LAGSOBA '94 Portal Admin for further enquiries or assistance.<br/>";

            mailModel.Message = message;

            //bool sent = Email.SendMail(mailModel);
            bool sent = MailClient.SendEmail(mailModel);
            if (sent)
            {
                return "Member request declined successfully.";
            }
            else
            {
                return "Member request declined successfully. Unable to send email notification, try re-sending.";
            }
        }

        //GET:/Account/confirm-again
        [Authorize(Roles = "Admin")]
        [ActionName("confirm-again")]
        public async Task<string> ConfirmAgainAsync(int id)
        {
            // init memberRequest
            var memberRequest = db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            // init user
            var user = db.Users.Where(x => x.Email == memberRequest.Email).FirstOrDefault();

            if (user == null)
                return "This user is not registered yet.";

            // get default password
            string defaultPassword = LGSecurity.Decrypt(memberRequest.DefaultPassword);

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = user.Email;
            mailModel.Subject = "LAGSOBA '94 - Registration Request Approved.";

            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

            var message = "Hello " + user.FirstName.ToUpper() + ",";
            message += "<br>Your request to join LAGSOBA '94 Portal has just been approved. This is your password <strong>" + defaultPassword + "</strong>";
            message += "<br>Kindly click the button below to confirm your email address and log in with the password above.";
            message += "<br><br> <html><body><a href='" + callbackUrl + "' style=\"padding: 8px 12px; border: 1px solid #17454E;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#17454E;text-decoration: none;font-weight:bold;display: inline-block;\">Login To Confirm Email</a></body></html>";

            mailModel.Message = message;

            //bool sent = Email.SendMail(mailModel);
            bool sent = MailClient.SendEmail(mailModel);
            if (sent)
            {
                return "Email re-sent successfully.";
            }
            else
            {
                return "Unable to re-send email, please try again later.";
            }
        }


        //GET:/Account/confirm-again
        [Authorize(Roles = "Admin")]
        [ActionName("decline-again")]
        public string DeclineAgain(int id)
        {
            // init requestDTO
            MembershipRequestDTO memberDTO =
                db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = memberDTO.Email;
            mailModel.Subject = "LAGSOBA '94 - Membership Registration Declined";

            var message = "Hello " + memberDTO.FirstName.ToUpper() + ",<br/>";
            message += "Your request to join LAGSOBA '94 Portal has been declined.<br/>";
            message += "Note: " + memberDTO.Comment + "<br/>";
            message += "Please contact LAGSOBA '94 Portal Admin for further enquiries or assistance.<br/>";

            mailModel.Message = message;

            //bool sent = Email.SendMail(mailModel);
            bool sent = MailClient.SendEmail(mailModel);
            if (sent)
            {
                return "Email re-sent successfully.";
            }
            else
            {
                return "Unable to re-send email, please try again later.";
            }
        }

        //GET:/Account/country-states
        [ActionName("country-states")]
        [AllowAnonymous]
        public ActionResult CountryStates(string countryName)
        {
            // get the file
            var jsonStr = System.IO.File.ReadAllText(Path.Combine(Server.MapPath("~/Content/assets"), "CountriesWithStates.json"));

            // parse it as json
            var parsed = JObject.Parse(jsonStr);

            // get states   
            var stateList = parsed["data"]
                .Where(x => x.Value<String>("name").Equals(countryName, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value<JArray>("states"))
                .FirstOrDefault();

            return Json(new { status = 0, data = stateList.ToString(Newtonsoft.Json.Formatting.None) }, JsonRequestBehavior.AllowGet);
        }
    }
}