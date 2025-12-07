using Microsoft.EntityFrameworkCore;
using MigrationTestProject.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MySqlConnector;
using Neo4j.Driver;
using AutoMapper;
using dotenv.net;
using MigrationTestProject.Mapper;
using MigrationTestProject.Models.MongoDB;

namespace MigrationTestProject
{
    class Program
    {
        private static IMapper _mapper = null!;
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
            // -------------------------------
            // Step 6: Neo4j migrate Employee nodes
            // -------------------------------
            /*builder.Services.AddSingleton<IDriver>(driver);
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();*/
        }
    }

}
