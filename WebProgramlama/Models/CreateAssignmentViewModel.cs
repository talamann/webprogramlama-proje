namespace WebProgramlama.Models
{
    public class CreateAssignmentViewModel
    {
        public string TeacherId { get; set; }
        public IEnumerable<Student> Students { get; set; }
        public int StudentCount => Students?.Count() ?? 0;
    }
}