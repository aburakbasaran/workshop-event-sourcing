﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;

namespace Reviews.Core.EventStore
{
    public delegate string GetStreamName(Type aggregateType, string aggregateId);
    
    public class GesAggrigateStore : IAggrigateStore
    {
        private const int MaximumReadSize = 4096;
        
        
        
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly ISerializer serializer;
        private readonly EventTypeMapper eventTypeMapper;
        private readonly GetStreamName getStreamName;
        
        private readonly UserCredentials userCredentials;
        
        
        public GesAggrigateStore(IEventStoreConnection eventStoreConnection, 
                                ISerializer serializer,
                                EventTypeMapper eventTypeMapper,
                                GetStreamName getStreamName,
                                UserCredentials userCredentials=null)
        {
            this.eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));;
            this.serializer = serializer ?? throw new ArgumentException(nameof(serializer));
            this.eventTypeMapper = eventTypeMapper ?? throw new ArgumentException(nameof(eventTypeMapper));
            this.getStreamName = getStreamName ?? throw new ArgumentException(nameof(getStreamName));

            this.userCredentials = userCredentials;
        }
        
        public async Task<(long NextExceptedVersion, long LastPosition, long CommitPosition)> Save<T>(T aggregate, CancellationToken cancellationToken = default) where T : Aggregate
        {
             if(aggregate==null)
                 throw new ArgumentException($"Aggregate null or empty :",nameof(aggregate));

            var changes = aggregate.GetChanges().Select(c => new EventData(
                    Guid.NewGuid(), 
                    eventTypeMapper.GetEventName(c.GetType()),
                    serializer.IsJsonSerializer,
                    serializer.Serialize(c),
                null));

            if (!changes.Any())
            {
                Console.WriteLine($"{aggregate.Id}-V{aggregate.Version} aggregate has no changes.");
                return default;
            }

            var stream = getStreamName(typeof(T), aggregate.Id.ToString());
            
            WriteResult result;
            try
            {
                result = await eventStoreConnection.AppendToStreamAsync(stream, aggregate.Version, changes, userCredentials);
            }
            catch (WrongExpectedVersionException ex)
            {
                var chunk = await eventStoreConnection.ReadStreamEventsBackwardAsync(stream, -1, 1,false, userCredentials);
                
                throw new InvalidExpectedStreamVersionException(
                    $"Failed to appand stream {stream} with expected version {aggregate.Version}. " +
                    $"{(chunk.Status == SliceReadStatus.StreamNotFound ? "Stream not found!" : $"Current Version: {chunk.LastEventNumber}")}");
            }
           
            return (result.NextExpectedVersion, result.LogPosition.CommitPosition, result.LogPosition.PreparePosition);
        }

        public async Task<T> Load<T>(string aggregateId, CancellationToken cancellationToken = default) where T : Aggregate,new()
        {
            if(string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("Value cannot be null or whitrespace",nameof(aggregateId));

            var stream = getStreamName(typeof(T), aggregateId);
            var aggregate = new T();
            
            var nextPageStart = 0L;
            do
            {
                //Get data from event store
                var chunk =  await eventStoreConnection.ReadStreamEventsForwardAsync(stream, nextPageStart,MaximumReadSize, false, userCredentials);
                
                //Build your aggregate
                aggregate.Load(chunk.Events.Select(e=> serializer.Deserialize(e.Event.Data,eventTypeMapper.GetEventType(e.Event.EventType))).ToArray());

                //check is there any other events ?
                nextPageStart = !chunk.IsEndOfStream ? chunk.NextEventNumber : -1;

            } while (nextPageStart !=-1);

            return aggregate;

        }
    }
}
