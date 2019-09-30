using System;
using System.Collections.Generic;

namespace Reviews.Core
{
    public static class EventTypeMapper
    {
        private static readonly Dictionary<string,Type> typeByName     = new Dictionary<string, Type>();
        private static readonly Dictionary<Type,string> nameByTypes     = new Dictionary<Type, string>();

        private static void Map(Type type, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = type.FullName;
            
            if(typeByName.ContainsKey(name))
                throw new InvalidOperationException($"'{type}' is already mapped with name: '{name}'");

            typeByName[name] = type;
            nameByTypes[type] = name;
        }

        public static void Map<T>(string name) => Map(typeof(T), name);

        private static bool TryGetEventType(string name, out Type type) => typeByName.TryGetValue(name, out type);
        private static bool TryGetEventName(Type type,out string name) => nameByTypes.TryGetValue(type, out name);
        
        public static string GetTypeName(Type type)
        {
            if (!TryGetEventName(type, out var name))
                throw new Exception($"Failed to find name mapped with '{type}'");

            return name;
        }

        public static Type GetType(string name)
        {
            if (!TryGetEventType(name, out var type))
                throw new Exception($"Failed to find type mapped with '{name}'");

            return type;
        }
    }

   
    
    public class InvalidExpectedStreamVersionException : Exception
    {
        public InvalidExpectedStreamVersionException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}