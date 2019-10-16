using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Reviews.Core;
using Reviews.Service.WebApi.Modules.Reviews;
using Xunit;
using Xunit.Abstractions;

namespace Reviews.Domain.Test
{
    public class create_review : Spesification<Review,Contracts.Reviews.V1.ReviewCreate>
    {
        public create_review(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public readonly Fixture AutoFixture = new Fixture();


        [Fact]
        public void review_is_created()
        {
            RaisedEvents.Should().BeEquivalentTo(new Events.V1.ReviewCreated
            {
                Id = Command.Id,
                Caption = Command.Caption,
                Content = Command.Content,
                Owner = Command.Owner,
                ProductId = Command.ProductId
            });
        }

        public override object[] Given()=> new object[0];
        public override Contracts.Reviews.V1.ReviewCreate When() => AutoFixture.Create<Contracts.Reviews.V1.ReviewCreate>();
        
        public override Func<Contracts.Reviews.V1.ReviewCreate, Task> GetHandler(SpecificationAggregateStore store)
        {
            return cmd => new ApplicationService(store).Handle(cmd);
        }
    }
}
