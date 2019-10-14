using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Reviews.Core.EventStore
{
    public static class EventStoreConnectionBuilder
    {
        public static async Task<IEventStoreConnection> ConfigureStore()
        {
            var connectionString = "host=localhost:1113;";
            var data = connectionString.Split(';');

            var hosts = data.Where(x => x.StartsWith("Host", StringComparison.CurrentCultureIgnoreCase));
            if (!hosts.Any())
                throw new ArgumentException("No Host parameter in eventstore connection string");


            var endpoints = hosts.Select(x =>
            {
                var addr = x.Substring(5).Split(':');
                if (addr[0] == "localhost")
                    return new IPEndPoint(IPAddress.Loopback, int.Parse(addr[1]));
                return new IPEndPoint(IPAddress.Parse(addr[0]), int.Parse(addr[1]));
            }).ToArray();

            var cred = new UserCredentials("admin", "changeit");
            var settings = ConnectionSettings.Create()
                .KeepReconnecting()
                .KeepRetrying()
                .SetGossipSeedEndPoints(endpoints)
                .SetClusterGossipPort(2113)
                .SetHeartbeatInterval(TimeSpan.FromSeconds(30))
                .SetGossipTimeout(TimeSpan.FromMinutes(5))
                .SetHeartbeatTimeout(TimeSpan.FromMinutes(5))
                .SetTimeoutCheckPeriodTo(TimeSpan.FromMinutes(1))
                .SetDefaultUserCredentials(cred);

            IEventStoreConnection client;
            if (hosts.Count() != 1)
            {
                var clusterSettings = ClusterSettings.Create()
                    .DiscoverClusterViaGossipSeeds()
                    .SetGossipSeedEndPoints(endpoints.Select(x => new IPEndPoint(x.Address, 2113)).ToArray())
                    .SetGossipTimeout(TimeSpan.FromMinutes(5))
                    .Build();

                client = EventStoreConnection.Create(settings, clusterSettings, "Demo-App");
            }
            else
                client = EventStoreConnection.Create(settings, endpoints.First(), "Demo-App");


            await client.ConnectAsync();

            return client;
        }
    }
}