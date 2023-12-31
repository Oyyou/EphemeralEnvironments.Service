﻿using EphemeralEnvironments.Service.Commands;
using EphemeralEnvironments.Service.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Npgsql;

namespace EphemeralEnvironments.Service
{
    internal class Program
    {
        private static string postgresConnectionString = "";
        static void Main(string[] args)
        {
            Helpers.Log("Host started");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                postgresConnectionString = configuration.GetConnectionString("DefaultConnection");
                var mongoConnectionString = configuration.GetConnectionString("MongoDB");

                MongoClient mongoClient;
                IMongoDatabase? mongoDatabase = null;

                Helpers.Retry(() =>
                {
                    Helpers.Log($"Attemping to connect to: {mongoConnectionString}");
                    mongoClient = new MongoClient(mongoConnectionString);
                    mongoDatabase = mongoClient.GetDatabase("vibes");
                    // Attempt to perform a simple operation to check if the connection is successful
                    mongoDatabase.RunCommand((Command<BsonDocument>)"{ping:1}");
                }, TimeSpan.FromSeconds(30), maxAttempts: 5);

                if (mongoDatabase == null)
                {
                    throw new Exception($"Unable to connect to mongo: {mongoConnectionString}");
                }

                Helpers.Log($"Connected to mongo successfully");

                var collection = mongoDatabase.GetCollection<BsonDocument>("EventJournal");

                // Read existing documents from the collection
                var existingDocuments = collection.Find(new BsonDocument()).ToList();
                foreach (var document in existingDocuments)
                {
                    ProcessEntry(document);
                }

                while (true)
                {
                    try
                    {
                        var newDocuments = collection.Find(new BsonDocument()).ToList();

                        foreach (var newDocument in newDocuments)
                        {
                            ProcessEntry(newDocument);
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        Helpers.Log($"Error: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                Helpers.Log($"Error: {e}");
            }
            Helpers.Log("Host stopped");
        }

        private static void ProcessEntry(BsonDocument? document)
        {
            if (document == null)
            {
                return;
            }

            var deserializedDocument = BsonSerializer.Deserialize<Document>(document);
            var id = deserializedDocument.Id;
            var payload = deserializedDocument.Payload;
            switch (payload.Type)
            {
                case "VibeCreated":
                    HandleVibeCreated(new VibeCreated(id.ToString(), payload.Value, payload.TimeAdded));
                    break;
            }
        }

        private static void HandleVibeCreated(VibeCreated vibe)
        {
            using (var postgresConnection = new NpgsqlConnection(postgresConnectionString))
            {
                postgresConnection.Open();

                if (HasEventBeenProcessed(vibe.EventId, postgresConnection))
                {
                    return;
                }

                Helpers.Log($"Processing 'VibeCreated'");

                using (var cmd = new NpgsqlCommand("INSERT INTO vibes (vibe) VALUES (@vibe)", postgresConnection))
                {
                    cmd.Parameters.AddWithValue("vibe", vibe.Value);
                    cmd.ExecuteNonQuery();
                }

                AddProcessEvent(vibe, postgresConnection);
            }
        }

        private static void AddProcessEvent(VibeCreated vibe, NpgsqlConnection postgresConnection)
        {
            using (var cmd = new NpgsqlCommand("INSERT INTO processed_events (event_id, event_created_date_time) VALUES (@event_id, @event_created_date_time)", postgresConnection))
            {
                cmd.Parameters.AddWithValue("event_id", vibe.EventId);
                cmd.Parameters.AddWithValue("event_created_date_time", vibe.TimeAdded);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool HasEventBeenProcessed(string eventId, NpgsqlConnection connection)
        {
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM processed_events WHERE event_id = @eventId", connection))
            {
                cmd.Parameters.AddWithValue("eventId", eventId);
                var t = cmd.ExecuteScalar();
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
