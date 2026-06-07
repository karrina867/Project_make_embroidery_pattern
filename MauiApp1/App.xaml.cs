using MauiApp1.Services;
using MauiApp1.Views;  // Добавлено

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            
            InitializeComponent();


            //удалить потом
            //var dbService = new DatabaseService();
            //dbService.DeleteDatabase();


            // Создаём NavigationPage
            var navigationPage = new NavigationPage(new AuthPage());

            // Отключаем навигационную панель через статический метод
            NavigationPage.SetHasNavigationBar(navigationPage, false);

            MainPage = navigationPage;


            //NavigationPage.SetHasNavigationBar(this, false);


          // MainPage = new NavigationPage(new AuthPage());  // Views.AuthPage можно сократить до AuthPage, если есть using
        }
    }
}