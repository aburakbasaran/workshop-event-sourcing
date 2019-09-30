using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Reviews.Core.EventStore
{
    public static class EventSerializer
    {
        public static bool IsJsonSerializer => true;
        public static object Deserialze(this ResolvedEvent resolvedEvent)
        {
            var dataType = EventTypeMapper.GetType(resolvedEvent.Event.EventType);
            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            var data = JsonConvert.DeserializeObject(jsonData, dataType);
            return data;
        }

        public static T Deserialze<T>(this ResolvedEvent resolvedEvent)
        {
            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
        
        public static byte[] Serialize(object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
    }
}