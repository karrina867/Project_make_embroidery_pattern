using MauiApp1.Services;
using MauiApp1.Models;
using System.Collections.ObjectModel;

namespace MauiApp1.Views
{
    public partial class MyPatternsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<PatternViewModel> _allPatterns;
        private ObservableCollection<PatternViewModel> _filteredPatterns;

        public MyPatternsPage(User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _currentUser = user;
            _allPatterns = new ObservableCollection<PatternViewModel>();
            _filteredPatterns = new ObservableCollection<PatternViewModel>();
            PatternsCollection.ItemsSource = _filteredPatterns;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPatterns();
        }

        private async void LoadPatterns()
        {
            try
            {
                // Получаем только схемы текущего пользователя
                var patterns = await _databaseService.GetUserPatterns(_currentUser.Id);

                _allPatterns.Clear();
                foreach (var pattern in patterns)
                {
                    var imagePath = _databaseService.GetImagePath(pattern.ImageFileName);

                    if (!File.Exists(imagePath))
                    {
                        imagePath = null;
                    }

                    _allPatterns.Add(new PatternViewModel
                    {
                        Id = pattern.Id,
                        Name = pattern.Name,
                        Width = pattern.Width,
                        Height = pattern.Height,
                        ColorCount = pattern.ColorCount,
                        CreatedAt = pattern.CreatedAt,
                        ImagePath = imagePath,
                        Info = $"{pattern.Width}x{pattern.Height} крестиков | {pattern.ColorCount} цветов | {pattern.CreatedAt:dd.MM.yyyy}"
                    });
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить схемы: {ex.Message}", "OK");
            }
        }


        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filteredPatterns.Clear();

            var searchText = SearchBar?.Text?.ToLower() ?? "";

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allPatterns
                : new ObservableCollection<PatternViewModel>(
                    _allPatterns.Where(p => p.Name.ToLower().Contains(searchText)));

            foreach (var item in filtered)
            {
                _filteredPatterns.Add(item);
            }

            EmptyLabel.IsVisible = _filteredPatterns.Count == 0;
        }

        //    private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        //    {
        //        if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
        //        {
        //            // Спрашиваем, что делать со схемой
        //            var action = await DisplayActionSheet(
        //                $"Схема: {selectedPattern.Name}",
        //                "Отмена",
        //                null,
        //                "Посмотреть схему",
        //                "Удалить схему");

        //            if (action == "Удалить схему")
        //            {
        //                var confirm = await DisplayAlert("Удаление",
        //                    $"Вы уверены, что хотите удалить схему '{selectedPattern.Name}'?",
        //                    "Да", "Нет");

        //                if (confirm)
        //                {
        //                    var pattern = new EmbroideryPattern
        //                    {
        //                        Id = selectedPattern.Id,
        //                        ImageFileName = Path.GetFileName(selectedPattern.ImagePath)
        //                    };

        //                    var deleted = await _databaseService.DeletePattern(pattern);
        //                    if (deleted)
        //                    {
        //                        await DisplayAlert("Успех", "Схема удалена", "OK");
        //                        LoadPatterns();
        //                    }
        //                    else
        //                    {
        //                        await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
        //                    }
        //                }
        //            }
        //            else if (action == "Посмотреть схему")
        //            {
        //                // Здесь будет открытие схемы
        //                await DisplayAlert("Просмотр", "Функция просмотра схемы будет добавлена позже", "OK");
        //            }
        //        }

        //        // Сбрасываем выделение
        //        PatternsCollection.SelectedItem = null;
        //    }
        //}
        //private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
        //    {
        //        // Получаем полную схему с проверкой прав
        //        var pattern = await _databaseService.GetPatternById(selectedPattern.Id, _currentUser.Id);

        //        if (pattern != null)
        //        {
        //            await Navigation.PushAsync(new PatternDetailsPage(pattern, _currentUser));
        //        }
        //        else
        //        {
        //            await DisplayAlert("Ошибка", "Схема не найдена или доступ запрещен", "OK");
        //        }
        //    }

        //    PatternsCollection.SelectedItem = null;
        //} 

        private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
            {
                var action = await DisplayActionSheet(
                    $"Схема: {selectedPattern.Name}",
                    "Отмена",
                    null,
                    "👁️ Просмотреть схему",
                    "📊 Посмотреть цвета",
                    "🗑️ Удалить схему");

                var pattern = await _databaseService.GetPatternById(selectedPattern.Id, _currentUser.Id);

                if (pattern == null)
                {
                    await DisplayAlert("Ошибка", "Схема не найдена", "OK");
                    return;
                }

                if (action == "👁️ Просмотреть схему")
                {
                    await Navigation.PushAsync(new PatternViewerPage(pattern, _currentUser));
                }
                else if (action == "📊 Посмотреть цвета")
                {
                    await Navigation.PushAsync(new PatternDetailsPage(pattern, _currentUser));
                }
                else if (action == "🗑️ Удалить схему")
                {
                    var confirm = await DisplayAlert("Удаление",
                        $"Вы уверены, что хотите удалить схему '{selectedPattern.Name}'?",
                        "Да", "Нет");

                    if (confirm)
                    {
                        var deleted = await _databaseService.DeletePattern(pattern.Id, _currentUser.Id);
                        if (deleted)
                        {
                            await DisplayAlert("Успех", "Схема удалена", "OK");
                            LoadPatterns();
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
                        }
                    }
                }
            }

            PatternsCollection.SelectedItem = null;
        }

        public class PatternViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int ColorCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string ImagePath { get; set; }
            public string Info { get; set; }
        }
    }
}
