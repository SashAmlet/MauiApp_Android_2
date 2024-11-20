using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.Pages;

public partial class ContactsPage : ContentPage
{
    private List<UContact> _contacts; // Оригінальний список контактів
    private List<UContact> _filteredContacts; // Відфільтрований список
    private readonly IDatabaseService<UContact> _databaseService;

    public ContactsPage()
    {
        InitializeComponent();
        SQLitePCL.Batteries_V2.Init();
        _databaseService = new DatabaseService<UContact>("Contacts");
        LoadContactsAsync();
    }

    private async void LoadContactsAsync()
    {

        try
        {
            _contacts = await _databaseService.GetAllAsync();

            _filteredContacts = new List<UContact>(_contacts);
            ContactsCollectionView.ItemsSource = _filteredContacts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void AddContact(UContact contact)
    {
        try
        {
            _contacts.Add(contact);

            _filteredContacts = new List<UContact>(_contacts);
            ContactsCollectionView.ItemsSource = _contacts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void OnAddContactClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                await DisplayAlert("Error", "All fields are required.", "OK");
                return;
            }

            var contact = new UContact
            {
                Name = FullNameEntry.Text,
                Phone = PhoneNumberEntry.Text,
                Address = AddressEntry.Text
            };

            await _databaseService.SaveAsync(contact);
            AddContact(contact);
            FilterContacts();

            // Очистити поля вводу
            FullNameEntry.Text = string.Empty;
            PhoneNumberEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void FilterContacts()
    {

        var filterText = FilterEntry.Text?.ToLower() ?? string.Empty;

        _filteredContacts = string.IsNullOrWhiteSpace(filterText)
            ? _contacts
            : _contacts.Where(c => c.Name.ToLower().Contains(filterText)).ToList();

        ContactsCollectionView.ItemsSource = _filteredContacts;

    }

    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterContacts();
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
            await DisplayAlert("Error", "Failed to build route.", "Ок");
        }
    }

    private async void ShowRouteOnMap(UContact contact)
    {
        string startAddress = AdditionalAddressEntry.Text;
        if (string.IsNullOrEmpty(startAddress))
        {
            await DisplayAlert("Error", "Enter the starting address.", "Ок");
            return;
        }

        var startLocation = await GeoService.GetCoordinatesFromAddress(startAddress);
        var destinationLocation = await GeoService.GetCoordinatesFromAddress(contact.Address);

        if (startLocation != null && destinationLocation != null)
        {
            await BuildRoute(startLocation, destinationLocation);
        }
        else
        {
            await DisplayAlert("Error", "Failed to get coordinates.", "Ок");
        }
    }
}