using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Reviews.Core.Projections.RavenDb
{
    public class RavenDbCheckPointStore: ICheckpointStore
    {
        private readonly Func<IDocumentSession> getSession;
        
        public RavenDbCheckPointStore(Func<IDocumentSession> session)
        {
            getSession = session;
        }
        
        public async Task<T> GetLastCheckpoint<T>(string projection)
        {
            using (var session = getSession())
            {
                var document = session.Load<CheckpointDocument>(GetCheckpointDocumentId(projection));

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
                
                var document = session.Load<CheckpointDocument>(docId);

                
                if (document != null)
                {
                    //change the checkpoint
                    document.Checkpoint = checkpoint;
                }
                else
                {
                    // add new checkpoint document
                    session.Store(new CheckpointDocument
                    {
                        Checkpoint = checkpoint
                    },docId);
                }

                session.SaveChanges();
            }
        }
        
        private static string GetCheckpointDocumentId(string projection) => $"checkpoints/{projection.ToLowerInvariant()}";
    }
}
