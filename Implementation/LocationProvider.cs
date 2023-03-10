using System.IO.MemoryMappedFiles;

namespace Implementation
{
    /// <summary>
    /// Service responsible for providing locations information.
    /// </summary>
    public interface ILocationProvider
    {
        /// <summary>
        /// Searches for up to maxResults locations around the passed one in maxDistance radius area.
        /// </summary>
        /// <param name="location">Current location.</param>
        /// <param name="maxDistance">Maximum distance of another locations that will be searched for.</param>
        /// <param name="maxResults">Limits the maximum of provided results.</param>
        /// <returns>Ordered set of LocationDistance object, which provides name of the location nearby and the distance to it.</returns>
        /// <exception cref="CsvProcessingException">Throws in case of CSV data parsing error.</exception>
        Task<IEnumerable<LocationDistance>> GetNearbyLocations(Location location, int maxDistance, int maxResults);
    }

    /// <inheritdoc/>
    public class LocationProvider : ILocationProvider
    {
        /// <inheritdoc/>
        public async Task<IEnumerable<LocationDistance>> GetNearbyLocations(Location location, int maxDistance, int maxResults)
        {
            const string path = "Resources\\locations.csv";
            const string delimiter = "\",\"";

            var resultList = new List<LocationDistance>();

            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open);
            await using var stream = file.CreateViewStream();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrEmpty(line) //skip empty lines
                    || line.Contains('\u0000') //skip end of file
                    || !line.StartsWith("\"")) //skip header or improperly formatted lines
                {
                    continue;
                }
                
                if (line.Trim('"').Split(delimiter) is [var name, var sLatitude, var sLongitude] &&
                    double.TryParse(sLatitude.Trim('"'), out var latitude) &&
                    double.TryParse(sLongitude.Trim('"'), out var longitude))
                {
                    var remoteLocation = new Location(latitude, longitude);
                    var distance = location.CalculateDistance(remoteLocation);
                    if (distance <= maxDistance)
                    {
                        resultList.Add(new LocationDistance(name.Trim('"'), distance));
                    }
                }
                else
                {
                    throw new CsvProcessingException($"CSV reading failed on line parsing. Line: {line}");
                }
            }

            return resultList.OrderBy(x => x.Distance).Take(maxResults).ToArray();
        }
    }
}