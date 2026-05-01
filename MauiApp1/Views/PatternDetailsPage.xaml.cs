using MauiApp1.Services;
using MauiApp1.Models;
using System.Collections.ObjectModel;

namespace MauiApp1.Views
{
    public partial class PatternDetailsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly EmbroideryPattern _pattern;
        private readonly User _currentUser;
        private List<EmbroideryColor> _colors;

        public PatternDetailsPage(EmbroideryPattern pattern, User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _pattern = pattern;
            _currentUser = user;

            LoadPatternDetails();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void LoadPatternDetails()
        {
            try
            {
                // Проверяем права доступа
                if (_pattern.UserId != _currentUser.Id)
                {
                    await DisplayAlert("Ошибка", "У вас нет доступа к этой схеме", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                PatternNameLabel.Text = _pattern.Name;
                PatternSizeLabel.Text = $"Размер: {_pattern.Width} x {_pattern.Height} крестиков";
                PatternColorsLabel.Text = $"Количество цветов: {_pattern.ColorCount}";

                _colors = await _databaseService.GetPatternColors(_pattern.Id, _currentUser.Id);
                ColorsCollection.ItemsSource = _colors;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить детали схемы: {ex.Message}", "OK");
            }
        }

        private async void OnSavePngClicked(object sender, EventArgs e)
        {
            try
            {
                var processor = new ImageProcessor();
                var (colorMatrix, width, height) = processor.LoadColorMatrix(_pattern.PatternDataFile);

                // Определяем размер схемы
                int totalStitches = _pattern.Width * _pattern.Height;
                bool isLargePattern = totalStitches > 2500;

                string qualityChoice;

                // Если схема большая, предлагаем сразу разделить на страницы
                if (isLargePattern)
                {
                    var split = await DisplayAlert("Большая схема",
                        $"Схема имеет размер {_pattern.Width}x{_pattern.Height} ({totalStitches} крестиков). " +
                        $"Разделить на страницы для удобной печати?",
                        "Да, разделить на страницы", "Нет, одним файлом");

                    if (split)
                    {
                        // Выбор размера страницы
                        var pageSize = await DisplayActionSheet(
                            "Выберите размер страницы (в крестиках)",
                            "Отмена",
                            null,
                            "📄 Большая (50x50) - как A4",
                            "📃 Средняя (35x35) - как A5",
                            "📏 Маленькая (30x30)");

                        if (pageSize == "Отмена") return;

                        int pageWidth = 50, pageHeight = 50;
                        switch (pageSize)
                        {
                            case "📃 Средняя (35x35) - как A5":
                                pageWidth = 35; pageHeight = 35;
                                break;
                            case "📏 Маленькая (30x30)":
                                pageWidth = 30; pageHeight = 30;
                                break;
                        }

                        // Выбор качества для страниц
                        var cellSize = await DisplayActionSheet(
                            "Выберите качество изображения для страниц",
                            "Отмена",
                            null,
                            "📱 Маленькое (клетка 15px) - для телефона",
                            "🖨️ Среднее (клетка 25px) - для печати",
                            "📄 Большое (клетка 40px) - высокое качество");

                        if (cellSize == "Отмена") return;

                        int selectedCellSize = 25;
                        switch (cellSize)
                        {
                            case "📱 Маленькое (клетка 15px) - для телефона":
                                selectedCellSize = 15;
                                break;
                            case "🖨️ Среднее (клетка 25px) - для печати":
                                selectedCellSize = 25;
                                break;
                            case "📄 Большое (клетка 40px) - высокое качество":
                                selectedCellSize = 40;
                                break;
                        }

                        // Показываем индикатор загрузки
                        var loading = await DisplayAlert("Сохранение PNG",
                            "Генерация страниц... Это может занять несколько секунд.",
                            "Продолжить", "Отмена");

                        if (!loading) return;

                        // Сохраняем постранично
                        var pngPages = processor.SavePatternAsPages(
                            _pattern, _colors, colorMatrix,
                            pageWidth, pageHeight, selectedCellSize, true, true);

                        // Предлагаем сохранить страницы
                        var action = await DisplayActionSheet(
                            $"Создано {pngPages.Count} страниц!",
                            "Отмена",
                            null,
                            "💾 Сохранить все на устройство",
                            "📤 Поделиться всеми страницами");

                        if (action == "💾 Сохранить все на устройство")
                        {
                            foreach (var page in pngPages)
                            {
                                await SavePngToDevice(page);
                            }
                            await DisplayAlert("Успех", $"Сохранено {pngPages.Count} страниц в галерею", "OK");
                        }
                        else if (action == "📤 Поделиться всеми страницами")
                        {
                            foreach (var page in pngPages)
                            {
                                await Share.Default.RequestAsync(new ShareFileRequest
                                {
                                    Title = $"{_pattern.Name} - страница {pngPages.IndexOf(page) + 1}",
                                    File = new ShareFile(page)
                                });
                            }
                        }

                        return;
                    }
                }

                // Если схема маленькая ИЛИ пользователь выбрал "Нет, одним файлом"
                // Показываем диалог выбора размера клетки
                var qualityChoiceNormal = await DisplayActionSheet(
                    "Выберите качество изображения",
                    "Отмена",
                    null,
                    "📱 Маленькое (клетка 15px) - для телефона",
                    "🖨️ Среднее (клетка 25px) - для печати",
                    "📄 Большое (клетка 40px) - высокое качество");

                if (qualityChoiceNormal == "Отмена") return;

                int selectedCellSizeNormal = 25;
                switch (qualityChoiceNormal)
                {
                    case "📱 Маленькое (клетка 15px) - для телефона":
                        selectedCellSizeNormal = 15;
                        break;
                    case "🖨️ Среднее (клетка 25px) - для печати":
                        selectedCellSizeNormal = 25;
                        break;
                    case "📄 Большое (клетка 40px) - высокое качество":
                        selectedCellSizeNormal = 40;
                        break;
                }

                // Показываем индикатор загрузки
                var loadingNormal = await DisplayAlert("Сохранение PNG",
                    "Генерация изображения... Это может занять несколько секунд.",
                    "Продолжить", "Отмена");

                if (!loadingNormal) return;

                // Сохраняем в PNG
                var pngPath = processor.SavePatternAsPng(_pattern, _colors, colorMatrix, selectedCellSizeNormal, true, true);

                // Предлагаем сохранить или поделиться
                var actionNormal = await DisplayActionSheet(
                    "PNG схема готова!",
                    "Отмена",
                    null,
                    "💾 Сохранить на устройство",
                    "📤 Поделиться");

                if (actionNormal == "💾 Сохранить на устройство")
                {
                    await SavePngToDevice(pngPath);
                }
                else if (actionNormal == "📤 Поделиться")
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = $"Схема вышивки {_pattern.Name}",
                        File = new ShareFile(pngPath)
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
        }

        private async Task SavePngToDevice(string pngPath)
        {
            try
            {
#if ANDROID
                // Для Android 13+ нужно запросить разрешение
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
        // Для iOS сохраняем в Photos
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
        // Для Windows сохраняем в папку Pictures
        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var destPath = Path.Combine(picturesPath, Path.GetFileName(pngPath));
        File.Copy(pngPath, destPath, true);
        await DisplayAlert("Успех", $"Схема сохранена в:\n{destPath}", "OK");
        await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(destPath) });
#else
        // Для других платформ просто показываем файл
        await DisplayAlert("Успех", $"Схема сохранена во временную папку:\n{pngPath}", "OK");
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            try
            {
                var exportText = $"Схема вышивки: {_pattern.Name}\n";
                exportText += $"Размер: {_pattern.Width} x {_pattern.Height}\n";
                exportText += $"Дата создания: {_pattern.CreatedAt:dd.MM.yyyy}\n\n";
                exportText += "Цвета и символы:\n";
                exportText += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";

                foreach (var color in _colors)
                {
                    exportText += $"{color.Symbol} - {color.ColorHex} (DMC: {color.DMCNumber}) - {color.PixelCount} крестиков\n";
                }

                var fileName = $"{_pattern.Name}_схема.txt";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                File.WriteAllText(filePath, exportText);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Экспорт схемы",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось экспортировать: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Удаление",
                $"Вы уверены, что хотите удалить схему '{_pattern.Name}'?",
                "Да", "Нет");

            if (confirm)
            {
                var deleted = await _databaseService.DeletePattern(_pattern.Id, _currentUser.Id);
                if (deleted)
                {
                    await DisplayAlert("Успех", "Схема удалена", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить схему", "OK");
                }
            }
        }
    }
}