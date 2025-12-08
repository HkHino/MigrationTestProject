

namespace MigrationTestProject.Models
{
    public class ShiftPlan
    {
        public string ShiftPlanId { get; set; } = null!; // CHAR(36)
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Shifts { get; set; } = null!; // JSON stored as string
    }
}
