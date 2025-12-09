using Microsoft.EntityFrameworkCore;

using MigrationTestProject.Repositories.Neo4j;
using MigrationTestProject.Repository;
namespace MigrationTestProject.Services
{
    public class MigrationServiceNeo4j
    {
        private readonly MySqlContext _sql;
        private readonly IAuditLogRepository _auditLogsRepo;
        private readonly IEmployeeRepository _employeesRepo;
        private readonly IBicycleRepository _bicyclesRepo;
        private readonly IRouteRepository _routesRepo;
        private readonly IListOfShiftRepository _listOfShiftsRepo;
        private readonly ISubstitutedsRepository _substitutedsRepo;
        private readonly IUsersRepository _usersRepo;
        private readonly IWorkHoursInMonthsRepository _workHoursInMonthsRepo;
        private readonly IShiftPlanRepository _shiftPlansRepo;
        public MigrationServiceNeo4j
            (
                MySqlContext sql,
                IAuditLogRepository auditLogsRepo,
                IEmployeeRepository employeesRepo,
                IBicycleRepository bicyclesRepo,
                IRouteRepository routesRepo,
                IListOfShiftRepository listOfShiftsRepo,
                ISubstitutedsRepository substitutedsRepo,
                IUsersRepository usersRepo,
                IWorkHoursInMonthsRepository workHoursInMonthsRepo,
                IShiftPlanRepository shiftPlansRepo)
        {
            _sql = sql;
            _auditLogsRepo = auditLogsRepo;
            _employeesRepo = employeesRepo;
            _bicyclesRepo = bicyclesRepo;
            _routesRepo = routesRepo;
            _listOfShiftsRepo = listOfShiftsRepo;
            _substitutedsRepo = substitutedsRepo;
            _usersRepo = usersRepo;
            _workHoursInMonthsRepo = workHoursInMonthsRepo;
            _shiftPlansRepo = shiftPlansRepo;
        }
        public async Task MigrateAllAsync()
        {
            // ========================
            // 1. Employees
            // ========================
            foreach (var e in await _sql.Employees.ToListAsync())
            {
                await _employeesRepo.CreateEmployeeAsync(new Models.Neo4j.EmployeeNeo4j
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Address = e.Address,
                    Phone = e.Phone,
                    Email = e.Email,
                    ExperienceLevel = e.ExperienceLevel
                });
            }
            // ========================
            // 2. Bicycles
            // ========================
            foreach (var b in await _sql.Bicycles.ToListAsync())
            {
                await _bicyclesRepo.CreateBicycleAsync(new Models.Neo4j.BicycleNeo4j
                {
                    Id = b.Id,
                    BicycleNumber = b.BicycleNumber,
                    InOperate = b.InOperate
                });
            }
            // ========================
            // 3. Routes
            // ========================
            foreach (var r in await _sql.Routes.ToListAsync())
            {
                await _routesRepo.CreateRouteAsync(new Models.Neo4j.RouteNeo4j
                {
                    Id = r.Id,
                    RouteNumber = r.RouteNumber
                });
            }
            // ========================
            // 4. Substituted
            // ========================
            foreach (var sub in await _sql.Substituteds.ToListAsync())
            {
                await _substitutedsRepo.CreateSubstitutedAsync(new Models.Neo4j.SubstitutedsNeo4j
                {
                    SubstitutedId = sub.SubstitutedId,
                    EmployeeId = sub.EmployeeId,
                    HasSubstituted = sub.HasSubstituted
                });
            }
            // ========================
            // 5. ListOfShift
            // ========================
            foreach (var l in await _sql.ListOfShifts.ToListAsync())
            {
                await _listOfShiftsRepo.CreateListOfShiftAsync(new Models.Neo4j.ListOfShiftNeo4j
                {
                    ShiftId = l.ShiftId,
                    DateOfShift = l.DateOfShift,
                    StartTime = l.StartTime,
                    EndTime = l.EndTime,
                    TotalHours = l.TotalHours,
                    EmployeeId = l.EmployeeId,
                    BicycleId = l.BicycleId,
                    RouteId = l.RouteId,
                    SubstitutedId = l.SubstitutedId
                });
            }
            // ========================
            // 6. Users
            // ========================
            foreach (var u in await _sql.Users.ToListAsync())
            {
                await _usersRepo.CreateUsersAsync(new Models.Neo4j.UsersNeo4j
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Hash = u.Hash,
                    Role = (Models.Neo4j.UserRole)u.Role,
                    EmployeeId = u.EmployeeId

                });
            }
            // ========================
            // 7. WorkHoursInMonths
            // ========================
            foreach (var w in await _sql.WorkHoursInMonths.ToListAsync())
            {
                await _workHoursInMonthsRepo.CreateWorkHoursInMonthsAsync(new Models.Neo4j.WorkHoursInMonthsNeo4j
                {
                    WorkHoursInMonthId = w.WorkHoursInMonthId,
                    EmployeeId = w.EmployeeId,
                    PayrollYear = w.PayrollYear,
                    PayrollMonth = w.PayrollMonth,
                    PeriodStart = w.PeriodStart,
                    PeriodEnd = w.PeriodEnd,
                    TotalHours = w.TotalHours,
                    HasSubstituted = w.HasSubstituted
                });
            }
            // ========================
            // 8. ShiftPlans
            // ========================
            foreach (var s in await _sql.ShiftPlans.ToListAsync())
            {
                await _shiftPlansRepo.CreateShiftPlansAsync(new Models.Neo4j.ShiftPlansNeo4j
                {
                    ShiftPlanId = s.ShiftPlanId,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Shifts = s.Shifts
                });
            }
            // ========================
            // 9. AuditLogs
            // ========================
            foreach (var a in await _sql.AuditLogs.ToListAsync())
            {
                await _auditLogsRepo.CreateAuditLogsAsync(new Models.Neo4j.AuditLogsNeo4j
                {
                    AuditId = a.AuditId,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Action = a.Action,
                    ChangedAt = a.ChangedAt,
                    ChangedBy = a.ChangedBy,
                    OldData = a.OldData,
                    NewData = a.NewData

                });
            }

        }
        public enum UserRole
        {
            Admin,
            Employee
        }
    }
}
