using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Lagsoba94.Models.Data
{
    [Table("tblOffice")]
    public class OfficeDTO
    {
        [Key]
        public int OfficeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Preference { get; set; }
    }
}