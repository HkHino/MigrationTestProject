using AutoMapper;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using MigrationTestProject.Mapper;
using MigrationTestProject.Models;
using MigrationTestProject.Models.MongoDB;
using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using MigrationTestProject.Repository;
using MigrationTestProject.Repository.Implementations;
using MigrationTestProject.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using MySqlConnector;
using Neo4j.Driver;
using MigrationTestProject.Mapper;

namespace MigrationTestProject
{
    class Program
    {   
        private static IMapper _mapper = null!;
        private static IAuditLogRepository auditLogsRepo;
        private static IEmployeeRepository employeesRepo;
        private static IBicycleRepository bicyclesRepo;
        private static IRouteRepository routesRepo;
        private static IListOfShiftRepository listOfShiftsRepo;
        private static ISubstitutedsRepository substitutedsRepo;
        private static IUsersRepository usersRepo;
        private static IWorkHoursInMonthsRepository workHoursInMonthsRepo;
        private static IShiftPlanRepository shiftPlansRepo;

        static async Task Main(string[] args)
        {
            // ========================
            // Load environment variables
            // ========================
            DotEnv.Load();
            var mySqlConnection = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__MYSQL");
            var mongoConnection = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__MONGO")!;
            var neo4jUri = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__NEO4J")!;
            var neo4jUser = Environment.GetEnvironmentVariable("NEO4J__USER")!;
            var neo4jPassword = Environment.GetEnvironmentVariable("NEO4J__PASSWORD")!;

            Console.WriteLine("MySQL connection string: " + mySqlConnection);
            Console.WriteLine("Mongo connection string: " + mongoConnection);
            if (mySqlConnection == null || mongoConnection == null)
            {
                throw new Exception("Please set the connection string in appsettings.json");
            }
            
            var mysqlOptions = new DbContextOptionsBuilder<MySqlContext>()
                .UseMySql(
                    mySqlConnection,
                    await Microsoft.EntityFrameworkCore.ServerVersion.AutoDetectAsync(mySqlConnection)
                )
                .Options;
            
            // -------------------------------
            // Step 2: Test MySQL connection
            // -------------------------------
            try
            {
                await using var testContext = new MySqlContext(mysqlOptions);
                await testContext.Database.OpenConnectionAsync();
                Console.WriteLine("MySQL connection successful!");
                await testContext.Database.CloseConnectionAsync();
            }
            catch (MySqlException mex)
            {
                Console.WriteLine("MySQL connection error: " + mex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                return;
            }
            
            // -------------------------------
            // Step 3: Connect to MongoDB
            // -------------------------------
            var mongoClient = new MongoClient(mongoConnection);
            var mongoDb = mongoClient.GetDatabase("vagtplanlaegning");
            
            // -------------------------------
            // AutoMapper initialization  ⭐
            // -------------------------------
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MigrationMappingProfile>();
            });

            _mapper = mapperConfig.CreateMapper();
            Console.WriteLine("AutoMapper initialized.");

            // -------------------------------
            // Step 4: Insert into MongoDB
            // -------------------------------
            try
            {
                Console.WriteLine("Starting Migration to Mongo...");
                await using var context = new MySqlContext(mysqlOptions);
                
                await MigrateCollection<Employee, EmployeeDocument>(context.Employees, mongoDb, "Employees");
                await MigrateCollection<Bicycle, BicycleDocument>(context.Bicycles, mongoDb, "Bicycles");
                await MigrateCollection<Substituted, SubstitutedDocument>(context.Substituteds, mongoDb, "Substituteds");
                await MigrateCollection<Route, RouteDocument>(context.Routes, mongoDb, "Routes");
                await MigrateCollection<User, UserDocument>(context.Users, mongoDb, "Users");
                await MigrateCollection<ShiftPlan, ShiftPlanDocument>(context.ShiftPlans, mongoDb, "ShiftPlans");
                await MigrateCollection<AuditLog, AuditLogDocument>(context.AuditLogs, mongoDb, "AuditLogs");
                await MigrateCollection<ListOfShift, ListOfShiftDocument>(context.ListOfShifts, mongoDb, "ListOfShift");
                await MigrateCollection<WorkHoursInMonths, WorkHoursInMonthsDocument>(context.WorkHoursInMonths, mongoDb, "WorkHoursMonths");
                
                async Task MigrateCollection<TSource, TDocument>(
                    DbSet<TSource> dbSet,
                    IMongoDatabase mongoDb,
                    string collectionName)
                    where TSource : class
                    where TDocument : class
                {
                    var collection = mongoDb.GetCollection<BsonDocument>(collectionName);

                    // Clear existing data
                    await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

                    // Load SQL rows
                    var items = await dbSet.ToListAsync();

                    // Map SQL → MongoDocument POCO
                    var mappedDocs = _mapper.Map<List<TDocument>>(items);

                    // Convert MongoDocument POCO → bson
                    var bsonDocs = mappedDocs.Select(d => d.ToBsonDocument()).ToList();

                    // Insert
                    if (bsonDocs.Count > 0)
                    {
                        await collection.InsertManyAsync(bsonDocs);
                        Console.WriteLine($"Migrated {bsonDocs.Count} → {collectionName}");
                    }
                    else
                    {
                        Console.WriteLine($"No {collectionName} found to migrate.");
                    }
                }
                
                Console.WriteLine("All tables migrated to MongoDB successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during migration: " + ex.Message);
            }
            
            Console.WriteLine("Migration done with SQL IDs preserved ✨");
            
            // -------------------------------
            // Step 5: Connect to Neo4j
            // -------------------------------


            var driver = GraphDatabase.Driver(
                neo4jUri,
                AuthTokens.Basic(neo4jUser, neo4jPassword)
            );
            await using var session = driver.AsyncSession();
            var result = await session.RunAsync("RETURN 1");
            var record = await result.SingleAsync();
            Console.WriteLine("Neo4j connection test result: " + record[0].As<int>());

            var employeesRepo = new EmployeesRepository(driver);
            var bicyclesRepo = new BicyclesRepository(driver);
            var routesRepo = new RouteRepository(driver);
            var listOfShiftsRepo = new ListOfShiftRepository(driver);
            var substitutedsRepo = new SubstitutedsRepository(driver);
            var usersRepo = new UsersRepository(driver);
            var workHoursInMonthsRepo = new WorkHoursInMonthsRepository(driver);
            var shiftPlansRepo = new ShiftPlansRepository(driver);
            var auditLogsRepo = new AuditLogsRepository(driver);
            // -------------------------------
            // Step 6: Neo4j migrate Employee nodes
            // -------------------------------

            // After initializing MySQL, Mongo, and Neo4j driver
            await using var mySqlContext = new MySqlContext(mysqlOptions);
            var migrationServiceNeo4j = new MigrationServiceNeo4j(
                mySqlContext,
                auditLogsRepo,
                employeesRepo,
                bicyclesRepo,
                routesRepo,
                listOfShiftsRepo,
                substitutedsRepo,
                usersRepo,
                workHoursInMonthsRepo,
                shiftPlansRepo
            );


            Console.WriteLine("Starting migration to Neo4j...");
            await migrationServiceNeo4j.MigrateAllAsync();
            Console.WriteLine("Neo4j migration completed!");
            
        }
    }

}
