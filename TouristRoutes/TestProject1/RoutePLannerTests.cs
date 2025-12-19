using FluentAssertions;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.Entity;
using TouristRoutes.Services;

namespace TestProject1;

public class RoutePlannerTests
{
    [Fact]
    public void PlanRoute_EndBeforeStart_ShouldThrow()
    {
        var planner = new RoutePlanner();
        var request = BaseRequest();
        request.EndTime = request.StartTime;
        
        Action ac = () => planner.PlanRoute(request, new List<Poi>());
        ac.Should().Throw<ArgumentException>().WithMessage("EndTime must be greater than StartTime"); 
    }

    [Fact]
    public void PlanRoute_ShouldRespectBudget()
    {
        var planner = new RoutePlanner();
        var request = BaseRequest();
        request.Budget = 100;

        var pois = new List<Poi>
        {
            TestPois.Poi("Cheap", 55.751, 49.101, fee: 0),
            TestPois.Poi("Expensive", 55.752, 49.102, fee: 50000)
        };
        
        var route = planner.PlanRoute(request, pois);

        route.TotalCost.Should().BeLessThanOrEqualTo(100);
        route.Points.Should().OnlyContain(p => p.Poi.EntranceFeeCents <= 10000);
    }
    
    [Fact]
    public void PlanRoute_ShouldFilterByCategory()
    {
        var planner = new RoutePlanner();
        var request = BaseRequest();
        request.PreferredCategories = new() { "Музей" };

        var pois = new List<Poi>
        {
            TestPois.Poi("Museum", 55.751, 49.101, "Музей"),
            TestPois.Poi("Park", 55.752, 49.102, "Парк")
        };

        var route = planner.PlanRoute(request, pois);

        route.Points.Should().HaveCount(1);
        route.Points[0].Poi.Category.Should().Be("Музей");
    }
    
    [Fact]
    public void PlanRoute_WhenNoPoisFit_ShouldReturnEmptyRoute()
    {
        var planner = new RoutePlanner();
        var request = BaseRequest();
        request.Budget = 0;
        request.EndTime = request.StartTime + TimeSpan.FromMinutes(30);

        var pois = new List<Poi>
        {
            TestPois.Poi("Long visit", 55.75, 49.10, visitMin: 120)
        };

        var route = planner.PlanRoute(request, pois);

        route.Points.Should().BeEmpty();
        route.RouteSummary.Should().Contain("Не удалось");
    }
    [Fact]
    public void PlanRoute_ShouldHaveValidArrivalAndDepartureTimes()
    {
        var planner = new RoutePlanner();
        var request = BaseRequest();

        var pois = new List<Poi>
        {
            TestPois.Poi("A", 55.751, 49.101),
            TestPois.Poi("B", 55.752, 49.102)
        };

        var route = planner.PlanRoute(request, pois);

        route.Points.Should().OnlyContain(p =>
            p.ArrivalTime < p.DepartureTime);
    }
    

    private static RouteBuildRequest BaseRequest() => new RouteBuildRequest()
    {
        Name = "Маршрут по Казани",
        City = "Казань",
        Budget = 1000m,
        UserLat = 55.796127,
        UserLon = 49.106414,
        StartTime = TimeSpan.FromHours(9),
        EndTime = TimeSpan.FromHours(18),
        TransportType = "Walk",
        PreferredCategories = new List<string>()
    };
}