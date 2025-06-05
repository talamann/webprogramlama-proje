namespace WebProgramlama.Models
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string TeacherId { get; set; }
        public string StudentId { get; set; }
        public bool kopyaTestResult { get; set; }
    }
}
