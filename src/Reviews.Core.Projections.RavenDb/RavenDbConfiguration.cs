using System;
using System.Reflection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Reviews.Core.Projections.RavenDb
{
    public static class RavenDbConfiguration
    {
        public static IDocumentStore Build(string url,string database)
        {
            var store = new DocumentStore {
                Urls     = new[] {url},
                Database = database
            };

            try 
            {
                store.Initialize();                
                Console.WriteLine($"Connection to {store.Urls[0]} document store established.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to establish connection to \"{store.Urls[0]}\" document store!" +
                    $"Please check if https is properly configured in order to use the certificate.", ex);
            }
            try
            {
                var record = store.Maintenance.Server.Send(new GetDatabaseRecordOperation(store.Database));
                if (record == null) 
                {
                    store.Maintenance.Server
                        .Send(new CreateDatabaseOperation(new DatabaseRecord(store.Database)));

                    Console.WriteLine($"{store.Database} document store database created.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to ensure that \"{store.Database}\" document store database exists!", ex);
            }
            
            try
            {
                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);
                Console.WriteLine($"{store.Database} document store database indexes created or updated.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to create or update \"{store.Database}\" document store database indexes!", ex);
            }
            
            return store;
            
        }
    }
}