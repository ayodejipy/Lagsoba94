using Lagsoba94.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lagsoba94.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class MembershipRequestVM
    {
        public MembershipRequestVM() { }

        public MembershipRequestVM(MembershipRequestDTO row)
        {
            Id = row.Id;
            Title = row.Title;
            FirstName = row.FirstName;
            LastName = row.LastName;
            OtherNames = row.OtherNames;
            Gender = row.Gender;
            DOB = row.DOB;
            Phone = row.Phone;
            Email = row.Email;
            Address = row.Address;
            City = row.City;
            State = row.State;
            Country = row.Country;
            Zip = row.Zip;
            Profession = row.Profession;
            Profile = row.Profile;
            FirstYear = row.FirstYear;
            FinalYear = row.FinalYear;
            NickName = row.NickName;
            ImageString = row.ImageString;
            Date = row.Date;
            UserApproved = row.UserApproved;
            DefaultPassword = row.DefaultPassword;
            Active = row.Active;
            Comment = row.Comment;
        }

        public int Id { get; set; }

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
        public DateTime DOB { get; set; }

        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select a country for your phone from the dropdown")]
        [Display(Name = "Issuing Country")]
        public string CountryCodeSelected { get; set; }

        [Required(ErrorMessage ="Your phone number is required")]
        [Display(Name = "Phone")]
        public string PhoneNumberRaw { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage ="Select a country where you live.")]
        public string Country { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Can't proceed without a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Where do you live please?")]
        public string Address { get; set; }

        [Required(ErrorMessage = "The city you reside please")]
        public string City { get; set; }
        [Required(ErrorMessage ="Please select a state where you live")]
        public string State { get; set; }

        public string Zip { get; set; }

        [Required(ErrorMessage = "What do you do please?")]
        public string Profession { get; set; }

        public string ImageString { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Your class group in your first year.")]
        public string FirstYear { get; set; }
        
        [Required(ErrorMessage = "Your class group in your last year.")]
        public string FinalYear { get; set; }

        [Required(ErrorMessage = "This is needed to help identify you.")]
        public string NickName { get; set; }

        public bool? UserApproved { get; set; }

        public string DefaultPassword { get; set; }

        public bool Active { get; set; }

        public string Comment { get; set; }
    }
}
