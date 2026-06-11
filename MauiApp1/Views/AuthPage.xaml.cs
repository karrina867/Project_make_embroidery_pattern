using MauiApp1.Services;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class AuthPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public AuthPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private void OnLoginModeClicked(object sender, EventArgs e)
        {
            LoginForm.IsVisible = true;
            RegisterForm.IsVisible = false;
            LoginModeBtn.BackgroundColor = Color.FromArgb("#2196F3");
            RegisterModeBtn.BackgroundColor = Color.FromArgb("#9E9E9E");
            MessageLabel.Text = "";

            AppTitleLabel.TextColor = Color.FromArgb("#2196F3");
        }

        private void OnRegisterModeClicked(object sender, EventArgs e)
        {
            LoginForm.IsVisible = false;
            RegisterForm.IsVisible = true;
            LoginModeBtn.BackgroundColor = Color.FromArgb("#9E9E9E");
            RegisterModeBtn.BackgroundColor = Color.FromArgb("#4CAF50");
            MessageLabel.Text = "";
            AppTitleLabel.TextColor = Color.FromArgb("#4CAF50");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var (success, message, user) = await _databaseService.LoginUser(
                LoginOrEmailEntry.Text,
                LoginPasswordEntry.Text
            );

            if (success && user != null)
            {
               // await DisplayAlert("Успех", message, "OK");

                var mainMenuPage = new MainMenuPage(user);
                var navigationPage = new NavigationPage(mainMenuPage);
                NavigationPage.SetHasNavigationBar(navigationPage, false);
                Application.Current.MainPage = navigationPage;
            }
            else
            {
                MessageLabel.Text = message;
                MessageLabel.TextColor = Colors.Red;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var login = RegLoginEntry.Text;
            var email = RegEmailEntry.Text;
            var password = RegPasswordEntry.Text;
            var confirmPassword = RegConfirmPasswordEntry.Text;

            var (success, message) = await _databaseService.RegisterUser(
                login, email, password, confirmPassword
            );

            if (success)
            {
                
                RegLoginEntry.Text = "";
                RegEmailEntry.Text = "";
                RegPasswordEntry.Text = "";
                RegConfirmPasswordEntry.Text = "";

               
                var (loginSuccess, loginMessage, user) = await _databaseService.LoginUser(login, password);


                if (loginSuccess && user != null)
                {
                   // await DisplayAlert("Успех", "Регистрация пройдена! Выполняется вход...", "OK");

                    var mainMenuPage = new MainMenuPage(user);
                    var navigationPage = new NavigationPage(mainMenuPage);
                    NavigationPage.SetHasNavigationBar(navigationPage, false);
                    Application.Current.MainPage = navigationPage;
                }
               
                else
                {
                    // Если автоматический вход не сработал, показываем форму входа
                    await DisplayAlert("Успех", "Регистрация пройдена! Теперь войдите в систему.", "OK");
                    OnLoginModeClicked(null, null);

                    // Автозаполняем поля входа
                    LoginOrEmailEntry.Text = login;
                    LoginPasswordEntry.Text = password;
                }
            }
            else
            {
                MessageLabel.Text = message;
                MessageLabel.TextColor = Colors.Red;
            }
        }
    }
}