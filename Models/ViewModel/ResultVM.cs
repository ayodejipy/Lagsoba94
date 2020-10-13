using System.Collections.Generic;

namespace Lagsoba94.Models.ViewModel
{
    public class ResultVM
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}