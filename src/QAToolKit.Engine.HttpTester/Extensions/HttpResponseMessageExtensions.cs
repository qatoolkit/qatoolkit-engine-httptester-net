using System;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        [Obsolete("This method is obsolete and will be deprecated. Use 'GetResponseJsonBody<T>' instead.")]
        public static async Task<T> GetResponseBody<T>(this HttpResponseMessage httpResponseMessage)
        {
            var bodyResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyResponse);
        }
        
        /// <summary>
        /// Deserialize JSON response body to object
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetResponseJsonBody<T>(this HttpResponseMessage httpResponseMessage)
        {
            var bodyResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyResponse);
        }
        
        /// <summary>
        /// Deserialize XML response body to object
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetResponseXmlBody<T>(this HttpResponseMessage httpResponseMessage)
        {
            var bodyResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            var xmlSerialize = new XmlSerializer(typeof(T));  
  
            var xmlResult = (T)xmlSerialize.Deserialize(new StringReader(bodyResponse));  
  
            return xmlResult ?? default(T);
        }

        /// <summary>
        /// Get response as string
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<string> GetResponseBodyString(this HttpResponseMessage httpResponseMessage)
        {
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get response as byte array
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetResponseBodyBytes(this HttpResponseMessage httpResponseMessage)
        {
            return await httpResponseMessage.Content.ReadAsByteArrayAsync();
        }
    }
}
