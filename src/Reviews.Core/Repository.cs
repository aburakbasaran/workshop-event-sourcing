using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Reviews.Core
{
    public class Repository : IRepository
    {
        private readonly IAggrigateStore aggregateStore;
        private readonly ISnapshotStore snapshotStore;

        public Repository(IAggrigateStore aggregateStore,ISnapshotStore snapshotStore)
        {
            this.aggregateStore = aggregateStore;
            this.snapshotStore = snapshotStore;
        }
        
        public async Task<T> GetByIdAsync<T>(Guid id) where T : Aggregate, new()
        {
            var isSnapshottable = typeof(ISnapshottable<T>).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());
           
            var aggregate = new T();
            
            Snapshot snapshot = null;
            if ((isSnapshottable) && (snapshotStore != null))
            {
                if (((ISnapshottable<T>) aggregate).SnapshotFrequency().Invoke())
                {
                    snapshot = ((ISnapshottable<T>) aggregate).TakeSnapshot();
                    snapshot = await snapshotStore.GetSnapshotAsync<T>(snapshot.GetType(),id);    
                }
            }
           
            //snapshot exists?
            if (snapshot != null)
            {
                Console.WriteLine(snapshot.Id);
                var item = (T)Activator.CreateInstance(typeof(T));
                
                ((ISnapshottable<T>)item).ApplySnapshot(snapshot);
                Console.WriteLine("getting data from snapshot:"+item.Version);
                var events = await aggregateStore.GetEvents<T>(id.ToString(), snapshot.Version + 1, int.MaxValue); 
                Console.WriteLine("load event over snapshot event count:"+events.Length);
                item.Load(events);

                return item;
            }

            return await aggregateStore.Load<T>(id.ToString());
        }

        public Task SaveAsync<T>(T aggregate) where T : Aggregate
        {
            var task = aggregateStore.Save(aggregate);
            
            #region Snapshot check & save it!

            Console.WriteLine("snapshot check");
            if (aggregate is ISnapshottable<T> snapshottable)
            {
                Console.WriteLine("snapshot check:true");
               
                if (snapshottable.SnapshotFrequency().Invoke())
                {
                    Console.WriteLine("taking snapshot...");
                    snapshotStore.SaveSnapshotAsync(snapshottable.TakeSnapshot());
                }
            }
            #endregion

            return task;

        }
    }
}