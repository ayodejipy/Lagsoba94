using Lagsoba94.Areas.Vote.Models.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lagsoba94.Areas.Vote.Models.ViewModel
{
    public class PositionVM
    {
        public PositionVM() { }

        public PositionVM(PositionDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            RequiredAge = row.RequiredAge;
            Qualification = row.Qualification;
            IsActive = row.IsActive;
        }

        public int Id { get; set; }
        [Display(Name = "Postion Name")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Required Age")]
        public string RequiredAge { get; set; }
        public string Qualification { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }
}