using System;

namespace Reviews.Domain.Events.V1
{
    public class ReviewCreated
    {
        public Guid Id { set; get; }
        public string Caption { get; set; }
        public string Content { get; set; }
        public Guid Owner { get;  set; }
        public Guid ProductId { get; set; }

        public override string ToString() => $"Review {Id} created by {Owner}";      
    }
}