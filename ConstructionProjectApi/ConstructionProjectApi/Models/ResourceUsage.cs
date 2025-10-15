namespace ConstructionProjectApi.Models
{
    public class ResourceUsage
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public ProjectTask? Task { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public int QuantityUsed { get; set; }
        public DateTime UsageDate { get; set; }
    }
}
