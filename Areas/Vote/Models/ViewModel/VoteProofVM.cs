using Lagsoba94.Areas.Vote.Models.Data;
using System;

namespace Lagsoba94.Areas.Vote.Models.ViewModel
{
    public class VoteProofVM
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public VoteUserDTO Candidate { get; set; }
        public DateTime VoteDate { get; set; }
        public string Position { get; set; }
    }
}