using System;
using Reviews.Core;

namespace Reviews.Domain
{
    public class ProductId : Identity
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