using MauiApp1.Views;  // Добавлено

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AuthPage());  // Views.AuthPage можно сократить до AuthPage, если есть using
        }
    }
}