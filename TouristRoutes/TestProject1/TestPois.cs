using TouristRoutes.Models.Entity;

namespace TestProject1;

public static class TestPois
{
    public static Poi Poi(
        string name,
        double lat,
        double lon,
        string category = "Исторический",
        int visitMin = 60,
        int fee = 0
    ) => new Poi
    {
        Name = name,
        Lat = lat,
        Lon = lon,
        Category = category,
        VisitDurationMin = visitMin,
        EntranceFeeCents = fee
    };
}