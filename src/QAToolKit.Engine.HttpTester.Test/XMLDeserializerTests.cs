using System;
using System.Net.Http;
using System.Threading.Tasks;
using QAToolKit.Engine.HttpTester.Extensions;
using QAToolKit.Engine.HttpTester.Test.Fixtures;
using Xunit;

namespace QAToolKit.Engine.HttpTester.Test
{
    public class XmlDeserializerTests
    {
        [Fact]
        public async Task HttpTesterClientSimple_Success()
        {
            using (var client = new HttpTesterClient())
            {
                var response = await client
                    .CreateHttpRequest(new Uri("https://www.w3schools.com/xml/note.xml"))
                    .WithMethod(HttpMethod.Get)
                    .Start();

                var msg = await response.GetResponseXmlBody<note>();

                Assert.True(client.Duration < 2000);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("Reminder", msg.heading);
                Assert.Equal("Jani", msg.from);
            }
        }
    }
}