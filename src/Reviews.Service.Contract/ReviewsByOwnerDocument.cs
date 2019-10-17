using System;
using System.Collections.Generic;

namespace Reviews.Service.Contract
{
    public class ReviewsByOwnerDocument
    {
        public string Id { get; set; }
        public IList<ReviewDocument> ListOfReviews { get; set; }
        

    }
    public class ReviewDocument
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }

        public string ProductId { get; set; }
        
        public string OwnerId { get; set; }
    }
}