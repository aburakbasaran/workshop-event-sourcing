using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Reviews.Core.Projections;

namespace Reviews.Core.EventStore
{
    public class ProjectionManagerBuilder
    {
        private IEventStoreConnection eventStoreConnection;
        private ICheckpointStore checkpointStore;
        private Projection[] projections;
        private UserCredentials userCredentials=null;
        
        private int maxLiveQueueSize=10000;
        private int readBatchSize=500;
        private bool verboseLogging=false;

        public ProjectionManagerBuilder Connection(IEventStoreConnection eventStoreConnection)
        {
            this.eventStoreConnection = eventStoreConnection;
            return this;
        }
        
        public ProjectionManagerBuilder CheckpointStore(ICheckpointStore checkpointStore)
        {
            this.checkpointStore = checkpointStore;
            return this;
        }
        public ProjectionManagerBuilder MaxLiveQueueSize(int maxLiveQueueSize)
        {
            this.maxLiveQueueSize = maxLiveQueueSize;
            return this;
        }

        public ProjectionManagerBuilder ReadBatchSize(int readBatchSize)
        {
            this.readBatchSize = readBatchSize;
            return this;
        }
        public ProjectionManagerBuilder UserCredentials(UserCredentials userCredentials)
        {
            this.userCredentials = userCredentials;
            return this;
        }
        public ProjectionManagerBuilder EnableLogging()
        {
            this.verboseLogging = true;
            return this;
        }
        public ProjectionManagerBuilder AddProjection(Projection projection)
        {
            this.projections.Append(projection);
            return this;
        }
        public ProjectionManagerBuilder SetProjections(params Projection[] projections)
        {
            this.projections = projections;
            return this;
        }
        private ProjectionManager Build() =>
            new ProjectionManager(
                eventStoreConnection,
                checkpointStore,
                projections,
                maxLiveQueueSize,
                readBatchSize,
                verboseLogging,
                userCredentials
            );
        
        public async Task<ProjectionManager> StartAll()
        {
            var manager = Build();
            await manager.StartAll();
            return manager;
        }
        
        private ProjectionManagerWithPersistantSubscription BuildWithPersist() =>
            new ProjectionManagerWithPersistantSubscription(
                eventStoreConnection,
                checkpointStore,
                projections,
                maxLiveQueueSize,
                readBatchSize,
                verboseLogging,
                userCredentials
            );
        
        public async Task<ProjectionManagerWithPersistantSubscription> StartAllWithPersist()
        {
            var manager = BuildWithPersist();
            await manager.StartAll();
            return manager;
        }
        
    }
}