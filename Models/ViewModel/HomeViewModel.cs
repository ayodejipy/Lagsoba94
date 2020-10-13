using Lagsoba94.Models.Data;
using System.Collections.Generic;

namespace Lagsoba94.Models.ViewModels
{
    public class MembersVM
    {
        public MembersVM() {
            Preference = 999;
        }

        public string Title { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public string Profession { get; set; }
        public string Office { get; set; }
        public int UserId { get; set; }

        public bool IsExecutive { get; set; }
        public int Preference { get; set; }

        public string Image { get; set; }

    }

    public class HomeVM
    {
        public IEnumerable<NewsVM> News { get; set; }

        public List<MembersVM> Members { get; set; }
    }

    public class AboutVM
    {
        public string Story_First { get; set; }
        public string Story_Second { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string Achievements { get; set; }
        public string AimsAndObjectives { get; set; }
    }
}