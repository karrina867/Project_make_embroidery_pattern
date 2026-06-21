using MauiApp1.Services;
using MauiApp1.Views; 

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



            var navigationPage = new NavigationPage(new AuthPage());

            NavigationPage.SetHasNavigationBar(navigationPage, false);

            MainPage = navigationPage;

        }
    }
}