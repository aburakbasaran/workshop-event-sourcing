using System;

namespace Reviews.Domain
{
    public class ReviewNotFoundException : Exception
    {
        public ReviewNotFoundException(Guid id) : base($"Review with id '{id}' was not found") { }
    }
    
    public class InvalidEntityStateException : Exception
    {
        public InvalidEntityStateException(object entity, string message)
            : base($"Entity {entity.GetType().Name} state change rejected, {message}")
        {
        }
    }
}