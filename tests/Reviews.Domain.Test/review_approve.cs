using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Reviews.Core;
using Reviews.Service.WebApi.Modules.Reviews;
using Xunit;
using Xunit.Abstractions;

namespace Reviews.Domain.Test
{
    public class review_approve : Spesification<Review,Contracts.Reviews.V1.ReviewApprove>
    {
        public review_approve(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public readonly Fixture AutoFixture = new Fixture();


        [Fact]
        public void review_content_is_approved()
        {
            var expected = new List<Events.V1.ReviewApproved>
            {
                new Events.V1.ReviewApproved
                {
                    Id = ReviewId,
                    ReviewBy = Reviewer,
                    OwnerId = OwnerId,
                    Caption = Caption,
                    Content = Content,
                    ProductId = ProductId
                }
            };

            RaisedEvents.Should().BeEquivalentTo(expected,
                o => o.ExcludingFields()
                    .Excluding(q=>q.Caption)
                    .Excluding(e=>e.ReviewAt)
            );
        }

        private Guid ReviewId { get; } = Guid.NewGuid();
        private Guid OwnerId { get; } = Guid.NewGuid();
        private Guid ProductId { get; } = Guid.NewGuid();
        private Guid Reviewer { get; } = Guid.NewGuid();
        private string Caption { get; } = "Changed subjects";
        private string Content { get; } = "This is my first review...";

        public override object[] Given()
        {
            var o = new object[2];
            o[0]= AutoFixture.Build<Events.V1.ReviewCreated>()
                .With(e => e.Id, ReviewId)
                .With(e=>e.Owner,OwnerId)
                .With(e=>e.Caption,Caption)
                .With(e=>e.Content,Content)
                .With(e=>e.ProductId,ProductId)
                .Create();
            
            o[1]=AutoFixture.Build<Events.V1.ReviewPublished>()
                .With(e => e.Id, ReviewId)
                .Create();

            return o;
        }

        public override Contracts.Reviews.V1.ReviewApprove When() => AutoFixture
            .Build<Contracts.Reviews.V1.ReviewApprove>()
            .With(e => e.Id, ReviewId)
            .With(e=>e.Reviewer,Reviewer)
            .Create();
        
        
        public override Func<Contracts.Reviews.V1.ReviewApprove, Task> GetHandler(SpecificationAggregateStore store,SpesificationAggregateSnapshotStore snapshotStore)
        {
            return cmd => new ApplicationService(new Repository(store,null)).Handle(cmd);
        }

       
    }
}