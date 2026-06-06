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

        protected override async void OnAppearing()  // ← изменили на async
        {
            base.OnAppearing();
            await LoadPatterns();  // ← теперь можно await
        }

        private async Task LoadPatterns()  // ← изменили с void на Task
        {
            try
            {
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
                    "✏️ Переименовать",
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
                else if (action == "✏️ Переименовать")
                {
                    await RenamePattern(pattern, selectedPattern);
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
                            await LoadPatterns();  // ← теперь можно await
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

        private async Task RenamePattern(EmbroideryPattern pattern, PatternViewModel viewModel)
        {
            var newName = await DisplayPromptAsync("Переименование схемы",
                "Введите новое название:",
                initialValue: pattern.Name,
                maxLength: 50,
                keyboard: Keyboard.Text);

            if (!string.IsNullOrEmpty(newName) && newName != pattern.Name)
            {
                var patterns = await _databaseService.GetUserPatterns(_currentUser.Id);
                if (patterns.Any(p => p.Name == newName && p.Id != pattern.Id))
                {
                    await DisplayAlert("Ошибка", "Схема с таким названием уже существует", "OK");
                    return;
                }

                pattern.Name = newName;
                await _databaseService.UpdatePattern(pattern);

                await LoadPatterns();  // ← теперь можно await

                await DisplayAlert("Успех", "Схема переименована", "OK");
            }
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