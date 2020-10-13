using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Areas.Vote.Models.Data
{
    [Table("tblVoteRoles")]
    public class VoteRolesDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}