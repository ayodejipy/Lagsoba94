using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblStates")]
    public class StatesDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}