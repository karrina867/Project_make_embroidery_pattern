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

        protected override async void OnAppearing() 
        {
            base.OnAppearing();
            await LoadPatterns();
        }

        private async Task LoadPatterns()
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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
        private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
            {

                var action = await DisplayActionSheet(
                    $"Схема: {selectedPattern.Name}",
                    "Отмена",
                    null,
                    "Просмотреть схему",
                    "Вышивать схему",
                    "Просмотреть нитки",
                    "Сохранить в формат PNG",
                    "Переименовать схему",
                    "Удалить схему");

                var pattern = await _databaseService.GetPatternById(selectedPattern.Id, _currentUser.Id);

                if (pattern == null)
                {
                    await DisplayAlert("Ошибка", "Схема не найдена", "OK");
                    return;
                }

                if (action == "Просмотреть схему")
                {
                    await Navigation.PushAsync(new PatternViewerPage(pattern, _currentUser));
                }
                else if (action == "Вышивать схему")
                {
                    await Navigation.PushAsync(new PatternProgressPage(pattern, _currentUser));
                }
                else if (action == "Просмотреть нитки")
                {
                    await Navigation.PushAsync(new PatternDetailsPage(pattern, _currentUser));
                }
                else if (action == "Сохранить в формат PNG")
                {
                    await ExportPatternToPng(pattern);

                }
                else if (action == "Переименовать схему")
                {
                    await RenamePattern(pattern, selectedPattern);
                }
                else if (action == "Удалить схему")
                {
                    var confirm = await DisplayAlert("Удаление",
                        $"Вы уверены, что хотите удалить схему '{selectedPattern.Name}'?",
                        "Да", "Нет");

                    if (confirm)
                    {
                        var deleted = await _databaseService.DeletePattern(pattern.Id, _currentUser.Id);
                        if (deleted)
                        {
                            await LoadPatterns();
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
            var newName = await DisplayPromptAsync(
                "Переименование схемы",
                "Введите новое название:",
                "Переименовать", 
                "Отмена",
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

                await LoadPatterns(); 

            }
        }

        private async Task ExportPatternToPng(EmbroideryPattern pattern)
        {
            try
            {
                var processor = new ImageProcessor();
                var (colorMatrix, width, height) = processor.LoadColorMatrix(pattern.PatternDataFile);
                var patternColors = await _databaseService.GetPatternColors(pattern.Id, _currentUser.Id);

                var choice = await DisplayActionSheet(
                    "Выберите тип схемы",
                    "Отмена",
                    null,
                    "Цветная",
                    "Контурная");

                if (choice == "Отмена" || choice == null) return;
                bool isOutline = choice == "Контурная";

                List<string> pages = null;
                await Task.Run(() =>
                {
                    pages = processor.ExportToA4Auto(pattern, patternColors, colorMatrix, isOutline);
                });

                int saved = 0;
                foreach (var page in pages)
                {
                    await ListPageSavePngToDevice(page, silent: true);
                    saved++;
                }

                string msg = pages.Count == 1
                    ? "Схема сохранена в галерею"
                    : $"Схема сохранена в галерею ({pages.Count} стр.)";
                await DisplayAlert("Готово", msg, "Понятно");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
        }

        private async Task ListPageSavePngToDevice(string pngPath, bool silent = false)
        {
            try
            {
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var status = await Permissions.RequestAsync<Permissions.Media>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Ошибка", "Нет разрешения на сохранение файлов", "OK");
                return;
            }
        }
        else
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Ошибка", "Нет разрешения на сохранение файлов", "OK");
                return;
            }
        }

        using (var stream = File.OpenRead(pngPath))
        {
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);

            var contentValues = new Android.Content.ContentValues();
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, Path.GetFileName(pngPath));
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryPictures + "/EmbroideryPatterns");

            var uri = Android.App.Application.Context.ContentResolver.Insert(
                Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                contentValues);

            if (uri != null)
            {
                using (var outputStream = Android.App.Application.Context.ContentResolver.OpenOutputStream(uri))
                {
                    await outputStream.WriteAsync(bytes, 0, bytes.Length);
                }
                if (!silent)
                    await DisplayAlert("Успех", "Схема сохранена в папку Pictures/EmbroideryPatterns", "OK");
            }
        }
#elif IOS
        var image = UIKit.UIImage.FromFile(pngPath);
        image.SaveToPhotosAlbum((img, error) =>
        {
            if (error != null && !silent)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось сохранить: {error.LocalizedDescription}", "OK");
                });
            }
        });
#elif WINDOWS
        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var destPath = Path.Combine(picturesPath, Path.GetFileName(pngPath));
        File.Copy(pngPath, destPath, true);
        if (!silent)
        {
            await DisplayAlert("Успех", $"Схема сохранена в:\n{destPath}", "OK");
            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(destPath) });
        }
#else
        if (!silent)
            await DisplayAlert("Успех", $"Схема сохранена во временную папку:\n{pngPath}", "OK");
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
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