using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Reviews.Service.WebApi.IntegrationTests
{
    public class CommandsApiControllerTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;

        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper outputHelper;
        
        private static Guid OwnerId = Guid.NewGuid();
        private static Guid ReviewId = Guid.NewGuid();
        private static Guid ReviewUserId = Guid.NewGuid();
        
        public CommandsApiControllerTest(CustomWebApplicationFactory<Startup> factory,ITestOutputHelper outputHelper)
        {
            _factory = factory;
            this.outputHelper = outputHelper;
            client = _factory.CreateClient();
        }
        
       
     
        [Fact]
        public async Task CreateReview_Should_Return_OkResult()
        {
            //Arrange
            var createReview = new Contracts.Reviews.V1.ReviewCreate
            {
                Id = ReviewId,
                Owner = OwnerId,
                Caption = "Test Review",
                Content = "This is awesome product... You should buy it!"
            };
            outputHelper.WriteLine("ReviewId:" + ReviewId.ToString());
            //Act
            var response = await client.PostAsJsonAsync("/reviews",createReview);
            var payload = await response.Content.ReadAsStringAsync();
            outputHelper.WriteLine(payload);
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
           
            
        }
        
        [Fact]
        public async Task PublishReview_Should_Return_OkResult()
        {
            //Arrange
            var publishReview = new Contracts.Reviews.V1.ReviewPublish
            {
                Id = ReviewId
            };
            //Act
            var response = await client.PutAsJsonAsync("/reviews/publish",publishReview);
            var payload = await response.Content.ReadAsStringAsync();
            outputHelper.WriteLine(payload);
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
        }
        
        /*
        [Theory]
        public async Task ApproveReview_Should_Return_OkResult()
        {
            //Arrange
            var approveReview = new Contracts.Reviews.V1.ReviewApprove()
            {
                Id = ReviewId,
                Reviewer = ReviewUserId
            };
            
            //Act
            var response = await client.PutAsJsonAsync("/reviews/approve",approveReview);
            var payload = await response.Content.ReadAsStringAsync();
            outputHelper.WriteLine(payload);
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
        }
        */
        

    }
}