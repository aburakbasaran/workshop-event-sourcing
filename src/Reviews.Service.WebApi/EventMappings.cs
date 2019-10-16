using static Reviews.Core.EventTypeMapper;

namespace Reviews.Service.WebApi
{
    public static class EventMappings
    {
        public static void MapEventTypes()
        {
            Map<Domain.Events.V1.ReviewCreated>("reviewCreated");
        }
    }
}