using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.Pages;

public partial class MainPage : ContentPage
{
    private readonly IDatabaseService<Student> _databaseService;
    private List<Student> _students; // Оригінальний список контактів
    private List<Student> _filteredStudents; // Відфільтрований список

    public MainPage()
    {
        InitializeComponent();
        SQLitePCL.Batteries_V2.Init();
        _databaseService = new DatabaseService<Student>("Students");
        LoadStudentsAsync();
    }

    private async void LoadStudentsAsync()
    {
        try
        {
            _students = await _databaseService.GetAllAsync();

            _filteredStudents = new List<Student>(_students);
            StudentsCollectionView.ItemsSource = _students;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void AddStudent(Student student)
    {
        try
        {
            _students.Add(student);

            _filteredStudents = new List<Student>(_students);
            FilterStudents();
            StudentsCollectionView.ItemsSource = _students;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void FilterStudents()
    {
        if (!string.IsNullOrWhiteSpace(ThresholdEntry.Text))
        {
            var threshold = double.Parse(ThresholdEntry.Text);

            _filteredStudents = _students.Where(c => c.AverageGrade > threshold).ToList();
            FilteredStudentsCollectionView.ItemsSource = _filteredStudents;

            var percentage = _filteredStudents.Count > 0
                ? (_filteredStudents.Count / (double)(await _databaseService.GetAllAsync()).Count) * 100
                : 0;

            PercentageLabel.Text = $"Selected Percentage: {percentage:0.00}%";

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

            await _databaseService.SaveAsync(student);
            AddStudent(student);

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

            FilterStudents();
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