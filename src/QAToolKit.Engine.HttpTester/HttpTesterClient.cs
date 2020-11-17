using Newtonsoft.Json;
using QAToolKit.Engine.HttpTester.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QAToolKit.Engine.HttpTester
{
    /// <summary>
    /// An implementation of HTTP tester client
    /// </summary>
    public class HttpTesterClient : IHttpTesterClient, IDisposable
    {
        /// <summary>
        /// HttpClient object
        /// </summary>
        public HttpClient HttpClient { get; set; }
        private string _path = null;
        private Dictionary<string, string> _headers = null;
        private string _body = null;
        private HttpMethod _httpMethod;
        private Dictionary<string, string> _queryParameters = null;
        private HttpResponseMessage _responseMessage = null;

        /// <summary>
        /// Measured HTTP request duration
        /// </summary>
        public long Duration { get; private set; }

        /// <summary>
        /// Create HTTP request client with or without certificate validation
        /// </summary>
        /// <param name="baseAddress">Base address of the API, can be http or https.</param>
        /// <param name="validateCertificate">Validate certificate, default is true and it's ignored if http is used.</param>
        /// <returns></returns>
        public IHttpTesterClient CreateHttpRequest(Uri baseAddress, bool validateCertificate = true)
        {
            if (!validateCertificate &&
                (baseAddress.Scheme == Uri.UriSchemeHttp || baseAddress.Scheme == Uri.UriSchemeHttps))
            {
                NonValidatingClient(baseAddress);
            }
            else
            {
                ValidatingClient(baseAddress);
            }

            return this;
        }

        private void ValidatingClient(Uri baseAddress)
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = baseAddress
            };
        }

        private void NonValidatingClient(Uri baseAddress)
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };

            HttpClient = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
        }

        /// <summary>
        /// Add URL path to the HTTP client
        /// </summary>
        /// <param name="urlPath"></param>
        /// <returns></returns>
        public IHttpTesterClient WithPath(string urlPath)
        {
            _path = urlPath;

            return this;
        }

        /// <summary>
        /// Add HTTP headers to the HTTP client
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public IHttpTesterClient WithHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;

            return this;
        }

        /// <summary>
        /// Add JSON body to the HTTP client
        /// </summary>
        /// <typeparam name="T">Type of DTO object</typeparam>
        /// <param name="bodyObject">DTO object that needs to be serialized to JSON</param>
        /// <returns></returns>
        public IHttpTesterClient WithJsonBody<T>(T bodyObject)
        {
            if (bodyObject == null)
            {
                return this;
            }

            if (typeof(T) == typeof(String))
            {
                _body = bodyObject.ToString();
            }
            else
            {
                _body = JsonConvert.SerializeObject(bodyObject);
            }

            return this;
        }

        /// <summary>
        /// Add HTTP method to the HTTP client
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public IHttpTesterClient WithMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;

            return this;
        }

        /// <summary>
        /// Specify HTTP query paramters to HTTP client
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        public IHttpTesterClient WithQueryParams(Dictionary<string, string> queryParameters)
        {
            _queryParameters = queryParameters;

            return this;
        }

        /// <summary>
        /// Start the HTTP request
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Start()
        {
            if (HttpClient == null)
            {
                throw new QAToolKitEngineHttpTesterException("HttpClient is null. Create an object first with 'CreateHttpRequest'.");
            }

            if (_httpMethod == null)
            {
                throw new QAToolKitEngineHttpTesterException("Define method for a HTTP request.");
            }

            if (_httpMethod == HttpMethod.Get && _body != null)
            {
                throw new QAToolKitEngineHttpTesterException("'Get' method can not have a HTTP body.");
            }

            string queryString = "";
            if (_queryParameters != null)
            {
                queryString = "?";

                foreach (var query in _queryParameters)
                {
                    queryString += $"{query.Key}={query.Value}";
                }
            }

            var sw = new Stopwatch();
            sw.Start();
            using (var requestMessage = new HttpRequestMessage(_httpMethod, _path + queryString))
            {
                if (_headers != null)
                {
                    foreach (var header in _headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                if (_body != null)
                {
                    requestMessage.Content = new StringContent(_body, Encoding.UTF8, "application/json");
                }

                _responseMessage = await HttpClient.SendAsync(requestMessage);
            }
            sw.Stop();

            Duration = sw.ElapsedMilliseconds;

            return _responseMessage;
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            HttpClient?.Dispose();
        }
    }
}
