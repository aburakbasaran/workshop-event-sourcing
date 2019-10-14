using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reviews.Core;

namespace Reviews.Domain.Test
{

    public class SpesificationAggregateSnapshotStore : ISnapshotStore
    {
        private Dictionary<Guid,object> snapshotStore=new Dictionary<Guid, object>();
        
        public Task<Snapshot> GetSnapshotAsync<T>(Guid aggregateId) where T:Aggregate
        {
            var s = default(T);
            return  Task.FromResult((Snapshot) snapshotStore.GetValueOrDefault(aggregateId));
        }

        public Task<long> SaveSnapshotAsync<T>(Snapshot snapshot) where T:Aggregate
        {
            snapshotStore.Add(snapshot.AggregateId, snapshot);
            return Task.FromResult(long.MaxValue);
        }
    }
    
    public class SpecificationAggregateStore : IAggregateStore
    {
        private Aggregate aggregate;
        
        public object[] RaisedEvents { get; private set; }

        public SpecificationAggregateStore(Aggregate agg) => aggregate = agg;
        
        public Task<(long NextExceptedVersion, long LastPosition, long CommitPosition)> Save<T>(T agg, CancellationToken cancellationToken = default) where T : Aggregate
        {
            aggregate = agg;
            
            RaisedEvents = aggregate.GetChanges();
            
            var version =  aggregate.Version + aggregate.GetChanges().Length;
            
            return Task.FromResult<(long NextExpectedVersion, long LogPosition, long CommitPosition)>(
                (version,version,version));
        }

        public Task<T> Load<T>(string aggregateId, CancellationToken cancellationToken = default) where T : Aggregate,new()
            => Task.FromResult((T)aggregate);

        public Task<object[]> GetEvents<T>(string aggregateId, long start, int count) where T : Aggregate
            => Task.FromResult(RaisedEvents);
    }
}