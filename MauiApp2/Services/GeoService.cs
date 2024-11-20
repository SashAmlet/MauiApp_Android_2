using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public static class GeoService
{
    private const string ApiKey = "AIzaSyAlo3JO2wODu0u0VRC3ABTte1fMt4dwLQ0";

    public static async Task<Location> GetCoordinatesFromAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("The address cannot be empty.", nameof(address));

        string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKey}";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"An error occurred while requesting the Google Maps API: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Перевірка статусу відповіді
        if (root.TryGetProperty("status", out var status) && status.GetString() != "OK")
        {
            Console.WriteLine($"Error: {status.GetString()}");
            return null;
        }

        // Отримання координат
        if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
        {
            var location = results[0].GetProperty("geometry").GetProperty("location");
            var lat = location.GetProperty("lat").GetDouble();
            var lng = location.GetProperty("lng").GetDouble();

            return new Location(lat, lng);
        }

        Console.WriteLine("Coordinates not found.");
        return null;
    }
}
