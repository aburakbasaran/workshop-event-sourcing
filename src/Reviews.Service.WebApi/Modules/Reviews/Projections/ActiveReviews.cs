﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Raven.Client.Documents.Session;
using Reviews.Core.Projections;
using Reviews.Service.Contract;

namespace Reviews.Service.WebApi.Modules.Reviews.Projections
{
    public class ActiveReviews : Projection
    {
        private readonly Func<IDocumentSession> getSession;

        public ActiveReviews(Func<IDocumentSession> session)=> getSession = session;
        
       
        public override async Task Handle(object e)
        {
            using (var session = getSession())
            {
                switch (e)
                {
                    case Domain.Events.V1.ReviewApproved view:
                        var document = new ActiveReviewDocument
                        {
                            Id = DocumentId(view.Id),
                            Caption = view.Caption,
                            Content = view.Content.ToUpper(),
                            Owner =  view.OwnerId.ToString(),
                            ReviewAt = view.ReviewAt,
                            ReviewBy = view.ReviewBy.ToString(),
                            ProductId = view.ProductId.ToString()
                        };
                        session.Store(document);
                        break;
                    
                    //need to review again,if published befores
                    case Domain.Events.V1.CaptionAndContentChanged view:
                        
                        session.Delete(DocumentId(view.Id));
                        break;
                }
                
                session.SaveChanges() ;
            }

        }
        
        private static string DocumentId(Guid id) => $"ActiveReviews/{id}";
    }
}
