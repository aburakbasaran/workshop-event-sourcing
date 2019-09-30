using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using EventStore.ClientAPI;
using Reviews.Service.WebApi;

namespace Reviews.Core.EventStore.Tests
{
    public class GesBaseTest
    {
        protected IEventStoreConnection Connection { get; }
        protected Fixture AutoFixture { get; }

        protected GesBaseTest()
        {
            Connection = GetConnection().GetAwaiter().GetResult();
            AutoFixture = new Fixture();

            try
            {
                EventMappings.MapEventTypes();
            }
            catch (Exception e)
            {
                //ignore the already mapped events...
                Console.WriteLine(e);
            }
           
        }

        private static async Task<IEventStoreConnection> GetConnection()
        {
            var connection = EventStoreConnection.Create(
                new IPEndPoint(IPAddress.Loopback, 1113)
            );

            await connection
                .ConnectAsync()
                .ConfigureAwait(false);

            return connection;
        }
    }
}