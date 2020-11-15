using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace QAToolKit.Engine.HttpTester.Extensions
{
    /// <summary>
    /// Http Response message extensions
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Get response body object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<T> GetResponseBody<T>(this HttpResponseMessage httpResponseMessage)
        {
            var bodyResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyResponse);
        }
    }
}
