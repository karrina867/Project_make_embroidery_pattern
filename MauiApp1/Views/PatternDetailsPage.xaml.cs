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

        // НОВЫЙ МЕТОД - Сохранение в PNG
        private async void OnSavePngClicked(object sender, EventArgs e)
        {
            try
            {
                // Показываем диалог выбора размера клетки
                var cellSize = await DisplayActionSheet(
                    "Выберите качество изображения",
                    "Отмена",
                    null,
                    "📱 Маленькое (клетка 15px) - для телефона",
                    "🖨️ Среднее (клетка 25px) - для печати",
                    "📄 Большое (клетка 40px) - высокое качество");

                int selectedCellSize = 25; // по умолчанию

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
                    default:
                        if (cellSize == "Отмена") return;
                        break;
                }

                // Показываем индикатор загрузки
                var loading = await DisplayAlert("Сохранение PNG",
                    "Генерация изображения... Это может занять несколько секунд.",
                    "Продолжить", "Отмена");

                if (!loading) return;

                var processor = new ImageProcessor();

                // Загружаем матрицу цветов
                var (colorMatrix, width, height) = processor.LoadColorMatrix(_pattern.PatternDataFile);

                // Сохраняем в PNG
                var pngPath = processor.SavePatternAsPng(_pattern, _colors, colorMatrix, selectedCellSize, true, true);

                // Предлагаем сохранить или поделиться
                var action = await DisplayActionSheet(
                    "PNG схема готова!",
                    "Отмена",
                    null,
                    "💾 Сохранить на устройство",
                    "📤 Поделиться");

                if (action == "💾 Сохранить на устройство")
                {
                    await SavePngToDevice(pngPath);
                }
                else if (action == "📤 Поделиться")
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
                await DisplayAlert("Ошибка", $"Не удалось сохранить PNG: {ex.Message}", "OK");
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