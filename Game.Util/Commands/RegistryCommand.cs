namespace Game.Util.Commands
{
    using Game.API.Common.Models.Auditing;
    using Google.Cloud.Firestore;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Subcommand("list", typeof(List))]
    [Subcommand("migrate", typeof(MigrateDatabase))]
    class RegistryCommand : CommandBase
    {
        class List : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var list = await RegistryAPI.Registry.ListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(list));
            }
        }

        class MigrateDatabase : CommandBase
        {
            private Dictionary<string, object> fields = null;

            protected async override Task ExecuteAsync()
            {
                var firestore = FirestoreDb.Create("daud-230416");

                CollectionReference eventsRef = firestore.Collection("events");
                Query query = eventsRef.Limit(100000);
                QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
                foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
                {

                    AuditEventBase auditEvent = null;
                    fields = documentSnapshot.ToDictionary();
                    var type = GetField("Type", null as string);
                    switch (type)
                    {
                        case "AuditEventSpawn":
                            auditEvent = new AuditEventSpawn()
                            {
                                Player = PlayerFrom("Player")
                            };
                            break;
                        case "AuditEventDeath":
                            auditEvent = new AuditEventDeath()
                            {
                                Killer = PlayerFrom("Killer"),
                                Victim = PlayerFrom("Victim")
                            };
                            break;
                    }
                    auditEvent.AdvertisedPlayerCount = (int)GetField("AdvertisedPlayerCount", 0L);
                    auditEvent.Created = DateTimeOffset.UnixEpoch.AddSeconds(GetField("Created", 0L)).DateTime;
                    auditEvent.GameID = GetField("GameID", null as string);
                    auditEvent.GameTime = GetField("GameTime", 0L);
                    auditEvent.PublicURL = GetField("PublicURL", null as string);
                    auditEvent.Type = GetField("Type", null as string);
                    auditEvent.WorldKey = GetField("WorldKey", null as string);

                    Console.WriteLine(JsonConvert.SerializeObject(auditEvent, Formatting.Indented));

                    await RegistryAPI.Registry.PostEvents(new[] { auditEvent });
                }
            }

            private AuditModelPlayer PlayerFrom(string fieldName)
            {
                return new AuditModelPlayer
                {
                    AliveSince = GetField($"{fieldName}.AliveSince", 0L),
                    ComboCounter = (int)GetField($"{fieldName}.ComboCounter", 0L),
                    FleetID = (uint)GetField($"{fieldName}.FleetID", 0L),
                    FleetName = GetField($"{fieldName}.FleetName", null as string),
                    FleetSize = (int)GetField($"{fieldName}.FleetSize", 0L),
                    KillCount = (int)GetField($"{fieldName}.KillCount", 0L),
                    KillStreak = (int)GetField($"{fieldName}.KillStreak", 0L),
                    Latency = (uint)GetField($"{fieldName}.Latency", 0L),
                    LoginID = (ulong)GetField($"{fieldName}.LoginID", 0L),
                    LoginName = GetField($"{fieldName}.LoginName", null as string),
                    MaxCombo = (int)GetField($"{fieldName}.MaxCombo", 0L),
                    PlayerID = GetField($"{fieldName}.PlayerID", null as string),
                    Momentum = new System.Numerics.Vector2
                    {
                        X = (float)GetField($"{fieldName}.Momentum.X", (double)0),
                        Y = (float)GetField($"{fieldName}.Momentum.Y", (double)0)
                    },
                    Position = new System.Numerics.Vector2
                    {
                        X = (float)GetField($"{fieldName}.Position.X", (double)0),
                        Y = (float)GetField($"{fieldName}.Position.Y", (double)0)
                    },
                    Score = (int)GetField($"{fieldName}.Score", 0L)
                };
            }

            private T GetField<T>(string name, T defaultValue)
            {
                if (fields.ContainsKey(name))
                    return (T)fields[name];
                else
                    return defaultValue;
            }
        }
    }
}
