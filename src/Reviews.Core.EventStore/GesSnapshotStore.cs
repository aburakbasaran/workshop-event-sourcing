using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Reviews.Core.EventStore
{
    public class GesSnapshotStore : ISnapshotStore
    {
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly UserCredentials userCredentials;
        
        public GesSnapshotStore(IEventStoreConnection eventStoreConnection, 
            UserCredentials userCredentials=null)
        {
            this.eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));;
            this.userCredentials = userCredentials;
        }

        public async Task<long> SaveSnapshotAsync<T>(Snapshot snapshot) where T:Aggregate
        {
            var stream = GetStreamName<T>(snapshot.AggregateId);
            
            var snapshotEvent =  new EventData(
                snapshot.Id, 
                EventTypeMapper.GetTypeName(snapshot.GetType()),
                EventSerializer.IsJsonSerializer,
                EventSerializer.Serialize(snapshot),
                null);

            var result = await eventStoreConnection.AppendToStreamAsync(stream,ExpectedVersion.Any, snapshotEvent);

            return result.LogPosition.CommitPosition;
        }

        public async Task<Snapshot> GetSnapshotAsync<T>(Guid aggregateId) where T:Aggregate
        {
            var stream = GetStreamName<T>(aggregateId);
            var streamEvents = await eventStoreConnection.ReadStreamEventsBackwardAsync(stream,
                StreamPosition.End, 
                1, 
                false);

            if (!streamEvents.Events.Any()) return null;
            
            var result = streamEvents.Events.FirstOrDefault();
            return (Snapshot)result.Deserialze();
        }
        
        private static string GetStreamName<T>(Guid aggregateId) where T:Aggregate
            => $"{typeof(T).Name}Snapshot-{aggregateId}";
    }
}