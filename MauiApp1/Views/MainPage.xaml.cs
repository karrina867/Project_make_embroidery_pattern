namespace MauiApp1.Views
{
    public partial class MainPage : ContentPage
    {
        private Models.User _currentUser;

        public MainPage(Models.User user)
        {
            InitializeComponent();
            _currentUser = user;
            UserInfoLabel.Text = $"Вы вошли как: {user.Login}\nEmail: {user.Email}";
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AuthPage());
        }
    }
}