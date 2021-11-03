using QAToolKit.Engine.HttpTester.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;

namespace QAToolKit.Engine.HttpTester.Interfaces
{
    /// <summary>
    /// Http test asserter interface
    /// </summary>
    public interface IHttpTestAsserter
    {
        /// <summary>
        /// HTTP body contains a string (ignores case)
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        IHttpTestAsserter ResponseContentContains(string keyword, bool caseInsensitive = true);

        /// <summary>
        /// Verify request duration
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="predicateFunction"></param>
        /// <param name="predicateFunctionExpression"></param>
        /// <returns></returns>
        IHttpTestAsserter RequestDurationEquals(long duration, Expression<Func<long, bool>> predicateFunction, string predicateFunctionExpression = null);
        /// <summary>
        /// Verify if response code equals
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        IHttpTestAsserter ResponseStatusCodeEquals(HttpStatusCode httpStatusCode);
        /// <summary>
        /// HTTP response contains a header
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        IHttpTestAsserter ResponseHasHttpHeader(string headerName);
        /// <summary>
        /// HTTP response status code is one of 2xx
        /// </summary>
        /// <returns></returns>
        IHttpTestAsserter ResponseStatusCodeIsSuccess();
        /// <summary>
        /// HTTP response body is empty
        /// </summary>
        /// <returns></returns>
        IHttpTestAsserter ResponseBodyIsEmpty();
        /// <summary>
        /// Check if the response contains specified Content Type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        IHttpTestAsserter ResponseContentTypeEquals(string contentType);
        /// <summary>
        /// Return all Assert messages of the Asserter
        /// </summary>
        /// <returns></returns>
        IEnumerable<AssertResult> AssertAll();
    }
}
