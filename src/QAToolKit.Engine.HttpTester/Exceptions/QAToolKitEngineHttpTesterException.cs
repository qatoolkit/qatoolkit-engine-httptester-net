using System;
using System.Runtime.Serialization;

namespace QAToolKit.Engine.HttpTester.Exceptions
{
    /// <summary>
    /// QA Toolkit core exception
    /// </summary>
    [Serializable]
    public class QAToolKitEngineHttpTesterException
        : Exception
    {
        /// <summary>
        /// QA Toolkit core exception
        /// </summary>
        public QAToolKitEngineHttpTesterException(string message) : base(message)
        {
        }

        /// <summary>
        /// QA Toolkit core exception
        /// </summary>
        public QAToolKitEngineHttpTesterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
