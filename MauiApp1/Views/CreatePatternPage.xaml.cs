using MauiApp1.Services;
using MauiApp1.Models;
using SkiaSharp;

namespace MauiApp1.Views
{
    public partial class CreatePatternPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private string _selectedImagePath;
        private int _originalImageWidth = 0;
        private int _originalImageHeight = 0;
        private bool _isUpdating = false; // Чтобы избежать зацикливания

        public CreatePatternPage(User user)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _currentUser = user;
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                // Настройка поддерживаемых форматов изображений
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                { DevicePlatform.Android, new[] { "image/jpeg", "image/png", "image/webp" } },
                { DevicePlatform.iOS, new[] { "public.image" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".webp" } }
                    });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите изображение",
                    FileTypes = customFileType  // ← используем кастомный тип с поддержкой WEBP
                });

                if (result != null)
                {
                    _selectedImagePath = result.FullPath;
                    SelectedImage.Source = ImageSource.FromFile(_selectedImagePath);

                    // Получаем оригинальные размеры изображения
                    using (var stream = File.OpenRead(_selectedImagePath))
                    using (var bitmap = SKBitmap.Decode(stream))
                    {
                        if (bitmap != null)
                        {
                            _originalImageWidth = bitmap.Width;
                            _originalImageHeight = bitmap.Height;
                        }
                    }

                    SaveButton.IsEnabled = true;
                    MessageLabel.Text = "";

                    await AnalyzeImage();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить изображение: {ex.Message}", "OK");
            }
        }

        private void OnWidthChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (KeepAspectCheckBox.IsChecked && _originalImageWidth > 0 && _originalImageHeight > 0)
            {
                if (int.TryParse(WidthEntry.Text, out int newWidth) && newWidth > 0)
                {
                    _isUpdating = true;

                    // Вычисляем высоту по пропорции: newHeight = newWidth * (originalHeight / originalWidth)
                    int newHeight = (int)(newWidth * (double)_originalImageHeight / _originalImageWidth);
                    HeightEntry.Text = newHeight.ToString();

                    _isUpdating = false;
                }
            }

            AnalyzeImage();
        }

        private void OnHeightChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (KeepAspectCheckBox.IsChecked && _originalImageWidth > 0 && _originalImageHeight > 0)
            {
                if (int.TryParse(HeightEntry.Text, out int newHeight) && newHeight > 0)
                {
                    _isUpdating = true;

                    // Вычисляем ширину по пропорции: newWidth = newHeight * (originalWidth / originalHeight)
                    int newWidth = (int)(newHeight * (double)_originalImageWidth / _originalImageHeight);
                    WidthEntry.Text = newWidth.ToString();

                    _isUpdating = false;
                }
            }

            AnalyzeImage();
        }

        private async Task AnalyzeImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(WidthEntry.Text) &&
                    !string.IsNullOrEmpty(HeightEntry.Text) &&
                    !string.IsNullOrEmpty(_selectedImagePath))
                {
                    int width = int.Parse(WidthEntry.Text);
                    int height = int.Parse(HeightEntry.Text);

                    ColorInfoLabel.IsVisible = true;

                    string aspectInfo = "";
                    if (KeepAspectCheckBox.IsChecked && _originalImageWidth > 0)
                    {
                        double originalRatio = (double)_originalImageWidth / _originalImageHeight;
                        double currentRatio = (double)width / height;
                        if (Math.Abs(originalRatio - currentRatio) > 0.01)
                        {
                            aspectInfo = "\n⚠️ Пропорции отличаются от оригинала!";
                        }
                    }

                    ColorInfoLabel.Text = $"📊 Анализ изображения:\n" +
                                         $"Размер схемы: {width} x {height} крестиков\n" +
                                         $"Всего крестиков: {width * height}\n" +
                                         $"Ожидаемое количество цветов: ~{Math.Min(width * height / 100, 50)}" +
                                         aspectInfo;
                }
            }
            catch { }
        }

        private const int MAX_PATTERN_SIZE = 500;
        private const int MAX_PATTERNS_PER_USER = 100;

        private async void OnSavePatternClicked(object sender, EventArgs e)
        {
            // Валидация названия
            if (string.IsNullOrWhiteSpace(PatternNameEntry.Text))
            {
                MessageLabel.Text = "Введите название схемы";
                return;
            }

            // Проверка размера схемы
            if (!int.TryParse(WidthEntry.Text, out int width) || width <= 0 || width > MAX_PATTERN_SIZE)
            {
                MessageLabel.Text = $"Введите корректную ширину (1-{MAX_PATTERN_SIZE})";
                return;
            }

            if (!int.TryParse(HeightEntry.Text, out int height) || height <= 0 || height > MAX_PATTERN_SIZE)
            {
                MessageLabel.Text = $"Введите корректную высоту (1-{MAX_PATTERN_SIZE})";
                return;
            }

            // Проверка количества схем пользователя
            var userPatterns = await _databaseService.GetUserPatterns(_currentUser.Id);
            if (userPatterns.Count >= MAX_PATTERNS_PER_USER)
            {
                await DisplayAlert("Ограничение", $"Вы можете создать не более {MAX_PATTERNS_PER_USER} схем", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageLabel.Text = "Выберите изображение";
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Text = "Обработка изображения...";

                var processor = new ImageProcessor();
                var (colors, colorMatrix) = await processor.ProcessImageWithMatrix(_selectedImagePath, width, height);

                SaveButton.Text = "Сохранение схемы...";

                var matrixFileName = processor.SaveColorMatrix(colorMatrix, PatternNameEntry.Text);

                byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);

                using (var stream = new MemoryStream(imageBytes))
                {
                    var pattern = new EmbroideryPattern
                    {
                        Name = PatternNameEntry.Text,
                        Width = width,
                        Height = height,
                        ColorCount = colors.Count,
                        CellSize = 30,
                        PatternDataFile = matrixFileName
                    };

                    var (success, message, patternId) = await _databaseService.SavePattern(pattern, stream, _currentUser.Id);

                    if (success)
                    {
                        await _databaseService.SavePatternColors(patternId, colors);
                        await _databaseService.UpdatePatternColorCount(patternId, colors.Count);

                        await DisplayAlert("Успех",
                            $"Схема '{PatternNameEntry.Text}' сохранена!\n" +
                            $"Размер: {width} x {height} крестиков\n" +
                            $"Количество цветов: {colors.Count}",
                            "OK");

                        await Navigation.PopAsync();
                    }
                    else
                    {
                        MessageLabel.Text = message;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = "💾 Сохранить схему";
            }
        }
        //private async void OnSavePatternClicked(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(PatternNameEntry.Text))
        //    {
        //        MessageLabel.Text = "Введите название схемы";
        //        return;
        //    }

        //    if (!int.TryParse(WidthEntry.Text, out int width) || width <= 0 || width > 500)
        //    {
        //        MessageLabel.Text = "Введите корректную ширину (1-500)";
        //        return;
        //    }

        //    if (!int.TryParse(HeightEntry.Text, out int height) || height <= 0 || height > 500)
        //    {
        //        MessageLabel.Text = "Введите корректную высоту (1-500)";
        //        return;
        //    }

        //    if (string.IsNullOrEmpty(_selectedImagePath))
        //    {
        //        MessageLabel.Text = "Выберите изображение";
        //        return;
        //    }

        //    try
        //    {
        //        SaveButton.IsEnabled = false;
        //        SaveButton.Text = "Обработка изображения...";

        //        var processor = new ImageProcessor();
        //        var (colors, colorMatrix) = await processor.ProcessImageWithMatrix(_selectedImagePath, width, height);

        //        SaveButton.Text = "Сохранение схемы...";

        //        var matrixFileName = processor.SaveColorMatrix(colorMatrix, PatternNameEntry.Text);

        //        byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);

        //        using (var stream = new MemoryStream(imageBytes))
        //        {
        //            var pattern = new EmbroideryPattern
        //            {
        //                Name = PatternNameEntry.Text,
        //                Width = width,
        //                Height = height,
        //                ColorCount = colors.Count,
        //                CellSize = 30,
        //                PatternDataFile = matrixFileName
        //            };

        //            var (success, message, patternId) = await _databaseService.SavePattern(pattern, stream, _currentUser.Id);

        //            if (success)
        //            {
        //                await _databaseService.SavePatternColors(patternId, colors);
        //                await _databaseService.UpdatePatternColorCount(patternId, colors.Count);

        //                await DisplayAlert("Успех",
        //                    $"Схема '{PatternNameEntry.Text}' сохранена!\n" +
        //                    $"Размер: {width} x {height} крестиков\n" +
        //                    $"Количество цветов: {colors.Count}",
        //                    "OK");

        //                await Navigation.PopAsync();
        //            }
        //            else
        //            {
        //                MessageLabel.Text = message;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageLabel.Text = $"Ошибка: {ex.Message}";
        //    }
        //    finally
        //    {
        //        SaveButton.IsEnabled = true;
        //        SaveButton.Text = "💾 Сохранить схему";
        //    }
        //}
    }
}