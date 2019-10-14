using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AutoFixture;
using Xunit;
using FluentAssertions;
using FluentAssertions.Formatting;

namespace Reviews.Core.EventStore.Tests
{
    public class GesAggregateStoreTest : GesBaseTest
    {
        [Fact]
        public async Task can_save_aggregate()
        {
            var aggregate = new Reviews.Domain.Review();

            aggregate.Apple(AutoFixture.Create<Domain.Events.V1.ReviewCreated>());
            aggregate.Apple(AutoFixture.Create<Domain.Events.V1.ReviewApproved>());

            var sut = new GesAggregateStore(Connection, null);

            var result = await sut.Save(aggregate);
            
            result.NextExceptedVersion.Should().Be(1);
        }
        
        private Guid AggregateId { get; } = Guid.NewGuid();
        
        [Fact]
        public async Task can_load_aggregate()
        {
            var aggregate = new Reviews.Domain.Review();

            aggregate.Apple(AutoFixture.Build<Domain.Events.V1.ReviewCreated>().With(e=>e.Id,AggregateId).Create());
            aggregate.Apple(AutoFixture.Build<Domain.Events.V1.ReviewApproved>().With(e=>e.Id,AggregateId).Create());

           
            var sut = new GesAggregateStore(Connection, null);
            
            var saveResult = await sut.Save(aggregate);
            
            var result = await sut.Load<Domain.Review>(AggregateId.ToString());

            result.Id.Should().Be(AggregateId);
        }
    }
}
