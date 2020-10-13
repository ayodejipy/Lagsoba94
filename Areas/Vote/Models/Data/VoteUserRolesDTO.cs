using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Areas.Vote.Models.Data
{
    [Table("tblVoteUserRoles")]
    public class VoteUserRolesDTO
    {
        [Key]
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }
}