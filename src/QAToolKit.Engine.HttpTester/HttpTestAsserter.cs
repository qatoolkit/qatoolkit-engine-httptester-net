using QAToolKit.Core.Helpers;
using QAToolKit.Engine.HttpTester.Interfaces;
using QAToolKit.Engine.HttpTester.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;

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
            _httpResponseMessage = httpResponseMessage ??
                                   throw new ArgumentNullException($"{nameof(httpResponseMessage)} is null.");
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
        /// <param name="keyword">Check if the HTTP response body contains a keyword.</param>
        /// <param name="caseInsensitive">Use case sensitive string comparison.</param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseContentContains(string keyword, bool caseInsensitive = true)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentNullException($"{nameof(keyword)} is null.");
            }

            var bodyString = _httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var assertResult = new AssertResult()
            {
                Name = nameof(ResponseContentContains)
            };

            assertResult.IsTrue = caseInsensitive
                ? StringHelper.ContainsCaseInsensitive(bodyString, keyword)
                : bodyString.Contains(keyword);
            assertResult.Message = assertResult.IsTrue
                ? $"Response body contains keyword '{keyword}'."
                : $"Response body does not contain keyword '{keyword}'.";

            _assertResults.Add(assertResult);

            return this;
        }

        /// <summary>
        /// Check if the response contains specified Content Type
        /// </summary>
        /// <param name="contentType">Check if the response content type equals the parameter.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IHttpTestAsserter ResponseContentTypeEquals(string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException($"{nameof(contentType)} is null.");
            }

            var responseContentType = _httpResponseMessage.Content.Headers.ContentType.MediaType;

            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseContentTypeEquals),
                Message = $"Expected content-type = '{contentType}', actual = '{responseContentType}'.",
                IsTrue = _httpResponseMessage.Content.Headers.ContentType.MediaType == contentType
            });

            return this;
        }

        /// <summary>
        /// Verify request duration
        /// </summary>
        /// <param name="duration">Actual duration of the HTTP request execution</param>
        /// <param name="predicateFunction">It's a function that validates the duration</param>
        /// <param name="predicateFunctionExpression">String predicateFunctionExpression for the report. If set it will be used in the assert result message.</param>
        /// <returns></returns>
        public IHttpTestAsserter RequestDurationEquals(long duration, Expression<Func<long, bool>> predicateFunction, string predicateFunctionExpression = null)
        {
            var isTrue = predicateFunction.Compile()(duration);

            var assertResult = new AssertResult()
            {
                Name = nameof(RequestDurationEquals),
                IsTrue = isTrue
            };

            if (isTrue)
            {
                if (string.IsNullOrEmpty(predicateFunctionExpression))
                {
                    assertResult.Message = $"Duration is '{duration}ms' and is valid.";
                }
                else
                {
                    assertResult.Message = $"Duration is '{duration}ms' and is valid with predicateFunctionExpression '{predicateFunctionExpression}'.";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(predicateFunctionExpression))
                {
                    assertResult.Message = $"Duration is '{duration}ms' and is invalid.";
                }
                else
                {
                    assertResult.Message = $"Duration is '{duration}ms' and is invalid with predicateFunctionExpression '{predicateFunctionExpression}'.";
                }
            }
            
            _assertResults.Add(assertResult);

            return this;
        }

        /// <summary>
        /// HTTP response contains a header
        /// </summary>
        /// <param name="headerName">Check if the HTTP response contains the header with the name.</param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseHasHttpHeader(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentNullException($"{nameof(headerName)} is null.");
            }

            var assertResult = new AssertResult
            {
                Name = nameof(ResponseHasHttpHeader),
                IsTrue = _httpResponseMessage.Headers.TryGetValues(headerName, out var values)
            };

            assertResult.Message = assertResult.IsTrue
                ? $"Response message contains header '{headerName}'."
                : $"Response message does not contain header '{headerName}'.";

            _assertResults.Add(assertResult);

            return this;
        }

        /// <summary>
        /// Verify if response code equals
        /// </summary>
        /// <param name="httpStatusCode">Check if the HTTP response status code equals to this parameter.</param>
        /// <returns></returns>
        public IHttpTestAsserter ResponseStatusCodeEquals(HttpStatusCode httpStatusCode)
        {
            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseStatusCodeEquals),
                Message =
                    $"Expected status code = '{httpStatusCode}', actual = '{_httpResponseMessage.StatusCode}'.",
                IsTrue = _httpResponseMessage.StatusCode == httpStatusCode
            });

            return this;
        }

        /// <summary>
        /// HTTP response status code is one of 2xx
        /// </summary>
        /// <returns></returns>
        public IHttpTestAsserter ResponseStatusCodeIsSuccess()
        {
            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseStatusCodeIsSuccess),
                Message = $"Expected status code = '2xx', actual = '{_httpResponseMessage.StatusCode}'.",
                IsTrue = _httpResponseMessage.IsSuccessStatusCode
            });

            return this;
        }

        /// <summary>
        /// HTTP response body is empty
        /// </summary>
        /// <returns></returns>
        public IHttpTestAsserter ResponseBodyIsEmpty()
        {
            var bodyString = _httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            _assertResults.Add(new AssertResult()
            {
                Name = nameof(ResponseBodyIsEmpty),
                Message = $"Expected empty response body, actual = '{bodyString}'.",
                IsTrue = string.IsNullOrEmpty(bodyString)
            });

            return this;
        }
    }
}