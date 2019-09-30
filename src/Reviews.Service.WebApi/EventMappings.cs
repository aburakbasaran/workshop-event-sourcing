using static Reviews.Core.EventTypeMapper;

namespace Reviews.Service.WebApi
{
    public static class EventMappings
    {
        public static void MapEventTypes()
        {
            Map<Domain.Events.V1.ReviewCreated>("reviewCreated");
            Map<Domain.Events.V1.CaptionAndContentChanged>("reviewUpdated");
            Map<Domain.Events.V1.ReviewPublished>("reviewPublished");
            Map<Domain.Events.V1.ReviewApproved>("reviewApproved");
            
            //Snapshot
            Map<Domain.ReviewSnapshot>("reviewSnapshot");
        }
    }
}