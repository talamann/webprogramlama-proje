namespace WebProgramlama.Models
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
    }
}
