using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblComments")]
    public class CommentDTO
    {
        [Key]
        public int CommentId { get; set; }
        public int NewsId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}