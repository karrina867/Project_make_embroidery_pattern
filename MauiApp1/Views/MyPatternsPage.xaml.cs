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

        //private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
        //    {
        //        var action = await DisplayActionSheet(
        //            $"Схема: {selectedPattern.Name}",
        //            "Отмена",
        //            null,
        //            "👁️ Просмотреть схему",
        //            "📊 Посмотреть цвета",
        //            "✏️ Переименовать",
        //            "🗑️ Удалить схему");

        //        var pattern = await _databaseService.GetPatternById(selectedPattern.Id, _currentUser.Id);

        //        if (pattern == null)
        //        {
        //            await DisplayAlert("Ошибка", "Схема не найдена", "OK");
        //            return;
        //        }

        //        if (action == "👁️ Просмотреть схему")
        //        {
        //            await Navigation.PushAsync(new PatternViewerPage(pattern, _currentUser));
        //        }
        //        else if (action == "📊 Посмотреть цвета")
        //        {
        //            await Navigation.PushAsync(new PatternDetailsPage(pattern, _currentUser));
        //        }
        //        else if (action == "✏️ Переименовать")
        //        {
        //            await RenamePattern(pattern, selectedPattern);
        //        }
        //        else if (action == "🗑️ Удалить схему")
        //        {
        //            var confirm = await DisplayAlert("Удаление",
        //                $"Вы уверены, что хотите удалить схему '{selectedPattern.Name}'?",
        //                "Да", "Нет");

        //            if (confirm)
        //            {
        //                var deleted = await _databaseService.DeletePattern(pattern.Id, _currentUser.Id);
        //                if (deleted)
        //                {
        //                    await DisplayAlert("Успех", "Схема удалена", "OK");
        //                    await LoadPatterns();  // ← теперь можно await
        //                }
        //                else
        //                {
        //                    await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
        //                }
        //            }
        //        }
        //    }

        //    PatternsCollection.SelectedItem = null;
        //}
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
        private async void OnPatternSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PatternViewModel selectedPattern)
            {
                // 1. Формируем обновленный список действий в окне
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

                // Подгружаем саму схему из базы данных
                var pattern = await _databaseService.GetPatternById(selectedPattern.Id, _currentUser.Id);

                if (pattern == null)
                {
                    await DisplayAlert("Ошибка", "Схема не найдена", "OK");
                    return;
                }

                // 2. Обрабатываем выбор пользователя
                if (action == "Просмотреть схему")
                {
                    await Navigation.PushAsync(new PatternViewerPage(pattern, _currentUser));
                }
                else if (action == "Вышивать схему")
                {
                    // Переход на страницу процесса вышивки (замени PatternProgressPage на твое точное название класса, если оно отличается)
                    //работает но подгружает не откуда надо (или нет я не помню)
                    await Navigation.PushAsync(new PatternProgressPage(pattern, _currentUser));
                }
                else if (action == "Просмотреть нитки")
                {
                    await Navigation.PushAsync(new PatternDetailsPage(pattern, _currentUser));
                }
                else if (action == "Сохранить в формат PNG")
                {
                    // Вызываем метод сохранения в PNG. 
                    // Обрати внимание: если этот метод у тебя был написан внутри другой страницы, 
                    // убедись, что он доступен здесь или перенеси его логику (обычно через ImageProcessor или DatabaseService)

                    await ExportPatternToPng(pattern);
                    //ЭТО НЕ ТО, ПОМЕНЯТЬ
                    //await ExportToPng(pattern);
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
                            await DisplayAlert("Успех", "Схема удалена", "OK");
                            await LoadPatterns();
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
                        }
                    }
                }
            }

            // Сбрасываем выделение элемента в списке, чтобы по нему можно было кликнуть снова
            PatternsCollection.SelectedItem = null;
        }




        private async Task RenamePattern(EmbroideryPattern pattern, PatternViewModel viewModel)
        {
            var newName = await DisplayPromptAsync(
                "Переименование схемы",
                "Введите новое название:",
                "Переименовать",          // Текст для кнопки подтверждения
                "Отмена",      // Текст для кнопки отмены
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

        private async Task ExportPatternToPng(EmbroideryPattern pattern)
        {
            try
            {
                var processor = new ImageProcessor();
                var (colorMatrix, width, height) = processor.LoadColorMatrix(pattern.PatternDataFile);

                // Подгружаем цвета схемы из базы данных, так как на этой странице нет глобального списка цветов
                var patternColors = await _databaseService.GetPatternColors(pattern.Id, _currentUser.Id);

                // ШАГ 1: Выбор режима (цветной/контурный)
                var colorMode = await DisplayActionSheet(
                    "Выберите режим сохранения",
                    "Отмена",
                    null,
                    "🎨 Цветной",
                    "⚫ Контурный (только сетка и символы)");

                if (colorMode == "Отмена") return;
                bool isOutline = colorMode == "⚫ Контурный (только сетка и символы)";

                // ШАГ 2: Выбор размера клетки (качество)
                var cellSize = await DisplayActionSheet(
                    "Выберите размер клетки",
                    "Отмена",
                    null,
                    "📱 Маленькая (15px) - для телефона",
                    "🖨️ Средняя (25px) - для печати",
                    "📄 Большая (40px) - высокое качество");

                if (cellSize == "Отмена") return;

                int selectedCellSize = 25;
                switch (cellSize)
                {
                    case "📱 Маленькая (15px) - для телефона":
                        selectedCellSize = 15;
                        break;
                    case "📄 Большая (40px) - высокое качество":
                        selectedCellSize = 40;
                        break;
                }

                // ШАГ 3: Определяем размер схемы и спрашиваем про разделение на страницы
                int totalStitches = pattern.Width * pattern.Height;
                bool needSplit = totalStitches > 2500;

                string splitChoice = "📄 Одним файлом";
                if (needSplit)
                {
                    splitChoice = await DisplayActionSheet(
                        $"Схема имеет размер {pattern.Width}x{pattern.Height} ({totalStitches} крестиков).",
                        "Отмена",
                        null,
                        "📄 Одним файлом",
                        "📚 Разделить на страницы (для удобной печати)");

                    if (splitChoice == "Отмена") return;
                }

                // ШАГ 4: Если выбрано разделение на страницы - выбираем размер страницы
                int stitchesPerPage = 50;
                if (splitChoice == "📚 Разделить на страницы (для удобной печати)")
                {
                    var pageSize = await DisplayActionSheet(
                        "Выберите размер страницы",
                        "Отмена",
                        null,
                        "📄 Большая (50x70) - как лист A4",
                        "📃 Средняя (35x50)",
                        "📏 Маленькая (30x42)");

                    if (pageSize == "Отмена") return;

                    switch (pageSize)
                    {
                        case "📃 Средняя (35x50)":
                            stitchesPerPage = 35;
                            break;
                        case "📏 Маленькая (30x42)":
                            stitchesPerPage = 30;
                            break;
                    }
                }

                // ШАГ 5: Сообщение о начале генерации
                var loading = await DisplayAlert("Сохранение",
                    "Генерация изображения... Это может занять несколько секунд.",
                    "Продолжить", "Отмена");

                if (!loading) return;

                // ШАГ 6: Генерация и сохранение
                if (splitChoice == "📚 Разделить на страницы (для удобной печати)")
                {
                    List<string> pages;

                    if (isOutline)
                    {
                        pages = processor.SavePatternAsOutlineA4Pages(
                            pattern, patternColors, colorMatrix,
                            selectedCellSize, true, true, stitchesPerPage);
                    }
                    else
                    {
                        pages = processor.SavePatternAsA4Pages(
                            pattern, patternColors, colorMatrix,
                            selectedCellSize, true, true, stitchesPerPage);
                    }

                    // ШАГ 7: Сообщение о количестве страниц
                    var action = await DisplayActionSheet(
                        $"Создано {pages.Count} страниц!",
                        "Отмена",
                        null,
                        "💾 Сохранить все на устройство",
                        "📤 Поделиться всеми страницами");

                    if (action == "💾 Сохранить все на устройство")
                    {
                        await DisplayAlert("Сохранение",
                            $"Сохранение {pages.Count} страниц в галерею...",
                            "OK");

                        for (int i = 0; i < pages.Count; i++)
                        {
                            await ListPageSavePngToDevice(pages[i]);
                            if (i < pages.Count - 1)
                            {
                                await Task.Delay(300);
                            }
                        }
                        await DisplayAlert("Успех", $"Сохранено {pages.Count} страниц", "OK");
                    }
                    else if (action == "📤 Поделиться всеми страницами")
                    {
                        await DisplayAlert("Поделиться",
                            $"Открывается окно для каждой из {pages.Count} страниц...",
                            "OK");

                        for (int i = 0; i < pages.Count; i++)
                        {
                            await Share.Default.RequestAsync(new ShareFileRequest
                            {
                                Title = $"{pattern.Name} - страница {i + 1} из {pages.Count}",
                                File = new ShareFile(pages[i])
                            });
                            if (i < pages.Count - 1)
                            {
                                await Task.Delay(500);
                            }
                        }
                    }
                }
                else // Одним файлом
                {
                    string pngPath;

                    if (isOutline)
                    {
                        pngPath = processor.SavePatternAsOutlinePng(pattern, patternColors, colorMatrix, selectedCellSize, true, true);
                    }
                    else
                    {
                        pngPath = processor.SavePatternAsPng(pattern, patternColors, colorMatrix, selectedCellSize, true, true);
                    }

                    var action = await DisplayActionSheet(
                        "PNG схема готова!",
                        "Отмена",
                        null,
                        "💾 Сохранить на устройство",
                        "📤 Поделиться");

                    if (action == "💾 Сохранить на устройство")
                    {
                        await ListPageSavePngToDevice(pngPath);
                    }
                    else if (action == "📤 Поделиться")
                    {
                        await Share.Default.RequestAsync(new ShareFileRequest
                        {
                            Title = $"Схема вышивки {pattern.Name}",
                            File = new ShareFile(pngPath)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
        }

        private async Task ListPageSavePngToDevice(string pngPath)
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
                await DisplayAlert("Успех", "Схема сохранена в папку Pictures/EmbroideryPatterns", "OK");
            }
        }
#elif IOS
        var image = UIKit.UIImage.FromFile(pngPath);
        image.SaveToPhotosAlbum((img, error) =>
        {
            if (error != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось сохранить: {error.LocalizedDescription}", "OK");
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Успех", "Схема сохранена в галерею", "OK");
                });
            }
        });
#elif WINDOWS
        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var destPath = Path.Combine(picturesPath, Path.GetFileName(pngPath));
        File.Copy(pngPath, destPath, true);
        await DisplayAlert("Успех", $"Схема сохранена в:\n{destPath}", "OK");
        await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(destPath) });
#else
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