using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class ListOfShift
    {
        public int ShiftId { get; set; }
        public DateTime DateOfShift { get; set; }
        public int EmployeeId { get; set; }
        public int BicycleId { get; set; }
        public int SubstitutedId { get; set; }
        public int RouteId { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal? TotalHours { get; set; }

        // Navigation
        public Employee? Employee { get; set; }
        public Bicycle? Bicycle { get; set; }
        public Substituted? Substituted { get; set; }
        public Route? Route { get; set; }
    }
}
