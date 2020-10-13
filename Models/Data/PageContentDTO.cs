using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Lagsoba94.Models.Data
{
    [Table("tblPageContent")]
    public class PageContentDTO
    {
        [Key]
        public int PageContentId { get; set; }
        public string ChairmanSpeech { get; set; }
        public string Story { get; set; }
        public string AimsAndObjectives { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string Achievements { get; set; }
    }
}