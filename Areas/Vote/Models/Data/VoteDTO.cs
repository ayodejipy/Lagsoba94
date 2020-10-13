using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Lagsoba94.Areas.Vote.Models.Data
{
    [Table("tblVotes")]
    public class VoteDTO
    {
        [Key]
        public int Id { get; set; }
        public string VoterId { get; set; }
        public int? PositionId { get; set; }
        public DateTime DateUtc { get; set; }
        public string CandidateId { get; set; }
    }
}