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
        
        private static string ProductDocumentId(string id) => $"ReviewsByProducts/{id}";
        
        private static string ReviewOwnerDocumentId(string id) => $"ReviewsByOwner/{id}";

        
        private readonly Func<IAsyncDocumentSession> getSession;

        public QueryService(Func<IAsyncDocumentSession> session) => getSession = session;

        public Task<List<ReviewsByOwnerDocument>> GetAllActiveReviewDocuments(string user_id)
        {
            var session = getSession();

            return session.Query<ReviewsByOwnerDocument>()
                .Where(q => q.Id == ReviewOwnerDocumentId(user_id)).ToListAsync();

        }

        public Task<ActiveReviewDocument> GetActiveReviewById(string id)
        {
            var session = getSession();

            return session.Query<ActiveReviewDocument>().Where(q => q.Id == DocumentId(id)).FirstAsync();
        }
        
        public Task<ReviewsByProductDocument> GetReviewsByProductId(string id)
        {
            var session = getSession();

            return session.Query<ReviewsByProductDocument>().Where(q => q.Id == ProductDocumentId(id)).FirstAsync();
        }
    }
}
