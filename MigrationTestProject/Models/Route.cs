

namespace MigrationTestProject.Models
{
    public class Route
    {
        public int Id { get; set; }
        public int RouteNumber { get; set; }

        public ICollection<ListOfShift>? Shifts { get; set; }
    }
}
