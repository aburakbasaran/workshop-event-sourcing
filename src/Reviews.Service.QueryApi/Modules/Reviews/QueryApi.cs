using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Reviews.Service.Contract;
using Sparrow.Json;

namespace Reviews.Service.QueryApi.Modules.Reviews
{
    [Route("/reviews")]
    [ApiController]
    public class QueryApi
    {
        
        
        private readonly QueryService reviewQueryService;

        public QueryApi(QueryService queryService)=>  reviewQueryService = queryService;
        
        [Route("status")]
        [HttpGet]
        public async Task<IActionResult> Get() => new OkResult();

        [Route("active/user-id")]
        [HttpGet]
        public Task<List<ActiveReviewDocument>> GetAll(string user_id)
        {
            return reviewQueryService.GetAllActiveReviewDocuments(user_id);
        }
        
        [Route("active")]
        [HttpGet]
        public Task<ActiveReviewDocument> Get(string id)
        {
            return reviewQueryService.GetActiveReviewById(id);
        }

    }
}
