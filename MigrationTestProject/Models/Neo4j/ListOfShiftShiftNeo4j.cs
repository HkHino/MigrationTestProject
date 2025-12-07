

namespace MigrationTestProject.Models.Neo4j
{
    class ListOfShiftShiftNeo4j
    {
        public int ShiftId { get; set; }
        public DateTime DateOfShift { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal? TotalHours { get; set; }

    }
}
