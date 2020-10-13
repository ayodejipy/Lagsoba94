using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblNews")]
    public class NewsDTO
    {
        [Key]
        public int NewsId { get; set; }
        public string  Slug { get; set; }
        public string Headline { get; set; }
        public string Heading { get; set; }
        public string NewsBody { get; set; }
        public int NewsCaster { get; set; }
        public int AuthorizedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public int TypeId { get; set; }
        public bool? Authorized { get; set; }

        public virtual NewsTypeDTO NewsType { get; set; }
    }
}