using System.Collections.Generic;

namespace Lagsoba94.Areas.Vote.Models.ViewModel
{
    public class VotingVM
    {
      public IEnumerable<Candidates> Candidates { get; set; }
        public bool VoteCasted { get; set; }
        public string PostionName { get; set; }
        public bool IsAdmin { get; set; }
        public string VotedCandidateId { get; set; }
    }

    public class Candidates
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int? PositionId { get; set; }
    }
}