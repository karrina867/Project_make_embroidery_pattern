using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class MainMenuPage : ContentPage
    {
        private User _currentUser;

        public MainMenuPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private async void OnCreatePatternClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreatePatternPage(_currentUser));
        }

        private async void OnMyPatternsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyPatternsPage(_currentUser));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AuthPage());
        }
    }
}
