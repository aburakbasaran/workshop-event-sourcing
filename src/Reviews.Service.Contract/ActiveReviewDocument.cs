using System;

namespace Reviews.Service.Contract
{
    public class ActiveReviewDocument
    {
        public string Id { get; set; }
        
        public string Caption { get; set; }

        public string Content { get; set; }

        public string Owner { get; set; }
        
        public string ReviewBy { get;  set; }
        
        public DateTime ReviewAt { get; set; }

        public string ProductId { get; set; }

    }
}