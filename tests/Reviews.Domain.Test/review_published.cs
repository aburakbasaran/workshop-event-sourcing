using System;
using System.Collections.Generic;
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
    public class review_published : Spesification<Review,Contracts.Reviews.V1.ReviewPublish>
    {
        public review_published(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public readonly Fixture AutoFixture = new Fixture();


        [Fact]
        public void review_content_is_published()
        {
            var expected = new List<Events.V1.ReviewPublished>
            {
                new Events.V1.ReviewPublished {Id = ReviewId, ChangedAt = ChangedAt}
            };

            RaisedEvents.Should().BeEquivalentTo(expected,
                o => o.ExcludingFields().Excluding(q=>q.ChangedAt));
        }

        private Guid ReviewId { get; } = Guid.NewGuid();
        private Guid OwnerId { get; } = Guid.NewGuid();
        private DateTime ChangedAt { get; } = DateTime.UtcNow;
        
        public override object[] Given() => AutoFixture.Build<Events.V1.ReviewCreated>()
            .With(e => e.Id, ReviewId)
            .With(e=>e.Owner,OwnerId)
            .With(e=> e.Caption,"First Review")
            .With(e=> e.Content, "This is my first review.")
            .CreateMany(1)
            .ToArray();

        public override Contracts.Reviews.V1.ReviewPublish When() => AutoFixture
            .Build<Contracts.Reviews.V1.ReviewPublish>()
            .With(e => e.Id, ReviewId)
            .Create();
        
        
        public override Func<Contracts.Reviews.V1.ReviewPublish, Task> GetHandler(SpecificationAggregateStore store)
        {
            return cmd => new ApplicationService(store).Handle(cmd);
        }

       
    }
}