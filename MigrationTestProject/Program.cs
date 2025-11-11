using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static async Task Main(string[] args)
    {
        var mysqlConnString = "Server=127.0.0.1;Port=3306;Database=cykelBudDB;User=root;Password=1234;";
        await using var mysql = new MySqlConnection(mysqlConnString);
        await mysql.OpenAsync();

        var databaseName = "vagtPlanlægning";
        var mongoClient = new MongoClient("mongodb+srv://mpfugl_db_user:Wy0ngZaAEtoUZLbA@vagtplanlaegning.bceb2r2.mongodb.net/");
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
                var sqlId = reader.GetInt32("bicycleId");
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

        Console.WriteLine("Migration done with SQL IDs preserved ✨");
    }
}
