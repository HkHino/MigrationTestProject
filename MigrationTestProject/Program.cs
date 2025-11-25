using MongoDB.Bson;
using MongoDB.Driver;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // MySQL source ---------------------------------------------------------
        var mysqlConnString = "Server=127.0.0.1;Port=3306;Database=cykelBudDB;User=root;Password=1234;";
        await using var mysql = new MySqlConnection(mysqlConnString);
        await mysql.OpenAsync();

        // Mongo target ---------------------------------------------------------
        var databaseName = "vagtplanlaegning"; // no æ to keep it simple
        var mongoClient = new MongoClient("mongodb+srv://mpfugl_db_user:Wy0ngZaAEtoUZLbA@vagtplanlaegning.bceb2r2.mongodb.net/");
        var mongoDb = mongoClient.GetDatabase(databaseName);

        // clear target collections (dev only) ---------------------------------
        await mongoDb.GetCollection<BsonDocument>("employees").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await mongoDb.GetCollection<BsonDocument>("bicycles").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await mongoDb.GetCollection<BsonDocument>("routes").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await mongoDb.GetCollection<BsonDocument>("shifts").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await mongoDb.GetCollection<BsonDocument>("workHoursInMonths").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        // maps ----------------------------------------------------------------
        var employeeIdMap = new Dictionary<int, ObjectId>();
        var bicycleIdMap = new Dictionary<int, ObjectId>();
        var routeIdMap = new Dictionary<int, ObjectId>();
        var substitutedMap = new Dictionary<int, (int sqlEmployeeId, bool hasSubstituted)>();

        // EMPLOYEES ------------------------------------------------------------
        var employeesCol = mongoDb.GetCollection<BsonDocument>("employees");
        await using (var cmd = new MySqlCommand(
            "SELECT employeeId, firstName, lastName, address, phone, email, experienceLevel FROM Employees",
            mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var sqlId = reader.GetInt32("employeeId");
                var mongoId = ObjectId.GenerateNewId();

                var doc = new BsonDocument
                {
                    { "_id", mongoId },
                    { "sqlEmployeeId", sqlId },
                    { "firstName", reader.GetString("firstName") },
                    { "lastName", reader.GetString("lastName") },
                    { "address", reader.GetString("address") },
                    { "phone", reader.GetString("phone") },
                    { "email", reader.GetString("email") },
                    { "experienceLevel", reader.GetInt32("experienceLevel") }
                };

                await employeesCol.InsertOneAsync(doc);
                employeeIdMap[sqlId] = mongoId;
            }
        }

        // BICYCLES -------------------------------------------------------------
        var bicyclesCol = mongoDb.GetCollection<BsonDocument>("bicycles");
        await using (var cmd = new MySqlCommand(
            "SELECT id, bicycleNumber, inOperate FROM Bicycles",
            mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var sqlId = reader.GetInt32("id");
                var mongoId = ObjectId.GenerateNewId();

                var doc = new BsonDocument
                {
                    { "_id", mongoId },
                    { "sqlBicycleId", sqlId },
                    { "bicycleNumber", reader.GetInt32("bicycleNumber") },
                    { "inOperate", reader.GetBoolean("inOperate") }
                };

                await bicyclesCol.InsertOneAsync(doc);
                bicycleIdMap[sqlId] = mongoId;
            }
        }

        // ROUTES ---------------------------------------------------------------
        var routesCol = mongoDb.GetCollection<BsonDocument>("routes");
        await using (var cmd = new MySqlCommand(
            "SELECT id, routeNumber FROM Routes",
            mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var sqlId = reader.GetInt32("id");
                var routeNumber = reader.GetInt32("routeNumber");
                var mongoId = ObjectId.GenerateNewId();

                var doc = new BsonDocument
                {
                    { "_id", mongoId },
                    { "sqlRouteId", sqlId },
                    { "routeNumber", routeNumber }
                };

                await routesCol.InsertOneAsync(doc);
                routeIdMap[sqlId] = mongoId;
            }
        }

        // SUBSTITUTEDS ---------------------------------------------------------
        await using (var cmd = new MySqlCommand(
            "SELECT substitutedId, employeeId, hasSubstituted FROM Substituteds",
            mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var subId = reader.GetInt32("substitutedId");
                var empId = reader.GetInt32("employeeId");
                var hasSub = reader.GetBoolean("hasSubstituted");

                substitutedMap[subId] = (empId, hasSub);
            }
        }

        // SHIFTS ---------------------------------------------------------------
        var shiftsCol = mongoDb.GetCollection<BsonDocument>("shifts");
        await using (var cmd = new MySqlCommand("SELECT * FROM ListOfShift", mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            var batch = new List<BsonDocument>();

            while (await reader.ReadAsync())
            {
                var sqlShiftId = reader.GetInt32("shiftId");

                // Ny struktur: direkte dato
                var date = reader.GetDateTime("dateOfShift");

                var sqlEmpId = reader.GetInt32("employeeId");
                var sqlBikeId = reader.GetInt32("bicycleId");
                var sqlRouteId = reader.GetInt32("routeId");

                var doc = new BsonDocument
                {
                    { "sqlShiftId", sqlShiftId },
                    { "date", date },
                    { "employeeId", employeeIdMap[sqlEmpId] },
                    { "sqlEmployeeId", sqlEmpId },
                    { "bicycleId", bicycleIdMap[sqlBikeId] },
                    { "sqlBicycleId", sqlBikeId },
                    { "routeId", routeIdMap[sqlRouteId] },
                    { "sqlRouteId", sqlRouteId }
                };

                // startTime
                var startOrdinal = reader.GetOrdinal("startTime");
                if (!await reader.IsDBNullAsync(startOrdinal))
                    doc["startTime"] = reader.GetTimeSpan(startOrdinal).ToString();
                else
                    doc["startTime"] = BsonNull.Value;

                // endTime
                var endOrdinal = reader.GetOrdinal("endTime");
                if (!await reader.IsDBNullAsync(endOrdinal))
                    doc["endTime"] = reader.GetTimeSpan(endOrdinal).ToString();
                else
                    doc["endTime"] = BsonNull.Value;

                // totalHours
                var totalOrdinal = reader.GetOrdinal("totalHours");
                if (!await reader.IsDBNullAsync(totalOrdinal))
                    doc["totalHours"] = reader.GetDecimal(totalOrdinal);
                else
                    doc["totalHours"] = BsonNull.Value;

                // substitute (embedded)
                var subOrdinal = reader.GetOrdinal("substitutedId");
                if (!await reader.IsDBNullAsync(subOrdinal))
                {
                    var subSqlId = reader.GetInt32(subOrdinal);
                    if (substitutedMap.TryGetValue(subSqlId, out var subData))
                    {
                        var subEmpSqlId = subData.sqlEmployeeId;
                        doc["substitute"] = new BsonDocument
                        {
                            { "sqlSubstitutedId", subSqlId },
                            { "sqlEmployeeId", subEmpSqlId },
                            { "employeeId", employeeIdMap[subEmpSqlId] },
                            { "hasSubstituted", subData.hasSubstituted }
                        };
                    }
                    else
                    {
                        // hvis der er et substitutedId uden match i tabellen
                        doc["substitute"] = new BsonDocument
                        {
                            { "sqlSubstitutedId", subSqlId },
                            { "missing", true }
                        };
                    }
                }

                batch.Add(doc);
            }

            if (batch.Count > 0)
                await shiftsCol.InsertManyAsync(batch);
        }

        // WORK HOURS -----------------------------------------------------------
        var whCol = mongoDb.GetCollection<BsonDocument>("workHoursInMonths");
        await using (var cmd = new MySqlCommand("SELECT * FROM WorkHoursInMonths", mysql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            var docs = new List<BsonDocument>();
            while (await reader.ReadAsync())
            {
                var sqlEmpId = reader.GetInt32("employeeId");

                // Hvis der ikke er en employee i map (teoretisk),
                // kan vi vælge at skippe eller gemme kun sqlEmployeeId
                if (!employeeIdMap.TryGetValue(sqlEmpId, out var empMongoId))
                {
                    // spring over, eller lav en fallback:
                    // continue;
                    empMongoId = ObjectId.Empty;
                }

                var doc = new BsonDocument
                {
                    { "sqlWorkHoursInMonthId", reader.GetInt32("workHoursInMonthId") },
                    { "employeeId", empMongoId },
                    { "sqlEmployeeId", sqlEmpId },
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
                await whCol.InsertManyAsync(docs);
        }

        Console.WriteLine("Migration done ✅");
    }
}
