using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public class DirectionsService
{
    private const string ApiKey = "AIzaSyAlo3JO2wODu0u0VRC3ABTte1fMt4dwLQ0";

    public async Task<List<Location>> GetRouteCoordinates(Location start, Location end)
    {
        string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={start.Latitude},{start.Longitude}&destination={end.Latitude},{end.Longitude}&key={ApiKey}";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"HTTP error: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Перевіряємо статус відповіді
            if (root.TryGetProperty("status", out var status) && status.GetString() != "OK")
            {
                Console.WriteLine($"Error: {status.GetString()}");
                return null;
            }

            // Отримуємо полілінію з JSON
            if (root.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
            {
                var overviewPolyline = routes[0].GetProperty("overview_polyline");

                if (overviewPolyline.TryGetProperty("points", out var polyline))
                {
                    return DecodePolyline(polyline.GetString());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return null;
        }

        return null;
    }

    private List<Location> DecodePolyline(string encodedPoints)
    {
        if (string.IsNullOrWhiteSpace(encodedPoints))
        {
            Console.WriteLine("Error: empty or invalid polyline.");
            return null;
        }

        var polyline = new List<Location>();
        char[] chars = encodedPoints.ToCharArray();
        int index = 0;

        int currentLat = 0;
        int currentLng = 0;

        try
        {
            while (index < chars.Length)
            {
                int latChange = DecodeNextValue(chars, ref index);
                int lngChange = DecodeNextValue(chars, ref index);

                currentLat += latChange;
                currentLng += lngChange;

                polyline.Add(new Location(currentLat / 1E5, currentLng / 1E5));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Polyline decoding error: {ex.Message}");
            return null;
        }

        return polyline;
    }

    private int DecodeNextValue(char[] chars, ref int index)
    {
        int result = 0;
        int shift = 0;
        int next;

        do
        {
            next = chars[index++] - 63;
            result |= (next & 0x1F) << shift;
            shift += 5;
        } while (next >= 0x20);

        return (result & 1) == 1 ? ~(result >> 1) : (result >> 1);
    }
}