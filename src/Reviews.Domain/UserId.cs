using System;
using Reviews.Core;

namespace Reviews.Domain
{
    public class UserId : Value<UserId>
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
    
    public class ProductId : Value<ProductId>
    {
        private readonly Guid value; 
        
        public ProductId(Guid value)
        {
            this.value = value;
        }
        
        public static implicit operator Guid(ProductId self) => self.value;

        public static implicit operator ProductId(Guid value) => new ProductId(value);

        public override string ToString() => value.ToString();

    }
}