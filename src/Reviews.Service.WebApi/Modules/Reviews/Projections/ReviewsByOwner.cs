using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Reviews.Core.Projections;
using Reviews.Core.Projections.RavenDb;
using Reviews.Service.Contract;

namespace Reviews.Service.WebApi.Modules.Reviews.Projections
{
    public class ReviewsByOwner : Projection
    {
        private static string DocumentId(Guid id) => $"ReviewsByOwner/{id}";
        private readonly Func<IDocumentSession> getSession;

        public ReviewsByOwner(Func<IDocumentSession> session)=> getSession = session;

        public override async Task Handle(object e)
        {
            using (var session = getSession())
            {
                switch (e)
                {
                    case Domain.Events.V1.ReviewCreated ev:

                        var documentId = DocumentId(ev.Owner);
                        var document = session.Load<ReviewsByOwnerDocument>(documentId);

                        if (document == null)
                        {
                            document = new ReviewsByOwnerDocument
                            {
                                Id = documentId,
                                ListOfReviews = new List<ReviewDocument>()
                            };
                            session.Store(document);
                        }
                        
                        document.ListOfReviews.Add(new ReviewDocument
                        {
                            Id = ev.Id,
                            Caption = ev.Caption.ToUpperInvariant(),
                            Content = ev.Content,
                            Status = "Draft",
                            ProductId = ev.ProductId.ToString(),
                        });
                        break;
                    
                    case Domain.Events.V1.ReviewApproved ev:

                        session.Update<ReviewsByOwnerDocument>(DocumentId(ev.OwnerId), doc =>
                        {
                            var review = doc.ListOfReviews.First(q => q.Id == ev.Id);
                            review.Status = "Approved";
                        });
                        break;
                    case Domain.Events.V1.CaptionAndContentChanged ev:

                        session.Update<ReviewsByOwnerDocument>(DocumentId(ev.Owner), doc =>
                        {
                            var review = doc.ListOfReviews.First(q => q.Id == ev.Id);
                            review.Content = ev.Content;
                            review.Caption = ev.Caption;
                            review.Status = "Draft";
                        });
                        break;
                }

                session.SaveChanges();
            }
        }

        
    }
}