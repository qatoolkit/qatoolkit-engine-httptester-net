using QAToolKit.Engine.HttpTester.Extensions;
using QAToolKit.Engine.HttpTester.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace QAToolKit.Engine.HttpTester.Test
{
    public class HttpTestAsserterTests
    {
        [Fact]
        public async Task HttpTestAsserterSimple_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);

                var duration = client.Duration;
                var assertResults = asserter
                    .ResponseContentContains("scott")
                    .RequestDurationEquals(duration, (x) => x < 2000)
                    .ResponseStatusCodeEquals(HttpStatusCode.OK)
                    .ResponseHasHttpHeader("Date")
                    .AssertAll();

                foreach (var result in assertResults)
                {
                    Assert.True(result.IsTrue, result.Message);
                }
            }
        }

        [Fact]
        public async Task HttpTestAsserterDoesNotContainHeader_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);

                var assertResults = asserter
                    .ResponseHasHttpHeader("authentication")
                    .AssertAll();

                foreach (var result in assertResults)
                {
                    Assert.False(result.IsTrue, result.Message);
                }
            }
        }

        [Fact]
        public async Task HttpTestAsserterDoesNotContainKeywordInBody_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);

                var assertResults = asserter
                    .ResponseContentContains("giant")
                    .AssertAll();

                foreach (var result in assertResults)
                {
                    Assert.False(result.IsTrue, result.Message);
                }
            }
        }

        [Fact]
        public async Task HttpTestAsserterHeaderMissing_Fails()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);

                var duration = client.Duration;
                var httpDuration = client.HttpDuration;
                Assert.Throws<ArgumentNullException>(() => asserter
                    .ResponseContentContains("scott")
                    .RequestDurationEquals(duration, (x) => x < 2000)
                    .RequestDurationEquals(httpDuration, (x) => x < 1800)
                    .ResponseStatusCodeEquals(HttpStatusCode.OK)
                    .ResponseHasHttpHeader(null)
                    .AssertAll());
            }
        }

        [Fact]
        public async Task HttpTestAsserterBodyNull_Fails()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);

                var duration = client.Duration;
                Assert.Throws<ArgumentNullException>(() => asserter
                    .ResponseContentContains(null)
                    .RequestDurationEquals(duration, (x) => x < 1000)
                    .ResponseStatusCodeEquals(HttpStatusCode.OK)
                    .AssertAll());
            }
        }

        [Fact]
        public async Task HttpTestAsserterAlternativeDurationPredicate_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseJsonBody<List<Bicycle>>();

                var asserter = new HttpTestAsserter(response);
                var duration = client.Duration;
                var assertResults = asserter
                    .ResponseContentContains("scott")
                    .ResponseContentContains("id")
                    .RequestDurationEquals(duration, (x) => (x > 100 && x < 1000))
                    .ResponseStatusCodeEquals(HttpStatusCode.OK)
                    .ResponseStatusCodeIsSuccess()
                    .AssertAll();

                Assert.Equal(5, assertResults.ToList().Count);
                foreach (var result in assertResults)
                {
                    Assert.True(result.IsTrue, result.Message);
                }
            }
        }

        [Fact]
        public async Task HttpTestAsserterDeleteIsBodyEmpty_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Delete)
                 .WithPath("/api/bicycles/1")
                 .Start();

                var asserter = new HttpTestAsserter(response);
                var duration = client.Duration;
                var assertResults = asserter
                    .ResponseBodyIsEmpty()
                    .RequestDurationEquals(duration, (x) => (x > 100 && x < 2000))
                    .ResponseStatusCodeIsSuccess()
                    .AssertAll();

                Assert.Equal(3, assertResults.ToList().Count);
                foreach (var result in assertResults)
                {
                    Assert.True(result.IsTrue, result.Message);
                }
            }
        }
    }
}
