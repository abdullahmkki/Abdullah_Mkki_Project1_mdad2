namespace ConstructionProjectApi.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public ConstructionProject? Project { get; set; }
    }
}
