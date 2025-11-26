using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MigrationTestProject.Models;



namespace MigrationTestProject
{
    public class MySqlContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Bicycle> Bicycles { get; set; } = null!;
        public DbSet<Substituted> Substituteds { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<ListOfShift> ListOfShifts { get; set; } = null!;
        public DbSet<ShiftPlan> ShiftPlans { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<WorkHoursInMonths> WorkHoursInMonths { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        // Constructor for DI / options
        public MySqlContext(DbContextOptions<MySqlContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                throw new InvalidOperationException("DbContext options not configured.");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ========================
            // Primary Keys
            // ========================
            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeId);
            modelBuilder.Entity<Bicycle>().HasKey(b => b.Id);
            modelBuilder.Entity<Route>().HasKey(r => r.Id);
            modelBuilder.Entity<Substituted>().HasKey(s => s.SubstitutedId);
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<WorkHoursInMonths>().HasKey(w => w.WorkHoursInMonthId);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.AuditId);
            modelBuilder.Entity<ShiftPlan>().HasKey(sp => sp.ShiftPlanId);
            modelBuilder.Entity<ListOfShift>().HasKey(s => s.ShiftId);
            // ========================
            // Relationships
            // ========================
            modelBuilder.Entity<Substituted>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Substituteds)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkHoursInMonths>()
                .HasOne(w => w.Employee)
                .WithMany(e => e.WorkHoursInMonths)
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListOfShift>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Shifts)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListOfShift>()
                .HasOne(s => s.Bicycle)
                .WithMany(b => b.Shifts)
                .HasForeignKey(s => s.BicycleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListOfShift>()
                .HasOne(s => s.Substituted)
                .WithMany(sub => sub.Shifts)
                .HasForeignKey(s => s.SubstitutedId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListOfShift>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Shifts)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
            // ========================
            // Indexes / Unique Constraints
            // ========================
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Bicycle>()
                .HasIndex(b => b.BicycleNumber)
                .IsUnique();

            modelBuilder.Entity<Route>()
                .HasIndex(r => r.RouteNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<WorkHoursInMonths>()
                .HasIndex(w => new { w.EmployeeId, w.PeriodStart, w.PeriodEnd })
                .IsUnique();

            // ========================
            // Table Mappings
            // ========================
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Bicycle>().ToTable("Bicycles");
            modelBuilder.Entity<Route>().ToTable("Routes");
            modelBuilder.Entity<Substituted>().ToTable("Substituteds");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<WorkHoursInMonths>().ToTable("WorkHoursInMonths");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLog");
            modelBuilder.Entity<ShiftPlan>().ToTable("ShiftPlans");
            modelBuilder.Entity<ListOfShift>().ToTable("ListOfShift");

        }
    }
}
