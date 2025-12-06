//using MigrationTestProject.Models.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int ExperienceLevel { get; set; } = 1;

        // Navigation properties
        public ICollection<Substituted> Substituteds { get; set; } = new HashSet<Substituted>();
        public ICollection<ListOfShift> Shifts { get; set; }= new HashSet<ListOfShift>();
        public ICollection<WorkHoursInMonths> WorkHoursInMonths { get; set; } = new HashSet<WorkHoursInMonths>();
        public User User { get; set; }


    }
}

