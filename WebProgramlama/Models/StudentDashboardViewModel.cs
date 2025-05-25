using System.Diagnostics;

namespace WebProgramlama.Models
{
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
    }
}
