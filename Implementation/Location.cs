namespace Implementation;

public record Location(double Latitude, double Longitude)
{
    /// <summary>
    /// Calculates the distance between this location and another one, in meters.
    /// </summary>
    public double CalculateDistance(Location location)
    {
        var rlat1 = Math.PI * Latitude / 180;
        var rlat2 = Math.PI * location.Latitude / 180;
        var rlon1 = Math.PI * Longitude / 180;
        var rlon2 = Math.PI * location.Longitude / 180;
        var theta = Longitude - location.Longitude;
        var rtheta = Math.PI * theta / 180;
        var dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(dist);
        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;

        return dist * 1609.344;
    }
}