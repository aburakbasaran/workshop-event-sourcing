using System;

namespace Reviews.Contracts
{
    public static class Reviews
    {
        public static class V1
        {
            /// <summary>
            /// Create a new review
            /// </summary>
            public class ReviewCreate
            {
                /// <summary>
                /// New Review Id
                /// </summary>
                public Guid Id { get; set; }
                
                /// <summary>
                /// Review caption
                /// </summary>
                public string Caption { get; set; }
                /// <summary>
                /// Review content
                /// </summary>
                public string Content { get; set; }
                /// <summary>
                /// creater of review
                /// </summary>
                public Guid Owner { get;  set; }

                public Guid ProductId { get; set; }
            }

            public class ReviewApprove
            {
                /// <summary>
                /// New Review Id
                /// </summary>
                public Guid Id { get; set; }
                /// <summary>
                /// creater of Reviewer
                /// </summary>
                public Guid Reviewer { get;  set; }
                
            }

            public class UpdateReview
            {
                /// <summary>
                /// New Review Id
                /// </summary>
                public Guid Id { get; set; }
                
                public string Caption { get; set; }
                
                public string Content { get; set; }
                
                public DateTime ChangedAt { get; set; }
            }

            public class ReviewPublish
            {
                public Guid Id { get; set; }
            }
        }
    }
}
