using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lagsoba94.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Lagsoba94.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public bool isAdmin { get; set; }
        public string UserImage { get; set; }
        public bool IsExecutive { get; set; }

        public IList<string> UserRoles { get; set; }

        public virtual User Profile { get; set; }
        public virtual AddressesDTO UserAddresss { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class AllNewsVM
    {
        public AllNewsVM() { }

        public AllNewsVM(NewsDTO row)
        {
            NewsId = row.NewsId;
            Slug = row.Slug;
            Headline = row.Headline;
            NewsBody = row.NewsBody;
            DatePosted = row.UploadDate.ToLocalTime();
            Authorized = row.Authorized;
            NewsCasterId = row.NewsCaster;
        }

        public int NewsId { get; set; }
        public string Slug { get; set; }
        public string Headline { get; set; }
        public string NewsBody { get; set; }
        public string PostedBy { get; set; }
        public DateTime DatePosted { get; set; }
        public bool? Authorized { get; set; }
        public string ImageSrc { get; set; }
        public int NewsCasterId { get; set; }
    }

    public class UsersList
    {
        public int Count { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string RoleNames { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class NamesAndMemberId
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string MemberId { get; set; }
        public int ChapelId { get; set; }
        public bool CurrentAdmin { get; set; }
    }

    public class EditProfile
    {
        private DbContext db = new DbContext();
        public EditProfile() { }

        public EditProfile(User row)
        {
            UserId = row.Id;
            Title = row.Title;
            FirstName = row.FirstName;
            LastName = row.LastName;
            OtherNames = row.OtherNames;
            Profile = row.Profile;
            Gender = row.Gender;
            DOB = row.DOB;
            Phone = row.PhoneNumber;
            PhoneNumberRaw = row.PhoneNumber;
            Email = row.Email;
            Profession = row.Profession;
            ImageString = db.Images.Where(x => x.UserId == row.Id).Select(x => x.Image1).FirstOrDefault();

            IEnumerable<SelectListModel> genders = new List<SelectListModel>()
            {
                new SelectListModel { Text = "Male", Value = "Male" },
                new SelectListModel { Text = "Female", Value = "Female" }
            };
            GenderList = new System.Web.Mvc.SelectList(genders, "Value", "Text", genders.Where(x => x.Value == row.Gender));

            

            SetAddress(row.AddressId);
        }

        public int UserId { get; set; }
        public string Title { get; set; }

        [Required(ErrorMessage = "Please provide your first name")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Your last name please")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Names")]
        public string OtherNames { get; set; }

        [MaxLength(2000, ErrorMessage = "Maximum of 2000 characters accepted.")]
        public string Profile { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Provide your date of birth")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }

        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select a country for your phone from the dropdown")]
        [Display(Name = "Issuing Country")]
        public string CountryCodeSelected { get; set; }

        [Required(ErrorMessage = "Your phone number is required")]
        [Display(Name = "Phone")]
        public string PhoneNumberRaw { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Select a country where you live.")]
        public string Country { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Where do you live please?")]
        public string Address { get; set; }

        [Required(ErrorMessage = "The city you reside please")]
        public string City { get; set; }
        [Required(ErrorMessage = "Please select a state where you live")]
        public string State { get; set; }

        public string Zip { get; set; }

        [Required(ErrorMessage = "What do you do please?")]
        public string Profession { get; set; }

        public string ImageString { get; set; }


        // Lists for dropdowns
        public System.Web.Mvc.SelectList GenderList { get; set; }
        public System.Web.Mvc.SelectList TitleList { get; set; }
        public System.Web.Mvc.SelectList CountryCodeList { get; set; }
        public System.Web.Mvc.SelectList CountryList { get; set; }
        public System.Web.Mvc.SelectList StateList { get; set; }

        public void SetAddress(int id)
        {
            var address = db.Address.Where(x => x.AddressId == id).FirstOrDefault();
            if (address != null)
            {
                Address = address.Address;
                City = address.City;
                State = address.State;
                Country = address.Country;
                Zip = address.Zip;
            }
        }
    }

    public class SelectListModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}