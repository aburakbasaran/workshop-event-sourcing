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


        public Task Handle(Contracts.Reviews.V1.ReviewApprove command) =>
            HandleForUpdate(command.Id, (r) => r.Approve(command.Reviewer,DateTime.UtcNow));
        public async Task Handle(Contracts.Reviews.V1.UpdateReview command) =>
            HandleForUpdate(command.Id, r => r.UpdateCaptionAndContent(command.Caption, command.Content,command.ChangedAt));

        
        
        public Task Handle(Contracts.Reviews.V1.ReviewPublish command) =>
            HandleForUpdate(command.Id, (r) => r.Publish());
        private async Task HandleForUpdate(Guid aggregateId, Action<Domain.Review> handle)
        {
            var aggregate = await aggregateStore.Load<Domain.Review>(aggregateId.ToString());
            handle(aggregate);
            await aggregateStore.Save(aggregate);
        }
        
    }
}
