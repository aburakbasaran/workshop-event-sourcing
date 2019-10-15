using System;
using System.Threading.Tasks;
using AutoFixture;
using EventStore.ClientAPI;
using FluentAssertions;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Xunit;

namespace Reviews.Core.Projections.RavenDb.Test
{
    public class RavenCheckpointStoreTests :IDisposable
    {
        
        private Func<IAsyncDocumentSession> GetDocumentSession { get; }
        private Fixture AutoFixture { get; }
        
        public RavenCheckpointStoreTests()
        {
            AutoFixture = new Fixture();
            GetDocumentSession = () => LazyStore.Value.OpenAsyncSession();
        }
        
        [Fact]
        public async Task can_set_and_get_checkpoint()
        {
            
            //can set checkpoint
            //Given
            var sut = new RavenDbCheckPointStore(GetDocumentSession);
            var projection = AutoFixture.Create<string>();
            var expectedCheckpoint = new Position();

            //When
            Func<Task> setcheckpoint = ()=> sut.SetCheckpoint(expectedCheckpoint, projection);

            var checkpoint = await sut.GetLastCheckpoint<Position>(projection);
            
            //Then
            setcheckpoint.Should().NotThrow();
            checkpoint.Should().BeEquivalentTo(expectedCheckpoint);

        }

        public void Dispose()
        {
            LazyStore.Value?.Dispose();
        }
        
        //you may create database via ravenDb UI http://localhost:8080
        private static readonly Lazy<IDocumentStore> LazyStore = new Lazy<IDocumentStore>(() =>
        {
            var store = new DocumentStore {
                Urls     = new[] {"http://localhost:8080"},
                Database = "Reviews"
            };
            
            var idocumentStore= store.Initialize();

            try
            {
                idocumentStore = store.Initialize();
                return idocumentStore;
            }
            catch (DatabaseDoesNotExistException ex)
            {
                try
                {
                    var record = store.Maintenance.Server.Send(new GetDatabaseRecordOperation(store.Database));
                    if (record == null) 
                    {
                        store.Maintenance.Server
                            .Send(new CreateDatabaseOperation(new DatabaseRecord(store.Database)));

                    }

                    return idocumentStore;
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"Failed to ensure that \"{store.Database}\" document store database exists!", ex);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to establish connection to \"{store.Urls[0]}\" document store!" +
                    $"Please check if https is properly configured in order to use the certificate.", ex);
            }

        });
    }
}
