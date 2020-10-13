using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblNewsType")]
    public class NewsTypeDTO
    {
        [Key]
        public int TypeId { get; set; }
        public string Description { get; set; }
    }
}