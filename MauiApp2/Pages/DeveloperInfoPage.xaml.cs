namespace MauiApp2.Pages;

public partial class DeveloperInfoPage : ContentPage
{
    public DeveloperInfoPage()
    {
        InitializeComponent();
    }
    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

}