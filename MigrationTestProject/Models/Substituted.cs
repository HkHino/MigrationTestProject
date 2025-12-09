

namespace MigrationTestProject.Models
{
    public class Substituted
    {
        public int SubstitutedId { get; set; }
        public int EmployeeId { get; set; }
        public bool HasSubstituted { get; set; } = false;

        // Navigation
        public Employee? Employee { get; set; }
        public ICollection<ListOfShift>? Shifts { get; set; }
    }
}
