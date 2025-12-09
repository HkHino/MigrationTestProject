
namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IRelationshipRepository
    {
        // Employee ↔ User
        Task CreateEmployeeUserRelationshipAsync(int employeeId, int userId);

        // You can add more relationships here, e.g. Employee ↔ Shift, Bicycle ↔ Route
    }
}