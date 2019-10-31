using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Documents;

namespace Reviews.Core.Projections.RavenDb
{
    public static class DocumentSessionExtentions
    {
        public static void Update<T>(this IDocumentSession session,string id,Action<T> handle,bool isThrowNeeded=false)
        {
            var doc = session.Load<T>(id);
            
            if (isThrowNeeded && doc == null)
                throw new DocumentDoesNotExistException(typeof(T).Name, id);

            if (doc != null)
            {
                handle(doc);
            }
            else
            {
                Console.WriteLine($"document {typeof(T).Name}-{id} not found..");
            }
        }
    }
}