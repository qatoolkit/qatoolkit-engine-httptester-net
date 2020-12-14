using QAToolKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace QAToolKit.Engine.HttpTester.Interfaces
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
        IHttpTesterClient CreateHttpRequest(Uri baseAddress, bool validateCertificate = true);
        /// <summary>
        /// Create a HTTP request client from QAToolKit HttpRequest object
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="validateCertificate"></param>
        /// <returns></returns>
        IHttpTesterClient CreateHttpRequest(HttpRequest httpRequest, bool validateCertificate = true);
        /// <summary>
        /// Add URL path to the HTTP client
        /// </summary>
        /// <param name="urlPath"></param>
        /// <returns></returns>
        IHttpTesterClient WithPath(string urlPath);
        /// <summary>
        /// Replace URL path with path parameters from passed dictionary
        /// </summary>
        /// <param name="pathParameters"></param>
        /// <returns></returns>
        IHttpTesterClient WithPathReplacementValues(Dictionary<string, string> pathParameters);
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
        /// Use basic authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IHttpTesterClient WithBasicAuthentication(string userName, string password);
        /// <summary>
        /// Bearer token authentication
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        IHttpTesterClient WithBearerAuthentication(string accessToken);
        /// <summary>
        /// Use NTLM authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IHttpTesterClient WithNTLMAuthentication(string userName, string password);
        /// <summary>
        /// Use NTLM authentication which represents the authentication credentials for the current security context in which the application is running.
        /// </summary>
        /// <returns></returns>
        IHttpTesterClient WithNTLMAuthentication();
        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="fileByteArray"></param>
        /// <param name="httpContentName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IHttpTesterClient WithMultipart(byte[] fileByteArray, string httpContentName, string fileName);
        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="httpContentName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IHttpTesterClient WithMultipart(string httpContentName, string value);
        /// <summary>
        /// Start the HTTP request
        /// </summary>
        /// <returns></returns>
        Task<HttpResponseMessage> Start();
    }
}
