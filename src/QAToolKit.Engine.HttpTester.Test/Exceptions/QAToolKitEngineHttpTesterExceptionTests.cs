using QAToolKit.Engine.HttpTester.Exceptions;
using System;
using Xunit;

namespace QAToolKit.Engine.HttpTester.Test.Exceptions
{
    public class QAToolKitEngineHttpTesterExceptionTests
    {
        [Fact]
        public void CreateExceptionTest_Successful()
        {
            var exception = new QAToolKitEngineHttpTesterException("my error");

            Assert.Equal("my error", exception.Message);
        }

        [Fact]
        public void CreateExceptionWithInnerExceptionTest_Successful()
        {
            var innerException = new Exception("Inner");
            var exception = new QAToolKitEngineHttpTesterException("my error", innerException);

            Assert.Equal("my error", exception.Message);
            Assert.Equal("Inner", innerException.Message);
        }
    }
}
