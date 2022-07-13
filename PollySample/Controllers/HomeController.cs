using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.CircuitBreaker;
using Polly.Fallback;
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

        private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private CircuitBreakerPolicy<HttpResponseMessage> basicCircuitBreakerPolicy;
        private readonly AsyncFallbackPolicy<IActionResult> _fallbackPolicy;
        private readonly AsyncRetryPolicy<IActionResult> _retryPolicy;
        private readonly AsyncPolicyWrap<IActionResult> _policy;
        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _httpClient = clientFactory.CreateClient();
            //if (_circuitBreakerPolicy == null)
            //{
            //    _circuitBreakerPolicy = Policy
            //        .Handle<Exception>()
            //        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
            //}
            //CircuitBreakerPolicy<HttpResponseMessage> basicCircuitBreakerPolicy = Policy
            //       .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //        .CircuitBreaker(2, TimeSpan.FromSeconds(60));

            _fallbackPolicy = Policy<IActionResult>
                .Handle<Exception>()
                .FallbackAsync(Content("Sorry, we are currently experiencing issues. Please try again laterkjfgkjkdfjgkjfkgjdk"));

            _retryPolicy = Policy<IActionResult>
              .Handle<Exception>()
              .RetryAsync(5);


            if (_circuitBreakerPolicy == null)
            {
                _circuitBreakerPolicy = Policy
                    .Handle<NotImplementedException>()
                    .CircuitBreakerAsync(200, TimeSpan.FromSeconds(1));
            }

            _policy = Policy<IActionResult>
               .Handle<Exception>()
               .FallbackAsync(Content("Sorry, we are currently experiencing issues. Please try again later"))
               .WrapAsync(_retryPolicy);


        }
        
        [HttpGet("Test")]
        public async Task<string> GetAsync()
        {
            // define a 404 page  
            var url = $"https://www.c-sharpcorner.com/mytestpagefor404";

            var client = _clientFactory.CreateClient("csharpcorner");
            //var client = _clientFactory.CreateClient("jayez");
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        [HttpGet("GetRabbit")]
        public ActionResult<IEnumerable<string>> Get()
        {
            var message = Encoding.UTF8.GetBytes("hello, retry pattern");

            var retry = Policy
                .Handle<Exception>()
                .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            try
            {
                retry.Execute(() =>
                {
                    //Console.WriteLine($"begin at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.");
                    //var factory = new ConnectionFactory
                    //{
                    //    HostName = "localhost",
                    //    UserName = "guest",
                    //    Password = "guest"
                    //};

                    //var connection = factory.CreateConnection();
                    //var model = connection.CreateModel();
                    //model.ExchangeDeclare("retrypattern", ExchangeType.Topic, true, false, null);
                    //model.BasicPublish("retrypattern", "retrypattern.#", false, null, message);
                });
            }
            catch
            {
                Console.WriteLine("exception here.");
            }

            return new string[] { "value1", "value2" };
        }

        [HttpGet("")]
        public async Task<string> Tesss()
        {
            var client = _clientFactory.CreateClient("csharpcorner");
            var response1 = await client.GetAsync($"https://www.c-sharpcorner.com/mytestpagefor404");
            //var response =
            //                    await basicCircuitBreakerPolicy.Execute(() =>
            //                        client.GetAsync($"https://www.c-sharpcorner.com/mytestpagefor404"));
            return new string("new");
        }

        [HttpGet("Fall")]
        public async Task<IActionResult> FallTest()
        {
            var client = _clientFactory.CreateClient("csharpcorner");
            var response1 = await client.GetAsync($"https://www.c-sharpcorner.com/mytestpagefor404");
           return await _fallbackPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));
           
        }
        [HttpGet("Retry")]
        public async Task<IActionResult> RetryTest()
        {
           // var response1 = await client.GetAsync($"https://www.c-sharpcorner.com/mytestpagefor404");
            return await _retryPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));
        }
        [HttpGet("Wrape")]
        public async Task<IActionResult> Wrape()
        {
            return await _policy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync("https://www.c-sharpcorner.com/mytestpagefor404")));



        }

        //public async Task<IActionResult> RetryTest2()
        //{
        //     RetryPolicy < HttpResponseMessage > httpRetryWithReauthorizationPolicy =
        //           Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        //               .RetryAsync(3, onRetry: (response, retryCount) =>
        //               {
        //                   if (response.Result.StatusCode == HttpStatusCode.Unauthorized)
        //                   {
        //                   //  PerformReauthorization();
        //               }
        //               });
        //    return httpRetryWithReauthorizationPolicy.Execute
        //}
        //[HttpGet]
        //public async Task<IActionResult> Authors()
        //     => await ProxyTo("https://localhost:5001/authors");

        //private async Task<IActionResult> ProxyTo(string url)
        //    => await _fallbackPolicy.ExecuteAsync(async () => Content(await _httpClient.GetStringAsync(url)));
    }
}
