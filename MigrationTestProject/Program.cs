using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MigrationTestProject;
using MigrationTestProject.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MySqlConnector;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;


namespace MigrationTestProject
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // -------------------------------
            // Step 0: Load configuration
            // -------------------------------
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // folder where the app runs
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            // -------------------------------
            // Step 1: Setup MySQL DbContext options
            // -------------------------------
            var mysqlOptions = new DbContextOptionsBuilder<MySqlContext>()
                .UseMySql(
                    config.GetConnectionString("MySql3"),
                    new MySqlServerVersion(new Version(8, 0, 30))
                )
                .Options;
            // -------------------------------
            // Step 2: Test MySQL connection
            // -------------------------------
            try
            {
                using var testContext = new MySqlContext(mysqlOptions);
                await testContext.Database.OpenConnectionAsync();
                Console.WriteLine("MySQL connection successful!");
                await testContext.Database.CloseConnectionAsync();
            }
            catch (MySqlConnector.MySqlException mex)
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
            var mongoConnString = config.GetConnectionString("Mongo");
            var mongoClient = new MongoClient(mongoConnString);
            var mongoDb = mongoClient.GetDatabase("vagtplanlaegning");

            // -------------------------------
            // Step 4: Insert into MongoDB
            // -------------------------------
            try
            {
                Console.WriteLine("Starting Migration to Mongo...");
                using var context = new MySqlContext(mysqlOptions);

                await MigrateCollection(context.Employees, mongoDb, "Employees");
                await MigrateCollection(context.Bicycles, mongoDb, "Bicycles");
                await MigrateCollection(context.Substituteds, mongoDb, "Substituteds");
                await MigrateCollection(context.Routes, mongoDb, "Routes");
                await MigrateCollection(context.Users, mongoDb, "Users");
                await MigrateCollection(context.ShiftPlans, mongoDb, "ShiftPlans");
                await MigrateCollection(context.AuditLogs, mongoDb, "AuditLogs");
                await MigrateCollection(context.ListOfShifts, mongoDb, "ListOfShift");
                await MigrateCollection(context.WorkHoursInMonths, mongoDb, "WorkHoursInMonths");

                Console.WriteLine("All tables migrated to MongoDB successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during migration: " + ex.Message);
            }

);
            /*
            //----------------------------------------------------------------------------------------

             var mysqlConnString = config.GetConnectionString("MySql3");

             await using var mysql = new MySqlConnection(mysqlConnString);
             await mysql.OpenAsync();
             //----------------------------------------------------------------------------------------
             var databaseName = "vagtplanlaegning";
             var mongoConnString = config.GetConnectionString("Mongo");

             var mongoClient = new MongoClient(mongoConnString);
             var mongoDb = mongoClient.GetDatabase(databaseName);
             

            // clear target (dev only)
            await mongoDb.GetCollection<BsonDocument>("employees").DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
            await mongoDb.GetCollection<BsonDocument>("bicycles").DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
            await mongoDb.GetCollection<BsonDocument>("routes").DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
            await mongoDb.GetCollection<BsonDocument>("shifts").DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
            await mongoDb.GetCollection<BsonDocument>("workHoursInMonths").DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
        
            // Get the list of existing databases

            var dbList = await mongoClient.ListDatabaseNamesAsync();
            var dbNames = await dbList.ToListAsync();

            if (!dbNames.Contains(databaseName))
            {
                // MongoDB doesn’t create empty databases — create a collection to trigger it
                await mongoDb.CreateCollectionAsync("init_collection");
                Console.WriteLine($"Database '{databaseName}' created (well, technically forced into existence).");
            }
            else
            {
                Console.WriteLine($"Database '{databaseName}' already exists.");
            }

            var employeeIdMap = new Dictionary<int, ObjectId>();
            var bicycleIdMap = new Dictionary<int, ObjectId>();
            var routeIdMap = new Dictionary<int, ObjectId>();
            var dayIdMap = new Dictionary<int, DateTime>();
            var substitutedMap = new Dictionary<int, (ObjectId employeeId, bool hasSubstituted)>();

            // EMPLOYEES
            var employeesCol = mongoDb.GetCollection<BsonDocument>("employees");
            await using (var cmd = new MySqlCommand("SELECT * FROM Employees", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var sqlId = reader.GetInt32("employeeId");
                    var mongoId = ObjectId.GenerateNewId();

                    var doc = new BsonDocument
                    {
                        { "_id", mongoId },
                        { "sqlEmployeeId", sqlId },               // <-- keep SQL id
                        { "firstName", reader.GetString("firstName") },
                        { "lastName", reader.GetString("lastName") },
                        { "address", reader.GetString("address") },
                        { "phone", reader.GetString("phone") },
                        { "email", reader.GetString("email") }
                    };

                    await employeesCol.InsertOneAsync(doc);
                    employeeIdMap[sqlId] = mongoId;
                }
            }

            // BICYCLES
            var bicyclesCol = mongoDb.GetCollection<BsonDocument>("bicycles");
            await using (var cmd = new MySqlCommand("SELECT * FROM Bicycles", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var sqlId = reader.GetInt32("id");
                    var mongoId = ObjectId.GenerateNewId();

                    var doc = new BsonDocument
                    {
                        { "_id", mongoId },
                        { "sqlBicycleId", sqlId },                // <-- keep SQL id
                        { "inOperate", reader.GetBoolean("inOperate") }
                    };

                    await bicyclesCol.InsertOneAsync(doc);
                    bicycleIdMap[sqlId] = mongoId;
                }
            }

            // ROUTES
            var routesCol = mongoDb.GetCollection<BsonDocument>("routes");
            await using (var cmd = new MySqlCommand("SELECT * FROM Route", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var sqlId = reader.GetInt32("routeNumberId");
                    var mongoId = ObjectId.GenerateNewId();

                    var doc = new BsonDocument
                    {
                        { "_id", mongoId },
                        { "sqlRouteNumberId", sqlId },            // <-- keep SQL id
                        { "routeNumber", sqlId }
                    };

                    await routesCol.InsertOneAsync(doc);
                    routeIdMap[sqlId] = mongoId;
                }
            }

            // DAYS (just in memory)
            await using (var cmd = new MySqlCommand("SELECT * FROM Days", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dayIdMap[reader.GetInt32("dayId")] = reader.GetDateTime("day");
                }
            }

            // SUBSTITUTEDS (to embed later)
            await using (var cmd = new MySqlCommand("SELECT * FROM Substituteds", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var subId = reader.GetInt32("substitutedId");
                    var empSqlId = reader.GetInt32("employeeId");
                    var hasSub = reader.GetBoolean("hasSubstituted");

                    substitutedMap[subId] = (employeeIdMap[empSqlId], hasSub);
                }
            }

            // SHIFTS
            var shiftsCol = mongoDb.GetCollection<BsonDocument>("shifts");
            await using (var cmd = new MySqlCommand("SELECT * FROM ListOfShift", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var batch = new List<BsonDocument>();

                while (await reader.ReadAsync())
                {
                    var sqlShiftId = reader.GetInt32("shiftId");             // <-- from SQL
                    var dayId = reader.GetInt32("dayId");
                    var date = dayIdMap[dayId];

                    var empMongoId = employeeIdMap[reader.GetInt32("employeeId")];
                    var bikeMongoId = bicycleIdMap[reader.GetInt32("bicycleId")];
                    var routeMongoId = routeIdMap[reader.GetInt32("routeNumberId")];

                    var doc = new BsonDocument
                    {
                        { "sqlShiftId", sqlShiftId },           // <-- keep SQL id
                        { "date", date },
                        { "employeeId", empMongoId },
                        { "bicycleId", bikeMongoId },
                        { "routeId", routeMongoId },
                        { "meetInTime", reader.GetTimeSpan("meetInTime").ToString() }
                    };

                    // optional times
                    var startOrdinal = reader.GetOrdinal("startTime");
                    if (!await reader.IsDBNullAsync(startOrdinal))
                        doc["startTime"] = reader.GetTimeSpan(startOrdinal).ToString();
                    else
                        doc["startTime"] = BsonNull.Value;

                    var endOrdinal = reader.GetOrdinal("endTime");
                    if (!await reader.IsDBNullAsync(endOrdinal))
                        doc["endTime"] = reader.GetTimeSpan(endOrdinal).ToString();
                    else
                        doc["endTime"] = BsonNull.Value;

                    var totalOrdinal = reader.GetOrdinal("totalHours");
                    if (!await reader.IsDBNullAsync(totalOrdinal))
                        doc["totalHours"] = reader.GetDecimal(totalOrdinal);
                    else
                        doc["totalHours"] = BsonNull.Value;

                    // embedded substitute
                    var subId = reader.GetInt32("substitutedId");
                    if (substitutedMap.TryGetValue(subId, out var subData))
                    {
                        doc["substitute"] = new BsonDocument
                        {
                            { "employeeId", subData.employeeId },
                            { "hasSubstituted", subData.hasSubstituted },
                            { "sqlSubstitutedId", subId }        // <-- also keep the original substitutedId
                        };
                    }

                    batch.Add(doc);
                }

                if (batch.Count > 0)
                    await shiftsCol.InsertManyAsync(batch);
            }

            // WORK HOURS
            var workHoursCol = mongoDb.GetCollection<BsonDocument>("workHoursInMonths");
            await using (var cmd = new MySqlCommand("SELECT * FROM WorkHoursInMonths", mysql))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                var docs = new List<BsonDocument>();
                while (await reader.ReadAsync())
                {
                    var empSqlId = reader.GetInt32("employeeId");
                    var doc = new BsonDocument
                    {
                        { "sqlWorkHoursInMonthId", reader.GetInt32("workHoursInMonthId") }, // keep sql PK
                        { "employeeId", employeeIdMap[empSqlId] },
                        { "sqlEmployeeId", empSqlId },                                      // for join-back
                        { "payrollYear", reader.GetInt32("payrollYear") },
                        { "payrollMonth", reader.GetInt32("payrollMonth") },
                        { "periodStart", reader.GetDateTime("periodStart") },
                        { "periodEnd", reader.GetDateTime("periodEnd") },
                        { "totalHours", reader.GetDecimal("totalHours") },
                        { "hasSubstituted", reader.GetBoolean("hasSubstituted") }
                    };
                    docs.Add(doc);
                }

                if (docs.Count > 0)
                    await workHoursCol.InsertManyAsync(docs);
            }
            */
            Console.WriteLine("Migration done with SQL IDs preserved ✨");
        }
        private static async Task MigrateGraphDatabase<T>(DbSet<T> dbSet, string nodeName) where T : class
        {

            // -------------------------------
            // Step 1: Connection
            // -------------------------------

            var driver = GraphDatabase.Driver(
                "neo4j+s://<your-instance>.databases.neo4j.io",
                AuthTokens.Basic("neo4j", "<password>"));
            // -------------------------------
            // Step 2: Create nodes
            // -------------------------------
            using var session = driver.AsyncSession();
            await session.RunAsync(
            "CREATE (p:Person {id: $id, name: $name})",
            new { /*id = person.Id, name = person.Name */ });

            // -------------------------------
            // Step 3:  Create relationships
            // -------------------------------
            await session.RunAsync(
            @"MATCH (a:Person {id: $from}), (b:Person {id: $to})
            CREATE (a)-[:FRIENDS_WITH]->(b)",
            new { /*from = idA, to = idB */});


        }
        private static async Task MigrateCollection<T>(DbSet<T> dbSet,IMongoDatabase mongoDb,string collectionName) where T : class
        {

            // Get the MongoDB collection as BsonDocument
            var collection = mongoDb.GetCollection<BsonDocument>(collectionName);

            // Optional: clear existing collection
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

            var items = await dbSet.ToArrayAsync();
            // Convert each item to BsonDocument (excluding navigation properties)
            var docs = new List<BsonDocument>();

            foreach (var item in items)
            {
                var doc = new BsonDocument();

                foreach (var prop in typeof(T).GetProperties())
                {
                    var value = prop.GetValue(item);

                    if (value == null)
                    {
                        doc[prop.Name] = BsonNull.Value;
                        continue;
                    }

                    var type = prop.PropertyType;

                    if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                    {
                        // Convert TimeSpan to string
                        var ts = (TimeSpan)value;
                        doc[prop.Name] = ts.ToString(@"hh\:mm\:ss");
                    }
                    else if (type.IsValueType || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(bool) || type == typeof(bool?))
                    {
                        doc[prop.Name] = BsonValue.Create(value);

                    }
                    /*else if (type.IsClass && type != typeof(string))
                    {//MigrationTestProject  MigrationTestProject
                        // Skip navigation property
                        doc[prop.Name] = BsonValue.Create(value);
                    }*/
                    else // ignore navigation properties
                    {
                        // Skip navigation properties (any class type that is not a primitive/string/etc.)
                        continue;
                    }
                }

                docs.Add(doc);
            }

            // Insert into MongoDB as BsonDocument
            if (docs.Count > 0)
            //if (items.Length > 0)
            {
                //await collection.InsertManyAsync(items);
                await collection.InsertManyAsync(docs);
            }
            else
            {
                Console.WriteLine($"No {collectionName} found to migrate.");
            }

            //Console.WriteLine($"Migrated {items.Length} items to {collectionName}.");
            Console.WriteLine($"Migrated {docs.Count} items to {collectionName}.");
        }
    }
}
