﻿using System;
using System.Collections.Generic;
using Reviews.Core;
using Reviews.Domain.Events.V1;

namespace Reviews.Domain
{
    public class Review :Aggregate
    {
        public string Caption { get; private set; }
        public string Content { get; private set; }
        public Status CurrentStatus { get; private set; }
        public Guid Owner { get; private set; }

        public ProductId ProductId { get; set; }
        private IList<History> History {get; set; } = new List<History>();

        public IList<History> GetHistory() => History;
        
        protected override void When(object e)
        {
            switch (e)
            {
                case Events.V1.ReviewCreated x:
                    Id = x.Id;
                    Caption = x.Caption;
                    Content = x.Content;
                    CurrentStatus = Status.Draft;
                    Owner = x.Owner;
                    ProductId = x.ProductId;
                    break;
                
                case Events.V1.ReviewPublished x:
                    CurrentStatus = Status.PendingApprove;
                    break;
                
                case Events.V1.ReviewApproved x:
                    CurrentStatus = Status.Approved;
                    History.Add(new History(x.ReviewAt,x.ReviewBy,Status.Approved));
                    break;
                
                case Events.V1.CaptionAndContentChanged x:
                    Caption = x.Caption;
                    Content = x.Content;
                    CurrentStatus = Status.Draft;
                    
                    if(CurrentStatus == Status.Approved || CurrentStatus==Status.Rejected)
                        CurrentStatus = Status.PendingApprove;
                    
                    break;
                
            } 
        }

        protected override void EnsureValidState()
        {
            var valid = Id != null && Owner != null && ProductId!=null;
            switch (CurrentStatus)
            {
                case Status.Draft:
                    valid = valid
                            && Caption != null
                            && Content != null;
                    break;
                
                case Status.Approved:
                    valid = valid
                            && Caption != null
                            && Content != null
                            && History?.Count > 0;
                    break;
                case Status.PendingApprove:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!valid)
                throw new InvalidEntityStateException(this, $"Post-checks failed in state {CurrentStatus}");
        }

        public static Review Create(Guid id,UserId ownerId,ProductId productId, string caption, string context)
        {
            var review = new Review();
            
            review.Apple(new Events.V1.ReviewCreated
            {
                Id = id,
                Caption = caption,
                Content = context,
                Owner = ownerId,
                ProductId =productId
                
            });
            return review;
        }

        public void Publish()
        {
            if (Version == -1)
            {
                throw new ReviewNotFoundException(Id);
            }

            if (CurrentStatus == Status.Draft || CurrentStatus == Status.Rejected)
            {
                Apple(new ReviewPublished
                {
                    Id = Id,
                    ChangedAt = DateTime.UtcNow
                });
            }
        }

        public void Approve(UserId reviewBy, DateTime reviewAt)
        {
            if (Version == -1)
                throw new ReviewNotFoundException(Id);

            if (CurrentStatus != Status.PendingApprove)
            {
                throw new Exception($"you can't approve thats. Review : {Id}-V:{Version}  Status:{CurrentStatus}");
            }
            
            Apple(new Events.V1.ReviewApproved
            {
                Id = Id,
                ReviewBy = reviewBy,
                ReviewAt = reviewAt,
                Caption = Caption,
                Content = Content,
                OwnerId = Owner,
                ProductId = ProductId
            });
        }

        public void UpdateCaptionAndContent(string caption, string content,DateTime changedAt)
        {
            if (Version == -1)
                throw new ReviewNotFoundException(Id);
            
            Apple(new Events.V1.CaptionAndContentChanged
            {
                Id=Id,
                Caption=caption,
                Content=content,
                ChangedAt=changedAt,
                Owner = Owner,
                ProductId = ProductId
            });
        }
    }
}

