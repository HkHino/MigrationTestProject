using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class Bicycle
    {
        public int Id { get; set; }
        public int BicycleNumber { get; set; }
        public bool InOperate { get; set; } = false;

        public ICollection<ListOfShift>? Shifts { get; set; }
    }
}
