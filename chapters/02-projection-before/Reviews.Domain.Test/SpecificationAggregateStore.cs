using System.Threading;
using System.Threading.Tasks;
using Reviews.Core;

namespace Reviews.Domain.Test
{
    public class SpecificationAggregateStore : IAggrigateStore
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
        
       
    }
}