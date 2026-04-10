using MauiApp1.Services;  // Исправлено пространство имен
using MauiApp1.Models;    // Добавлено для User

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
        }

        private void OnRegisterModeClicked(object sender, EventArgs e)
        {
            LoginForm.IsVisible = false;
            RegisterForm.IsVisible = true;
            LoginModeBtn.BackgroundColor = Color.FromArgb("#9E9E9E");
            RegisterModeBtn.BackgroundColor = Color.FromArgb("#4CAF50");
            MessageLabel.Text = "";
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var (success, message, user) = await _databaseService.LoginUser(
                LoginOrEmailEntry.Text,
                LoginPasswordEntry.Text
            );

            if (success)
            {
                await DisplayAlert("Успех", message, "OK");
                await Navigation.PushAsync(new MainMenuPage(user)); // Теперь открываем меню
            }
            else
            {
                MessageLabel.Text = message;
                MessageLabel.TextColor = Colors.Red;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var (success, message) = await _databaseService.RegisterUser(
                RegLoginEntry.Text,
                RegEmailEntry.Text,
                RegPasswordEntry.Text,
                RegConfirmPasswordEntry.Text
            );

            if (success)
            {
                await DisplayAlert("Успех", message, "OK");
                RegLoginEntry.Text = "";
                RegEmailEntry.Text = "";
                RegPasswordEntry.Text = "";
                RegConfirmPasswordEntry.Text = "";

                OnLoginModeClicked(null, null);
            }
            else
            {
                MessageLabel.Text = message;
                MessageLabel.TextColor = Colors.Red;
            }
        }
    }
}