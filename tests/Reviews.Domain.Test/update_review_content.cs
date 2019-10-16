using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Reviews.Core;
using Reviews.Service.WebApi.Modules.Reviews;
using Xunit;
using Xunit.Abstractions;

namespace Reviews.Domain.Test
{
    public class update_review_content : Spesification<Review,Contracts.Reviews.V1.UpdateReview>
    {
        public update_review_content(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public readonly Fixture AutoFixture = new Fixture();


        [Fact]
        public void review_content_is_updated()
        {
            RaisedEvents.Should().BeEquivalentTo(new Domain.Events.V1.CaptionAndContentChanged
            {
                Id = ReviewId,
                Caption = NewCaption,
                Content = NewContent,
                ChangedAt = ChangedAt,
                Owner = OwnerId,
                ProductId = ProductId
            });
        }
        private Guid ReviewId { get; } = Guid.NewGuid();
        private Guid OwnerId { get; } = Guid.NewGuid();
        private string NewCaption { get; } = "Changed subjects";
        private string NewContent { get; } = "This is my first review...";
        private DateTime ChangedAt { get; } = DateTime.UtcNow;
        public Guid ProductId { get; set; }
        
        public override object[] Given() => AutoFixture.Build<Events.V1.ReviewCreated>()
            .With(e => e.Id, ReviewId)
            .With(e=>e.Owner,OwnerId)
            .With(e=>e.ProductId,ProductId)
            .With(e=> e.Caption,"First Review")
            .With(e=> e.Content, "This is my first review.")
            .CreateMany(1)
            .ToArray();

        public override Contracts.Reviews.V1.UpdateReview When() => AutoFixture
            .Build<Contracts.Reviews.V1.UpdateReview>()
            .With(e => e.Id, ReviewId)
            .With(e => e.Caption, NewCaption)
            .With(e => e.Content, NewContent)
            .With(e=>e.ChangedAt,ChangedAt)
            .Create();
        
        
        public override Func<Contracts.Reviews.V1.UpdateReview, Task> GetHandler(SpecificationAggregateStore store )
        {
            return cmd => new ApplicationService(store).Handle(cmd);
        }

       
    }
}