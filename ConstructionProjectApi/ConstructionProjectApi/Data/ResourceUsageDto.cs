namespace ConstructionProjectApi.DTOs
{
    public class ResourceUsageDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ResourceId { get; set; }
        public int QuantityUsed { get; set; }
        public DateTime UsageDate { get; set; }
    }
}
