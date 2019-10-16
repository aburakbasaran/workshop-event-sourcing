using System;
using System.Collections.Generic;
using Reviews.Core;

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
    }
}

