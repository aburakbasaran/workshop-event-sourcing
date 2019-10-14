using System;

namespace Reviews.Core
{
    public class Snapshot
    {
        public Guid Id { get; }
        public Guid AggregateId { get; }
        public long Version { get; }

        protected Snapshot()
        {
            
        }

        public Snapshot(Guid id, Guid aggregateId, long version):base()
        {
            Id = id;
            AggregateId = aggregateId;
            Version = version;
        }
    }
}