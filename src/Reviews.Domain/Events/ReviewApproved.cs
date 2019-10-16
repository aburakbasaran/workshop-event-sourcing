using System;

namespace Reviews.Domain.Events.V1
{
    public class ReviewApproved
    {
        public Guid Id { get; set; }
        public Guid ReviewBy { get; set; }
        public Guid OwnerId { get; set; }
        public string Caption { get; set; }
        public string Content { get; set; }
        public Guid ProductId { get; set; }

        public DateTime ReviewAt { get; set; }
    }
}