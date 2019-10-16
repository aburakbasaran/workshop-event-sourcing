using System;
using System.Collections.Generic;

namespace Reviews.Service.Contract
{
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