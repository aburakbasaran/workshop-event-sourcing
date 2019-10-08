using System;

namespace Reviews.Domain.Events.V1
{
    public class CaptionAndContentChanged
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string Content { get; set; }
        public DateTime ChangedAt { get; set; }
        public Guid Owner { get; set; }
        public Guid ProductId { get; set; }
    }
}