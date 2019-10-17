using System.Collections.Generic;

namespace Reviews.Service.Contract
{
    public class ReviewsByProductDocument
    {
        public string Id { get; set; }
        public IList<ReviewDocument> ListOfReviews { get; set; }
    }
}