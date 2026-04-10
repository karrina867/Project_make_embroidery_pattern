using MauiApp1.Services;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class CreatePatternPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private string _selectedImagePath;  // Сохраняем путь вместо потока
        private byte[] _selectedImageBytes; // Или сохраняем в байты

        public CreatePatternPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
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
            // Валидация
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
                // Показываем индикатор загрузки
                SaveButton.IsEnabled = false;
                SaveButton.Text = "Сохранение...";

                // Читаем файл в массив байтов
                byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);

                // Создаем поток из байтов
                using (var stream = new MemoryStream(imageBytes))
                {
                    // Создаем схему
                    var pattern = new EmbroideryPattern
                    {
                        Name = PatternNameEntry.Text,
                        Width = width,
                        Height = height,
                        ColorCount = 0 // Позже рассчитаем реальное количество цветов
                    };

                    var (success, message, patternId) = await _databaseService.SavePattern(pattern, stream);

                    if (success)
                    {
                        await DisplayAlert("Успех", $"Схема '{PatternNameEntry.Text}' успешно сохранена!", "OK");
                        await Navigation.PopAsync(); // Возвращаемся в главное меню
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