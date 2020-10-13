using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Lagsoba94.Models.ViewModel
{
    public class EditSettings
    {
        [Display(Name = "Chairman Speech")]
        [Required]
        [AllowHtml]
        public string ChairmanSpeech { get; set; }

        [Required]
        [AllowHtml]
        public string Story { get; set; }

        [Display(Name = "Aims And Objectives")]
        [Required]
        [AllowHtml]
        public string AimsAndObjectives { get; set; }

        [Required]
        [AllowHtml]
        public string Mission { get; set; }

        [Required]
        [AllowHtml]
        public string Vision { get; set; }

        [AllowHtml]
        public string Achievements { get; set; }
    }
}