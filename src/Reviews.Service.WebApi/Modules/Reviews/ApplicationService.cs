using System;
using System.Threading.Tasks;
using Reviews.Core;
using Reviews.Domain;

namespace Reviews.Service.WebApi.Modules.Reviews
{
    public interface  IDomainService{}
    
    public class ApplicationService : IDomainService
    {
        private readonly IRepository repository;

        public ApplicationService(IRepository repository)
        {
            this.repository = repository;
        }

        public Task Handle(Contracts.Reviews.V1.ReviewCreate command) =>
            repository.SaveAsync(Domain.Review.Create(command.Id,command.Owner,command.ProductId,command.Caption,command.Content));

        public Task Handle(Contracts.Reviews.V1.ReviewApprove command) => 
            HandleForUpdate(command.Id, r => r.Approve(new UserId(command.Reviewer), DateTime.UtcNow));


        public async Task Handle(Contracts.Reviews.V1.UpdateReview command) =>
            HandleForUpdate(command.Id, r => r.UpdateCaptionAndContent(command.Caption, command.Content,command.ChangedAt));


        public async Task Handle(Contracts.Reviews.V1.ReviewPublish command)
            => HandleForUpdate(command.Id, r => r.Publish());

        private async Task HandleForUpdate(Guid aggregateId, Action<Domain.Review> handle)
        {
            var aggregate = await repository.GetByIdAsync<Domain.Review>(aggregateId);
            handle(aggregate);
            await repository.SaveAsync(aggregate);
        }
        
    }
}
