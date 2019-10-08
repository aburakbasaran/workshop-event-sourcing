using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Reviews.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Reviews.Core.EventStore.Tests
{
    public class GesSnapshotStoreTest : GesBaseTest
    {
        private readonly ITestOutputHelper outputHelper;

        public GesSnapshotStoreTest(ITestOutputHelper outputHelper):base()
        {
            this.outputHelper = outputHelper;
        }
        
        [Fact]
        public async Task can_take_snapshot()
        {
            //Given
            var aggregate = new Reviews.Domain.Review();
            aggregate.Apple(AutoFixture.Create<Domain.Events.V1.ReviewCreated>());
            aggregate.Apple(AutoFixture.Create<Domain.Events.V1.ReviewApproved>());
            
            var sut = new GesSnapshotStore(Connection, (a, b) => $"{a}-{b}", null);
            
            //When
            var result =await  sut.SaveSnapshotAsync(aggregate.TakeSnapshot());

            //Then
            outputHelper.WriteLine($"Snapshot result last position:{result}");
            result.Should().BeGreaterThan(0);
        }
        
        private Guid AggregateId { get; } = Guid.NewGuid();
        private Guid ProductId { get; } = Guid.NewGuid();
        private string Caption { get; } = "Test Review";
        private string Content { get; } = "Test content";
        
        [Fact]
        public async Task can_get_snapshot()
        {
            //Given
            var aggregate = new Reviews.Domain.Review();
            aggregate.Apple(AutoFixture.Build<Domain.Events.V1.ReviewCreated>()
                .With(e=>e.Caption,Caption)
                .With(e=>e.Content,Content)
                .With(e=>e.ProductId,ProductId)
                .With(e=>e.Id,AggregateId).Create());
            aggregate.Apple(AutoFixture.Build<Domain.Events.V1.ReviewPublished>().With(e=>e.Id,AggregateId).Create());
            aggregate.Apple(AutoFixture.Build<Domain.Events.V1.ReviewApproved>().With(e=>e.Id,AggregateId).Create());
            
            var sut = new GesSnapshotStore(Connection, (a, b) => $"{a}-{b}", null);
            var snap = aggregate.TakeSnapshot();
            await  sut.SaveSnapshotAsync(snap); 
            
            //When
            var result = await sut.GetSnapshotAsync<Review>(typeof(ReviewSnapshot), AggregateId);

            //Then
            outputHelper.WriteLine($"Snapshot result last Version:{result.Version}");
            result.Should().BeEquivalentTo(new ReviewSnapshot(Guid.Empty, AggregateId,-1,Caption,Content,Status.Approved,ProductId),
                o => o.ExcludingFields().Excluding(q=>q.Id).Excluding(q=>q.CurrentStatus).Excluding(q=>q.ProductId));
               
        }
    }
    
}