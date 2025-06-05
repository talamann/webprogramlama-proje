using System.Diagnostics;

namespace WebProgramlama.Models
{
    public class TeacherDashboardViewModel
    {
        public Teacher Teacher { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }
    }
}