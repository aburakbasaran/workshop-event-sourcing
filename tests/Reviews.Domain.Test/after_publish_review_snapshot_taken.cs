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
    public class after_publish_review_snapshot_taken : Spesification<Review,Contracts.Reviews.V1.ReviewApprove>
    {

        public after_publish_review_snapshot_taken(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public readonly Fixture AutoFixture = new Fixture();


        [Fact]
        public void review_snapshot_is_taken()
        {
            Snapshots.Should()
                .BeEquivalentTo(new ReviewSnapshot(Guid.Empty, ReviewId,1,
                        NewCaption,NewContent,Status.Approved,ProductId),
                o => o.ExcludingFields().Excluding(q=>q.Id)
                );
            
        }
        private Guid ReviewId { get; } = Guid.NewGuid();
        private Guid OwnerId { get; } = Guid.NewGuid();
        private string NewCaption { get; } = "Changed subjects";
        private string NewContent { get; } = "This is my first review...";
        private DateTime ChangedAt { get; } = DateTime.UtcNow;
        private Guid Reviewer { get; } = Guid.NewGuid();
        private Guid ProductId { get; } = Guid.NewGuid();

        public override object[] Given()
        {
            var o = new object[2];
            o[0]= AutoFixture.Build<Events.V1.ReviewCreated>()
                .With(e => e.Id, ReviewId)
                .With(e=>e.Owner,OwnerId)
                .With(e=> e.Caption,NewCaption)
                .With(e=> e.Content, NewContent)
                .With(f=>f.ProductId,ProductId)
                .Create();
            
            o[1]=AutoFixture.Build<Events.V1.ReviewPublished>()
                .With(e => e.Id, ReviewId)
                .With(e=>e.ChangedAt,ChangedAt)
                .Create();
            
            return o;
        } 

        //When the approve command fired, snapshot will be taken
        public override Contracts.Reviews.V1.ReviewApprove When() => AutoFixture.Build<Contracts.Reviews.V1.ReviewApprove>()
            .With(e => e.Id, ReviewId)
            .With(e=>e.Reviewer,Reviewer)
            .Create();
        
        
        
        public override Func<Contracts.Reviews.V1.ReviewApprove, Task> GetHandler(SpecificationAggregateStore store,SpesificationAggregateSnapshotStore snapshotStore)
        {
            return cmd => new ApplicationService(new Repository(store,snapshotStore)).Handle(cmd);
        }

       
    }
}