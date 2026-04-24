using MauiApp1.Services;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class CreatePatternPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private string _selectedImagePath;  // Сохраняем путь вместо потока
        private byte[] _selectedImageBytes; // Или сохраняем в байты

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
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите изображение",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    _selectedImagePath = result.FullPath;

                    // Показываем превью
                    SelectedImage.Source = ImageSource.FromFile(_selectedImagePath);

                    await AnalyzeImage();

                    SaveButton.IsEnabled = true;
                    MessageLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить изображение: {ex.Message}", "OK");
            }
        }

        private async Task AnalyzeImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(WidthEntry.Text) && !string.IsNullOrEmpty(HeightEntry.Text) && !string.IsNullOrEmpty(_selectedImagePath))
                {
                    int width = int.Parse(WidthEntry.Text);
                    int height = int.Parse(HeightEntry.Text);

                    ColorInfoLabel.IsVisible = true;
                    ColorInfoLabel.Text = $"📊 Анализ изображения:\n" +
                                         $"Размер схемы: {width} x {height} крестиков\n" +
                                         $"Ожидаемое количество цветов: ~{Math.Min(width * height / 100, 50)}\n" +
                                         $"Совет: чем меньше размер, тем проще вышивать";
                }
            }
            catch { }
        }

        private async void OnSavePatternClicked(object sender, EventArgs e)
        {
            // Валидация (как была)
            if (string.IsNullOrWhiteSpace(PatternNameEntry.Text))
            {
                MessageLabel.Text = "Введите название схемы";
                return;
            }

            if (!int.TryParse(WidthEntry.Text, out int width) || width <= 0 || width > 500)
            {
                MessageLabel.Text = "Введите корректную ширину (1-500)";
                return;
            }

            if (!int.TryParse(HeightEntry.Text, out int height) || height <= 0 || height > 500)
            {
                MessageLabel.Text = "Введите корректную высоту (1-500)";
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
                var colors = await processor.ProcessImage(_selectedImagePath, width, height);

                SaveButton.Text = "Сохранение схемы...";

                byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);

                using (var stream = new MemoryStream(imageBytes))
                {
                    var pattern = new EmbroideryPattern
                    {
                        Name = PatternNameEntry.Text,
                        Width = width,
                        Height = height,
                        ColorCount = colors.Count,
                        CellSize = 10
                    };

                    // Передаем ID текущего пользователя
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

        private void OnWidthHeightChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(WidthEntry.Text) && !string.IsNullOrEmpty(HeightEntry.Text) && !string.IsNullOrEmpty(_selectedImagePath))
            {
                AnalyzeImage();
            }
        }
    }
}