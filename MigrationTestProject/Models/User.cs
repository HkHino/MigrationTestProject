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
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!; // hashed
        public int EmployeeId { get; set; }

        // Navigation
        public Employee? Employee { get; set; }
    }
}
