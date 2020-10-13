using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Lagsoba94.Areas.Vote.Models.Data
{
    [Table("tblPositions")]
    public class PositionDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string RequiredAge { get; set; }
        public string Qualification { get; set; }
        public bool IsActive { get; set; }
    }
}