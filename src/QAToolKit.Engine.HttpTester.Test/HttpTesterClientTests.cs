using ExpectedObjects;
using QAToolKit.Core.HttpRequestTools;
using QAToolKit.Core.Models;
using QAToolKit.Engine.HttpTester.Exceptions;
using QAToolKit.Engine.HttpTester.Extensions;
using QAToolKit.Engine.HttpTester.Test.Fixtures;
using QAToolKit.Source.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace QAToolKit.Engine.HttpTester.Test
{
    public class HttpTesterClientTests
    {
        [Fact]
        public async Task HttpTesterClientWithSwagger_Success()
        {
            var urlSource = new SwaggerUrlSource(options =>
            {
                options.AddBaseUrl(new Uri("https://qatoolkitapi.azurewebsites.net/"));
                options.AddRequestFilters(new RequestFilter()
                {
                    EndpointNameWhitelist = new string[] { "NewBike" }
                });
                options.UseSwaggerExampleValues = true;
            });

            var requests = await urlSource.Load(new Uri[] {
                new Uri("https://qatoolkitapi.azurewebsites.net/swagger/v1/swagger.json")
            });

            var replacementValues = new Dictionary<string, object> {
                { "api-version", 1}
              };

            var urlGenerator = new HttpRequestUrlGenerator(requests.FirstOrDefault(), options =>
            {
                options.AddReplacementValues(replacementValues);
            });

            using (var client = new HttpTesterClient())
            {
                var response = await client
                     .CreateHttpRequest(new Uri(urlGenerator.GetUrl()))
                     .WithHeaders(new Dictionary<string, string>() { { "Content-Type", "application/json" } })
                     .WithMethod(HttpMethod.Post)
                     .WithJsonBody(BicycleFixture.Get())
                     .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientSimple_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseBody<List<Bicycle>>();

                var expecterResponse = BicycleFixture.GetBicycles().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task HttpTesterClientSimpleGet_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles/1")
                 .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                var expecterResponse = BicycleFixture.GetFoil().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Scott", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientWithoutHeaders_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Post)
                 .WithJsonBody(BicycleFixture.GetCfr())
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                var expecterResponse = BicycleFixture.GetCfr().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientWithoutQueryParams_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithHeaders(new Dictionary<string, string>() { { "Content-Type", "application/json" } })
                 .WithMethod(HttpMethod.Post)
                 .WithJsonBody(BicycleFixture.Get())
                 .WithPath("/api/bicycles")
                 .Start();

                Assert.True(client.Duration < 2000);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task HttpTesterClientWithoutPath_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithHeaders(new Dictionary<string, string>() { { "Content-Type", "application/json" } })
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Post)
                 .WithJsonBody(BicycleFixture.Get())
                 .Start();

                Assert.True(client.Duration < 2000);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task HttpTesterClientWithoutMethod_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                var response = client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithHeaders(new Dictionary<string, string>() { { "Content-Type", "application/json" } })
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithJsonBody(BicycleFixture.Get());

                await Assert.ThrowsAsync<QAToolKitEngineHttpTesterException>(async () => await client.Start());
            }
        }

        [Fact]
        public async Task HttpTesterClientGetWithBody_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                var response = client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithHeaders(new Dictionary<string, string>() { { "Content-Type", "application/json" } })
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithJsonBody(BicycleFixture.Get());

                await Assert.ThrowsAsync<QAToolKitEngineHttpTesterException>(async () => await client.Start());
            }
        }

        [Fact]
        public async Task HttpTesterClientOnlyBaseUrl_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                var response = client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"));

                await Assert.ThrowsAsync<QAToolKitEngineHttpTesterException>(async () => await client.Start());
            }
        }

        [Fact]
        public async Task HttpTesterClientOnlyInstantiation_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                await Assert.ThrowsAsync<QAToolKitEngineHttpTesterException>(async () => await client.Start());
            }
        }

        [Fact]
        public async Task HttpTesterClientGetWithBodyDisableSSLValidationWithValidCert_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"), false)
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles/1")
                 .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                var expecterResponse = BicycleFixture.GetFoil().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Scott", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientGetWithBodyDisableSSLValidationWithInvalidCert_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://swagger-demo.qatoolkit.io/"), false)
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles/1")
                 .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                var expecterResponse = BicycleFixture.GetFoil().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Scott", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientGetWithBodyDisableSSLValidationWithHttpUrl_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("http://swagger-demo.qatoolkit.io/"), false)
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles/1")
                 .Start();

                var msg = await response.GetResponseBody<Bicycle>();

                var expecterResponse = BicycleFixture.GetFoil().ToExpectedObject();
                expecterResponse.ShouldEqual(msg);

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Scott", msg.Brand);
            }
        }

        [Fact]
        public async Task HttpTesterClientGetWithBodyDisableSSLValidationWithInvalidCertAndUrl2_Exception()
        {
            using (var client = new HttpTesterClient())
            {
                var response = client
                 .CreateHttpRequest(new Uri("http://swagger-demo.qatoolkit.io/"), true)
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Get)
                 .WithPath("/api/bicycles/1");

                await Assert.ThrowsAsync<HttpRequestException>(async () => await client.Start());
            }
        }

        [Fact]
        public async Task HttpTesterClientReturnDynamic_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                 .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
                 .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
                 .WithMethod(HttpMethod.Post)
                 .WithJsonBody(BicycleFixture.GetCfr())
                 .WithPath("/api/bicycles")
                 .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostStringBodyWithFulUrl_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody("{\"id\": 5,\"name\":\"EXCEED CFR\",\"brand\":\"Giant\",\"type\":2}")
                   .WithMethod(HttpMethod.Post)
                   .Start();

                var msg = await response.Content.ReadAsStringAsync();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                //Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostObjectBodyWithFulUrl_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody(BicycleFixture.GetCfr())
                   .WithMethod(HttpMethod.Post)
                   .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostObjectBodyWithFulUrlWithBasicAuthorization_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody(BicycleFixture.GetCfr())
                   .WithMethod(HttpMethod.Post)
                   .WithBasicAuthentication("user", "pass")
                   .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(client.HttpClient.DefaultRequestHeaders.Contains("Authorization"));
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostObjectBodyWithFulUrlWithBearerAuthorization_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody(BicycleFixture.GetCfr())
                   .WithMethod(HttpMethod.Post)
                   .WithBearerAuthentication("123")
                   .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(client.HttpClient.DefaultRequestHeaders.Contains("Authorization"));
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostObjectBodyWithFulUrlWithNTLMDefaultAuthorization_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody(BicycleFixture.GetCfr())
                   .WithMethod(HttpMethod.Post)
                   .WithNTLMAuthentication()
                   .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }

        [Fact]
        public async Task HttpTesterClientPostObjectBodyWithFulUrlWithNTLMAuthorization_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                   .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net/api/bicycles?api-version=1"))
                   .WithJsonBody(BicycleFixture.GetCfr())
                   .WithMethod(HttpMethod.Post)
                   .WithNTLMAuthentication("user","pass")
                   .Start();

                var msg = await response.GetResponseBody<dynamic>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Giant", msg.brand.ToString());
            }
        }
    }
}
