using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Lagsoba94.Areas.Vote.Models.Data
{
    [Table("tblVoteUsers")]
    public class VoteUserDTO
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VoterId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? PositionId { get; set; }
        public string ImageString { get; set; }
        public DateTime DateAddedUtc { get; set; }        
        public DateTime? LastVoteDateUtc { get; set; }        
    }
}