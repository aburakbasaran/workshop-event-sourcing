using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Reviews.Core.Projections.RavenDb
{
    public class RavenDbCheckPointStore: ICheckpointStore
    {
        private readonly Func<IAsyncDocumentSession> getSession;
        
        public RavenDbCheckPointStore(Func<IAsyncDocumentSession> session)
        {
            getSession = session;
        }
        
        public async Task<T> GetLastCheckpoint<T>(string projection)
        {
            using (var session = getSession())
            {
                var document = await session.LoadAsync<CheckpointDocument>(GetCheckpointDocumentId(projection));

                if (document == null) return default;
                
                var checkpoint = (T) document.Checkpoint;
                return checkpoint;
            }
        }

        public async Task SetCheckpoint<T>(T checkpoint, string projection)
        {
            using (var session = getSession())
            {
                var docId = GetCheckpointDocumentId(projection);
                
                var document = await session.LoadAsync<CheckpointDocument>(docId);

                
                if (document != null)
                {
                    //change the checkpoint
                    document.Checkpoint = checkpoint;
                }
                else
                {
                    // add new checkpoint document
                    await session.StoreAsync(new CheckpointDocument
                    {
                        Checkpoint = checkpoint
                    },docId);
                }

                await session.SaveChangesAsync();
            }
        }
        
        private static string GetCheckpointDocumentId(string projection) => $"checkpoints/{projection.ToLowerInvariant()}";
    }
}
