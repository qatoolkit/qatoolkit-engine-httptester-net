namespace QAToolKit.Engine.HttpTester.Test.Fixtures
{
    public class Bicycle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public BicycleType Type { get; set; }
    }

    public enum BicycleType
    {
        Road,
        Gravel,
        Mountain,
        City
    }
}
