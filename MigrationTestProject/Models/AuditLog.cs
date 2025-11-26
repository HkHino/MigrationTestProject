using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTestProject.Models
{
    public class AuditLog
    {
        public int AuditId { get; set; } // Primary key
        public string TableName { get; set; } = null!;
        public string RecordId { get; set; } = null!;
        public string Action { get; set; } = null!; // "INSERT", "UPDATE", "DELETE"
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? OldData { get; set; } // JSON as string
        public string? NewData { get; set; } // JSON as string
    }
}
