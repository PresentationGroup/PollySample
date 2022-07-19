using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly HttpClient _client;
 
      
        private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy = Policy.Handle<Exception>()
           .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromMinutes(2),
               onBreak: (_, duration) => Console.WriteLine($"Circuit open for duration {duration}"),
               onReset: () => Console.WriteLine("Circuit closed and is allowing requests through"),
               onHalfOpen: () => Console.WriteLine("Circuit is half-opened and will test the service with the next request"));

        public TestController()
        {
            //_client = new HttpClient
            //{
            //    BaseAddress = new Uri($"https://www.c-sharpcorner.com/mytestpagefor404"),
            //};          

        }
        [HttpGet("CirTest")]
        public async Task<string> CirTest()

        {

            try
            {
                Console.WriteLine($"Circuit State: {_circuitBreakerPolicy.CircuitState}");
                return await _circuitBreakerPolicy.ExecuteAsync<string>(async () =>
                {
                    return await TestMethod();
                });
             

            }
            catch (BrokenCircuitException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }




        }
        private async Task<string> TestMethod()
        {
            Console.WriteLine("TestMethod running");
            ThrowRandomException();
            return nameof(TestMethod);
        }
        private void ThrowRandomException()
        {
            var diceRoll = new Random().Next(0, 10);

            if (diceRoll > 1)
            {
                Console.WriteLine("ERROR! Throwing Exception");
                throw new Exception("Exception in Random");
            }
        }      
       

        //private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy = Policy.Handle<Exception>()
        //     .CircuitBreakerAsync(1, TimeSpan.FromMinutes(20),
        //     (ex, t) =>
        //     {
        //         Console.WriteLine("Circuit broken!");
        //     },
        //     () =>
        //     {
        //         Console.WriteLine("Circuit Reset!");
        //     });

        //public async Task<string> GetGoodbyeMessage()
        //{
        //    Console.WriteLine("MessageRepository GetGoodbyeMessage running");
        //    ThrowRandomException();
        //    return "GoodbyeMessage";
        //   // return _messageOptions.GoodbyeMessage;
        //}

    }
}
