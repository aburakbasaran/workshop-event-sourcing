using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Reviews.Core.Projections;

namespace Reviews.Core.EventStore
{
    public class ProjectionManagerWithPersistantSubscription
    {
        public static readonly ProjectionManagerBuilder With = new ProjectionManagerBuilder();
        
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly ICheckpointStore checkpointStore;
        private readonly Projection[] projections;
        private readonly UserCredentials userCredentials;

        private readonly int maxLiveQueueSize ;
        private readonly int readBatchSize;
        private readonly bool verboseLogging;
        private readonly string Group="GROUP-1";
        
        private List<EventStorePersistentSubscriptionBase> eventStorePersistentSubscriptionBases = new List<EventStorePersistentSubscriptionBase>();
        

        internal ProjectionManagerWithPersistantSubscription(IEventStoreConnection eventStoreConnection, 
            ICheckpointStore checkpointStore,
            Projection[] projections,
            int maxLiveQueueSize,
            int readBatchSize,
            bool verboseLogging,
            UserCredentials userCredentials=null
        )
        {
            this.eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));
            this.checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
            this.projections = projections;
            this.userCredentials = userCredentials;

            this.maxLiveQueueSize = maxLiveQueueSize;
            this.readBatchSize = readBatchSize;
            this.verboseLogging = verboseLogging;
        }

        public Task StartAll() => Task.WhenAll(this.projections.Select(StartProjection));

        private async Task StartProjection(Projection projection)
        {
            CreateSubscription(projection);
            ConnectToSubscription(projection);
        }
       
        private void ConnectToSubscription(Projection projection)
        {
            var bufferSize = 10;
            var autoAck = true;

            var subscription = eventStoreConnection.ConnectToPersistentSubscription(
                projection.ToString(), 
                Group, 
                EventAppeared(projection), 
                SubscriptionDropped(projection),
                userCredentials,
                bufferSize, 
                autoAck);
            
            eventStorePersistentSubscriptionBases.Add(subscription);
        }

        private Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> SubscriptionDropped(
            Projection projection)
            => async (eventStorePersistentSubscriptionBase, subscriptionDropReason, ex) =>
            {
                ConnectToSubscription(projection);
            };

        private Action<EventStorePersistentSubscriptionBase, ResolvedEvent> EventAppeared(Projection projection)
            => async (eventStorePersistentSubscriptionBase, resolvedEvent) =>
            {
                var data = Encoding.ASCII.GetString(resolvedEvent.Event.Data);
                Console.WriteLine("Received: " + resolvedEvent.Event.EventStreamId + ":" + resolvedEvent.Event.EventNumber);
                Console.WriteLine(data);
            };
        
        private void CreateSubscription(Projection projection)
        {
            PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
                .DoNotResolveLinkTos()
                .StartFromBeginning();

            try
            {
                Console.WriteLine($"Persistent subscriptişng is creating for {projection}");
                
                eventStoreConnection.CreatePersistentSubscriptionAsync(projection.ToString(), Group, settings, userCredentials).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() != typeof (InvalidOperationException)
                    && ex.InnerException?.Message != $"Subscription group {Group} on stream {projection.ToString()} already exists")
                {
                    throw;
                }
            }
        }
        
    }
}