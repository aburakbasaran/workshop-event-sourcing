using System;

namespace Reviews.Domain
{
    public class History
    {
        public History(DateTime reviewAt,Guid reviewer,Status status)
        {
            ReviewAt = reviewAt;
            Reviewer = reviewer;
            Action = status;
        }
        public Guid Reviewer { get; }
        public DateTime ReviewAt { get; }
        public Status Action { get; }
    }
}