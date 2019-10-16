using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Reviews.Service.WebApi.Modules.Reviews
{
    [Route("/reviews")]
    [ApiController]
    public class CommandsApi
    {
        private readonly ApplicationService applicationService;

        public CommandsApi(ApplicationService appService)
        {
            applicationService = appService;
        }

        [HttpPost]
        public Task<IActionResult> Post(Contracts.Reviews.V1.ReviewCreate command) => HandleOrThrow(command, app => applicationService.Handle(app));

        [Route("approve")]
        [HttpPut]
        public Task<IActionResult> Put(Contracts.Reviews.V1.ReviewApprove command) => HandleOrThrow(command, app => applicationService.Handle(app));

        [Route("publish")]
        [HttpPut]
        public Task<IActionResult> Put(Contracts.Reviews.V1.ReviewPublish command) => HandleOrThrow(command, app => applicationService.Handle(app));

        [Route("update")]
        [HttpPut]
        public Task<IActionResult> Put(Contracts.Reviews.V1.UpdateReview command) => HandleOrThrow(command, app => applicationService.Handle(app));

        
        private async Task<IActionResult> HandleOrThrow<T>(T request,Func<T,Task> handle)
        {
            try
            {
                await handle(request);
                return new OkResult();
            }
            catch (Exception e)
            {
                return  new BadRequestResult();

            }
        }
    }
}
