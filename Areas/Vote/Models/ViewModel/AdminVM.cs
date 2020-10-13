using Lagsoba94.Areas.Vote.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lagsoba94.Areas.Vote.Models.ViewModel
{
    public class VoteResult
    {
        //public List<SingleVote> Votes { get; set; }
        public List<CandidateScore> CandidateScores { get; set; }
        public IEnumerable<PositionVM> Positions { get; set; }
        public string Name { get; set; }
    }

    public class SingleVote
    {
        public int SN { get; set; }
        public string VoterName { get; set; }
        public string VoterPhone { get; set; }
        public string VotedFor { get; set; }
    }

    public class CandidateScore
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int VoteCount { get; set; }
        public int? PositionId { get; set; }
    }

    public class VoterVM
    {
        public VoterVM() { }

        public VoterVM(VoteUserDTO row)
        {
            UserId = row.UserId;
            VoterId = row.VoterId;
            FirstName = row.FirstName;
            LastName = row.LastName;
            Email = row.Email;
            Phone = row.Phone;
        }

        public int UserId { get; set; }

        public string VoterId { get; set; }
        [Required(ErrorMessage ="Please provide a first name")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class CandidateVM
    {
        public CandidateVM() { }
        public CandidateVM(VoteUserDTO row)
        {
            UserId = row.UserId;
            VoterId = row.VoterId;
            FirstName = row.FirstName;
            LastName = row.LastName;
            Phone = row.Phone;
            Email = row.Email;
            PositionId = row.PositionId;
            ImageString = row.ImageString;
        }

        public int UserId { get; set; }

        public string VoterId { get; set; }

        [Required]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name ="Last Name")]
        public string LastName { get; set; }
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public int? PositionId { get; set; }
        public string PositionName { get; set; }

        [Required(ErrorMessage ="Please select an image for this candidate")]
        [Display(Name = "Image")]
        public string ImageString { get; set; }
    }

    public class NewCandidatesVM
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsVoter { get; set; }
        public bool MakeCandidate { get; set; }
    }

    public class ElectionVM
    {
        public ElectionVM() { }

        public ElectionVM(ElectionDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
    }
}