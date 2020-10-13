using Lagsoba94.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lagsoba94.Models.ViewModel
{
    public class OfficeVM
    {
        public OfficeVM() { }

        public OfficeVM(OfficeDTO row)
        {
            OfficeId = row.OfficeId;
            Name = row.Name;
            Description = row.Description;
            Preference = row.Preference;
        }

        public int OfficeId { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public int Preference { get; set; }
    }
}