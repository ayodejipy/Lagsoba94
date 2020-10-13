using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lagsoba94.Models.Data
{
    [Table("tblMembershipRequest")]
    public class MembershipRequestDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Profession { get; set; }
        public string Profile { get; set; }
        public string FirstYear { get; set; }
        public string FinalYear { get; set; }
        public string NickName { get; set; }
        public string ImageString { get; set; }
        public DateTime Date { get; set; }
        public bool? UserApproved { get; set; }
        public string DefaultPassword { get; set; }
        public bool Active { get; set; }
        public string Comment { get; set; }
    }
}