using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Reviews.Service.Contract;

namespace Reviews.Service.QueryApi.Modules.Reviews
{
    public class QueryService
    {

        private static string DocumentId(string id) => $"ActiveReviews/{id}";
        
        private readonly Func<IAsyncDocumentSession> getSession;

        public QueryService(Func<IAsyncDocumentSession> session) => getSession = session;

        public Task<List<ActiveReviewDocument>> GetAllActiveReviewDocuments(string user_id)
        {
            var session = getSession();
            
            return session.Query<ActiveReviewDocument>().Where(q=>q.Owner==user_id).ToListAsync();
            
        }

        public Task<ActiveReviewDocument> GetActiveReviewById(string id)
        {
            var session = getSession();

            return session.Query<ActiveReviewDocument>().Where(q => q.Id == DocumentId(id)).FirstAsync();
        }
    }
}
