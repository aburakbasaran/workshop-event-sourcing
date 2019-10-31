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
    public class ReviewsByProducts : Projection
    {
        private static string DocumentId(Guid id) => $"ReviewsByProducts/{id}";
        private readonly Func<IDocumentSession> getSession;

        public ReviewsByProducts(Func<IDocumentSession> session)=> getSession = session;

        public override async Task Handle(object e)
        {
            using (var session = getSession())
            {
                switch (e)
                {
                    case Domain.Events.V1.ReviewCreated ev:

                        var documentId = DocumentId(ev.ProductId);
                        var document = session.Load<ReviewsByProductDocument>(documentId);

                        if (document == null)
                        {
                            document = new ReviewsByProductDocument
                            {
                                Id = documentId,
                                ListOfReviews = new List<ReviewDocument>()
                            };
                            session.Store(document);
                        }
                        
                        document.ListOfReviews.Add(new ReviewDocument
                        {
                            Id = ev.Id,
                            Caption = ev.Caption,
                            Content = ev.Content,
                            Status = "Draft",
                            OwnerId = ev.Owner.ToString()
                        });
                        break;
                    
                    case Domain.Events.V1.ReviewApproved ev:

                        await session.Update<ReviewsByProductDocument>(DocumentId(ev.ProductId), doc =>
                        {
                            var review = doc.ListOfReviews.First<ReviewDocument>(q => q.Id == ev.Id);
                            review.Status = "Approved";
                        });
                        break;
                    case Domain.Events.V1.CaptionAndContentChanged ev:

                        await session.Update<ReviewsByProductDocument>(DocumentId(ev.ProductId), doc =>
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