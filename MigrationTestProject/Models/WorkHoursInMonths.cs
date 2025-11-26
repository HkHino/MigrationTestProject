using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class WorkHoursInMonths
    {
        public int WorkHoursInMonthId { get; set; }
        public int EmployeeId { get; set; }
        public int PayrollYear { get; set; }
        public int PayrollMonth { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalHours { get; set; } = 0;
        public bool HasSubstituted { get; set; } = false;

        // Navigation
        public Employee? Employee { get; set; }
    }
}
