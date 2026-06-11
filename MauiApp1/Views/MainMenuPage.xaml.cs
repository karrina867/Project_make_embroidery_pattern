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
            bool answer = await DisplayAlert("Выход из аккаунта", "Вы уверены, что хотите выйти?", "Да", "Нет");

            if (answer)
            {
                var authPage = new AuthPage();
                var navigationPage = new NavigationPage(authPage);
                NavigationPage.SetHasNavigationBar(navigationPage, false);
                Application.Current.MainPage = navigationPage;
            }
        }
    }
}
