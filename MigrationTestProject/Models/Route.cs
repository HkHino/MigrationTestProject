using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class Route
    {
        public int Id { get; set; }
        public int RouteNumber { get; set; }

        public ICollection<ListOfShift>? Shifts { get; set; }
    }
}
