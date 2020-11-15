using System.Collections.Generic;

namespace QAToolKit.Engine.HttpTester.Test.Fixtures
{
    public static class BicycleFixture
    {
        public static Bicycle Get()
        {
            return new Bicycle
            {
                Id = 10,
                Name = "Badger",
                Brand = "Cannondale",
                Type = BicycleType.Road
            };
        }

        public static Bicycle GetFoil()
        {
            return new Bicycle
            {
                Id = 1,
                Name = "Foil",
                Brand = "Scott",
                Type = BicycleType.Road
            };
        }

        public static Bicycle GetCfr()
        {
            return new Bicycle
            {
                Id = 5,
                Name = "EXCEED CFR",
                Brand = "Giant",
                Type = BicycleType.Mountain
            };
        }

        public static List<Bicycle> GetBicycles()
        {
            return new List<Bicycle>() {
                new Bicycle
                {
                    Id = 1,
                    Name = "Foil",
                    Brand = "Scott",
                     Type = BicycleType.Road
                },
                new Bicycle
                {
                   Id = 2,
                   Name = "CAADX",
                   Brand = "Cannondale",
                   Type = BicycleType.Gravel
                },
                new Bicycle
                {
                   Id = 3,
                   Name = "Turbo Vado SL Equipped",
                   Brand = "Specialized",
                   Type = BicycleType.City
                },
                new Bicycle
                {
                  Id = 4,
                  Name = "EXCEED CFR",
                  Brand = "Canyon",
                  Type = BicycleType.Mountain
                },
        };
        }
    }
}
