using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.Pages;

public partial class MainPage : ContentPage
{
    private readonly IDatabaseService _databaseService;

    public MainPage()
    {
        InitializeComponent();
        SQLitePCL.Batteries_V2.Init();
        _databaseService = new DatabaseService();
        LoadStudentsAsync();
    }

    private async void LoadStudentsAsync()
    {
        try
        {
            var students = await _databaseService.GetAllStudentsAsync();
            StudentsCollectionView.ItemsSource = students;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(Subject1GradeEntry.Text) ||
                string.IsNullOrWhiteSpace(Subject2GradeEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                await DisplayAlert("Error", "All fields are required.", "OK");
                return;
            }

            var student = new Student
            {
                FullName = FullNameEntry.Text,
                Subject1Grade = double.Parse(Subject1GradeEntry.Text),
                Subject2Grade = double.Parse(Subject2GradeEntry.Text),
                Address = AddressEntry.Text
            };

            await _databaseService.SaveStudentAsync(student);
            LoadStudentsAsync();

            // Очистити поля вводу
            FullNameEntry.Text = string.Empty;
            Subject1GradeEntry.Text = string.Empty;
            Subject2GradeEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnApplyFilterClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ThresholdEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a threshold.", "OK");
                return;
            }

            var threshold = double.Parse(ThresholdEntry.Text);
            var filteredStudents = await _databaseService.GetFilteredStudentsAsync(threshold);
            FilteredStudentsCollectionView.ItemsSource = filteredStudents;

            var percentage = filteredStudents.Count > 0
                ? (filteredStudents.Count / (double)(await _databaseService.GetAllStudentsAsync()).Count) * 100
                : 0;

            PercentageLabel.Text = $"Selected Percentage: {percentage:0.00}%";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Method for displaying "about the developer"
    private async void OnInstructionsClicked(object sender, EventArgs e)
    {
        var developerInfoView = new DeveloperInfoPage();
        var contentPage = new ContentPage
        {
            Content = developerInfoView.Content
        };

        await Shell.Current.Navigation.PushModalAsync(contentPage);
    }

    private async void OnAddressBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ContactsPage());
    }
}