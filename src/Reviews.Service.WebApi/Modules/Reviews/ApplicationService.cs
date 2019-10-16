using System;
using System.Threading.Tasks;
using Reviews.Core;
using Reviews.Domain;

namespace Reviews.Service.WebApi.Modules.Reviews
{
    public interface  IDomainService{}
    
    public class ApplicationService : IDomainService
    {
        private readonly IAggregateStore aggregateStore;

        public ApplicationService(IAggregateStore aggregateStore)
        {
            this.aggregateStore = aggregateStore;
        }

        public Task Handle(Contracts.Reviews.V1.ReviewCreate command) =>
            aggregateStore.Save(Domain.Review.Create(command.Id,command.Owner,command.ProductId,command.Caption,command.Content));

        private async Task HandleForUpdate(Guid aggregateId, Action<Domain.Review> handle)
        {
            var aggregate = await aggregateStore.Load<Domain.Review>(aggregateId.ToString());
            handle(aggregate);
            await aggregateStore.Save(aggregate);
        }
        
    }
}
