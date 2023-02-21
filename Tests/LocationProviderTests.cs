using System.Diagnostics;
using Implementation;
using JetBrains.dotMemoryUnit;

namespace Tests
{
    public class LocationProviderTests
    {
        private ILocationProvider locationProvider = null!;

        [SetUp]
        public void Setup()
        {
            locationProvider = new LocationProvider();
        }

        public static IEnumerable<(string Name, double Latitude, double Longitude, int MaxDistance, int Limit)> TestCases()
        {
            return new[]
            {
                ("HeatUp", 50.91414, 5.95549, 50000, 5),
                ("SimpleRun", 50.91414, 5.95549, 50000, 50),
                ("SimpleRunEnlarged", 50.91414, 5.95549, 50000, 500),
                ("SimpleRunMaxSize", 50.91414, 5.95549, 50000, 100000)
            };
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task GetNearbyLocations_PerformanceTest((string Name, double Latitude, double Longitude, int MaxDistance, int Limit) testCase)
        {
            // Arrange
            var stopwatch = new Stopwatch();
            stopwatch.Restart();

            // Act
            var locations = await locationProvider.GetNearbyLocations(new Location(testCase.Latitude, testCase.Longitude), testCase.MaxDistance, testCase.Limit);

            // Assert
            stopwatch.Stop();

            Assert.NotNull(locations);
            Assert.LessOrEqual(locations.Count(), testCase.Limit);
            Assert.LessOrEqual(stopwatch.Elapsed.Milliseconds, 200);

            // Statistics
            Console.WriteLine($"For {testCase.Name} it takes {stopwatch.Elapsed} and returns {locations.Count()} of maximum {testCase.Limit} results.");
        }

        [Test]
        [Ignore("Should be started under dotMemory Unit")]
        [TestCaseSource(nameof(TestCases))]
        [DotMemoryUnit(CollectAllocations = true)]
        [AssertTraffic(AllocatedSizeInBytes = 1024 * 1024 * 150)]
        public async Task GetNearbyLocations_MemoryTest((string Name, double Latitude, double Longitude, int MaxDistance, int Limit) testCase)
        {
            // Arrange
            const int memoryDelta = 20000;
            const int memoryPerItem = 150;
            var checkPoint1 = dotMemory.Check();

            // Act
            await locationProvider.GetNearbyLocations(new Location(testCase.Latitude, testCase.Longitude), testCase.MaxDistance, testCase.Limit);

            // Assert
            dotMemory.Check(memory =>
            {
                var allocatedBytes = memory.GetTrafficFrom(checkPoint1).AllocatedMemory.SizeInBytes;
                var collectedBytes = memory.GetTrafficFrom(checkPoint1).CollectedMemory.SizeInBytes;
                Assert.LessOrEqual(allocatedBytes - collectedBytes, testCase.Limit * memoryPerItem + memoryDelta);

                Console.WriteLine($"For {testCase.Name} {allocatedBytes} bytes were allocated and {collectedBytes} bytes were collected after.");
            });
        }
    }
}