namespace QAToolKit.Engine.HttpTester.Models
{
    /// <summary>
    /// Assert result object
    /// </summary>
    public class AssertResult
    {
        /// <summary>
        /// Assert name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Is assert true
        /// </summary>
        public bool IsTrue { get; set; }
        /// <summary>
        /// Assert message
        /// </summary>
        public string Message { get; set; }
    }
}
