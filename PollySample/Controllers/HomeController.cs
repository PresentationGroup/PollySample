using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;

using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _httpClient;


        private readonly AsyncFallbackPolicy<IActionResult> _fallbackPolicy;

        private readonly AsyncRetryPolicy<IActionResult> _retryPolicy;

        private readonly AsyncRetryPolicy<IActionResult> waitAndRetry;

        private readonly AsyncPolicyWrap<IActionResult> _wrapPolicy;

        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _httpClient = clientFactory.CreateClient();

           
            //****************
 
            _retryPolicy = Policy<IActionResult>
              .Handle<HttpRequestException>()
              .RetryAsync(5);

            waitAndRetry = Policy<IActionResult>
              .Handle<Exception>()
              .WaitAndRetryAsync(
                  3,
                  _ => TimeSpan.FromSeconds(10)
              );

            _fallbackPolicy = Policy<IActionResult>
               .Handle<Exception>()
               .FallbackAsync(Content("خطا رخ داد"));

            _wrapPolicy = Policy<IActionResult>
               .Handle<Exception>()
               .FallbackAsync(Content("خطا رخ داد داخل wrap"))
               .WrapAsync(_retryPolicy);

          

        }

      

        [HttpGet("Fall")]
        public async Task<IActionResult> FallTest()
        {
            
            return await _fallbackPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));

        }
        [HttpGet("Retry")]
        public async Task<IActionResult> RetryTest()
        {
          
            try
            {
                return  await _retryPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                throw;
            }
           
        }
        [HttpGet("WaitAndRetry")]
        public async Task<IActionResult> WaitAndRetry()
        {
            return await waitAndRetry.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));

        }

        [HttpGet("Wrape")]
        public async Task<IActionResult> Wrape()
        {
            return await _wrapPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));
           
        }


        //[HttpGet("Test")]
        //public async Task<string> GetAsync()
        //{
        //    // define a 404 page  
        //    var url = $"https://www.c-sharpcorner.com/mytestpagefor404";

        //    var client = _clientFactory.CreateClient("csharpcorner");
        //    //var client = _clientFactory.CreateClient("jayez");
        //    var response = await client.GetAsync(url);
        //   // var result = await response.Content.ReadAsStringAsync();
        //    return response.StatusCode.ToString();
        //}
        //[HttpGet("GetRabbit")]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    var message = Encoding.UTF8.GetBytes("hello, retry pattern");

        //    var retry = Policy
        //        .Handle<Exception>()
        //        .WaitAndRetry(20, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        //    try
        //    {
        //        retry.Execute(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404"))
        //        //Console.WriteLine($"begin at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.");
        //        //var factory = new ConnectionFactory
        //        //{
        //        //    HostName = "localhost",
        //        //    UserName = "guest",
        //        //    Password = "guest"
        //        //};

        //        //var connection = factory.CreateConnection();
        //        //var model = connection.CreateModel();
        //        //model.ExchangeDeclare("retrypattern", ExchangeType.Topic, true, false, null);
        //        //model.BasicPublish("retrypattern", "retrypattern.#", false, null, message);
        //        );
        //    }
        //    catch
        //    {
        //        Console.WriteLine("exception here.");
        //    }

        //    return new string[] { "value1", "value2" };
        //}

    }
}
