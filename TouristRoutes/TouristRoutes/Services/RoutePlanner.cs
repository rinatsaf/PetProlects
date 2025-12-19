using System.Text.Json;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Services;

public class RoutePlanner : IRoutePlanner
{
    public RouteResponse PlanRoute(RouteBuildRequest request, List<Poi> pois)
    {
        var availableTime = request.EndTime - request.StartTime;
        if (availableTime <= TimeSpan.Zero)
            throw new ArgumentException("EndTime must be greater than StartTime");

        var availableBudget = request.Budget;

        double currentLat = request.UserLat;
        double currentLon = request.UserLon;

        TimeSpan currentTime = request.StartTime;

        var result = new RouteResponse(){ Name = request.Name };
        
        var points = new List<RoutePoint>();

        // 1. Фильтрация категорий
        if (request.PreferredCategories?.Count > 0)
        {
            pois = pois.Where(p => request.PreferredCategories.Contains(p.Category)).ToList();
        }

        // 2. Фильтрация по времени работы
        pois = pois.Where(p => IsOpenAt(p, request.StartTime)).ToList();

        while (true)
        {
            // выбираем ближайший POI, который помещается по времени/бюджету и открыт к моменту прибытия
            var next = pois
                .Select(p =>
                {
                    var dist = Haversine(currentLat, currentLon, p.Lat, p.Lon);
                    var travel = TimeSpan.FromMinutes(dist / 3.5 * 60); // пешком
                    var arrival = currentTime + travel;

                    return new
                    {
                        Poi = p,
                        Distance = dist,
                        Travel = travel,
                        Arrival = arrival,
                        Visit = TimeSpan.FromMinutes(p.VisitDurationMin)
                    };
                })
                .Where(x =>
                    x.Travel + x.Visit <= availableTime &&
                    x.Poi.EntranceFeeCents / 100m <= availableBudget &&
                    IsOpenAt(x.Poi, x.Arrival))
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (next == null)
                break;

            // travel
            double distKm = next.Distance;
            var travelTimeSpan = next.Travel;

            // arrival/departure times
            var arrival = next.Arrival;
            var departure = arrival + next.Visit;

            // **добавляем новую точку**
            points.Add(new RoutePoint
            {
                Poi = next.Poi,
                ArrivalTime = arrival,
                DepartureTime = departure,
                // пока не знаем расстояние и время до следующего — поставим 0,
                // обновим позже, когда появится следующая точка
                DistanceToNextKm = 0,
                TravelTimeToNextMin = 0
            });

            // обновляем ограничения
            currentTime = departure;
            availableTime -= (travelTimeSpan + next.Visit);
            availableBudget -= next.Poi.EntranceFeeCents / 100m;

            result.TotalDistanceKm += distKm;
            result.TotalTravelTime += travelTimeSpan;
            result.TotalCost += next.Poi.EntranceFeeCents / 100m;
            result.TotalDuration += travelTimeSpan + next.Visit;

            // обновляем позицию
            currentLat = next.Poi.Lat;
            currentLon = next.Poi.Lon;

            pois.Remove(next.Poi);
        }

        // 3. Теперь делаем TravelTimeToNextMin/DistanceToNextKm осмысленными
        for (int i = 0; i < points.Count - 1; i++)
        {
            var curr = points[i];
            var next = points[i + 1];

            var dist = Haversine(curr.Poi.Lat, curr.Poi.Lon, next.Poi.Lat, next.Poi.Lon);
            var tmin = dist / 3.5 * 60;

            curr.DistanceToNextKm = dist;
            curr.TravelTimeToNextMin = tmin;
        }

        result.Points = points;
        result.RouteSummary = BuildSummary(result);

        return result;
    }

    private string BuildSummary(RouteResponse r)
    {
        if (r.Points.Count == 0)
            return "Не удалось построить маршрут по заданным условиям.";

        return
            $"Точек: {r.Points.Count}. " +
            $"Дистанция: {r.TotalDistanceKm:F1} км. " +
            $"Стоимость: {r.TotalCost:F0}₽. " +
            $"Время: {r.TotalDuration.TotalMinutes:F0} минут.";
    }

    private bool IsOpenAt(Poi poi, TimeSpan time)
    {
        if (string.IsNullOrEmpty(poi.OpeningHoursJson))
            return true;

        Dictionary<string, List<int>>? dict;
        try
        {
            dict = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(poi.OpeningHoursJson);
        }
        catch
        {
            return true;
        }

        if (dict == null)
            return true;
        var today = DateTime.UtcNow.DayOfWeek.ToString();

        if (!dict.TryGetValue(today, out var hours))
            return true;

        var open = TimeSpan.FromMinutes(hours[0]);
        var close = TimeSpan.FromMinutes(hours[1]);

        return time >= open && time <= close;
    }

    private double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
