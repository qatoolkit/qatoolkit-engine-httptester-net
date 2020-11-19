using QAToolKit.Core.Helpers;
using QAToolKit.Engine.HttpTester.Interfaces;
using QAToolKit.Engine.HttpTester.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace QAToolKit.Engine.HttpTester
{
    /// <summary>
    /// Http test asserter implementation
    /// </summary>
    public class HttpTestAsserter : IHttpTestAsserter
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly List<AssertResult> _assertResults;

        /// <summary>
        /// Create new object
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        public HttpTestAsserter(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage ?? throw new ArgumentNullException($"{nameof(httpResponseMessage)} is null.");
            _assertResults = new List<AssertResult>();
        }

        /// <summary>
        /// Return all Assert messages of the Asserter
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AssertResult> AssertAll()
        {
            return _assertResults;
        }

        /// <summary>
        /// HTTP body contains a string (ignores case)
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseContentContains(string keyword, bool caseInsensitive = true)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentNullException($"{nameof(keyword)} is null.");
            }

            var bodyString = _httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseContentContains),
                Message = $"Body contains '{keyword}'.",
                IsTrue = caseInsensitive ? StringHelper.ContainsCaseInsensitive(bodyString, keyword) : bodyString.Contains(keyword)
            });

            return this;
        }

        /// <summary>
        /// Verify request duration
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="predicateFunction"></param>
        /// <returns></returns>
        public IHttpTestAsserter RequestDurationEquals(long duration, Func<long, bool> predicateFunction)
        {
            var isTrue = predicateFunction.Invoke(duration);
            _assertResults.Add(new AssertResult()
            {
                Name = nameof(RequestDurationEquals),
                Message = $"Duration is '{duration}'.",
                IsTrue = isTrue
            });

            return this;
        }

        /// <summary>
        /// HTTP response contains a header
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseHasHttpHeader(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentNullException($"{nameof(headerName)} is null.");
            }

            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseHasHttpHeader),
                Message = $"Contains header '{headerName}'.",
                IsTrue = _httpResponseMessage.Headers.Contains(headerName)
            });

            return this;
        }

        /// <summary>
        /// Verify if response code equals
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseStatusCodeEquals(HttpStatusCode httpStatusCode)
        {
            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseStatusCodeEquals),
                Message = $"Expected status code is '{httpStatusCode}' return code is '{_httpResponseMessage.StatusCode}'.",
                IsTrue = _httpResponseMessage.StatusCode == httpStatusCode
            });

            return this;
        }
    }
}
