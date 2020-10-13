using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblGallery")]
    public class GalleryDTO
    {
        [Key]
        public int GalleryId { get; set; }
        public string ImageUrl { get; set; }
    }
}