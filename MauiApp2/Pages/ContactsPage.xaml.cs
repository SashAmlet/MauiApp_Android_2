using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.Pages;

public partial class ContactsPage : ContentPage
{
    private List<UContact> _contacts; // Оригінальний список контактів
    private List<UContact> _filteredContacts; // Відфільтрований список

    public ContactsPage()
    {
        InitializeComponent();
        LoadContacts();
    }

    private void LoadContacts()
    {
        _contacts = new List<UContact>
        {
            new UContact { Name = "Ivan Ivanov", Phone = "+380501234567", Address = "Київ, вул. Хрещатик, 1" },
            new UContact { Name = "Maria Petrovna", Phone = "+380671234567", Address = "Одеса, вул. Дерибасівська, 10" }
        };

        _filteredContacts = new List<UContact>(_contacts);
        ContactsCollectionView.ItemsSource = _filteredContacts;
    }

    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        var filterText = FilterEntry.Text?.ToLower() ?? string.Empty;

        _filteredContacts = string.IsNullOrWhiteSpace(filterText)
            ? _contacts
            : _contacts.Where(c => c.Name.ToLower().Contains(filterText)).ToList();

        ContactsCollectionView.ItemsSource = _filteredContacts;
    }

    private void OnContactButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var contact = button?.BindingContext as UContact;

        if (contact == null)
            return;

        ShowRouteOnMap(contact);
    }
    private async Task BuildRoute(Location startLocation, Location destinationLocation)
    {
        var directionsService = new DirectionsService();
        var routeCoordinates = await directionsService.GetRouteCoordinates(startLocation, destinationLocation);

        if (routeCoordinates != null)
        {
            var route = new Polyline
            {
                StrokeColor = Color.FromArgb("#0000FF"),
                StrokeWidth = 5
            };

            foreach (var point in routeCoordinates)
                route.Geopath.Add(point);

            Map.MapElements.Clear();
            Map.MapElements.Add(route);

            Map.Pins.Clear();
            Map.Pins.Add(new Pin
            {
                Label = "Початок",
                Location = startLocation,
                Type = PinType.Place
            });
            Map.Pins.Add(new Pin
            {
                Label = "Кінець",
                Location = destinationLocation,
                Type = PinType.Place
            });

            Map.MoveToRegion(MapSpan.FromCenterAndRadius(startLocation, Distance.FromMiles(1)));
        }
        else
        {
            await DisplayAlert("Помилка", "Не вдалося побудувати маршрут.", "Ок");
        }
    }

    private async void ShowRouteOnMap(UContact contact)
    {
        string startAddress = AdditionalAddressEntry.Text;
        if (string.IsNullOrEmpty(startAddress))
        {
            await DisplayAlert("Помилка", "Введіть початкову адресу.", "Ок");
            return;
        }

        var startLocation = await GetCoordinatesFromAddress(startAddress);
        var destinationLocation = GeoService.GetCoordinatesFromAddress(contact.Address);

        if (startLocation != null && destinationLocation != null)
        {
            await BuildRoute(startLocation, destinationLocation);
        }
        else
        {
            await DisplayAlert("Помилка", "Не вдалося отримати координати.", "Ок");
        }
    }
    private async Task<Location> GetCoordinatesFromAddress(string address)
    {
        // Замініть цей метод на реальну логіку для отримання координат через геосервіс (наприклад, через Google Maps API)
        // Для демонстрації повертається фіксована позиція
        return new Location(50.45316305193355, 30.51392281079863); // Повертає координати для Ванкувера
    }
}