using Newtonsoft.Json;
using QAToolKit.Engine.HttpTester.Exceptions;
using QAToolKit.Engine.HttpTester.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public HttpClient HttpClient { get; private set; }
        private HttpClientHandler HttpHandler { get; set; }
        private string _path = null;
        private Dictionary<string, string> _headers = null;
        private string _body = null;
        private HttpMethod _httpMethod;
        private Dictionary<string, string> _queryParameters = null;
        private HttpResponseMessage _responseMessage = null;
        private MultipartFormDataContent _multipartFormDataContent = null;

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
            HttpHandler = new HttpClientHandler();

            if (!validateCertificate &&
                (baseAddress.Scheme == Uri.UriSchemeHttp || baseAddress.Scheme == Uri.UriSchemeHttps))
            {
                HttpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                HttpHandler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            }

            HttpClient = new HttpClient(HttpHandler)
            {
                BaseAddress = baseAddress
            };

            return this;
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
            if (_multipartFormDataContent != null)
                throw new QAToolKitEngineHttpTesterException("Body multipart/form-data already defined on the HTTP client. Can not add application/json content type.");

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
        /// Use basic authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IHttpTesterClient WithBasicAuthentication(string userName, string password)
        {
            var authenticationString = $"{userName}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            return this;
        }

        /// <summary>
        /// Use bearer token authentication
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public IHttpTesterClient WithBearerAuthentication(string accessToken)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return this;
        }

        /// <summary>
        /// Use NTLM authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IHttpTesterClient WithNTLMAuthentication(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException($"{nameof(userName)} is null.");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException($"{nameof(password)} is null.");

            var credentials = new NetworkCredential(userName, password);

            var credentialsCache = new CredentialCache { { HttpClient.BaseAddress, "NTLM", credentials } };
            HttpHandler.Credentials = credentialsCache;

            return this;
        }

        /// <summary>
        /// Use NTLM authentication which represents the authentication credentials for the current security context in which the application is running.
        /// </summary>
        /// <returns></returns>
        public IHttpTesterClient WithNTLMAuthentication()
        {
            var credentials = CredentialCache.DefaultNetworkCredentials;
            var credentialsCache = new CredentialCache { { HttpClient.BaseAddress, "NTLM", credentials } };
            HttpHandler.Credentials = credentialsCache;

            return this;
        }

        /// <summary>
        /// Create a multipart form data content and add a file content
        /// </summary>
        /// <param name="fileByteArray"></param>
        /// <param name="httpContentName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IHttpTesterClient WithMultipart(byte[] fileByteArray, string httpContentName, string fileName)
        {
            if (!string.IsNullOrEmpty(_body))
                throw new QAToolKitEngineHttpTesterException("Body application/json already defined on the HTTP client. Can not add multipart/form-data content type.");
            if (fileByteArray == null)
                throw new ArgumentNullException($"{nameof(fileByteArray)} is null.");
            if (string.IsNullOrEmpty(httpContentName))
                throw new ArgumentNullException($"{nameof(httpContentName)} is null.");

            if (_multipartFormDataContent == null)
            {
                _multipartFormDataContent = new MultipartFormDataContent();
            }

            _multipartFormDataContent.Add(new ByteArrayContent(fileByteArray), httpContentName, fileName);

            return this;
        }

        /// <summary>
        /// Add a string to a multipart form data
        /// </summary>
        /// <param name="httpContentName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpTesterClient WithMultipart(string httpContentName, string value)
        {
            if (!string.IsNullOrEmpty(_body))
                throw new QAToolKitEngineHttpTesterException("Body application/json already defined on the HTTP client. Can not add multipart/form-data content type.");
            if (string.IsNullOrEmpty(httpContentName))
                throw new ArgumentNullException($"{nameof(httpContentName)} is null.");
            if (value == null)
                throw new ArgumentNullException($"{nameof(value)} is null.");

            if (_multipartFormDataContent == null)
            {
                _multipartFormDataContent = new MultipartFormDataContent();
            }

            _multipartFormDataContent.Add(new StringContent(value), httpContentName);

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

                if (_multipartFormDataContent != null)
                {
                    requestMessage.Content = _multipartFormDataContent;
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
            HttpHandler?.Dispose();
        }
    }
}
