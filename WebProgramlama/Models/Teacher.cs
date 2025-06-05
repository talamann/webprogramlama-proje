namespace WebProgramlama.Models
{
    public class Teacher
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Teacher(string id, string name)
        {
            Id = id;
            Name = name;
        }

    }
}
