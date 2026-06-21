using MauiApp1.Services;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class PatternDetailsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;

        public PatternDetailsPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;
            PatternNameLabel.Text = pattern.Name;
            PatternSizeLabel.Text = $"{pattern.Width * pattern.Height} крестиков";
            PatternColorsLabel.Text = $"{pattern.ColorCount} цветов";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadColors();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async Task LoadColors()
        {
            try
            {
                var colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);
                ColorsCollection.ItemsSource = colors;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить цвета: {ex.Message}", "OK");
            }
        }
    }
}
