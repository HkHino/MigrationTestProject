

namespace MigrationTestProject.Models.Neo4j
{
    public class WorkHoursInMonthsNeo4j
    {
        public int WorkHoursInMonthId { get; set; }
        public int EmployeeId { get; set; }
        public int PayrollYear { get; set; }
        public int PayrollMonth { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalHours { get; set; }
        public bool HasSubstituted { get; set; }
    }
}
