using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Reviews.Core
{
    public class Repository : IRepository
    {
        private readonly IAggregateStore aggregateStore;
        private readonly ISnapshotStore gesSnapshotStore;

        public Repository(IAggregateStore aggregateStore, ISnapshotStore gesSnapshotStore)
        {
            this.aggregateStore = aggregateStore;
            this.gesSnapshotStore = gesSnapshotStore;
        }

        public async Task<T> GetByIdAsync<T>(Guid id) where T : Aggregate, new()
        {
            var isSnapShottable = typeof(ISnapshottable<T>).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());

            Snapshot snapshot = null;

            if (isSnapShottable && gesSnapshotStore != null)
            {
                snapshot = await gesSnapshotStore.GetSnapshotAsync<T>(id);
            }

            if (snapshot != null)
            {
                var aggregate = new T();
                
                ((ISnapshottable<T>)aggregate).ApplySnapshot(snapshot);

                var events = await aggregateStore.GetEvents<T>(id.ToString(),aggregate.Version + 1, int.MaxValue);
                
                aggregate.Load(events);

                return aggregate;
            }

            return await aggregateStore.Load<T>(id.ToString());

        }

        public Task SaveAsync<T>(T aggregate) where T : Aggregate
        {
            var task =aggregateStore.Save(aggregate);

            if (aggregate is ISnapshottable<T> snapshotable)
            {
                if (snapshotable.SnapshotFrequency().Invoke())
                {
                    gesSnapshotStore.SaveSnapshotAsync<T>(snapshotable.TakeSnapshot());
                }
            }

            return task;
        }
    }
}