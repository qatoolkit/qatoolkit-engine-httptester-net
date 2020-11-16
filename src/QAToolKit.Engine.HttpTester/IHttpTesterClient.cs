using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace QAToolKit.Engine.HttpTester
{
    /// <summary>
    /// Interface of HTTP tester client
    /// </summary>
    public interface IHttpTesterClient
    {
        /// <summary>
        /// Create HTTP request client
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="validateCertificate"></param>
        /// <returns></returns>
        IHttpTesterClient CreateHttpRequest(Uri baseAddress, bool validateCertificate);
        /// <summary>
        /// Add URL path to the HTTP client
        /// </summary>
        /// <param name="urlPath"></param>
        /// <returns></returns>
        IHttpTesterClient WithPath(string urlPath);
        /// <summary>
        /// Add HTTP method to the HTTP client
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        IHttpTesterClient WithMethod(HttpMethod httpMethod);
        /// <summary>
        /// Add HTTP headers to the HTTP client
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        IHttpTesterClient WithHeaders(Dictionary<string, string> headers);
        /// <summary>
        /// Add JSON body to the HTTP client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bodyObject"></param>
        /// <returns></returns>
        IHttpTesterClient WithJsonBody<T>(T bodyObject);
        /// <summary>
        /// Specify HTTP query paramters to HTTP client
        /// </summary>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        IHttpTesterClient WithQueryParams(Dictionary<string, string> queryParameters);
        /// <summary>
        /// Start the HTTP request
        /// </summary>
        /// <returns></returns>
        Task<HttpResponseMessage> Start();
    }
}
