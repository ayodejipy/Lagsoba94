using System.ComponentModel.DataAnnotations;

namespace Lagsoba94.Areas.Vote.Models.ViewModel
{
    public class LoginVM
    {
        [Display(Name = "Voter Id")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Please provide a user id.")]
        public string VoterId { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Please input your phone number.")]
        public string Email { get; set; }
    }
}