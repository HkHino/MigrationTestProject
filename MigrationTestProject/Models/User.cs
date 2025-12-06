using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
        public string Hash { get; set; }

        // Has one employee connected to it
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}
