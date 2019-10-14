using System;
using Reviews.Core;

namespace Reviews.Domain
{
    public class UserId : Identity
    {
        private readonly Guid value; 
        
        public UserId(Guid value)
        {
            this.value = value;
        }
        
        public static implicit operator Guid(UserId self) => self.value;

        public static implicit operator UserId(Guid value) => new UserId(value);

        public override string ToString() => value.ToString();

    }
}