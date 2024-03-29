﻿using Newtonsoft.Json;
using QAToolKit.Core.Models;
using QAToolKit.Engine.HttpTester.Exceptions;
using QAToolKit.Engine.HttpTester.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        /// Measured duration of the whole request execution
        /// </summary>
        public long Duration { get; private set; }
        
        /// <summary>
        /// Measured duration of the HTTP request execution (HttpClient.StartAsync(...))
        /// </summary>
        public long HttpDuration { get; private set; }

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
                (httpRequestMessage, cert, cetChain, policyErrors) => true;
            }

            HttpClient = new HttpClient(HttpHandler)
            {
                BaseAddress = baseAddress
            };

            return this;
        }

        /// <summary>
        /// Create a HTTP tester client from QAToolKit HttpRequest object
        /// </summary>
        /// <param name="httpRequest">Create tester client with BaseUrl, Path, HttpMethod, Headers and URL Query parameters read from HttpRequest object. Specify other values and parameters manually.</param>
        /// <param name="validateCertificate"></param>
        /// <returns></returns>
        public IHttpTesterClient CreateHttpRequest(HttpRequest httpRequest, bool validateCertificate = true)
        {
            if (httpRequest == null)
                throw new QAToolKitEngineHttpTesterException("'HttpRequest' is null. Pass in the valid object.");
            if (HttpClient != null)
                throw new QAToolKitEngineHttpTesterException("HttpClient is already instantiated. Create new 'HttpTesterClient'.");

            var baseAddress = new Uri(httpRequest.BasePath);

            HttpHandler = new HttpClientHandler();

            if (!validateCertificate &&
                (baseAddress.Scheme == Uri.UriSchemeHttp || baseAddress.Scheme == Uri.UriSchemeHttps))
            {
                HttpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                HttpHandler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true;
            }

            HttpClient = new HttpClient(HttpHandler)
            {
                BaseAddress = baseAddress
            };

            if (string.IsNullOrEmpty(httpRequest.Path))
            {
                throw new QAToolKitEngineHttpTesterException("HttpRequest Path is required.");
            }

            _path = httpRequest.Path;

            _httpMethod = httpRequest.Method;

            //Query parameters
            _queryParameters ??= new Dictionary<string, string>();

            foreach (var parameter in httpRequest.Parameters.Where(t => t.Location == Location.Query && t.Value != null))
            {
                _queryParameters.Add(parameter.Name, parameter.Value);
            }

            //Headers
            _headers ??= new Dictionary<string, string>();

            foreach (var header in httpRequest.Parameters.Where(t => t.Location == Location.Header && t.Value != null))
            {
                _headers.Add(header.Name, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Add URL path to the HTTP client
        /// </summary>
        /// <param name="urlPath"></param>
        /// <returns></returns>
        public IHttpTesterClient WithPath(string urlPath)
        {
            _path = urlPath ?? throw new ArgumentException($"{nameof(urlPath)} is null.");

            return this;
        }

        /// <summary>
        /// Replace URL path with path parameters from passed dictionary
        /// </summary>
        /// <param name="pathParameters"></param>
        /// <returns></returns>
        public IHttpTesterClient WithPathReplacementValues(Dictionary<string, string> pathParameters)
        {
            if (pathParameters == null)
                throw new ArgumentException($"{nameof(pathParameters)} is null.");

            if (string.IsNullOrEmpty(_path))
                throw new QAToolKitEngineHttpTesterException("Uri Path is empty. Use 'WithPath' before calling 'WithPathReplacementValues'.");

            foreach (var parameter in pathParameters)
            {
                _path = _path.Replace($"{{{parameter.Key}}}", parameter.Value);
            }

            return this;
        }

        /// <summary>
        /// Add HTTP headers to the HTTP client
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public IHttpTesterClient WithHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentException($"{nameof(headers)} is null.");
            }
            
            if (_headers == null || _headers.Count == 0)
            {
                _headers = headers;
            }
            else
            {
                foreach (var header in headers)
                {
                    if (!_headers.ContainsKey(header.Key))
                    {
                        _headers.Add(header.Key, header.Value);                        
                    }
                }
            }
            
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

            if (typeof(T) == typeof(string))
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
            if (httpMethod == null)
                throw new ArgumentException($"{nameof(httpMethod)} is null.");

            _httpMethod = httpMethod;

            return this;
        }

        /// <summary>
        /// Specify HTTP query parameters to HTTP client
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        public IHttpTesterClient WithQueryParams(Dictionary<string, string> queryParameters)
        {
            _queryParameters = queryParameters ?? throw new ArgumentException($"{nameof(queryParameters)} is null.");

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
            if (userName == null)
                throw new ArgumentException($"{nameof(userName)} is null.");
            if (password == null)
                throw new ArgumentException($"{nameof(password)} is null.");

            var authenticationString = $"{userName}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
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
            if (accessToken == null)
                throw new ArgumentException($"{nameof(accessToken)} is null.");

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
        /// Use client certificate for authentication
        /// </summary>
        /// <returns></returns>
        public IHttpTesterClient WithCertificateAuthentication(X509Certificate2 certificate)
        {
            HttpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                    {
                        return true;
                    }

                    throw new QAToolKitEngineHttpTesterException("Certificate validation failed.");
                };

            if (certificate != null)
            {
                HttpHandler.ClientCertificates.Add(certificate);
            }

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

            _multipartFormDataContent ??= new MultipartFormDataContent();

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

            _multipartFormDataContent ??= new MultipartFormDataContent();

            _multipartFormDataContent.Add(new StringContent(value), httpContentName);

            return this;
        }

        /// <summary>
        /// Specify the HTTP client agent data. It's useful for serverside to identify your HTTP calls.
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="productVersion"></param>
        /// <param name="productUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="QAToolKitEngineHttpTesterException"></exception>
        public IHttpTesterClient WithHttpAgent(string productName, string productVersion, string productUrl)
        {
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentNullException($"{nameof(productName)} is null.");
            if (string.IsNullOrEmpty(productVersion))
                throw new ArgumentNullException($"{nameof(productVersion)} is null.");
            if (string.IsNullOrEmpty(productUrl))
                throw new ArgumentNullException($"{nameof(productUrl)} is null.");
            if (HttpClient == null)
                throw new QAToolKitEngineHttpTesterException("HttpClient is not instantiated. Create new 'HttpTesterClient'.");
            
            var productValue = new ProductInfoHeaderValue(productName, productVersion);
            var commentValue = new ProductInfoHeaderValue($"(+{productUrl})");

            HttpClient.DefaultRequestHeaders.UserAgent.Add(productValue);
            HttpClient.DefaultRequestHeaders.UserAgent.Add(commentValue);

            return this;
        }

        /// <summary>
        /// Start the HTTP request
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Start()
        {
            var sw = new Stopwatch();
            sw.Start();
            
            if (HttpClient == null)
                throw new QAToolKitEngineHttpTesterException("HttpClient is null. Create an object first with 'CreateHttpRequest'.");
            if (_httpMethod == null)
                throw new QAToolKitEngineHttpTesterException("Define method for a HTTP request.");
            if (_httpMethod == HttpMethod.Get && _body != null)
                throw new QAToolKitEngineHttpTesterException("'Get' method can not have a HTTP body.");

            var queryString = new StringBuilder();
            if (_queryParameters != null)
            {
                queryString.Append("?");

                var array = _queryParameters.Select(query => $"{query.Key}={query.Value}").ToList();

                queryString.Append(string.Join("&", array));
            }

            using (var requestMessage = new HttpRequestMessage(_httpMethod, _path + queryString.ToString()))
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

                var swHttp = new Stopwatch();
                swHttp.Start();
                _responseMessage = await HttpClient.SendAsync(requestMessage);
                swHttp.Stop();

                HttpDuration = swHttp.ElapsedMilliseconds;
            }
            
            sw.Stop();

            Duration = sw.ElapsedMilliseconds;

            return _responseMessage;
        }

        public Dictionary<string, string> GetRequestHeaders()
        {
            return _headers;
        }
        
        public Dictionary<string, string> GetQueryParameters()
        {
            return _queryParameters;
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